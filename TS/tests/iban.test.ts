import { IBAN } from "../iban";

test('getData', () => {
  expect(IBAN.getData("NL05RABO4526312517")).toBe("NL05RABO4526312517");
  expect(IBAN.getData("NL05 RABO 4526 3125 17")).toBe("NL05RABO4526312517");
});

test('validate', () => {
  expect(IBAN.validate("NL05RABO4526312517")).toBe(true);
  expect(IBAN.validate("NL05 RABO 4526 3125 17")).toBe(true);
  expect(IBAN.validate("NL05 rabO 4526 3125 17")).toBe(true);
});
