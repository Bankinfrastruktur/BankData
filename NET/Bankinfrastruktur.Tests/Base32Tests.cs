using Bankinfrastruktur.Helpers;

namespace Bankinfrastruktur.Tests;

public class Base32Tests
{
    /// <summary>Vectors from rfc4648</summary>
    [TestCase("", "")]
    [TestCase("f", "MY======")]
    [TestCase("fo", "MZXQ====")]
    [TestCase("foo", "MZXW6===")]
    [TestCase("foob", "MZXW6YQ=")]
    [TestCase("fooba", "MZXW6YTB")]
    [TestCase("foobar", "MZXW6YTBOI======")]
    public void Base32Rfc4648VectorTest(string input, string output)
    {
        var inBytes = System.Text.Encoding.ASCII.GetBytes(input);
        Assert.Multiple(() =>
        {
            Assert.That(Base32.FromBytes(inBytes), Is.EqualTo(output.TrimEnd('=')));
            Assert.That(Base32.ToBytes(output.TrimEnd('=')), Is.EqualTo(inBytes));
        });
    }
}
