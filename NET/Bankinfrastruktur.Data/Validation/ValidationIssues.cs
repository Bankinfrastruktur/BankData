namespace Bankinfrastruktur.Validation;

[Flags]
public enum ValidationIssues
{
    None = 0,
    IncorrectClearingNumberLength = 1 << 0,
    IncorrectAccountNumberLength = 1 << 1,
    IncorrectAccountNumber = 1 << 2,
}
