namespace Bankinfrastruktur.Validation;

public static class SeMod10
{
    private static readonly int[] mod10map = new[] { 0, 2, 4, 6, 8, 1, 3, 5, 7, 9 }; // map index to mult x * 2 % 10 + 1
    private static int Sum(string number)
    {
        bool bit = true;
        int sum = 0;
        var l = number.Length;
        while (l != 0)
        {
            var p = number[--l] - '0';
            bit = !bit;
            sum += bit ? mod10map[p] : p;
        }
        return sum;
    }

    public static bool Check(string number)
    {
        var sum = Sum(number);
        return sum != 0 && sum % 10 == 0;
    }

    /// <summary>Adds zero at the end and calculates the expected last digit</summary>
    public static int Calc(string number)
    {
        var sum = Sum(number + "0") % 10;
        return sum == 0 ? 0 : 10 - sum;
    }
}
