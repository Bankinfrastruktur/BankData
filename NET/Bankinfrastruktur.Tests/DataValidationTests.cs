using System.Text;

namespace Bankinfrastruktur.Tests;

public class DataValidationTests
{
    [Test]
    public void DumpBankList()
    {
        // verifies that we can create all records, and then recreate the text
        var banksFull = Data.Banks.GetBanks();
        var banksFullRecreated = Data.Banks.RecreateBankList(banksFull);
        Console.WriteLine(banksFullRecreated);
        Assert.Multiple(() =>
        {
            Assert.That(banksFullRecreated, Is.EqualTo(banksFull));
            Assert.That(Helpers.GetLines(Data.Banks.SourcePsv), Is.EqualTo(Helpers.GetLines(banksFull)), "Incorrect sortorder?");
        });
    }
}
