# Rutiner för uppdatering

## Om det finns något nytt
Spara ny version på https://web.archive.org/save/, Länka till versionen på archive.org i filer och commits

## Typ 1
> Marknaden strävar dock efter en ökad standardisering och tillåter sedan ett antal år tillbaka endast kontotyp 1 för nya banker och betalinstitut.

Alla nya banker ska vara Typ 1, det betyder 7 siffror och IBAN Method 1.  
Information som behövs är Clearing från-till, IbanId, BIC, Banknamn och "kommentar".

## Datakällor
* Kontrollera bankinfrastruktur.se efter IbanId, Bic. Kopieras in i [IbanBic.txt](IbanBic.txt)  
https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/iban-och-svenskt-nationellt-kontonummer
* Kontrollera bankgirot för information om (typ och) kommentar. Kopieras in i [Bankgirot.txt](Bankgirot.txt)  
https://www.bankgirot.se/globalassets/dokument/anvandarmanualer/bankernaskontonummeruppbyggnad_anvandarmanual_sv.pdf
* bankinfrastruktur.se separat lista med bara clearing och namn  
https://www.bankinfrastruktur.se/framtidens-betalningsinfrastruktur/konto-och-clearingnummer

## Lägga till / ta bort banker
* Banker kan bara läggas till i den kombinerade filen om fullt dataset finns med Iban och kommentar
* Banker tas bort ifall de är borta från alla datakällorna

# Bankens namn
Använd det kortaste eller mest användarvänliga för gemene man

# Avvikelser mellan källorna?
Meddela relevanta parter om dessa och efterfråga rättelse
