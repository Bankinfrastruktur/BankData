
using Bankinfrastruktur.Helpers;

Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("sv-SE");

var diDocCache = Directory.CreateDirectory(".doc_cache");
var diData = new DirectoryInfo(Directory.GetCurrentDirectory());
while (true)
{
    var di = diData.EnumerateDirectories("Data").FirstOrDefault();
    if (di is not null)
    {
        diData = di;
        break;
    }
    diData = diData.Parent ?? throw new Exception("Data spath not found");
}
Console.WriteLine($"Working with {diDocCache.FullName} and {diData.FullName} ...");
var ghStepSummaryFile = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");

var pagesTask = UpdateChecker.GrabAndDownload.GetPages();
var originalFiles = diDocCache.EnumerateFiles().ToList();
var oldFilesToRemove = originalFiles.ToDictionary(fi => Path.GetFileName(fi.Name));
var filesWithHash = originalFiles.AsParallel().ToDictionary(fi => fi, DocumentHelpers.GetFileSha1Base32Async);
foreach (var fwh in filesWithHash)
{
    var digest = await fwh.Value;
    var fi = fwh.Key;
    Console.WriteLine($" * Existing local file: {Path.GetFileName(fi.Name)}\t{fi.Length}\t{digest}");
}

var modifiedDocuments = new List<UpdateChecker.GrabAndDownload.Document>();
var documents = (await pagesTask).SelectMany(p => p.Documents);
var pdfDocs = new List<(UpdateChecker.GrabAndDownload.Document, FileInfo)>();
if (true)
{
    foreach (var odoc in documents)
    {
        var doc = odoc;
        var file = doc.LocalName;
        oldFilesToRemove.Remove(file);
        Console.Write($"\n Validating {doc}");
        var fi = new FileInfo(Path.Combine(diDocCache.FullName, file));
        if (doc.ArchiveMetadata?.Mimetype == WaybackSnapshot.MimeApplicationPdf)
            pdfDocs.Add((doc, fi));

        doc = await DocumentExtractor.RepetableRequestHtmlDocument(doc);
        await foreach (var dataDoc in DocumentExtractor.GetDataDocuments(doc))
        {
            var fiParsed = new FileInfo(Path.Combine(diData.FullName, dataDoc.LocalName));
            var fiParsedSha1 = await fiParsed.GetFileSha1Base32Async();
            if (dataDoc.Sha1 != fiParsedSha1)
            {
                await dataDoc.Data.WriteAsync(fiParsed);
            }
        }

        var fileSha1 = await fi.GetFileSha1Base32Async();
        if (doc.Sha1 != fileSha1)
        {
            var logline = $"* from: {fileSha1} -> {doc.Sha1} to {fi.FullName}";
            Console.WriteLine(logline);
            if (!doc.DidWaybackSaveCall)
            {
                await WaybackSnapshot.RequestSaveAsync(doc.Url);
                doc.DidWaybackSaveCall = true;
            }
            if (ghStepSummaryFile is not null)
                await File.AppendAllTextAsync(ghStepSummaryFile, $"{logline} src: {doc}\n");
            modifiedDocuments.Add(doc);
            await doc.Data.WriteAsync(fi);
            fileSha1 = await fi.GetFileSha1Base32Async();
            if (fileSha1 != doc.Sha1)
                throw new Exception($"* {fi.FullName} On-disk hash was {fileSha1} expected {doc.Sha1}, size: {fi.Length} expected {doc.Data.Length}");
        }
    }
}

var pdfStatusFileTask = File.WriteAllLinesAsync(".pdfstats", pdfDocs
    .Select(docfi => $"{docfi.Item2.FullName}|{docfi.Item1.UrlPreferArchive}|{docfi.Item1.ArchiveMetadata?.Timestamp:yyyy-MM-dd}"));

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

await pdfStatusFileTask;
