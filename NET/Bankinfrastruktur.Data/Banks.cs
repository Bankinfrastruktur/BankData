using System.Text;

namespace Bankinfrastruktur.Data;

public static partial class Banks
{
    private static List<BankRecord>? _bankList;
    public static List<BankRecord> BankList => _bankList ??= GetList(SourcePsv);

    public static BankRecord? GetBankFromClearing(int clearingNumber) =>
        BankList.FirstOrDefault(b => b.MatchClearing(clearingNumber));

    public static string GetBanks() => GetBanks(BankList);
    public static string RecreateBankList(string data) => GetBanks(GetList(data));
    public static string GetBanks(List<BankRecord> bankDatas)
    {
        var sb = new StringBuilder();
        foreach (var b in bankDatas.OrderBy(bi => bi.ClearingStart))
        {
            sb.AppendLine(b.ToString());
        }
        return sb.ToString();
    }

    public static List<BankRecord> GetList(string data)
    {
        var bankDatas = new List<BankRecord>();
        foreach (var l in data.Split(["\r\n", "\r", "\n"], StringSplitOptions.None))
        {
            if (string.IsNullOrEmpty(l) ||
                l[0] == '#') continue;
            try
            {
                bankDatas.Add(new BankRecord(l.Split('|')));
            }
            catch (Exception ex)
            {
                ex.Data["line"] = l;
                throw;
            }
        }
        return bankDatas;
    }
}
