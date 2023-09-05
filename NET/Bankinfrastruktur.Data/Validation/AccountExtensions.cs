using Bankinfrastruktur.Data;

namespace Bankinfrastruktur.Validation;

public static class AccountExtensions
{
    public static string KeepDigits(this string s) =>
        new(s.Where(x => 0 <= x - '0' && x - '0' <= 9).ToArray());

    public static bool ValidateChecksum(this BankRecord br, string clearing, string bankAccount) => br.AccountType switch
    {
        AccountTypeType.Type1 => SeMod11.Check((clearing + bankAccount.PadLeft(7, '0'))
            .Substring(br.CheckDigitType == CheckDigitTypeType.Comment1 ? 1 : 0)),
        AccountTypeType.Type2 => br.CheckDigitType switch
        {
            CheckDigitTypeType.Comment2 => SeMod11.Check(bankAccount.PadLeft(9, '0')),
            CheckDigitTypeType.Comment3 when Account.IsSwb8(clearing) =>
                clearing.Length == Account.ClearingLengthSwb8 &&
                SeMod10.Check(clearing) &&
                SeMod10.Check(bankAccount),
            _ => SeMod10.Check(bankAccount.PadLeft(10, '0')),
        },
        _ => throw new ArgumentOutOfRangeException(nameof(br.AccountType), br.AccountType, $"Invalid AccountTypeType"),
    };

    public static bool CanConvertToIban(this BankRecord? br) =>
        br is not null && br.IbanMethod != IbanMethodType.MethodUnknown;

    public static bool CanConvertToIban(this Account? acc) =>
        acc?.BankRecord?.CanConvertToIban() ?? false;
}
