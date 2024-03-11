import { kontose_mini, kontosehelpers } from '../bankdata_mini';
import { kontose } from '../bankdata_full';
console.log(kontose_mini.banks);
console.log(kontose.banks);

test('banks array lengths', () => {
  expect(kontose_mini.banks).toHaveLength(9);
  expect(kontose.banks.length).toBeGreaterThan(10);
});


type GetBankDataSet = [clr: number, comment: number];
it.each<GetBankDataSet>([
  [3300, 1],
  [9549, 3],
])('kontose GetBank clearing %i', (clr: number, comment: number) => {
  expect(kontosehelpers.getBank(kontose_mini.banks, clr)!.c).toBe(comment);
});

type CheckKontoDataSet = [konto: string, valid: boolean];
it.each<CheckKontoDataSet>([
  ["3300, 1010101010", true],
  ["3300, 1010101011", false],
  ["600, 123456789", false], // to short clearing
  ["6000, 123456789", true],
  ["06000, 123456789", true],
  ["6000,  12345679", true], // zero fill
  ["6000123456789", true], // split on clearing length
  ["6000, 123456780", false],
  ["6000, 123", false], // to short
  ["81509, 1", false],
  ["9710, 1234560", true], // not full validation == 2 matching
  ["9710, 1234569", true],
  ["9710, 1234561", false], // invalid checksum
  ["9710, 01234569", false], // to long
])('kontose valid konto %s', (konto: string, valid: boolean) => {
  expect(kontose_mini.valid(konto)).toBe(valid);
});

test('kontose getBank not valid', () => {
  expect(kontosehelpers.getBank(kontose_mini.banks, 1000)).toBeNull();
});

