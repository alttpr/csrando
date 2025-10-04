import { customAlphabet } from "nanoid";

const ALPHABET =
  "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
const ID_LENGTH = 16;

const generate = customAlphabet(ALPHABET, ID_LENGTH);

export function generateId(): string {
  return generate();
}
