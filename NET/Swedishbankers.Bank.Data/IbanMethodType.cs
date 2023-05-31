namespace Swedishbankers.Bank.Data;

/// <summary> How to generate IBAN
/// https://www.swedishbankers.se/fraagor-vi-arbetar-med/betalningar/iban-och-svenskt-nationellt-kontonummer/
/// </summary>
public enum IbanMethodType
{
    MethodUnknown = 0,
    /// <summary>För Metod 1 ska clearingnumret medtas - nollutfyllnad till vänster om clearingnummer.</summary>
    Method1 = 1,
    /// <summary>För Metod 2 ska clearingnumret inte medtas - nollutfyllnad till vänster om kontonummer.</summary>
    Method2 = 2,
    /// <summary>För Metod 3 ska clearingnumret medtas inklusive den femte siffran som då blir en del av clearingnumret
    /// – först nollutfyllnad mellan kontonummer och clearingnummer om kontonummer är &lt;10 tecken, därefter nollutfyllnad till vänster om clearingnummer.</summary>
    Method3 = 3,
}
