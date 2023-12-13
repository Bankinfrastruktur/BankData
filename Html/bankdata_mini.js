"use strict";
class kontose {
    static getNums(input) {
        return input ? input.replace(/\D/g, "") : "";
    }
    static mod10check(value) {
        var w = 2;
        var sum = 0;
        var l = value.length;
        for (var i = (l - 2); i >= 0; i--, w = 3 - w) {
            var p = +value[i] * w;
            sum += (p < 10) ? p : p % 10 + 1;
        }
        return ((10 * Math.ceil(sum / 10)) - sum) == +value[l - 1];
    }
    static mod11check(value) {
        var l = value.length;
        var sum = 0;
        for (var i = l - 1; i >= 0; i--) {
            var p = l - i;
            while (p > 10)
                p -= 10;
            sum += +value[i] * p;
        }
        return sum !== 0 && sum % 11 == 0;
    }
    static getBank(clearing) {
        for (let bank of this.banks) {
            if (bank.r[0] <= clearing &&
                clearing <= bank.r[1]) {
                return bank;
            }
        }
        return null;
    }
    static *getIbanBanks(ibanId) {
        for (let bank of this.banks) {
            if (bank.i == ibanId)
                yield bank;
        }
    }
    static stripstart0(input) {
        input = this.getNums(input);
        // remove initial zeros
        while (input.length != 0 && input[0] === "0")
            input = input.substring(1);
        return input;
    }
    static padstart0(s, len) {
        s = s + "";
        while (s.length < len)
            s = "0" + s;
        return s;
    }
    static split(account) {
        const nums = this.stripstart0(this.getNums(account));
        const isSwb8 = nums.length !== 0 && nums[0] == '8';
        const clrLen = isSwb8 ? 5 : 4;
        const commaParts = account.split(',');
        if (commaParts.length === 2) {
            return [
                kontose.stripstart0(kontose.getNums(commaParts[0].trim())),
                kontose.getNums(commaParts[1].trim()),
                nums
            ];
        }
        return [
            nums.substring(0, clrLen),
            nums.substring(clrLen),
            nums
        ];
    }
    static valid(value) {
        var [clearing, account, nums] = this.split(value);
        const isSwb8 = nums[0] === '8';
        const clrLen = isSwb8 ? 5 : 4;
        if (clearing.length != clrLen)
            return false;
        if (isSwb8 && !this.mod10check(clearing))
            return false;
        var acclen = account.length;
        var bank = this.getBank(+clearing.substring(0, 4));
        if (bank != null && bank.t === 2) { // only type2
            var minlen = bank.m;
            var maxlen = bank.c === 2 ? 9 : 10; // comment 2 is max 9, others 10
            if (acclen < minlen || maxlen < acclen) {
                return false;
            }
            account = this.padstart0(account, maxlen);
            // use mod11 on comment2 everything else is mod10
            return bank.c === 2
                ? this.mod11check(account)
                : this.mod10check(account);
        }
        // default is type 1
        // comment 1 / 2 we accept some false positives
        return acclen === 7 && (this.mod11check(nums) ||
            this.mod11check(nums.substring(1)));
    }
}
// Type 2 is special cases, Type 1 (not in list) always account length 7 with mod11 4 + 7 or 3 + 7
kontose.banks = [
    { r: [3300, 3300], i: 300, t: 2, c: 1, m: 10 }, // 3300 + 0,   10  Mod10(a10), Type2, Comment1, 10 - 10
    { r: [3782, 3782], t: 2, c: 1, m: 10 }, // 3782 + 0,   10  Mod10(a10), Type2, Comment1, 10 - 10
    { r: [6000, 6999], i: 600, t: 2, c: 2, m: 7 }, // 6000 + 999, 9   Mod11(a9),  Type2, Comment2, 7 - 9
    { r: [8000, 8999], i: 800, t: 2, c: 3, m: 5 }, // 8000 + 999, -10 Mod10(a10), Type2, Comment3, 5 - 10
    { r: [9180, 9189], i: 120, t: 2, c: 1, m: 7 }, // 9180 + 9,   10  Mod10(a10), Type2, Comment1, 7 - 10
    { r: [9300, 9349], i: 930, t: 2, c: 1, m: 7 }, // 9300 + 49,  10  Mod10(a10), Type2, Comment1, 7 - 10
    { r: [9500, 9549], i: 950, t: 2, c: 3, m: 7 }, // 9500 + 49,  -10 Mod10(a10), Type2, Comment3, 7 - 10
    { r: [9570, 9579], i: 957, t: 2, c: 1, m: 5 }, // 9570 + 9,   10  Mod10(a10), Type2, Comment1, 5 - 10
    { r: [9960, 9969], i: 950, t: 2, c: 3, m: 7 } // 9960 + 9,   -10 Mod10(a10), Type2, Comment3, 7 - 10
];
if (kontose.getBank(3300).c != 1)
    console.error("fail test");
if (kontose.getBank(9549).c != 3)
    console.error("fail test");
if (kontose.getBank(1000) != null)
    console.error("fail test");
if (!kontose.valid("3300, 1010101010"))
    console.error("fail test");
if (kontose.valid("3300, 1010101011"))
    console.error("fail test");
if (kontose.valid("81509, 1"))
    console.error("fail test");
