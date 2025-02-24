namespace Bankinfrastruktur.Data;

public class BankRecord
{
    private static void ValidateClearingRange(int clearing, string paramName)
    {
        if (clearing < 1000 || 9999 < clearing)
            throw new ArgumentOutOfRangeException(paramName, clearing, "Must be 4 numbers");
    }

    public static readonly BankRecord BasicType1Comment1 = new(AccountTypeType.Type1c1, "-");
    public static readonly BankRecord BasicType1Comment2 = new(AccountTypeType.Type1c2, "-");

    /// <summary>Create generic type1 bank</summary>
    private BankRecord(AccountTypeType accountType, string bankName)
    {
        AccountTypeCombined = accountType;
        BankName = bankName;
        BIC = string.Empty;
        AccountNumberMinLength = 7;
        AccountNumberLength = 7;
        IbanMethod = IbanMethodType.MethodUnknown; // Since IBAN ID is unknown
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
        var typeStr = s[i++];
        var commentStr = s[i++];
        if (Enum.TryParse(typeStr + commentStr.Replace("Comment", "c"), out AccountTypeType accountType) ||
            Enum.TryParse(typeStr, out accountType))
            AccountTypeCombined = accountType;
        if (AccountTypeCombined == AccountTypeType.Type1 || AccountTypeCombined == AccountTypeType.Type2)
            throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"unsupported {nameof(AccountTypeCombined)}");
        if (AccountType == AccountTypeType.Invalid)
            throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"{nameof(AccountType)} {AccountType}");
        if (Enum.TryParse(commentStr, out CheckDigitTypeType checkDigitType)
            && checkDigitType != CheckDigitType)
            throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"{nameof(CheckDigitType)} {CheckDigitType} != {checkDigitType}");
        if (Enum.TryParse(s[i++], out IbanMethodType ibanMethod))
            IbanMethod = ibanMethod;
        AccountNumberMinLength = int.Parse(s[i++]);
        AccountNumberLength = int.Parse(s[i++]);

        if (ClearingEnd < ClearingStart)
            throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"expected {nameof(ClearingStart)} {ClearingStart} <= {nameof(ClearingEnd)} {ClearingEnd}");
        if (CheckDigitType == CheckDigitTypeType.Invalid)
            throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"{nameof(CheckDigitType)} {CheckDigitType}");
        if (BIC.Length != 8)
            throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"expected {nameof(BIC)} length 8 {BIC}");
        if (AccountTypeCombined == AccountTypeType.Type1c1 || AccountTypeCombined == AccountTypeType.Type1c2)
        {
            if (AccountNumberLength != 7)
                throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"expected {nameof(AccountNumberLength)} 7 was {AccountNumberLength}");
            if (IbanMethod != IbanMethodType.Method1)
                throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"expected IBAN {nameof(IbanMethodType.Method1)} was {IbanMethod}");
        }
        else if (AccountTypeCombined == AccountTypeType.Type2c1 || AccountTypeCombined == AccountTypeType.Type2c2
            || AccountTypeCombined == AccountTypeType.Type2c3)
        {
            var expectedLength = AccountTypeCombined == AccountTypeType.Type2c2 ? 9 : 10;
            if (AccountNumberLength != expectedLength)
                throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"expected {nameof(AccountNumberLength)} {expectedLength} was {AccountNumberLength}");
        }
        else
        {
            throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"unhandled {nameof(AccountTypeCombined)}");
        }
        if (AccountNumberLength < AccountNumberMinLength)
            throw new BankRecordDataException(ClearingStart, BankName, AccountTypeCombined, $"expected {nameof(AccountNumberMinLength)} {AccountNumberMinLength} <= {nameof(AccountNumberLength)} {AccountNumberLength}");
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

    public AccountTypeType AccountTypeCombined { get; }
    public AccountTypeType AccountType => AccountTypeCombined switch {
        AccountTypeType.Type1c1 or
        AccountTypeType.Type1c2 => AccountTypeType.Type1,
        AccountTypeType.Type2c1 or
        AccountTypeType.Type2c2 or
        AccountTypeType.Type2c3 => AccountTypeType.Type2,
        _ => AccountTypeType.Invalid,
    };
    public CheckDigitTypeType CheckDigitType => AccountTypeCombined switch {
        AccountTypeType.Type1c1 or AccountTypeType.Type2c1 => CheckDigitTypeType.Comment1,
        AccountTypeType.Type1c2 or AccountTypeType.Type2c2 => CheckDigitTypeType.Comment2,
        AccountTypeType.Type2c3 => CheckDigitTypeType.Comment3,
        _ => CheckDigitTypeType.Invalid,
    };
    public IbanMethodType IbanMethod { get; }
    public int AccountNumberMinLength { get; }
    public int AccountNumberLength { get; }
}
