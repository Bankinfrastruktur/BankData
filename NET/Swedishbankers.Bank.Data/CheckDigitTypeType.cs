namespace Swedishbankers.Bank.Data;

public enum CheckDigitTypeType
{
    Invalid = 0,
    /// <summary>For <see cref="AccountTypeType.Type2"/> mod10 on account part</summary>
    Comment1 = 1,
    Comment2 = 2,
    /// <summary>Only <see cref="AccountTypeType.Type2"/> Swedbank & plusgirot mod10</summary>
    Comment3 = 3,
}
