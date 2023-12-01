
using Bankinfrastruktur.Helpers;

Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("sv-SE");

var diDocCache = Directory.CreateDirectory(".doc_cache");
Console.WriteLine($"Working with {diDocCache.FullName} ...");
var ghStepSummaryFile = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");

var pagesTask = UpdateChecker.GrabAndDownload.GetPages();
var originalFiles = diDocCache.EnumerateFiles().ToList();
var oldFilesToRemove = originalFiles.ToDictionary(fi => Path.GetFileName(fi.Name));
var filesWithHash = originalFiles.AsParallel().ToDictionary(fi => fi, DocumentHelpers.GetFileSha1Base32Async);
foreach (var fwh in filesWithHash)
{
    var digest = await fwh.Value.ConfigureAwait(false);
    var fi = fwh.Key;
    Console.WriteLine($"* Existing local file: {Path.GetFileName(fi.Name)}\t{fi.Length}\t{digest}");
}

var modifiedDocuments = new List<UpdateChecker.GrabAndDownload.Document>();
var documents = (await pagesTask).SelectMany(p => p.Documents);
if (true)
{
    foreach (var doc in documents)
    {
        var file = Path.GetFileName(doc.Url.LocalPath);
        oldFilesToRemove.Remove(file);
        var docSha1Task = DocumentHelpers.GetSha1Base32Async(doc.Data);
        Console.Write($"\n Validating {doc} -> {file}");
        var fileFullPath = Path.Combine(diDocCache.FullName, file);
        var fi = new FileInfo(fileFullPath);

        var fileSha1 = await fi.GetFileSha1Base32Async();
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
                fs.SetLength(doc.Data.Length); // ensure existing files are truncated to correct size
                await fs.FlushAsync();
                fs.Close();
            }

            fi.Refresh();
            fileSha1 = await fi.GetFileSha1Base32Async();
            if (fileSha1 != docSha1)
                throw new Exception($"* {fi.FullName} On-disk hash was {fileSha1} expected {docSha1}, size: {fi.Length} expected {doc.Data.Length}");

            var parsedData = DocumentExtractor.GetData(doc.Url, doc.Data);
            if (ghStepSummaryFile is not null && parsedData is not null)
                await File.AppendAllTextAsync(ghStepSummaryFile, $"Data: {parsedData}\n");
        }
    }
}

foreach (var fi in oldFilesToRemove.Values)
{
    Console.WriteLine($"* Cleanup old file: {Path.GetFileName(fi.Name)}\t{fi.Length}");
    fi.Delete();
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
