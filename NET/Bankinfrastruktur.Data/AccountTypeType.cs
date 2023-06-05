namespace Bankinfrastruktur.Data;

public enum AccountTypeType
{
    Invalid = 0,
    /// <summary>Clearing(4) + Account(7) mod 11, <see cref="CheckDigitTypeType.Comment1"/> skips first clearing diggit</summary>
    Type1 = 1,
    /// <summary>Only Account checked not clearing, <see cref="CheckDigitTypeType"/></summary>
    Type2 = 2,
}
