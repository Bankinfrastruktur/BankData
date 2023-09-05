namespace Bankinfrastruktur.Validation;

public static class SeMod11
{
    private static int Sum(string number)
    {
        int sum = 0;
        var l = number.Length;
        for (var i = l - 1; i >= 0; i--)
        {
            var p = l - i;
            while (p > 10)
                p -= 10;
            sum += (number[i] - '0') * p;
        }
        return sum;
    }

    public static bool Check(string number)
    {
        var sum = Sum(number);
        return sum != 0 && sum % 11 == 0;
    }

    /// <summary>Adds zero at the end and calculates the expected last digit</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> Is not a valid mod 11 number (rest is 1 which would result in 10 as last digit)</exception>
    public static int Calc(string number)
    {
        var sum = Sum(number + "0") % 11;
        if (sum == 1)
            throw new ArgumentOutOfRangeException(nameof(number), number, "Invalid number for mod11, resulting in rest 1 which gives last digit 10");
        return sum == 0 ? 0 : 11 - sum;
    }
}
