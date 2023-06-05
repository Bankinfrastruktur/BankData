namespace Bankinfrastruktur.Tests;

public class SimpleValidationsTests
{
    [Test]
    public void SourcePsvHasDataTest()
    {
        Assert.That(Data.Banks.SourcePsv, Is.Not.Null.Or.Empty,
            Data.Banks.SourcePsv);
    }

    [TestCase("source.psv")]
    public void SourceFileHasCorrectEncoding(string fileName)
    {
        var fi = Helpers.GetBaseDir().EnumerateFiles(fileName).First();
        var fileData = File.ReadAllText(fi.FullName, System.Text.Encoding.UTF8);
        Assert.That(fileData, Helpers.ContainsAnyUmlaut(),
            fileData);
    }

    [Test]
    public void SourcePsvHasCorrectEncoding()
    {
        Assert.That(Data.Banks.SourcePsv, Helpers.ContainsAnyUmlaut());
    }
}
