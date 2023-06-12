namespace Bankinfrastruktur.Helpers;

/// <summary>RFC4648</summary>
public static class Base32
{
    // https://stackoverflow.com/a/42231034
    private const string b32alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    public static string FromBytes(byte[] bytes)
    {
        var sb = new System.Text.StringBuilder(bytes.Length);
        for (int bitIndex = 0; bitIndex < bytes.Length * 8; bitIndex += 5)
        {
            int dualbyte = bytes[bitIndex / 8] << 8;
            if (bitIndex / 8 + 1 < bytes.Length)
                dualbyte |= bytes[bitIndex / 8 + 1];
            dualbyte = 0x1f & dualbyte >> 16 - bitIndex % 8 - 5;
            sb.Append(b32alphabet[dualbyte]);
        }

        return sb.ToString();
    }

    public static byte[] ToBytes(string base32)
    {
        var output = new List<byte>();
        for (int bitIndex = 0; bitIndex / 5 + 1 < base32.Length; bitIndex += 8)
        {
            var baseIdx = bitIndex / 5;
            int dualbyte = b32alphabet.IndexOf(base32[baseIdx]) << 10;
            if (baseIdx + 1 < base32.Length)
                dualbyte |= b32alphabet.IndexOf(base32[baseIdx + 1]) << 5;
            if (baseIdx + 2 < base32.Length)
                dualbyte |= b32alphabet.IndexOf(base32[baseIdx + 2]);

            dualbyte = 0xff & dualbyte >> 15 - bitIndex % 5 - 8;
            output.Add((byte)dualbyte);
        }
        return output.ToArray();
    }
}
