# Bank data Clearing och IBAN uppgifter för svenska banker
[![NuGet Badge (Bankinfrastruktur.Data)](https://buildstats.info/nuget/Bankinfrastruktur.Data)](https://www.nuget.org/packages/Bankinfrastruktur.Data)

Sammanställning av data i maskinläsbart format från Bankgirot och Bankinfrastruktur (BSAB) för validering och uträkning av clearingnummer och IBAN konto

## Data, updateringar och källor
Se [Readme i Data](https://github.com/Bankinfrastruktur/BankData/tree/main/Data)

## Avvikelser

### Nordea 3300, 3782
3782 används i praktiken inte, betalning via 3300 och 3782 går till samma konto. (Det förekommer att banker inte tillåter 3782)

### Handelsbanken 6000-6999
Alla konton i serien är unika, oavsett clearing. Konvertering från IBAN kan använda valfrit clearingnr.

### Swedbank 8000-8999
I bankgirots dokument har det [tidigare](https://web.archive.org/web/20230905155004/https://www.bankgirot.se/globalassets/dokument/anvandarmanualer/bankernaskontonummeruppbyggnad_anvandarmanual_sv.pdf) för Typ 2, Kommentar 3 stått
> "I sällsynta fall förekommer dock Swedbanks kontonummer som inte alls är kontrollerbara med kontrollsiffra."

Det är borttaget [2023-11-07](https://web.archive.org/web/20231115154902/https://www.bankgirot.se/globalassets/dokument/anvandarmanualer/bankernaskontonummeruppbyggnad_anvandarmanual_sv.pdf), och gäller inte längre. Alla konton är nu "kontrollerbara med kontrollsiffra"  
Swedbank har även tidigare hänvisat till sin [IBAN sida](https://www.swedbank.se/privat/betala-och-overfora/iban-och-bic.html) och skriver att om kontot inte är giltigt där så är kontot ogiltigt. ([Alternativ sida som är lättare att testa med](https://jsfiddle.net/mzwe8coh/2/))  
Mer information om uppdateringen finns i [disukssion](https://github.com/jop-io/kontonummer.js/issues/15#issuecomment-1798394596) och när [ändringen upptäcktes](https://github.com/Bankinfrastruktur/BankData/issues/32).

### Sparbanken Syd 9570-9579  
Använder endast 9570
