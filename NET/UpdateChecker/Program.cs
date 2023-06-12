
using Bankinfrastruktur.Helpers;

var diDocCache = Directory.CreateDirectory(".doc_cache");
Console.WriteLine($"Working with {diDocCache.FullName} ...");

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
            Console.WriteLine($" from: {fileSha1} -> {docSha1} to {fi.FullName}");
            modifiedDocuments.Add(doc);
            using (var fs = fi.OpenWrite())
            {
                doc.Data.WriteTo(fs);
                fs.Close();
            }
        }

    }
}

if (modifiedDocuments.Count != 0)
{
    Console.WriteLine("TODO create template.md");
}
