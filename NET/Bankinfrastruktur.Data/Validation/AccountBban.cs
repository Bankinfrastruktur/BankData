using Bankinfrastruktur.Data;

namespace Bankinfrastruktur.Validation;

public partial class Account
{
    public static Account? FromBban(string bbanIn)
    {
        var cleanbban = bbanIn.KeepDigits();
        // split to ibanId and remaining BBAN
        var ibanId = Convert.ToInt32(cleanbban.Substring(0, 3));
        var bban = cleanbban.Substring(3);
        var bbannum = bban.TrimStart('0');
        // check banks with matching IbanId
        var bankCandidates = Banks.BankList.Where(
            b => b.IbanId == ibanId)
            .Select(bank =>
            {
                var clrlen = bank.ClearingStart == 8000 ? ClearingLengthSwb8 : ClearingLengthOthers;
                // get clearing, first diggits based on AccountNumberLength on bank
                if (bank.IbanMethod != IbanMethodType.Method2 &&
                    bbannum.Length < (clrlen + bank.AccountNumberLength)) return null;
                var clearing = bank.IbanMethod == IbanMethodType.Method2 ?
                    bban.Substring(bban.Length - bank.AccountNumberLength - clrlen, clrlen) :
                    bbannum.Substring(0, clrlen);
                var clr4 = Convert.ToInt32(clearing.Substring(0, 4));

                // Use fallback clearing if no clearing in BBAN
                if (bank.IbanMethod == IbanMethodType.Method2 && clr4 == 0)
                {
                    // IbanId 300, 3300-3300 = 100%  Nordea          Clearing som saknas har alltid clearing 3300
                    // IbanId 600, 6000-6999 = 0.1%  Handelsbanken   Vilken clearing som helst kan användas i 6000-6999, varje konto är unikt (Officiel källa handelsbanken)
                    // IbanId 957, 9570-9579 = 10%   Sparbanken Syd  Endast 9570 används
                    clearing = (clr4 = bank.ClearingStart).ToString();
                }
                if (bank.MatchClearing(clr4))
                {
                    var account = bban.Substring(bban.Length - bank.AccountNumberLength);
                    var acc = new Account(clearing, account);
                    var bankRecord = acc.Issues == ValidationIssues.None ? acc.BankRecord : null;
                    var possibleBban = bankRecord.CanConvertToIban() ? acc.ToBban() : null;
#if DEBUG
                    if (possibleBban is not null && possibleBban != cleanbban)
                        throw new Exception($"Parsing {cleanbban} testing {bank} got {acc} but generated different BBAN: {possibleBban}");
#endif
                    if (possibleBban is not null && possibleBban == cleanbban)
                    {
                        return acc;
                    }
                }
                return null;
            }).Where(a => a is not null).Select(a => a!).ToList();
        if (bankCandidates.Count == 1)
            return bankCandidates.First();
        if (bankCandidates.Count != 1)
            throw new NotImplementedException($"Invalid Match Count {bankCandidates.Count} From {bban} {string.Join("\n", bankCandidates)}");

        return null;
    }

    private string PadBban(string bban) => BankRecord!.IbanId + bban.PadLeft(17, '0');

    public string ToBban()
    {
        if (BankRecord is null)
            throw new ArgumentNullException(nameof(BankRecord));

        var account = AccountNumber.KeepDigits();
        return BankRecord.IbanMethod switch
        {
            IbanMethodType.Method1 => PadBban(ClearingNumber + account),
            IbanMethodType.Method2 => PadBban(account),
            IbanMethodType.Method3 => PadBban(ClearingNumber.PadRight(5, '0') + account.PadLeft(10, '0')),
            _ => throw new NotImplementedException($"Unknown {BankRecord.IbanMethod}"),
        };
    }
}
