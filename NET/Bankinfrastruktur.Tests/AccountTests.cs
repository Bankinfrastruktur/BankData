using Bankinfrastruktur.Validation;

namespace Bankinfrastruktur.Tests;

[TestFixture]
public class AccountTests
{
    [Test]   //Valid
    [TestCase("1274", "0235 305", ValidationIssues.None)]        // 1, 1, 1 Danske Bank
    [TestCase("3300", "101010-1010", ValidationIssues.None)]     // 2, 1, 2 Nordea personkonto
    [TestCase("6114", "5171 82351", ValidationIssues.None)]      // 2, 2, 2 Handelsbanken
    [TestCase("81034", "978 3242", ValidationIssues.None)]       // 2, 3, 3 iban.se/iban-checker
    [TestCase("8327-9", "014 725 892-5", ValidationIssues.None)] // 2, 3, 3 Swedbank
    [TestCase("9022", "11 14884", ValidationIssues.None)]        // 1, 2, 1 https://www.lansforsakringar.se/jamtland/om-oss/press-media/nyheter/125843/
    [TestCase("9570", "12344", ValidationIssues.None)]           // 2, 1, 2 Sparbanken Syd   zero pad adds
    [TestCase("9570", "525 004 176 0", ValidationIssues.None)]   // 2, 1, 2 https://www.youtube.com/watch?v=81Af8fNUZIY
    [TestCase("5140", "0010004", ValidationIssues.None)]         // SEB Ok, zero padded required

    [TestCase("3782", "101010-1010", ValidationIssues.None)]     // Nordea personkonto, (legacy non valid) same as 3300 (2, 1)
    [TestCase("9180", "1111111116", ValidationIssues.None)]      // danske bank
    [TestCase("9190", "6543218", ValidationIssues.None)]         // DnB https://www.dnb.se/se/sv/kalkylator/iban.html
    [TestCase("9500", "1111111116", ValidationIssues.None)]      // plusgirot
    [TestCase("9960", "1111111116", ValidationIssues.None)]      // plusgirot

    // Valid unknown bank
    [TestCase("9999", "0123454", ValidationIssues.None)]       // Unknown validates with Comment 1
    [TestCase("9999", "0123456", ValidationIssues.None)]       // Unknown validates with Comment 2

    //Invalid
    [TestCase("5140", "10004", ValidationIssues.IncorrectAccountNumberLength)]       // SEB requires zero padding
    [TestCase("0000", "123456788", ValidationIssues.IncorrectClearingNumberLength)]  // clearing can not be less then 1000
    [TestCase("9999", "1234567", ValidationIssues.IncorrectAccountNumber)]           // Not valid with either Comment 1 or 2
    [TestCase("9999", "18", ValidationIssues.IncorrectAccountNumberLength)]          // to short
    [TestCase("9272", "18297523", ValidationIssues.IncorrectAccountNumberLength)]    // to long
    [TestCase("9272", "182975", ValidationIssues.IncorrectAccountNumberLength)]      // to short
    [TestCase("9272", "1829751", ValidationIssues.IncorrectAccountNumber)]           // checkdiggit incorrect
    [TestCase("3300", "12344", ValidationIssues.IncorrectAccountNumberLength)]       // Nordea personkonto paddar inte korta konton med nollor
    [TestCase("1234", "56789", ValidationIssues.IncorrectAccountNumberLength)]       // to short
    [TestCase("1234", "3456789", ValidationIssues.IncorrectAccountNumber)]           // checkdiggit incorrect
    [TestCase("6789", "123456788", ValidationIssues.IncorrectAccountNumber)]         // checkdiggit incorrect
    [TestCase("8424-1", "014 725 892-6", ValidationIssues.IncorrectAccountNumber)]   // checkdiggit incorrect
    [TestCase("83270", "123456782", ValidationIssues.IncorrectAccountNumber)]        // clearing checkdiggit incorrect
    [TestCase("8327", "123456782", ValidationIssues.IncorrectClearingNumberLength)]  // Incorrect clearing length
    [TestCase("64040", "123456782", ValidationIssues.IncorrectClearingNumberLength)] // Incorrect clearing length
    public void BankAccountValidationTest(string clearing, string account, ValidationIssues error)
    {
        var acc = new Account(clearing, account);
        Console.WriteLine($"{acc.BankRecord?.BankName}, {acc.BankRecord?.AccountType}, {acc.BankRecord?.CheckDigitType}, {acc.BankRecord?.IbanMethod}");
        var parsed = Account.Parse(acc.ToString());
        var parsed2 = Account.Parse(parsed?.ToString());
        Assert.Multiple(() =>
        {
            Assert.That(acc.Issues, Is.EqualTo(error));
            if (acc.BankRecord is null)
                Assert.That(acc.Issues, Is.Not.EqualTo(ValidationIssues.None));

            Assert.That(acc.ClearingNumber, Is.EqualTo(clearing.TrimStart('0').KeepDigits()));
            Assert.That(acc.AccountNumber.TrimStart('0'), Is.EqualTo(account.TrimStart('0')));

            Assert.That(parsed?.ClearingNumber, Is.EqualTo(clearing.TrimStart('0').KeepDigits()), acc.ToString());
            Assert.That(parsed?.AccountNumber.Replace(" ", "").TrimStart('0'), Is.EqualTo(account.Replace(" ", "").TrimStart('0')), acc.ToString());

            // ensure output is same as input
            Assert.That(parsed2?.ToString(), Is.EqualTo(acc.ToString()), acc.ToString());
        });
    }

