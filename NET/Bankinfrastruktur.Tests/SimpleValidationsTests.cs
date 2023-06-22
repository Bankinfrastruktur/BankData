using Bankinfrastruktur.Data;

namespace Bankinfrastruktur.Tests;

public class SimpleValidationsTests
{
    [Test]
    public void SourcePsvHasDataTest() => 
        Assert.That(Banks.SourcePsv, Is.Not.Null.Or.Empty,
            Banks.SourcePsv);

    [TestCase("source.psv")]
    public void SourceFileHasCorrectEncoding(string fileName)
    {
        var fileData = Helpers.FindAndReadTextFile(fileName);
        Assert.That(fileData, Helpers.ContainsAnyUmlaut(),
            fileData);
    }

    [Test]
    public void SourcePsvHasCorrectEncoding() => 
        Assert.That(Banks.SourcePsv, Helpers.ContainsAnyUmlaut());

    [TestCase("330|3300|", "ClearingStart", 330)]
    [TestCase("33000|3300|", "ClearingStart", 33000)]
    [TestCase("3300|330|", "ClearingEnd", 330)]
    [TestCase("3300|33000|", "ClearingEnd", 33000)]
    public void BankRecordCtorClearingThrowsTest(string line, string paramName, int actual)
    {
        var ex = Assert.Catch<ArgumentOutOfRangeException>(() => Banks.GetList(line));
        Console.WriteLine(ex.ToString());
        Assert.Multiple(() =>
        {
            Assert.That(ex, Has.Message.StartsWith($"Must be 4 numbers (Parameter"));
            Assert.That(ex, Has.Message.Contains($"(Parameter '{paramName}')"));
            Assert.That(ex, Has.Message.Contains($"Actual value was {actual}."));
        });
    }

    [TestCase("3x|")]
    [TestCase("300|3x")]
    public void BankRecordCtorThrowsFormatExceptionTest(string line)
    {
        var ex = Assert.Catch<FormatException>(() => Banks.GetList(line));
        Console.WriteLine(ex.ToString());
        Assert.That(ex, Has.Message.Contains(" was not in a correct format."));
    }

    [TestCase("9710|9719")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|Type1|Comment2|Method1|7")]
    public void BankRecordMissingFieldsCtorThrowsIndexOutOfRangeExceptionExceptionTest(string line)
    {
        var ex = Assert.Catch<IndexOutOfRangeException>(() => Banks.GetList(line));
        Console.WriteLine(ex.ToString());
    }

    [TestCase("1001|1000|971|LUNADK2B|x Bank|Type1|Comment2|Method1|7|7", 1001, "x Bank", AccountTypeType.Type1, "expected ClearingStart 1001 <= ClearingEnd 1000")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|x|Comment2|Method1|7|7", 9710, "x Bank", AccountTypeType.Invalid, "unhandled AccountType")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|Type1|x|Method1|7|7", 9710, "x Bank", AccountTypeType.Type1, "CheckDigitType Invalid")]
    [TestCase("9710|9719|971|LUNADK2|x Bank|Type1|Comment2|Method1|7|7", 9710, "x Bank", AccountTypeType.Type1, "expected BIC length 8 LUNADK2")]
    [TestCase("9710|9719|971|LUNADK2xx|x Bank|Type1|Comment2|Method1|7|7", 9710, "x Bank", AccountTypeType.Type1, "expected BIC length 8 LUNADK2xx")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|Type1|Comment2|Method1|7|8", 9710, "x Bank", AccountTypeType.Type1, "expected AccountNumberLength 7 was 8")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|Type1|Comment2|Method2|7|7", 9710, "x Bank", AccountTypeType.Type1, "expected IBAN Method1 was Method2")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|Type2|Comment2|Method2|7|7", 9710, "x Bank", AccountTypeType.Type2, "expected AccountNumberLength Not 7 was 7")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|Type2|Comment1|Method2|7|8", 9710, "x Bank", AccountTypeType.Type2, "expected AccountNumberLength 10 was 8")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|Type2|Comment3|Method2|7|8", 9710, "x Bank", AccountTypeType.Type2, "expected AccountNumberLength 10 was 8")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|Type2|Comment2|Method2|7|8", 9710, "x Bank", AccountTypeType.Type2, "expected AccountNumberLength 9 was 8")]
    [TestCase("9710|9719|971|LUNADK2B|x Bank|Type1|Comment2|Method1|8|7", 9710, "x Bank", AccountTypeType.Type1, "expected AccountNumberMinLength 8 <= AccountNumberLength 7")]
    public void BankRecordCtorThrowsTest(string line, int clearingStart, string bankName, AccountTypeType accountType, string issue)
    {
        var ex = Assert.Catch<BankRecordDataException>(() => Banks.GetList(line));
        Console.WriteLine(ex.ToString());
        Assert.Multiple(() =>
        {
            Assert.That(ex.ClearingStart, Is.EqualTo(clearingStart));
            Assert.That(ex.BankName, Is.EqualTo(bankName));
            Assert.That(ex.AccountType, Is.EqualTo(accountType));
            Assert.That(ex.Issue, Is.EqualTo(issue));
            Assert.That(ex, Has.Message.EqualTo($"Account {accountType} {issue} for: {clearingStart} : {bankName}"));
            Assert.That(ex.Data["line"], Is.EqualTo(line));
        });
    }

    [Test]
    public void EnsureFallbackSettingsTest()
    {
        Assert.Multiple(() =>
        {
            Assert.That(BankRecord.BasicType1Comment1.CheckDigitType, Is.EqualTo(CheckDigitTypeType.Comment1));
            Assert.That(BankRecord.BasicType1Comment2.CheckDigitType, Is.EqualTo(CheckDigitTypeType.Comment2));
        });
        foreach (var rec in new[] {
            BankRecord.BasicType1Comment1, 
            BankRecord.BasicType1Comment2})
        {
            Assert.Multiple(() =>
            {
                Assert.That(rec.ClearingStart, Is.EqualTo(0));
                Assert.That(rec.ClearingEnd, Is.EqualTo(0));
                Assert.That(rec.IbanId, Is.EqualTo(0));
                Assert.That(rec.BIC, Is.EqualTo(string.Empty));
                Assert.That(rec.BankName, Is.EqualTo("-"));
                Assert.That(rec.AccountType, Is.EqualTo(AccountTypeType.Type1));
                Assert.That(rec.IbanMethod, Is.EqualTo(IbanMethodType.MethodUnknown));
                Assert.That(rec.AccountNumberMinLength, Is.EqualTo(7));
                Assert.That(rec.AccountNumberLength, Is.EqualTo(7));
            });
        }
    }
}
