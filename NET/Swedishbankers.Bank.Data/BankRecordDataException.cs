namespace Swedishbankers.Bank.Data;

public class BankRecordDataException : Exception
{
    public int ClearingStart { get; }
    public string BankName { get; }
    public AccountTypeType AccountType { get; }
    public string Issue { get; }
    internal BankRecordDataException(int clearingStart, string bankName, AccountTypeType accountType, string message)
        : base($"Account {accountType} {message} for: {clearingStart} : {bankName}")
    {
        ClearingStart = clearingStart;
        BankName = bankName;
        AccountType = accountType;
        Issue = message;
    }
}
