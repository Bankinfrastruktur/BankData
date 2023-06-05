using System.Text;

namespace Swedishbankers.Bank.Tests;

public class DataValidationTests
{
    [Test]
    public void DumpBankList()
    {
        // verifies that we can create all records, and then recreate the text
        var banksFull = Data.Banks.GetBanks();
        var banksFullRecreated = Data.Banks.RecreateBankList(banksFull);
        Console.WriteLine(banksFullRecreated);
        Assert.That(banksFullRecreated, Is.EqualTo(banksFull));
    }
}
