// The fakes the connection tests share: a server in memory over the Core meta, its transport,
// a bare record, a transport that counts the calls of another, and a persistence provider in
// memory.
export * from './lib/fakes/counting-transport';
export * from './lib/fakes/fake-record';
export * from './lib/fakes/fake-server';
export * from './lib/fakes/fake-transport';
export * from './lib/fakes/memory-persistence-provider';
