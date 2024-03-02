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

# Lista på Clearingnummer, banker, IBAN, BIC och methoder
<!-- kontotable.html -->
<table class="sortable"><thead>
<tr><th>Start</th><th>Slut</th><th>IBAN ID</th><th>BIC</th><th>Bank</th><th>Typ</th><th>Kommentar</th><th>IBAN Metod</th><th>Min</th><th>Längd</th></tr>
</thead><tbody>
<tr><td>1100</td><td>1199</td><td>300</td><td>NDEASESS</td><td>Nordea</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>1200</td><td>1399</td><td>120</td><td>DABASESX</td><td>Danske Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>1400</td><td>2099</td><td>300</td><td>NDEASESS</td><td>Nordea</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>2300</td><td>2399</td><td>230</td><td>AABASESS</td><td>Ålandsbanken</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>2400</td><td>2499</td><td>120</td><td>DABASESX</td><td>Danske Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>3000</td><td>3299</td><td>300</td><td>NDEASESS</td><td>Nordea</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>3300</td><td>3300</td><td>300</td><td>NDEASESS</td><td>Nordea</td><td>Type2</td><td>Comment1</td><td>Method2</td><td>10</td><td>10</td></tr>
<tr><td>3301</td><td>3399</td><td>300</td><td>NDEASESS</td><td>Nordea</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>3400</td><td>3409</td><td>902</td><td>ELLFSESS</td><td>Länsförsäkringar Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>3410</td><td>3781</td><td>300</td><td>NDEASESS</td><td>Nordea</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>3782</td><td>3782</td><td>300</td><td>NDEASESS</td><td>Nordea</td><td>Type2</td><td>Comment1</td><td>MethodUnknown</td><td>7</td><td>10</td></tr>
<tr><td>3783</td><td>3999</td><td>300</td><td>NDEASESS</td><td>Nordea</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>4000</td><td>4999</td><td>300</td><td>NDEASESS</td><td>Nordea</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>5000</td><td>5999</td><td>500</td><td>ESSESESS</td><td>SEB</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>6000</td><td>6999</td><td>600</td><td>HANDSESS</td><td>Handelsbanken</td><td>Type2</td><td>Comment2</td><td>Method2</td><td>7</td><td>9</td></tr>
<tr><td>7000</td><td>7999</td><td>800</td><td>SWEDSESS</td><td>Swedbank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>8000</td><td>8999</td><td>800</td><td>SWEDSESS</td><td>Swedbank</td><td>Type2</td><td>Comment3</td><td>Method3</td><td>7</td><td>10</td></tr>
<tr><td>9020</td><td>9029</td><td>902</td><td>ELLFSESS</td><td>Länsförsäkringar Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9040</td><td>9049</td><td>904</td><td>CITISESX</td><td>Citibank (filial)</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9060</td><td>9069</td><td>902</td><td>ELLFSESS</td><td>Länsförsäkringar Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9070</td><td>9079</td><td>907</td><td>FEMAMTMT</td><td>Multitude Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9100</td><td>9109</td><td>910</td><td>NNSESES1</td><td>Nordnet Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9120</td><td>9124</td><td>500</td><td>ESSESESS</td><td>SEB</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9130</td><td>9149</td><td>500</td><td>ESSESESS</td><td>SEB</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9150</td><td>9169</td><td>915</td><td>SKIASESS</td><td>Skandiabanken</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9170</td><td>9179</td><td>917</td><td>IKANSE21</td><td>Ikanobanken</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9180</td><td>9189</td><td>120</td><td>DABASESX</td><td>Danske Bank</td><td>Type2</td><td>Comment1</td><td>MethodUnknown</td><td>7</td><td>10</td></tr>
<tr><td>9190</td><td>9199</td><td>919</td><td>DNBASESX</td><td>DnB Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9230</td><td>9239</td><td>923</td><td>MARGSESS</td><td>Marginalen Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9250</td><td>9259</td><td>925</td><td>SBAVSESS</td><td>SBAB Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9260</td><td>9269</td><td>919</td><td>DNBASESX</td><td>DnB Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9270</td><td>9279</td><td>927</td><td>IBCASES1</td><td>ICA Banken</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9280</td><td>9289</td><td>928</td><td>RESUSE21</td><td>Resurs Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9300</td><td>9349</td><td>930</td><td>SWEDSESS</td><td>Swedbank</td><td>Type2</td><td>Comment1</td><td>Method1</td><td>7</td><td>10</td></tr>
<tr><td>9390</td><td>9399</td><td>939</td><td>LAHYSESS</td><td>Landshypotek</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9400</td><td>9449</td><td>940</td><td>FORXSES1</td><td>Forex Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9460</td><td>9469</td><td>946</td><td>BSNOSESS</td><td>Santander Consumer Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9470</td><td>9479</td><td>947</td><td>FTSBSESS</td><td>BNP Paribas</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9500</td><td>9549</td><td>950</td><td>NDEASESS</td><td>Nordea (Plusgirot)</td><td>Type2</td><td>Comment3</td><td>MethodUnknown</td><td>7</td><td>10</td></tr>
<tr><td>9550</td><td>9569</td><td>955</td><td>AVANSES1</td><td>Avanza Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9570</td><td>9579</td><td>957</td><td>SPSDSE23</td><td>Sparbanken Syd</td><td>Type2</td><td>Comment1</td><td>Method2</td><td>5</td><td>10</td></tr>
<tr><td>9580</td><td>9589</td><td>958</td><td>BMPBSESS</td><td>AION Bank</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9590</td><td>9599</td><td>959</td><td>ERPFSES2</td><td>Erik Penser Bank AB</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9630</td><td>9639</td><td>963</td><td>LOSADKKK</td><td>Lån & Spar Bank A/S, filial</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9640</td><td>9649</td><td>964</td><td>NOFBSESS</td><td>NOBA Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9660</td><td>9669</td><td>966</td><td>SVEASES1</td><td>Svea Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9670</td><td>9679</td><td>967</td><td>JAKMSE22</td><td>JAK Medlemsbank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9680</td><td>9689</td><td>968</td><td>BSTPSESS</td><td>Bluestep Finans AB</td><td>Type1</td><td>Comment1</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9700</td><td>9709</td><td>970</td><td>EKMLSE21</td><td>Ekobanken</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9710</td><td>9719</td><td>971</td><td>LUNADK2B</td><td>Lunar Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9750</td><td>9759</td><td>975</td><td>NOHLSESS</td><td>Northmill Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9780</td><td>9789</td><td>978</td><td>KLRNSESS</td><td>Klarna Bank</td><td>Type1</td><td>Comment2</td><td>Method1</td><td>7</td><td>7</td></tr>
<tr><td>9960</td><td>9969</td><td>950</td><td>NDEASESS</td><td>Nordea (Plusgirot)</td><td>Type2</td><td>Comment3</td><td>MethodUnknown</td><td>7</td><td>10</td></tr>
</tbody></table>

