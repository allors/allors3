// The fakes the connection tests share: a server in memory over the Core meta, its transport,
// a bare record, and a transport that counts the calls of another.
export * from './lib/fakes/counting-transport';
export * from './lib/fakes/fake-record';
export * from './lib/fakes/fake-server';
export * from './lib/fakes/fake-transport';
