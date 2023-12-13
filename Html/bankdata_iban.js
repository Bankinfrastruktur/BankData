"use strict";
class IBAN {
    static getNumFromChar(n) {
        var code = n.charCodeAt(0);
        if (code >= this._0 && code <= this._9) {
            return code - this._0;
        }
        // A = 10, B = 11, ... Z = 35
        if (code >= this._A && code <= this._Z) {
            return code - this._A + 10;
        }
        if (code >= this._a && code <= this._z) {
            return code - this._a + 10;
        }
        return -1;
    }
    static mod97calc(iban) {
        var sum = 0;
        for (var i = 0; i < iban.length; i++) {
            var v = this.getNumFromChar(iban[i]);
            if (v === -1)
                continue;
            sum = (sum * (v >= 10 ? 100 : 10) + v) % 97;
        }
        return sum;
    }
    static validate(iban) {
        return iban.length > 4 &&
            this.mod97calc(iban.substring(4) + iban.substring(0, 4)) === 1;
    }
    static getData(iban) {
        return iban.replace(/[^0-9A-Za-z]/g, "");
    }
}
IBAN._0 = "0".charCodeAt(0);
IBAN._9 = "9".charCodeAt(0);
IBAN._A = "A".charCodeAt(0);
IBAN._Z = "Z".charCodeAt(0);
IBAN._a = "a".charCodeAt(0);
IBAN._z = "z".charCodeAt(0);
if (!IBAN.validate("NL05RABO4526312517"))
    console.error("fail test");
