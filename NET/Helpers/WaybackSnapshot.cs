namespace Bankinfrastruktur.Helpers;

/// <summary>https://archive.org/developers/tutorial-compare-snapshot-wayback.html</summary>
public class WaybackSnapshot
{
    public static readonly HttpClient HttpClient = DocumentHelpers.HttpClient;

    private const string ArchiveBase = "https://web.archive.org/";
    public static async Task RequestSaveAsync(Uri url)
    {
        Console.WriteLine($"WaybackSnapshot Request Save: {url}");
        var resp = await HttpClient.GetAsync(new Uri($"{ArchiveBase}save/{url}"));
#if DEBUG
        foreach (var hdr in resp.Headers)
        {
            Console.WriteLine($"{hdr.Key} {string.Join("|", hdr.Value)}");
        }
#endif
    }

    public const string MimeApplicationPdf = "application/pdf";

    public static async Task<WaybackSnapshot?> GetArchiveDataNoExceptions(Uri url, string? filterMime = MimeApplicationPdf)
    {
        try
        {
            return await GetArchiveData(url, filterMime);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is not HttpRequestException)
                throw;
            Console.WriteLine($"{ex}");
            return null;
        }
    }

    public static async Task<WaybackSnapshot?> GetArchiveData(Uri url, string? filterMime = MimeApplicationPdf)
    {
        var archiveUrl = new Uri($"{ArchiveBase}cdx/search/cdx?fl=timestamp,original,mimetype,digest,length&from={DateTime.Now.Year - 1}&filter=statuscode:200&collapse=digest&url={url}");
        try
        {
            return await GetParseArchiveInner(archiveUrl, filterMime);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Wayback fetch Fail {archiveUrl}, {ex.Message}", ex);
        }
    }
    
    private static async Task<WaybackSnapshot?> GetParseArchiveInner(Uri archiveUrl, string? filterMime)
    {
        using var s = await HttpClient.GetStreamAsync(archiveUrl);
        using var tr = new StreamReader(s);
        string? line;
        WaybackSnapshot? lastSnap = null;
        while ((line = await tr.ReadLineAsync()) != null)
        {
            var split = line.Trim().Split(' ');
            var snap = new WaybackSnapshot
            {
                Timestamp = DateTime.ParseExact(split[0], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None),
                Url = new Uri(split[1]),
                Mimetype = split[2],
                Digest = split[3],
                Length = Convert.ToInt64(split[4]),
            };
            Console.WriteLine($"Archive line: {snap}");
            if (filterMime != null &&
                snap.Mimetype != filterMime) continue;
            if (lastSnap is null ||
                snap.Timestamp > lastSnap.Timestamp) lastSnap = snap;
        }
        return lastSnap;
    }

    /// <summary>timestamp: A 14 digit date-time representation in the YYYYMMDDhhmmss format.</summary>
    public DateTime Timestamp { get; set; }
    /// <summary>original: The originally archived URL, which could be different from the URL you supplied.</summary>
    public Uri Url { get; set; } = null!;
    /// <summary>mimetype: The mimetype of the archived content</summary>
    public string Mimetype { get; set; } = null!;
    /// <summary>digest: The SHA1 hash digest of the content, excluding the headers. It’s usually a base-32-encoded string.</summary>
    public string Digest { get; set; } = null!;
    /// <summary>length: The compressed byte size of the corresponding WARC record, which includes WARC headers, HTTP headers, and content payload.</summary>
    public long Length { get; set; }

    public Uri ShowUrl => new($"{ArchiveBase}web/{Timestamp:yyyyMMddHHmmss}/{Url}");
    public Uri DataUrl => new($"{ArchiveBase}web/{Timestamp:yyyyMMddHHmmss}if_/{Url}");

    public override string ToString()
    {
        return $"{Url} -> {Timestamp} {Mimetype} {Digest} {Length:#,##0}";
    }
}
