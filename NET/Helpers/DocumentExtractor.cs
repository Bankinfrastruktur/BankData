using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Bankinfrastruktur.Helpers;

public static class DocumentExtractor
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
            sb.AppendLine();
        }
        return sb.Length == 0 ? null : sb.ToString();
    }

    public static string? GetData(UpdateChecker.GrabAndDownload.Document doc)
    {
        if (doc.LocalName.EndsWith(".docx"))
        {
            return ParseDocx(doc.Data.ToStream());
        }
        return null;
    }
}
