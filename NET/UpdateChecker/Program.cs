
using Bankinfrastruktur.Helpers;

Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("sv-SE");

var diDocCache = Directory.CreateDirectory(".doc_cache");
Console.WriteLine($"Working with {diDocCache.FullName} ...");
var ghStepSummaryFile = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");

var pages = await UpdateChecker.GrabAndDownload.GetPages();
var originalFiles = diDocCache.EnumerateFiles().ToList();
var modifiedDocuments = new List<UpdateChecker.GrabAndDownload.Document>();
foreach (var page in pages)
{
    foreach (var doc in page.Documents)
    {
        var file = Path.GetFileName(doc.Url.LocalPath);
        var docSha1Task = DocumentHelpers.GetSha1Base32Async(doc.Data);
        Console.Write($"\n Validating {doc} -> {file}");
        var fileFullPath = Path.Combine(diDocCache.FullName, file);
        var fi = new FileInfo(fileFullPath);

        string? fileSha1 = await DocumentHelpers.GetFileSha1Base32Async(fi);
        var docSha1 = await docSha1Task;
        if (docSha1 != fileSha1)
        {
            var logline = $"* from: {fileSha1} -> {docSha1} to {fi.FullName}";
            Console.WriteLine(logline);
            if (ghStepSummaryFile is not null)
                await File.AppendAllTextAsync(ghStepSummaryFile, $"{logline} src: {doc.Url}\n");
            modifiedDocuments.Add(doc);
            using (var fs = fi.OpenWrite())
            {
                doc.Data.WriteTo(fs);
                fs.Close();
            }

            var parsedData = DocumentExtractor.GetData(doc.Url, doc.Data);
            if (ghStepSummaryFile is not null && parsedData is not null)
                await File.AppendAllTextAsync(ghStepSummaryFile, $"Data: {parsedData}\n");
        }
    }
}

if (modifiedDocuments.Count != 0)
{
    var fi = new FileInfo("UpdateCheckResultIssue.md");
    Console.WriteLine($"Creating {fi.FullName}");
    var actionUrl = "{{ env.actionurl }}";
    File.WriteAllText(fi.FullName,
@$"---
title: Upptäckta ändringar i källfiler
---
Uppdaterade datafiler?

* {string.Join("\n* ", modifiedDocuments)}

{actionUrl}

");
}
