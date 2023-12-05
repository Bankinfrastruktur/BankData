#!/bin/bash

echo '<table class="sortable"><thead>' > kontotable.html
echo '<tr><th>Start</th><th>Slut</th><th>IBAN ID</th><th>BIC</th><th>Bank</th><th>Typ</th><th>Kommentar</th><th>IBAN Metod</th><th>Min</th><th>Längd</th></tr>' >> kontotable.html
echo '</thead><tbody>' >> kontotable.html
grep -v "^#" Data/source.psv | while read LINE; do echo "<tr><td>${LINE//|/</td><td>}</td></tr>"; done >> kontotable.html
echo '</tbody></table>' >> kontotable.html
perl -i -p0e 's#(<!-- kontotable.html -->.*?)<table.*?</table>#$1'"$(cat kontotable.html)"'#s' README.md
rm kontotable.html
