using Bankinfrastruktur.Helpers;

namespace UpdateChecker;

public static class GrabAndDownload
{
    public static Task<Page[]> GetPages()
    {
        return Task.WhenAll(
            GetPage(new Uri("https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer"), true),
            GetPage(new Uri("https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/konto-och-clearingnummer"), false),
            GetPage(new Uri("https://www.swedishbankers.se/fraagor-vi-arbetar-med/finansiell-infrastruktur/dataclearingen/"), false));
    }

    public class Document
    {
        public Uri Url { get; private set; }
        public MemoryStream Data { get; private set; }

        public Document(Uri uri, MemoryStream data)
        {
            Url = uri;
            Data = data;
        }

        public override string ToString() => $"{Url} {Data.Length}";
    }

    public class Page
    {
        public List<Document> Documents { get; } = new();
    }

    private static async Task<Page> GetPage(Uri url, bool ensureBankgirot)
    {
        Console.WriteLine($"Fetch HTML {url} ... ");
        WaybackSnapshot? lastSnap = null;
        // collect documents linked from page
        var pageArchiveMetadataTask = WaybackSnapshot.GetArchiveDataNoExceptions(url, filterMime: null);

        var documentUrls = (await DocumentHelpers.GetDocumentUrisFromPage(url)).ToHashSet();
        if (ensureBankgirot)
            documentUrls.Add(new Uri("https://www.bankgirot.se/globalassets/dokument/anvandarmanualer/bankernaskontonummeruppbyggnad_anvandarmanual_sv.pdf"));
        var page = new Page();
        foreach (var u in documentUrls)
        {
            var mime = u.PathAndQuery.EndsWith(".docx") ?
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" :
                "application/pdf";
            var archiveMetadataTask = WaybackSnapshot.GetArchiveDataNoExceptions(u, mime);
            var srcDataTask = DocumentHelpers.FetchToMemoryAsync(u);
            var archiveMetadata = await archiveMetadataTask;
            if (lastSnap is null ||
                archiveMetadata?.Timestamp > lastSnap.Timestamp) lastSnap = archiveMetadata;

            var srcData = await srcDataTask;
            var doc = new Document(u, srcData);
            var srcSha1 = await srcData.GetSha1Base32Async();
            page.Documents.Add(doc);
            Console.WriteLine($" * Downloaded {u} {srcSha1} {srcData.Length:#,##0}");
            if (archiveMetadata is null ||
                srcSha1 == archiveMetadata.Digest) continue;
            Console.WriteLine($"{u} new: {srcSha1} archived {archiveMetadata.Digest}");
            await WaybackSnapshot.RequestSaveAsync(u);
        }

        var pageArchiveMetadata = await pageArchiveMetadataTask;
        if (pageArchiveMetadata != null)
        {
            Console.WriteLine($" * maindl? {pageArchiveMetadata}\n * cmp ts  {lastSnap}");
            if (lastSnap is not null && pageArchiveMetadata?.Timestamp < lastSnap.Timestamp.AddDays(-10))
                await WaybackSnapshot.RequestSaveAsync(url);
        }

        return page;
    }
}
