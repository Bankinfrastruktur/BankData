namespace Bankinfrastruktur.Data;

public class BankRecord
{
    private static void ValidateClearingRange(int clearing, string paramName)
    {
        if (clearing < 1000 || 9999 < clearing)
            throw new ArgumentOutOfRangeException(paramName, clearing, "Must be 4 numbers");
    }

    public static readonly BankRecord BasicType1Comment1 = new(CheckDigitTypeType.Comment1, "-");
    public static readonly BankRecord BasicType1Comment2 = new(CheckDigitTypeType.Comment2, "-");

    /// <summary>Create generic type1 bank</summary>
    private BankRecord(CheckDigitTypeType checkDigitType, string bankName)
    {
        AccountType = AccountTypeType.Type1;
        CheckDigitType = checkDigitType;
        BankName = bankName;
        BIC = string.Empty;
        AccountNumberMinLength = 7;
        AccountNumberLength = 7;
        IbanMethod = IbanMethodType.MethodUnknown;
    }

    /// <summary>Create instance from pipe separated dataline</summary>
    public BankRecord(string[] psvData)
    {
        var s = psvData;

        var i = 0;
        ClearingStart = int.Parse(s[i++]);
        ClearingEnd = int.Parse(s[i++]);
        ValidateClearingRange(ClearingStart, nameof(ClearingStart));
        ValidateClearingRange(ClearingEnd, nameof(ClearingEnd));
        IbanId = int.Parse(s[i++]);
        BIC = s[i++];
        BankName = s[i++];
        if (Enum.TryParse(s[i++], out AccountTypeType accountType))
            AccountType = accountType;
        if (Enum.TryParse(s[i++], out CheckDigitTypeType checkDigitType))
            CheckDigitType = checkDigitType;
        if (Enum.TryParse(s[i++], out IbanMethodType ibanMethod))
            IbanMethod = ibanMethod;
        AccountNumberMinLength = int.Parse(s[i++]);
        AccountNumberLength = int.Parse(s[i++]);

        if (CheckDigitType == CheckDigitTypeType.Invalid)
            throw new BankRecordDataException(ClearingStart, BankName, AccountType, $"Invalid {nameof(CheckDigitType)} {CheckDigitType}");
        if (AccountType == AccountTypeType.Type1)
        {
            if (AccountNumberLength != 7)
                throw new BankRecordDataException(ClearingStart, BankName, AccountType, $"expected {nameof(AccountNumberLength)} 7 was {AccountNumberLength}");
            if (IbanMethod != IbanMethodType.Method1)
                throw new BankRecordDataException(ClearingStart, BankName, AccountType, $"expected IBAN {nameof(IbanMethodType.Method1)} was {IbanMethod}");
        }
        else if (AccountType == AccountTypeType.Type2)
        {
            if ((checkDigitType == CheckDigitTypeType.Comment1 ||
                checkDigitType == CheckDigitTypeType.Comment3) &&
                AccountNumberLength != 10)
                throw new BankRecordDataException(ClearingStart, BankName, AccountType, $"expected {nameof(AccountNumberLength)} 10 was {AccountNumberLength}");
            if (checkDigitType == CheckDigitTypeType.Comment2 &&
                AccountNumberLength != 9)
                throw new BankRecordDataException(ClearingStart, BankName, AccountType, $"expected {nameof(AccountNumberLength)} 9 was {AccountNumberLength}");
        }
        else
        {
            throw new BankRecordDataException(ClearingStart, BankName, AccountType, $"Invalid {nameof(AccountType)}");
        }
    }

    public override string ToString()
    {
        return string.Join("|",
            ClearingStart, ClearingEnd, IbanId, BIC, BankName,
            AccountType, CheckDigitType, IbanMethod,
            AccountNumberMinLength, AccountNumberLength);
    }

    public bool MatchClearing(int clearingNumber)
    {
        return ClearingStart <= clearingNumber &&
            ClearingEnd >= clearingNumber;
    }

    public int ClearingStart { get; }
    public int ClearingEnd { get; }
    public int IbanId { get; }
    public string BIC { get; }
    public string BankName { get; }

    public CheckDigitTypeType CheckDigitType { get; }
    public AccountTypeType AccountType { get; }
    public IbanMethodType IbanMethod { get; }
    public int AccountNumberLength { get; }
    public int AccountNumberMinLength { get; }
}
