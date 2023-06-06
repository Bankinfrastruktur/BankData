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
        var fileData = Helpers.FindAndReadTextFile(fileName);
        Assert.That(fileData, Helpers.ContainsAnyUmlaut(),
            fileData);
    }

    [Test]
    public void SourcePsvHasCorrectEncoding()
    {
        Assert.That(Data.Banks.SourcePsv, Helpers.ContainsAnyUmlaut());
    }
}
