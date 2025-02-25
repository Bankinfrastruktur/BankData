using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Bankinfrastruktur.Helpers;

public static partial class DocumentExtractor
{
    private static IEnumerable<OpenXmlElement> FilterTextElements(OpenXmlElement element)
    {
        foreach (var c in element)
        {
            if (c.HasChildren)
            {
                // TODO rewrite to not be recursive
                foreach (var ic in FilterTextElements(c))
                    yield return ic;
            }
            else if (c is TabChar || c is Text)
            {
                yield return c;
            }
        }
    }

    private static string? ParseDocx(Stream data)
    {
        using var wpd = WordprocessingDocument.Open(data, false);
        var wpb = wpd?.MainDocumentPart?.Document.Body;
        if (wpb is null)
            return null;

        var sb = new StringBuilder();
        foreach (var tbl in wpb.Descendants<Table>())
        {
            foreach (var capt in tbl.Descendants<TableCaption>())
            {
                sb.Append($"# {capt.InnerText}\n");
            }
            foreach (var row in tbl.Descendants<TableRow>())
            {
                var cells = row.Descendants<TableCell>().ToList();
                bool needSeparator = false;
                foreach (var cell in cells)
                {
                    if (needSeparator)
                    {
                        sb.Append('|');
                    }
                    else if (cells.Count == 1 || cell.InnerText.StartsWith("Clearing number"))
                    {
                        sb.Append("# ");
                    }
                    sb.Append(cell.InnerText.Trim());
                    needSeparator = true;
                }
                sb.Append('\n');
            }
        }

        foreach (var p in wpb.Descendants<Paragraph>())
        {
            var hasTab = p.Descendants<TabChar>().ToList();
            if (hasTab.Count == 0)
                continue; // there might be rows with incorrect handling of tabs and multiple lines
            if (p.InnerText.Length == 0)
                continue;
            OpenXmlElement? lastC = null;
            foreach (var c in FilterTextElements(p))
            {
                if (c is TabChar && lastC is TabChar)
                    continue;
                sb.Append(c is TabChar ? "|" : c.InnerText);
                lastC = c;
            }
            sb.Append('\n');
        }
        return sb.Length == 0 ? null : sb.ToString();
    }

    public static async IAsyncEnumerable<UpdateChecker.GrabAndDownload.Document> GetDataDocuments(UpdateChecker.GrabAndDownload.Document doc)
    {
        if (doc.LocalName.EndsWith(".docx"))
        {
            var localName = doc.LocalName;
            if (localName.Contains("_bokstavsordning."))
            {
                // 1906_clearingnummer-institut-241202_bokstavsordning.docx
                localName = "clearingnummer-institut_bokstavsordning.txt";
            }
            else if (localName.Contains("_nummerordning."))
            {
                // 1906_clearingnummer-institut-241202_nummerordning.docx
                localName = "clearingnummer-institut_nummerordning.txt";
            }
            else if (localName.Contains("iban-id-och-bic-adress-for-banker"))
            {
                // short lived docx version of the pdf
                localName = "IbanBic.txt";
            }
            else
            {
                Console.WriteLine($" ## did not handle {localName} from {doc}");
                yield break;
            }

            var data = ParseDocx(doc.Data.ToStream());
            if (data is not null)
            {
                var ddata = new BinaryData($"# {doc.UrlPreferArchive}\n{data}");
                yield return new UpdateChecker.GrabAndDownload.Document(doc.Url, ddata, await ddata.GetSha1Base32Async(), doc.ArchiveMetadata, localName);
            }
        }
        // PDFs handled by python parser
        else if (doc.ArchiveMetadata?.Mimetype == "text/html")
        {
            var docPtr = RegexWidthAttribute().Replace(doc.Data.ToString(), "")
                .Replace("\r\n", "\n")
                .Replace("\n\n", "\n")
                .Replace("<tr>\n", "<tr>")
                .Replace("<td>\n", "<td>")
                .Replace("</td>\n", "</td>")
                .Replace("</p>\n</td>", "</p></td>")
                .AsSpan();
            const StringComparison strcmp = StringComparison.OrdinalIgnoreCase;
            int tagStart;
            if ((tagStart = docPtr.IndexOf("<main", strcmp)) != -1)
                docPtr = docPtr[tagStart..];
            if ((tagStart = docPtr.IndexOf("<article class=\"Article\">\n    <h1 class=\"Article-heading\">", strcmp)) != -1)
                docPtr = docPtr[tagStart..];
            if ((tagStart = docPtr.IndexOf("</header>", strcmp)) != -1)
                docPtr = docPtr[(tagStart + "</header>".Length)..];
            if ((tagStart = docPtr.IndexOf("<footer", strcmp)) != -1)
                docPtr = docPtr[..tagStart];
            if ((tagStart = docPtr.IndexOf("<div class=\"Socialmedia\">", strcmp)) != -1)
                docPtr = docPtr[..tagStart];
            // some fallbacks to limit scope
            if ((tagStart = docPtr.IndexOf("<body", strcmp)) != -1)
                docPtr = docPtr[tagStart..];
            if ((tagStart = docPtr.IndexOf("</body>", strcmp)) != -1)
                docPtr = docPtr[..(tagStart + "</body>".Length)];

            var datasets = new List<(string, BinaryData)>
            {
                (Path.GetFileNameWithoutExtension(doc.LocalName) + ".cmp.html",
                new BinaryData($"# {doc.ArchiveMetadata?.ShowUrl ?? doc.UrlPreferArchive}\n{docPtr.ToString()}"))
            };
            // can not keep spans during yields or awaits, so yield later

            foreach (var (localName, ddata) in datasets)
            {
                yield return new UpdateChecker.GrabAndDownload.Document(doc.Url, ddata, await ddata.GetSha1Base32Async(), doc.ArchiveMetadata, localName);
            }
        }
    }

    /// <summary>Modify parts of data that changes on each request to check for actual data changes</summary>
    public static async Task<UpdateChecker.GrabAndDownload.Document> RepetableRequestHtmlDocument(UpdateChecker.GrabAndDownload.Document doc)
    {
        if (doc.ArchiveMetadata?.Mimetype != "text/html")
            return doc;
        var data = doc.Data.ToString();
        if (data.Contains("nonce="))
        {
            data = RegexNonceAttribute().Replace(data, "");
        }
        var ddata = new BinaryData(data);
        return new UpdateChecker.GrabAndDownload.Document(doc.Url, ddata, await ddata.GetSha1Base32Async(), doc.ArchiveMetadata, doc.LocalName);
    }

    // nonce="DuWld9YQzgA3bRBy/oCVWgk9rQ4MEmOk5Ya2T9RttMs="
    [System.Text.RegularExpressions.GeneratedRegex(" nonce=\"([^\"]{1,64})\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex RegexNonceAttribute();

    [System.Text.RegularExpressions.GeneratedRegex(" width=\"[0-9pxem]+\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex RegexWidthAttribute();
}
