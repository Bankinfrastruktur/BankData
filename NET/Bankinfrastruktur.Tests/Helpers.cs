using Bankinfrastruktur.Helpers;
using NUnit.Framework.Constraints;
using System.Text;

namespace Bankinfrastruktur.Tests;

internal static class Helpers
{
    public static DirectoryInfo GetBaseDir()
        => DocumentHelpers.GetDataDir();

    public static Constraint ContainsAnyUmlaut()
    {
        Constraint x = Does.Contain("Å");
        foreach (var c in "ÅÄÖåäö")
            x = x.Or.Contain(c);
        return x;
    }

    public static IEnumerable<string> GetLines(string data) =>
        data.Split(["\r\n", "\r", "\n"], StringSplitOptions.None)
        .Select(l => l.Trim())
        .Where(l => l.Length != 0 && l[0] != '#');

    public static string FindAndReadTextFile(string filename)
    {
        var fi = GetBaseDir().EnumerateFiles(filename).First();
        return File.ReadAllText(fi.FullName, Encoding.UTF8);
    }
}
