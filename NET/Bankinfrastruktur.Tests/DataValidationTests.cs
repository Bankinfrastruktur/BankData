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
    [TestCase(3300, 3300, 300, "NDEASESS", "Nordea", Data.AccountTypeType.Type2c1, Data.CheckDigitTypeType.Comment1, Data.IbanMethodType.Method2, 10, 10)]
    [TestCase(7100, 7999, 800, "SWEDSESS", "Swedbank", Data.AccountTypeType.Type1c1, Data.CheckDigitTypeType.Comment1, Data.IbanMethodType.Method1, 7, 7)]
    [TestCase(8900, 8999, 800, "SWEDSESS", "Swedbank", Data.AccountTypeType.Type2c3, Data.CheckDigitTypeType.Comment3, Data.IbanMethodType.Method3, 7, 10)]
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
            Assert.That(br.AccountTypeCombined, Is.EqualTo(accountType));
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
        lines.ToDictionary(l => Convert.ToInt32(l.Split(['|'], 2)[0]));

    [Test]
    public void VerifyAllRowsOfIbanBicTest()
    {
        var data = Helpers.FindAndReadTextFile("IbanBic.txt");
        Assert.Multiple(() =>
        {
            foreach (var l in Helpers.GetLines(data))
            {
                ParseIbanBicLine(l);
            }
        });
    }

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
        // Name|clearing - clearing|format|comment
        var parts = l.Split('|');
        Assert.That(parts, Has.Length.EqualTo(4), l);
        var name = parts[0];
        var clearinRange = parts[1];
        var format = parts[2];
        var comment = parts[3];
        Assert.That(comment, Has.Length.EqualTo(1), $"Comment should be one char {comment} in line {l}");
        Assert.That(comment, Is.AnyOf("1", "2", "3"), $"Comment should be one of 1,2,3 {comment} in line {l}");
        var formatLength = 0;
        var prefixLength = 0;
        foreach (var c in format.Reverse())
        {
            var allowedChars = formatLength == 0 ? ['C'] :
                prefixLength == 0 ? new char[] { 'x', '0' } :
                ['0'];
            Assert.That(c, Is.AnyOf(allowedChars), $"Unexpected format char in {format} from {l}");
            if (c == 'C' || c == 'x')
                formatLength++;
            else prefixLength++;
        }
        var debugLine = $"{l} : {string.Join('|', parts)} {prefixLength} : {formatLength}";
        Assert.Multiple(() =>
        {
            Assert.That(prefixLength + formatLength, Is.EqualTo(12), debugLine);
            Assert.That(clearinRange, Has.Length.EqualTo(9), debugLine);
            Assert.That(clearinRange[4], Is.AnyOf('-', '/'), debugLine);
        });

        var clrStart = Convert.ToInt32(clearinRange[..4]);
        var clrEnd = Convert.ToInt32(clearinRange[5..]);
        var typ = formatLength == 7 ? "1" : "2";
        name = name
            .Replace(" - personkonto", string.Empty)
            .Replace("/Plusgirot", " (Plusgirot)")
            .Replace(" Bank AB", " Bank");
        var normParts = new[] { "", "", name, $"Type{typ}", $"Comment{parts[3]}", $"{formatLength}" };
        var ranges = new[] { (clrStart, clrEnd) };
        if (clearinRange[4] == '/')
            ranges = [(clrStart, clrStart), (clrEnd, clrEnd)];
        foreach (var splClr in new[] { 3300, 3782 })
        {
            if (clrStart < splClr && splClr < clrEnd)
            {
                ranges = [(clrStart, splClr - 1), (splClr + 1, clrEnd)];
                normParts[2] = name.Replace($" (exkl. personkonton, cl {splClr})", string.Empty);
            }
        }

        return ranges.SelectMany(r =>
        {
            normParts[0] = $"{r.clrStart}";
            normParts[1] = $"{r.clrEnd}";
            // Console.WriteLine($"{string.Join('|', normParts)} src: {string.Join('|', parts)}");
            return new[] { normParts };
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
    private static readonly List<(string startsWith, string find, string[] replacewith)> TextCleaners =
    [
        ("3000", "3000-3399, (exkl. personkto, cl nr 3300)", ["3000-3299", "3301-3399"]),
        ("3300", "3300 (personkto)", ["3300-3300"]),
        ("3410", "3410-4999, (exkl. personkto, cl nr 3782)", ["3410-3781", "3783-3999", "4000-4999"]), // 3 och 4 har olika kommentarer
        ("3782", "3782 (personkto)", ["3782-3782"]),
        ("9190", " DnB NOR filial", [" DnB Bank"]),
        ("9280", " Resurs Bank AB", [" Resurs Bank"]),
    ];

    private static readonly Dictionary<int, string> ForceMethod = new()
    {
        { 3300, "2" },
    };

    private static IEnumerable<string[]> ParseIbanBicLine(string l)
    {
        foreach (var (startsWith, find, replacewith) in TextCleaners
            .Where(c => l.StartsWith(c.startsWith) && l.Contains(c.find)))
        {
            Console.WriteLine($" Modifying {l} -> {replacewith[0]} ...");
            return replacewith.SelectMany(rw => ParseIbanBicLine(l.Replace(find, rw)));
        }

        var parts = l.Split('|');
        Assert.That(parts, Has.Length.EqualTo(5), l);
        var clearing = parts[0];
        var sibanId = parts[1];
        var bic = parts[2];
        var name = parts[3];
        var method = parts[4];
        if (clearing[^1] == '*')  // ignore notes *Swedbank har meddelat att serien utgår i samband med att Dataclearingen avvecklas.
            clearing = clearing[..^1];

        Assert.Multiple(() =>
        {
            Assert.That(clearing, Has.Length.EqualTo(9), l);
            Assert.That(sibanId, Has.Length.EqualTo(3), l);
            Assert.That(method, Has.Length.AnyOf(0, 1), l);
            Assert.That(clearing[4], Is.EqualTo('-'), l);
        });

        var clearingStart = Convert.ToInt32(clearing[..4]);
        var clearingEnd = Convert.ToInt32(clearing[5..]);
        var ibanId = Convert.ToInt32(sibanId);
        if (method.Length == 0)
            method = ForceMethod.TryGetValue(clearingStart, out var forceMethod) ? forceMethod : "Unknown";
        Assert.That(method, Is.AnyOf("1", "2", "3", "4", "Unknown"), l);
        return [[$"{clearingStart}", $"{clearingEnd}", $"{ibanId}", bic, name, method]];
    }

    private static IEnumerable<string> ParseIbanBicText(string data)
    {
        var ibanIdBic = Bankinfrastruktur.Helpers.DocumentHelpers.IbanIdToBicMap();
        var usedIbanIds = new HashSet<int>();
        foreach (var s in Helpers.GetLines(data).SelectMany(ParseIbanBicLine))
        {
            var ibanId = Convert.ToInt32(s[2]);
            usedIbanIds.Add(ibanId);
            if (ibanIdBic.TryGetValue(ibanId, out var existingBic))
            {
                if (string.IsNullOrEmpty(s[3]))
                {
                    s[3] = existingBic;
                }
                Assert.That(s[3], Is.EqualTo(existingBic));
            }
            else if (!string.IsNullOrEmpty(s[3]))
            {
                ibanIdBic[ibanId] = s[3];
            }
            Assert.That(s[3], Has.Length.EqualTo(8), message: string.Join(", ", s));
            yield return string.Join("|", s[0], s[1], s[2], s[3], s[4], "Method" + s[5]);
        }

        var unusedIbanIdsInBicMap = ibanIdBic.Keys.Except(usedIbanIds).ToList();
        if (unusedIbanIdsInBicMap.Count != 0)
        {
            Console.WriteLine($"Unused IbanIds in BIC map: {string.Join(", ", unusedIbanIdsInBicMap)}");
        }
        Bankinfrastruktur.Helpers.DocumentHelpers.SaveIbanToBicMap(ibanIdBic);
    }
}
