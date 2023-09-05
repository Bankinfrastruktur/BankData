using Bankinfrastruktur.Data;

namespace Bankinfrastruktur.Validation;

public partial class Account
{
    /// <summary>If Swedbank clearing 8000-8999 which is a clearing of 5 chars instead of 4</summary>
    public static bool IsSwb8(string clearing) =>
        !string.IsNullOrEmpty(clearing)
            && clearing[0] == '8';

    public const int ClearingLengthSwb8 = 5;
    public const int ClearingLengthOthers = 4;

    public static int GetClearingLength(string clearing) =>
        IsSwb8(clearing) ? ClearingLengthSwb8 : ClearingLengthOthers;

    /// <summary>Validates <see cref="AccountTypeType.Type1"/> <see cref="CheckDigitTypeType.Comment1"/> or
    /// <see cref="CheckDigitTypeType.Comment2"/>, returns instance for valid</summary>
    private static BankRecord? GetFallbackBank(string clearing, string bankAccount) =>
        new[] { BankRecord.BasicType1Comment1, BankRecord.BasicType1Comment2 }
            .FirstOrDefault(b => b.ValidateChecksum(clearing, bankAccount));

    public static BankRecord? GetBank(string clearing, string account)
    {
        int clearingNumber = 0;
        var result = clearing.Length >= ClearingLengthOthers
            && int.TryParse(clearing.Substring(0, ClearingLengthOthers), out clearingNumber);
        var br = result ? Banks.GetBankFromClearing(clearingNumber) : null;
        // check fallback for new (unknown) banks which must be type1
        return br ?? GetFallbackBank(clearing, account);
    }

    public BankRecord? BankRecord { get; private set; }
    public string ClearingNumber { get; }
    public string AccountNumber { get; private set; }
    public ValidationIssues Issues { get; }

    public Account(string clearingNumber, string accountNumber)
    {
        ClearingNumber = clearingNumber.KeepDigits().TrimStart('0');
        AccountNumber = accountNumber;
        Issues = GetIssues(accountNumber.KeepDigits());
    }

    public override string ToString() =>
        ClearingNumber.Length == 0 && AccountNumber.KeepDigits().Length == 0 ? "-"
        : $"{ClearingNumber}, {AccountNumber}";

    private ValidationIssues GetIssues(string account)
    {
        if (ClearingNumber.Length < ClearingLengthOthers)
        {
            BankRecord = null;
            return ValidationIssues.IncorrectClearingNumberLength;
        }

        BankRecord = GetBank(ClearingNumber, account);
        if (BankRecord is null)
        {
            return ValidationIssues.IncorrectAccountNumber;
        }

        var issues = ValidationIssues.None;
        if (account.Length < BankRecord.AccountNumberMinLength || account.Length > BankRecord.AccountNumberLength)
        {
            issues |= ValidationIssues.IncorrectAccountNumberLength;
        }
        var padSize = BankRecord.AccountNumberLength - account.Length;
        if (padSize > 0)
        {
            // pad regardless of error so caller can get correct sized account back
            account = account.PadLeft(BankRecord.AccountNumberLength, '0');
            AccountNumber = new string('0', padSize) + AccountNumber;
        }

        if (ClearingNumber.Length != GetClearingLength(ClearingNumber))
        {
            issues |= ValidationIssues.IncorrectClearingNumberLength;
        }
        if (account.Length != BankRecord.AccountNumberLength)
        {
            // Length not correct after possible padding
            issues |= ValidationIssues.IncorrectAccountNumberLength;
        }
        if (issues == ValidationIssues.None && !BankRecord.ValidateChecksum(ClearingNumber, account))
        {
            // checksum validation fail
            issues |= ValidationIssues.IncorrectAccountNumber;
        }

        return issues;
    }

    /// <summary>Give parsed and validated account</summary>
    public static Account? Parse(string? accountString)
    {
        if (string.IsNullOrEmpty(accountString))
            return null;

        var split = accountString!.Split(new[] { ',' }, 3);
        if (split.Length == 2)
            return new(split[0].Trim(), split[1].Trim());

        var purge = accountString.KeepDigits();
        if (string.IsNullOrEmpty(purge))
            return null;

        var clrLen = GetClearingLength(purge);
        return purge.Length <= clrLen ? new(purge, string.Empty)
            : new(
            purge.Substring(0, clrLen),
            purge.Substring(clrLen));
    }
}
