/**
 * Gives new objects their workspace ids: negative, counting down from -1. A database id is
 * positive. A workspace owns a generator; the connection meets its ids in a push of new
 * objects and answers with the database ids.
 */
export type IdGenerator = () => number;

export function createIdGenerator(): IdGenerator {
  let counter = 0;
  return () => --counter;
}
