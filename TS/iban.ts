const _c0 = "0".charCodeAt(0);
const _c9 = "9".charCodeAt(0);
const _cA = "A".charCodeAt(0);
const _cZ = "Z".charCodeAt(0);
const _ca = "a".charCodeAt(0);
const _cz = "z".charCodeAt(0);

export class IBAN {
    private static getNumFromChar(n: string): number {
        var code = n.charCodeAt(0);
        if (code >= _c0 && code <= _c9) {
            return code - _c0;
        }
        // A = 10, B = 11, ... Z = 35
        if (code >= _cA && code <= _cZ) {
            return code - _cA + 10;
        }
        if (code >= _ca && code <= _cz) {
            return code - _ca + 10;
        }
        return -1;
    }

    static mod97calc(iban: string): number {
        var sum = 0;
        for (var i = 0; i < iban.length; i++) {
            var v = IBAN.getNumFromChar(iban[i]);
            if (v === -1)
                continue;
            sum = (sum * (v >= 10 ? 100 : 10) + v) % 97;
        }
        return sum;
    }

    static validate(iban: string): boolean {
        return iban.length > 4 &&
            IBAN.mod97calc(iban.substring(4) + iban.substring(0, 4)) === 1;
    }

    static getData(iban: string): string {
        return iban.replace(/[^0-9A-Za-z]/g, "");
    }
}
