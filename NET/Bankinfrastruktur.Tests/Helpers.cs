using NUnit.Framework.Constraints;
using System.Text;

namespace Bankinfrastruktur.Tests;

internal static class Helpers
{
    public static DirectoryInfo GetBaseDir()
    {
        var ncruncProj = Environment.GetEnvironmentVariable("NCrunch.OriginalProjectPath");
        var ncruncProjDi = ncruncProj == null ? null : new FileInfo(ncruncProj).Directory;
        var di = ncruncProjDi ?? new DirectoryInfo(Directory.GetCurrentDirectory());
        while (true)
        {
            Console.WriteLine($" {nameof(GetBaseDir)} Looking for Data dir .. {di}");
            var diData = di.EnumerateDirectories("Data").FirstOrDefault();
            if (diData != null)
                return diData;
            di = di.Parent;
            if (di == null)
                throw new Exception("Data spath not found");
        }
    }

    public static Constraint ContainsAnyUmlaut()
    {
        Constraint x = Does.Contain("Å");
        foreach (var c in "ÅÄÖåäö")
            x = x.Or.Contain(c);
        return x;
    }

    public static IEnumerable<string> GetLines(string data) =>
        data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
        .Select(l => l.Trim())
        .Where(l => l.Length != 0 && l[0] != '#');

    public static string FindAndReadTextFile(string filename)
    {
        var fi = GetBaseDir().EnumerateFiles(filename).First();
        return File.ReadAllText(fi.FullName, Encoding.UTF8);
    }
}