    [Test]
    [TestCase("957012344", "9570", "0000012344", "9570, 0000012344")]
    [TestCase("9570, 12344", "9570", "0000012344", "9570, 0000012344")]
    [TestCase("51400010004", "5140", "0010004", "5140, 0010004")]
    [TestCase("99990123454", "9999", "0123454", "9999, 0123454")]
    [TestCase("514010004", "5140", "0010004", "5140, 0010004")]
    [TestCase("5140, 10004", "5140", "0010004", "5140, 0010004")]
    [TestCase("83270123456782", "83270", "0123456782", "83270, 0123456782")]
    [TestCase("64040123456782", "6404", "0123456782", "6404, 0123456782")]
    [TestCase("64040, 123456782", "64040", "123456782", "64040, 123456782")]
    [TestCase("1000, 12345", "1000", "12345", "1000, 12345")]
    [TestCase("1000, 1234567", "1000", "1234567", "1000, 1234567")]
    [TestCase("1000, 123456789", "1000", "123456789", "1000, 123456789")]
    [TestCase("1000, 1010101010", "1000", "1010101010", "1000, 1010101010")]
    [TestCase("3300, 1010101010", "3300", "1010101010", "3300, 1010101010")]
    [TestCase("3300, 101010-1010", "3300", "101010-1010", "3300, 101010-1010")]
    [TestCase("8480-5, 1010101010", "84805", "1010101010", "84805, 1010101010")]
    [TestCase("123, 4", "123", "4", "123, 4")]
    [TestCase("gar be ge", null, null, null)]
    [TestCase("gar, be ge", "", "be ge", "-")]
    public void BankAccountParseTest(string parse, string clearing, string account, string written)
    {
        var parsed = Account.Parse(parse);
        if (clearing is null)
        {
            Assert.That(parsed, Is.Null);
            return;
        }
        else if (parsed is null)
        {
            throw new NullReferenceException(nameof(parsed));
        }

        Assert.Multiple(() =>
        {
            Assert.That(parsed.ClearingNumber, Is.EqualTo(clearing), parsed.ToString());
            Assert.That(parsed.AccountNumber, Is.EqualTo(account), parsed.ToString());

            Assert.That(parsed.ToString(), Is.EqualTo(written));
        });
    }

    [Test]
    [TestCase("3300", "Nordea")]
    [TestCase("4001", "Nordea")]
    [TestCase("5839", "SEB")]
    [TestCase("6501", "Handelsbanken")]
    [TestCase("7104", "Swedbank")]
    [TestCase("8129-9", "Swedbank")]
    [TestCase("8129", "Swedbank")]
    [TestCase("9272", "ICA Banken")]
    [TestCase("9420", "Forex Bank")]
    [TestCase("9570", "Sparbanken Syd")]
    [TestCase("9789", "Klarna Bank")]
    [TestCase("9999", "-")]
    public void BankNameFoundTest(string clearing, string expectedBank)
    {
        var acc = new Account(clearing, "1");
        Assert.Multiple(() =>
        {
            Assert.That(acc.BankRecord, Is.Not.Null);
            Assert.That(acc.BankRecord?.BankName, Is.EqualTo(expectedBank));
        });
    }

    [Test]
    [TestCase("", "", null)]
    // https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer
    [TestCase("1274", "0235 305", "SE41 1200 0000 0127 4023 5305")] // Metod 1
    [TestCase("6114", "5171 82351", "SE58 6000 0000 0005 1718 2351")] // Metod 2
    [TestCase("8327-9", "014 725 892-5", "SE55 8000 0832 7901 4725 8925")] // Metod 3

