using System.Security.Cryptography;

namespace Bankinfrastruktur.Helpers;

public static class DocumentHelpers
{
    public static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5),
    };

    public static async Task<string> GetSha1Base32Async(this Stream s)
    {
        s.Position = 0;
        return Base32.FromBytes(await SHA1.Create().ComputeHashAsync(s));
    }

    public static Task<string> GetSha1Base32Async(this BinaryData data) => data.ToStream().GetSha1Base32Async();

    public static async Task<string?> GetFileSha1Base32Async(this FileInfo fi)
    {
        if (!fi.Exists)
            return null;

        using var fs = fi.OpenRead();
        return await fs.GetSha1Base32Async();
    }

    public static async Task WriteAsync(this BinaryData data, FileInfo fi)
    {
        using var fs = fi.OpenWrite();
        await fs.WriteAsync(data.ToMemory());
        fs.SetLength(data.Length); // ensure existing files are truncated to correct size
        await fs.FlushAsync();
        fs.Close();
        fi.Refresh();
    }

    public static async Task<BinaryData> FetchToMemoryAsync(Uri url)
    {
        using var s = await HttpClient.GetStreamAsync(url);
        return await BinaryData.FromStreamAsync(s);
    }

    public static IList<Uri> GetDocumentUrisFromPage(BinaryData docData, Uri page) =>
        [.. GetDocumentUris(docData.ToString()).Select(t => new Uri(page, t))];

    public static IList<string> GetDocumentUris(ReadOnlySpan<char> html)
    {
        const StringComparison strcmp = StringComparison.OrdinalIgnoreCase;
        ReadOnlySpan<char> hrefValue = " href=";
        var list = new List<string>();
        var htmlPtr = html;
        int aStart;
        while ((aStart = htmlPtr.IndexOf("<a ", strcmp)) != -1)
        {
            htmlPtr = htmlPtr[aStart..];
            var aEnd = htmlPtr.IndexOf(">", strcmp); // </a to be able to parse text as well
            if (aEnd == -1)
                aEnd = htmlPtr.Length;
            var aPtr = htmlPtr[..aEnd];
            htmlPtr = htmlPtr[aEnd..];

            var hrefStart = aPtr.IndexOf(hrefValue, strcmp);
            if (hrefStart == -1)
                continue;
            if (!aPtr.Contains(".pdf", strcmp) &&
                !aPtr.Contains(".docx", strcmp) &&
                !aPtr.Contains(".xlsx", strcmp)) continue;

            var hrefPtr = aPtr[(hrefStart + hrefValue.Length)..];
            if (hrefPtr[0] == '"')
            {
                var hrefEnd = hrefPtr[1..].IndexOf("\"", strcmp);
                if (hrefEnd != -1)
                {
                    hrefPtr = hrefPtr[1..(hrefEnd + 1)];
                }
            }

            list.Add(hrefPtr.ToString());
        }
        return list;
    }

    public static DirectoryInfo GetDataDir()
    {
        var ncruncProj = Environment.GetEnvironmentVariable("NCrunch.OriginalProjectPath");
        var ncruncProjDi = ncruncProj is null ? null : new FileInfo(ncruncProj).Directory;
        var di = ncruncProjDi ?? new DirectoryInfo(Directory.GetCurrentDirectory());
        while (true)
        {
            var diData = di.EnumerateDirectories("Data").FirstOrDefault();
            if (diData is not null)
            {
                return diData;
            }
            di = di.Parent ?? throw new Exception("Data spath not found");
        }
    }
}
