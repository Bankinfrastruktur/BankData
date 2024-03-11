import { IBAN } from "./iban.js";
import { bankItem, kontosehelpers, kontose_mini } from './bankdata_mini.js';

export class kontose extends kontose_mini {
    static banks: bankItem[] = (() => {
        return [];
    })();

    static override valid(value: string, allowunknown: boolean = true) {
        return kontosehelpers.valid(kontose.banks, value, allowunknown);
    }

    static bbancc2iban(bban: String, cc: string) {
        return cc + kontosehelpers.padstart0(String(98 - IBAN.mod97calc(bban + cc + "00")), 2) + bban;
    }

    static geniban(ibanid: number, bbanbase: string) {
        return kontose.bbancc2iban(String(ibanid) + kontosehelpers.padstart0(bbanbase, 17), "SE");
    }
}
