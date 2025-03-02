using System.Security.Cryptography;
using System.Text;

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
        GetDocumentUris(docData.ToString().Replace('\t', ' '), page);

    public static IList<Uri> GetDocumentUris(ReadOnlySpan<char> html, Uri page)
    {
        const StringComparison strcmp = StringComparison.OrdinalIgnoreCase;
        ReadOnlySpan<char> hrefValue = " href=";
        var list = new HashSet<string>();
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
            var hrefPtr = aPtr[(hrefStart + hrefValue.Length)..];
            if (hrefPtr[0] == '"')
            {
                var hrefEnd = hrefPtr[1..].IndexOf("\"", strcmp);
                if (hrefEnd != -1)
                {
                    hrefPtr = hrefPtr[1..(hrefEnd + 1)];
                }
            }

            if (!hrefPtr.Contains(".pdf", strcmp) &&
                !hrefPtr.Contains(".docx", strcmp) &&
                !hrefPtr.Contains(".xlsx", strcmp)) continue;
            list.Add(hrefPtr.ToString());
        }
        return [.. list.Select(u => new Uri(page, u))];
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

    private static FileInfo? _fileIbanIdToBicMap;
    private static FileInfo FileIbanIdToBicMap => _fileIbanIdToBicMap ??= new(Path.Combine(GetDataDir().FullName, "IbanIdToBicMap.txt"));
    private static Dictionary<int, string>? _ibanIdToBicMap = null;
    public static Dictionary<int, string> IbanIdToBicMap()
    {
        var ibanIdBic = _ibanIdToBicMap;
        if (ibanIdBic is not null)
        {
            return ibanIdBic;
        }
        ibanIdBic = [];
        var fi = FileIbanIdToBicMap;
        var data = fi.Exists ? File.ReadAllText(fi.FullName, Encoding.UTF8) : "";
        foreach (var l in data.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var s = l.Split('|');
            if (s.Length != 2) throw new Exception($"Unexpected length {s.Length} from {l}");
            if (s[0].Length != 3) throw new Exception($"Unexpected ibanid {s[0]} from {l}");
            ibanIdBic.Add(Convert.ToInt32(s[0]), s[1]);
        }

        _ibanIdToBicMap = ibanIdBic;
        return ibanIdBic;
    }

    public static void SaveIbanToBicMap(Dictionary<int, string> map)
        => File.WriteAllText(
            FileIbanIdToBicMap.FullName,
            string.Join("", map.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}|{kvp.Value}\n")));
}
