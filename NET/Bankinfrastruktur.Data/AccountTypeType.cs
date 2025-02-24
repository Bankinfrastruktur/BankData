namespace Bankinfrastruktur.Data;

public enum AccountTypeType
{
    Invalid = 0,
    /// <summary>Clearing(4) + Account(7) mod 11, <see cref="CheckDigitTypeType.Comment1"/> skips first clearing diggit</summary>
    Type1 = 1,
    /// <summary>Clearing(4) + Account(7) mod 11, skips first clearing diggit</summary>
    Type1c1 = 11,
    /// <summary>Clearing(4) + Account(7) mod 11</summary>
    Type1c2 = 12,
    /// <summary>Only Account checked not clearing, <see cref="CheckDigitTypeType"/></summary>
    Type2 = 2,
    /// <summary>mod10 on account part</summary>
    Type2c1 = 21,
    Type2c2 = 22,
    /// <summary>Swedbank &amp; plusgirot mod10</summary>
    Type2c3 = 23,
}
