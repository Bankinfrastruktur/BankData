using Bankinfrastruktur.Data;

namespace Bankinfrastruktur.Validation;

public static class AccountExtensions
{
    public static string KeepDigits(this string s) =>
        new([.. s.Where(x => 0 <= x - '0' && x - '0' <= 9)]);

    public static bool ValidateChecksum(this BankRecord br, string clearing, string bankAccount) => br.AccountTypeCombined switch
    {
        AccountTypeType.Type1c1 => SeMod11.Check((clearing + bankAccount.PadLeft(7, '0')).Substring(1)),
        AccountTypeType.Type1c2 => SeMod11.Check(clearing + bankAccount.PadLeft(7, '0')),
        AccountTypeType.Type2c2 => SeMod11.Check(bankAccount.PadLeft(9, '0')),
        AccountTypeType.Type2c3 when Account.IsSwb8(clearing) =>
                clearing.Length == Account.ClearingLengthSwb8 &&
                SeMod10.Check(clearing) &&
                SeMod10.Check(bankAccount),
        AccountTypeType.Type2c1 or AccountTypeType.Type2c3 => SeMod10.Check(bankAccount.PadLeft(10, '0')),
        _ => throw new ArgumentOutOfRangeException(nameof(br.AccountTypeCombined), br.AccountTypeCombined, $"Invalid {nameof(AccountTypeType)}"),
    };

    public static bool CanConvertToIban(this BankRecord? br) =>
        br is not null && br.IbanMethod != IbanMethodType.MethodUnknown;

    public static bool CanConvertToIban(this Account? acc) =>
        acc?.BankRecord?.CanConvertToIban() ?? false;
}
