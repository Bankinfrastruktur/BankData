using Bankinfrastruktur.Helpers;

namespace UpdateChecker;

public static class GrabAndDownload
{
    public static Task<Page[]> GetPages()
        => Task.WhenAll(
            GetPage(new Uri("https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer"), true, "bankinfrastruktur_iban-och-svenskt-nationellt-kontonummer.html"),
            GetPage(new Uri("https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/konto-och-clearingnummer"), false, "bankinfrastruktur_konto-och-clearingnummer.html"),
            GetPage(new Uri("https://www.swedishbankers.se/fraagor-vi-arbetar-med/finansiell-infrastruktur/dataclearingen/"), false, "swedishbankers_dataclearingen.html"));

    public class Document(Uri uri, BinaryData data, string srcSha1, WaybackSnapshot? archiveMetadata, string? localName = null)
    {
        public string LocalName => localName ?? Path.GetFileName(Url.LocalPath);
        public Uri Url { get; } = uri;
        public BinaryData Data { get; } = data;
        public string Sha1 { get; } = srcSha1;
        public WaybackSnapshot? ArchiveMetadata { get; internal set; } = archiveMetadata;
        public bool DidWaybackSaveCall { get; set; }

        public Uri UrlPreferArchive => ArchiveMetadata?.Digest == Sha1 ? ArchiveMetadata.ShowUrl : Url;

        public override string ToString() => $"{UrlPreferArchive} {Data.Length} {Sha1}";
    }

    public class Page
    {
        public List<Document> Documents { get; } = [];
    }

    private static async Task<Page> GetPage(Uri url, bool ensureBankgirot, string localName)
    {
        Console.WriteLine($"Fetch HTML {url} ... ");
        // collect documents linked from page
        var pageArchiveMetadataTask = WaybackSnapshot.GetArchiveDataNoExceptions(url, filterMime: null);
        var pageData = await DocumentHelpers.FetchToMemoryAsync(url);
        var documentUrls = DocumentHelpers.GetDocumentUrisFromPage(pageData, url).ToHashSet();
        if (ensureBankgirot)
            documentUrls.Add(new Uri("https://www.bankgirot.se/globalassets/dokument/anvandarmanualer/bankernaskontonummeruppbyggnad_anvandarmanual_sv.pdf"));
        var page = new Page();
        foreach (var u in documentUrls)
        {
            var mime = u.PathAndQuery.EndsWith(".docx") ?
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" :
                WaybackSnapshot.MimeApplicationPdf;
            var archiveMetadataTask = WaybackSnapshot.GetArchiveDataNoExceptions(u, mime);
            var srcDataTask = DocumentHelpers.FetchToMemoryAsync(u);
            var archiveMetadata = await archiveMetadataTask;
            var srcData = await srcDataTask;
            var srcSha1 = await srcData.GetSha1Base32Async();
            var doc = new Document(u, srcData, srcSha1, archiveMetadata);
            page.Documents.Add(doc);
            Console.WriteLine($" * Downloaded {u} {srcSha1} {srcData.Length:#,##0}");
            if (archiveMetadata is null ||
                srcSha1 == archiveMetadata.Digest) continue;
            Console.WriteLine($"{u} new: {srcSha1} archived {archiveMetadata.Digest}");
            await WaybackSnapshot.RequestSaveAsync(u);
            doc.DidWaybackSaveCall = true;
            doc.ArchiveMetadata = await WaybackSnapshot.GetArchiveDataNoExceptions(u, mime);
        }

        var pageArchiveMetadata = await pageArchiveMetadataTask;
        var pageSha1 = await pageData.GetSha1Base32Async();
        var pageDoc = new Document(url, pageData, pageSha1, pageArchiveMetadata, localName);
        page.Documents.Add(pageDoc);
        if (pageArchiveMetadata is null)
        {
            Console.WriteLine($" * maindl new {url}\n ** Downloaded {pageSha1} {pageData.Length}");
            await WaybackSnapshot.RequestSaveAsync(url);
            pageDoc.DidWaybackSaveCall = true;
        }
        else
        {
            Console.WriteLine($" * maindl  {pageArchiveMetadata}\n ** Downloaded {pageSha1} {pageData.Length}");
        }

        return page;
    }
}
