/**
 * @param r - Range of clearingnumbers from, to
 * @param i - IBAN Id
 * @param t - Account Type from bankgirot
 * @param c - Account Comment from bankgirot
 * @param m - IBAN Method
 */
export interface bankItem {
    r: number[];
    i?: number;
    t?: number;
    c: number;
    m: number;
}

export class kontosehelpers {
    static getNums(input: string) {
        return input ? input.replace(/\D/g, "") : "";
    }

    static mod10check(value: string) {
        var w = 2;
        var sum = 0;
        var l = value.length;
        for (var i = (l - 2); i >= 0; i--, w = 3 - w) {
            var p = +value[i] * w;
            sum += (p < 10) ? p : p % 10 + 1;
        }
        return ((10 * Math.ceil(sum / 10)) - sum) == +value[l - 1];
    }

    static mod11check(value: string) {
        var l = value.length;
        var sum = 0;
        for (var i = l - 1; i >= 0; i--) {
            var p = l - i;
            while (p > 10) p -= 10;
            sum += +value[i] * p;
        }
        return sum !== 0 && sum % 11 == 0;
    }

    static getBank(banks: bankItem[], clearing: number) {
        for (let bank of banks) {
            if (bank.r[0] <= clearing &&
                clearing <= bank.r[1]) {
                return bank;
            }
        }
        return null;
    }

    static* getIbanBanks(banks: bankItem[], ibanId: number) : IterableIterator<bankItem> {
        for (let bank of banks) {
            if (bank.i == ibanId)
                yield bank;
        }
    }
    
    static stripstart0(input: string) {
        input = kontosehelpers.getNums(input);
        // remove initial zeros
        while (input.length != 0 && input[0] === "0")
            input = input.substring(1);
        return input;
    }

    static padstart0(s: string, len: number) {
        s=s+"";
        while (s.length < len)
            s = "0" + s;
        return s;
    }

    static split(account: string) {
        const nums = kontosehelpers.stripstart0(kontosehelpers.getNums(account));
        const isSwb8 = nums.length !== 0 && nums[0] == '8';
        const clrLen = isSwb8 ? 5 : 4;
        const commaParts = account.split(',');
        if (commaParts.length === 2) {
            return [
                kontosehelpers.stripstart0(kontosehelpers.getNums(commaParts[0].trim())),
                kontosehelpers.getNums(commaParts[1].trim()),
                nums];
        }
        return [
            nums.substring(0, clrLen),
            nums.substring(clrLen),
            nums];
    }

    static valid(banks: bankItem[], value: string, allowunknown: boolean) {
        var [clearing, account, nums] = kontosehelpers.split(value);
        const isSwb8 = nums[0] === '8';
        const clrLen = isSwb8 ? 5 : 4;

        if (clearing.length != clrLen)
            return false;

        if (isSwb8 && !kontosehelpers.mod10check(clearing))
            return false;

        var acclen = account.length;
        var bank = kontosehelpers.getBank(banks, +clearing.substring(0, 4));
        if (bank != null && bank.t === 2) {
            var minlen = bank.m;
            var maxlen = bank.c === 2 ? 9 : 10; // comment 2 is max 9, others 10
            if (acclen < minlen || maxlen < acclen) {
                return false;
            }
            account = kontosehelpers.padstart0(account, maxlen);

            // use mod11 on comment2 everything else is mod10
            return bank.c === 2
                ? kontosehelpers.mod11check(account)
                : kontosehelpers.mod10check(account);
        }
        else if (bank != null && bank.t === 1) {
            if (acclen !== 7) {
                return false;
            }

            return kontosehelpers.mod11check(nums.substring(bank.c === 1 ? 1 : 0));
        }

        // default is type 1
        // comment 1 / 2 we accept some false positives
        return allowunknown &&
            acclen === 7 && (
                kontosehelpers.mod11check(nums) ||
                kontosehelpers.mod11check(nums.substring(1)));
    }
}

export class kontose_mini {
    // List of exceptions from type 1 accounts, other accounts is always of type 1
    // This allowes all valid accounts, but is not strict on type 1 or future clearings
    // Type 2 is special cases, Type 1 (not in list) always account length 7 with mod11 4 + 7 or 3 + 7
    static banks: bankItem[] = [
        { r: [3300, 3300], i: 300, t: 2, c: 1, m: 10 }, // 3300 + 0,   10  Mod10(a10), Type2, Comment1, 10 - 10
        { r: [3782, 3782], t: 2, c: 1, m: 10 }, // 3782 + 0,   10  Mod10(a10), Type2, Comment1, 10 - 10
        { r: [6000, 6999], i: 600, t: 2, c: 2, m: 7 }, // 6000 + 999, 9   Mod11(a9),  Type2, Comment2, 7 - 9
        { r: [8000, 8999], i: 800, t: 2, c: 3, m: 5 }, // 8000 + 999, -10 Mod10(a10), Type2, Comment3, 5 - 10
        { r: [9180, 9189], i: 120, t: 2, c: 1, m: 7 }, // 9180 + 9,   10  Mod10(a10), Type2, Comment1, 7 - 10
        { r: [9300, 9349], i: 930, t: 2, c: 1, m: 7 }, // 9300 + 49,  10  Mod10(a10), Type2, Comment1, 7 - 10
        { r: [9500, 9549], i: 950, t: 2, c: 3, m: 7 }, // 9500 + 49,  -10 Mod10(a10), Type2, Comment3, 7 - 10
        { r: [9570, 9579], i: 957, t: 2, c: 1, m: 5 }, // 9570 + 9,   10  Mod10(a10), Type2, Comment1, 5 - 10
        { r: [9960, 9969], i: 950, t: 2, c: 3, m: 7 }  // 9960 + 9,   -10 Mod10(a10), Type2, Comment3, 7 - 10
    ];

    static valid(value: string, allowunknown: boolean = true) {
        return kontosehelpers.valid(kontose_mini.banks, value, allowunknown);
    }
}