    [TestCase("1294", "01 45237", "SE78 1200 0000 0129 4014 5237")] // Danske Bank https://www.danskebank.se/sv-se/businessonline-content/help/q-a/pages/var-hittar-jag-iban-nummer.aspx
    [TestCase("3300", "101010-1010", "SE95 3000 0000 0010 1010 1010")]
    [TestCase("3782", "101010-1010", "")] // Nordea personkonto, (legacy non valid) same as 3300
    [TestCase("3473", "17 01473", "SE81 3000 0000 0347 3170 1473")] // Nordea företagskonto https://www.swecogroup.com/wp-content/uploads/sites/2/2021/03/prospectus_invitation-to-subscribe-for-shares-in-sweco.pdf
    [TestCase("549 1", "000 0003", "SE35 5000 0000 0549 1000 0003")] // https://www.nordea.com/en/doc/nordea-account-structure-version-1-8.pdf
    [TestCase("5839", "8257466", "SE45 5000 0000 0583 9825 7466")] // SEB https://bank.codes/iban/bank/sweden/nordea-bank/
    [TestCase("5140", "0010004", "SE83 5000 0000 0514 0001 0004")] // SEB https://seb.se/foretag/betala-och-ta-betalt/betala-till-utlandet/iban-och-bic?showsofi=true
    [TestCase("81034", "978 3242", "SE72 8000 0810 3400 0978 3242")] // https://www.iban.se/iban-checker
    [TestCase("9022", "11 14884", "SE53 9020 0000 0902 2111 4884")] // https://www.lansforsakringar.se/jamtland/om-oss/press-media/nyheter/125843/
    [TestCase("9040", "1106944", "SE69 9040 0000 0904 0110 6944")] // https://www.schwab.com/public/file/P-12116714/mkt25455intl.pdf
    [TestCase("9180", "1111111116", "")] // danske bank
    [TestCase("9190", "6543218", "SE18 9190 0000 0919 0654 3218")] // DnB https://www.dnb.se/se/sv/kalkylator/iban.html
    [TestCase("9500", "1111111116", "")] // plusgirot unknown
    [TestCase("9570", "525 004 176 0", "SE37 9570 0000 0052 5004 1760")] // https://www.youtube.com/watch?v=81Af8fNUZIY
    [TestCase("9960", "1111111116", "")] // plusgirot unknown
    [TestCase("9999", "0123454", "")] // unknown
    public void ConvertToBbanTest(string clearing, string account, string? expectedIban)
    {
        var acc = new Account(clearing, account);
        Assert.Multiple(() =>
        {
            Assert.That(acc.Issues == ValidationIssues.None, Is.EqualTo(expectedIban is not null), "IsValidAccount " + acc.Issues);
            Assert.That(acc.CanConvertToIban(), Is.EqualTo(!string.IsNullOrEmpty(expectedIban)), "CanConvertToIban");
        });
        try
        {
            var bban = acc.ToBban();
            var bankRecord = acc.BankRecord!;
            Console.WriteLine($"{bankRecord.BankName} {bankRecord.BIC}");

            Assert.Multiple(() =>
            {
                Assert.That(expectedIban, Is.Not.Null, "If we got here, we must expect valid iban");
                Assert.That(bban, Is.EqualTo(expectedIban![4..].Replace(" ", string.Empty)), "ToIbanNumber Clean");
            });

            var accFrIban = Account.FromBban(bban);
            Assert.That(accFrIban, Is.Not.Null);
            // handelsbanken needs modification of clearing
            var kontoMatchIban = acc.ClearingNumber[0] == '6' ? new Account("6000", account) : acc;
            Assert.That(accFrIban.ToString().Replace(" ", ""), 
                Is.EqualTo(kontoMatchIban.ToString().Replace(" ", "").Replace("-", "")));
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine(ex.ToString());
            Assert.That(expectedIban, Is.Null, $"Exception and {nameof(expectedIban)} is not null");
        }
        catch (NotImplementedException ex)
        {
            Console.WriteLine(ex.ToString());
            Assert.That(expectedIban, Is.Empty, $"Exception and {nameof(expectedIban)} is not null");
        }
    }

    [TestCase("83279")]
    [TestCase("123456782")]
    [TestCase("3316812057492")]
    public void Mod10Test(string data)
    {
        Assert.Multiple(() =>
        {
            Assert.That(SeMod10.Calc(data[0..^1]), Is.EqualTo(int.Parse(data[^1..])), "last digit");
            Assert.That(SeMod10.Check(data), Is.True);
        });
    }

    [TestCase("71041234562")]
    [TestCase("71021234567")]
    [TestCase(" 2763608957")]
    [TestCase(" 12763608956")]
    [TestCase(" 912763608909")]
    [TestCase(" 1912763608957")]
    public void Mod11Test(string data)
    {
        var mod11Calc = SeMod11.Calc(data[1..^1]);
        Assert.Multiple(() =>
        {
            Assert.That(mod11Calc, Is.Not.EqualTo(10), "10 is invalid for Mod11");
            Assert.That(mod11Calc, Is.EqualTo(int.Parse(data[^1..])), "last digit");
            Assert.That(SeMod11.Check(data[1..]), Is.True);
        });
    }
}
