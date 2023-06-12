using System.IO;
using System.Security.Cryptography;

namespace Bankinfrastruktur.Helpers;

public static class DocumentHelpers
{
    public static readonly HttpClient HttpClient = new();

    public static async Task<string> GetSha1Base32Async(Stream s)
    {
        s.Position = 0;
        return Base32.FromBytes(await SHA1.Create().ComputeHashAsync(s));
    }

    public static async Task<string?> GetFileSha1Base32Async(FileInfo fi)
    {
        if (!fi.Exists)
            return null;

        using var fs = fi.OpenRead();
        return await GetSha1Base32Async(fs);
    }

    public static async Task<MemoryStream> FetchToMemoryAsync(Uri url)
    {
        using var s = await HttpClient.GetStreamAsync(url);
        var ms = new MemoryStream();
        await s.CopyToAsync(ms);
        ms.Position = 0;
        return ms;
    }

    public static async Task<IList<Uri>> GetDocumentUrisFromPage(Uri page) =>
        GetDocumentUris(await HttpClient.GetStringAsync(page))
        .Select(t => new Uri(page, t)).ToList();

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
                !aPtr.Contains(".docx", strcmp)) continue;

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
}
