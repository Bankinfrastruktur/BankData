using NUnit.Framework.Constraints;

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
}
