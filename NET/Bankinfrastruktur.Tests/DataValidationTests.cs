using System.Globalization;
using System.Text;

namespace Bankinfrastruktur.Tests;

public class DataValidationTests
{
    [TestCase(0)]
    [TestCase(999)]
    [TestCase(1000)]
    [TestCase(1099)]
    [TestCase(9999)]
    [TestCase(10000)]
    public void BankDataNotFoundTest(int clearing) => 
        Assert.That(Data.Banks.GetBankFromClearing(clearing), Is.Null);

    // based on data in data file, if that changes, these test parameters might need updates
    [TestCase(3300, 3300, 300, "NDEASESS", "Nordea", Data.AccountTypeType.Type2, Data.CheckDigitTypeType.Comment1, Data.IbanMethodType.Method2, 10, 10)]
    [TestCase(7100, 7999, 800, "SWEDSESS", "Swedbank", Data.AccountTypeType.Type1, Data.CheckDigitTypeType.Comment1, Data.IbanMethodType.Method1, 7, 7)]
    [TestCase(8900, 8999, 800, "SWEDSESS", "Swedbank", Data.AccountTypeType.Type2, Data.CheckDigitTypeType.Comment3, Data.IbanMethodType.Method3, 7, 10)]
    public void BankDataFoundValidationTest(int clearing, int clearingEnd,
        int ibanId, string bic, string bankName, Data.AccountTypeType accountType, Data.CheckDigitTypeType checkDigitType,
        Data.IbanMethodType ibanMethod, int accMinLen, int accLen)
    {
        var br = Data.Banks.GetBankFromClearing(clearing);
        Assert.That(br, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(br.ClearingEnd, Is.EqualTo(clearingEnd));
            Assert.That(br.IbanId, Is.EqualTo(ibanId));
            Assert.That(br.BIC, Is.EqualTo(bic));
            Assert.That(br.BankName, Is.EqualTo(bankName));
            Assert.That(br.AccountType, Is.EqualTo(accountType));
            Assert.That(br.CheckDigitType, Is.EqualTo(checkDigitType));
            Assert.That(br.IbanMethod, Is.EqualTo(ibanMethod));
            Assert.That(br.AccountNumberMinLength, Is.EqualTo(accMinLen));
            Assert.That(br.AccountNumberLength, Is.EqualTo(accLen));
        });
    }

    [Test]
    public void DumpBankList()
    {
        // verifies that we can create all records, and then recreate the text
        var banksFull = Data.Banks.GetBanks();
        var banksFullRecreated = Data.Banks.RecreateBankList(banksFull);
        Console.WriteLine(banksFullRecreated);
        Assert.Multiple(() =>
        {
            Assert.That(banksFullRecreated, Is.EqualTo(banksFull));
            Assert.That(Helpers.GetLines(Data.Banks.SourcePsv), Is.EqualTo(Helpers.GetLines(banksFull)), "Incorrect sortorder?");
        });
    }

    private static Dictionary<int, string> GetClearingDictLines(IEnumerable<string> lines) =>
        lines.ToDictionary(l => Convert.ToInt32(l.Split(new[] { '|' }, 2)[0]));

    [TestCase("Bankgirot.txt", "IbanBic.txt")]
    public void EnsureDataFromSeparateSources(string bgFile, string ibanBicFile)
    {
        var psvSource = Data.Banks.GetBanks();

        var psvBgLines = GetClearingDictLines(StripToBankgirotTypComment(psvSource));
        var cmpBg = GetClearingDictLines(ParseBankgirotText(Helpers.FindAndReadTextFile(bgFile)));

        // Tabell över Clearingnummer och deras respektive IBAN ID, BIC, Metod och namn på bank
        var psvIbanLines = GetClearingDictLines(StripToIbanBicMethod(psvSource));
        var cmpIban = GetClearingDictLines(ParseIbanBicText(Helpers.FindAndReadTextFile(ibanBicFile)));

        Assert.Multiple(() =>
        {
            Assert.That(CompareAccounts(psvBgLines, cmpBg, "bg", cmpIban), Is.Empty);
            Assert.That(CompareAccounts(psvIbanLines, cmpIban, "bi", cmpBg), Is.Empty);
        });
    }

    private static bool IsSep(char c) => c == ' ' || c == '|'
        || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;

    private static IEnumerable<string> StripToBankgirotTypComment(string data) =>
        Helpers.GetLines(data)
        .Select(l => l.Split('|'))
        .Select(s => string.Join("|", s[0], s[1], s[4], s[5], s[6], s[9]));

    private static IEnumerable<string> ParseBankgirotText(string data) =>
        Helpers.GetLines(data)
        .SelectMany(ParseBankgirotLine)
        .Select(s => string.Join("|", s));

    private static IEnumerable<string[]> ParseBankgirotLine(string l)
    {
        // Name, clearing - clearing format comment
        var i = l.Length - 1;
        var curS = new StringBuilder(i);
        var parts = new List<string>();
        var formatLength = 0;
        var prefixLength = 0;
        // field = comment = 0, format = 1, clearing = 2, name = 3
        while (i >= 0)
        {
            var c = l[i--];
            var isSepCandidate = IsSep(c);
            if (isSepCandidate && parts.Count <= 1)
            {
                if (parts.Count == 0)
                {
                    Assert.That(curS.Length, Is.EqualTo(1), $"Comment should be one char {curS} in line {l}");
                    Assert.That(curS.ToString(), Is.AnyOf("1", "2", "3"), $"Comment should be one of 1,2,3 {curS} in line {l}");
                }
                parts.Insert(0, curS.ToString());
                curS.Clear();
                continue;
            }
            if (parts.Count == 1)
            {
                var expectedChars = formatLength == 0 ? new char[] { 'C' } :
                    prefixLength == 0 ? new char[] { 'x', '0' } :
                    new char[] { '0' };
                Assert.That(c, Is.AnyOf(expectedChars), $"Unexpected format char, prev {curS} from {l}");
                if (c == 'C' || c == 'x')
                    formatLength++;
                else prefixLength++;
            }
            else if (isSepCandidate && parts.Count == 2)
            {
                var cnxt = l[i];
                var ctcnxt = CharUnicodeInfo.GetUnicodeCategory(cnxt);
                if (ctcnxt == UnicodeCategory.UppercaseLetter ||
                    ctcnxt == UnicodeCategory.LowercaseLetter ||
                    ctcnxt == UnicodeCategory.ClosePunctuation)
                {
                    parts.Insert(0, curS.ToString());
                    curS.Clear();
                }
                continue;
            }
            curS.Insert(0, c);
        }
        parts.Insert(0, curS.ToString());
        var debugLine = $"{l} : {string.Join('|', parts)} {prefixLength} : {formatLength}";
        var clearinRange = parts[1];
        Assert.Multiple(() =>
        {
            Assert.That(parts, Has.Count.EqualTo(4), debugLine);
            Assert.That(prefixLength + formatLength, Is.EqualTo(12), debugLine);
            Assert.That(clearinRange, Has.Length.EqualTo(9), debugLine);
            Assert.That(clearinRange[4], Is.AnyOf('-', '/'), debugLine);
        });

        var clrStart = Convert.ToInt32(clearinRange[..4]);
        var clrEnd = Convert.ToInt32(clearinRange[5..]);
        var typ = formatLength == 7 ? "1" : "2";
        var name = parts[0]
            .Replace(" - personkonto", string.Empty)
            .Replace("/Plusgirot", " (Plusgirot)")
            .Replace(" Bank AB", " Bank");
        var normParts = new[] { null, null, name, $"Type{typ}", $"Comment{parts[3]}", $"{formatLength}" };
        var ranges = new[] { (clrStart, clrEnd) };
        if (clearinRange[4] == '/')
            ranges = new[] { (clrStart, clrStart), 
                (clrEnd, clrEnd) };
        foreach (var splClr in new[] { 3300, 3782 })
        {
            if (clrStart < splClr && splClr < clrEnd)
            {
                ranges = new[] { (clrStart, splClr - 1),
                (splClr + 1, clrEnd) };
                normParts[2] = name.Replace($" (exkl. personkonton, cl {splClr})", string.Empty);
            }
        }

        return ranges.SelectMany(r =>
        {
            normParts[0] = $"{r.clrStart}";
            normParts[1] = $"{r.clrEnd}";
            // Console.WriteLine($"{string.Join('|', normParts)} src: {string.Join('|', parts)}");
            return new[] { normParts as string[] };
        });
    }

    private static string CompareAccounts(Dictionary<int, string> psvLines, Dictionary<int, string> cmp,
        string name, Dictionary<int, string> altCmp)
    {
        const string emptyN = "\n";
        var sbErrors = new StringBuilder(emptyN);
        var sbWarnings = new StringBuilder(emptyN);
        foreach (var bData in cmp)
        {
            if (!altCmp.TryGetValue(bData.Key, out var altLine))
                altLine = null;
            if (!psvLines.TryGetValue(bData.Key, out var bankLine))
            {
                var sb = altLine is null ? sbWarnings : sbErrors;
                sb.AppendLine($"Missing {name}: {bData.Value} alt: {altLine}");
            }
            else if (!string.Equals(bankLine, bData.Value, StringComparison.InvariantCultureIgnoreCase))
                sbWarnings.AppendLine(
                    $"Existing    {bankLine}\n" +
                    $"{name}          {bData.Value}");
        }

        if (sbErrors.Length == 1) sbErrors.Clear();
        if (sbWarnings.Length == 1) sbWarnings.Clear();
        Console.WriteLine(sbWarnings.ToString());
        return sbErrors.ToString();
    }

    private static IEnumerable<string> StripToIbanBicMethod(string data) => Helpers.GetLines(data)
        .Select(l => l.Split('|'))
        .Select(s => string.Join("|", s[0], s[1], s[2], s[3], s[4], s[7])); // , s[6]

    // Vi behöver dela upp och städa en del av datan ifrån PDFen
    private static readonly Dictionary<int, (string, string[])> TextCleaners = new()
    {
        {3000, ("3000-3399, (exkl. personkto, cl nr 3300)", new[] { "3000-3299", "3301-3399" })},
        {3300, ("3300 (personkto)",  new[] { "3300-3300"})},
        {3410, ("3410-4999, (exkl. personkto, cl nr 3782)", new[] { "3410-3781", "3783-3999", "4000-4999" })}, // 3 och 4 har olika kommentarer
        {3782, ("3782 (personkto)", new[] { "3782-3782" })},
        {9190, (" DnB NOR filial", new[] { " DnB Bank" })},
        {9280, (" Resurs Bank AB", new[] { " Resurs Bank" })},
    };

    private static readonly Dictionary<int, string> ForceMethod = new()
    {
        { 3300, "2" },
    };

    private static IEnumerable<string[]> ParseIbanBicLine(string l)
    {
        int i = 0;
        var clearingStart = Convert.ToInt32(l[..(i += 4)]);
        if (TextCleaners.TryGetValue(clearingStart, out var cleanThis) &&
            l.Contains(cleanThis.Item1))
        {
            Console.WriteLine($" Modifying {l} -> {cleanThis.Item2[0]} ...");
            return cleanThis.Item2.SelectMany(r => ParseIbanBicLine(l.Replace(cleanThis.Item1, r)));
        }
        var clearingEnd = clearingStart;
        if (l[i++] == '-')
        {
            clearingEnd = Convert.ToInt32(l.Substring(i, 4));
            i += 4;
        }
        if (l[i] == '*')
            i++; // ignore notes *Swedbank har meddelat att serien utgår i samband med att Dataclearingen avvecklas.
        if (!IsSep(l[i++]))
            throw new FormatException($"Expected Pipe at {i-1} (Between clearing end and IbanId) on {l}");
        var ibanId = Convert.ToInt32(l.Substring(i, 3));
        i += 3;
        if (!IsSep(l[i++]))
            throw new FormatException($"Expected Pipe at {i - 1} (Between IbanId and BIC) on {l}");
        var bic = l.Substring(i, 8);
        i += 8;
        if (!IsSep(l[i++]))
            throw new FormatException($"Expected Pipe at {i - 1} (Between BIC and name) on {l}");
        var name = l[i..];
        // TODO check for last diggit indicating method
        var method = name[^2..];
        if (IsSep(method[0]))
        {
            method = method[1..];
            name = name[..^2];
        }
        else method = ForceMethod.TryGetValue(clearingStart, out var forceMethod) ? forceMethod : "?";
        return new[] { new[] { $"{clearingStart}", $"{clearingEnd}", $"{ibanId}", bic, name, method } };
    }

    private static IEnumerable<string> ParseIbanBicText(string data)
    {
        var ibanIdBic = new Dictionary<int, string>();
        foreach (var s in Helpers.GetLines(data).SelectMany(ParseIbanBicLine))
        {
            var ibanId = int.Parse(s[2]);
            if (ibanIdBic.TryGetValue(ibanId, out var existingBic))
                Assert.That(s[3], Is.EqualTo(existingBic));
            ibanIdBic[ibanId] = s[3];
            var method = s[5];
            if (method == "0" || method == "?")
                method = "Unknown";
            yield return string.Join("|", s[0], s[1], s[2], s[3], s[4], "Method" + method);
        }
        // Dictionary to help with IbanID and BIC in full set
        Console.WriteLine($"var {nameof(ibanIdBic)} = new Dictionary<int, string>() {{");
        foreach (var kvp in ibanIdBic.OrderBy(kvp => kvp.Key))
            Console.WriteLine($"{{{kvp.Key}, \"{kvp.Value}\"}},");
        Console.WriteLine($"}};");
    }
}
