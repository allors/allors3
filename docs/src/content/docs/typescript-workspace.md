---
title: "The TypeScript workspace"
description: "The TypeScript libraries of the workspace, the code that the generator writes for them, the connection with a JSON client of the application's own, and where the TypeScript API differs from the .NET workspace."
audience: "Users"
kind: "Reference"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

The TypeScript workspace runs a workspace in a browser or in Node.js. It works with the database
through [the JSON API](json-api.md) of a server, as the Remote adapter of .NET does, and follows the
design that [The workspace](workspace.md) explains. This page lists its libraries, the code that
the generator writes, how a client creates a connection, and where its API differs from the .NET
workspace. The platform's tests run it against the Core test server with Core's test domain; the
sample application has no TypeScript client.

## The libraries

`typescript/modules` is an nx workspace. A library in `libs` is imported by its path:

| Import path | Holds |
| --- | --- |
| `@allors/system/common/protocol-json` | The requests and the responses of the JSON API, as TypeScript types. |
| `@allors/system/workspace/meta` | The interfaces of the metadata, such as `MetaPopulation`, `Class` and `RoleType`. |
| `@allors/system/workspace/meta-json` | `LazyMetaPopulation`, which builds the metadata from the data that the generator writes. |
| `@allors/system/workspace/domain` | The interfaces of the workspace, such as `IWorkspace`, `ISession`, `IObject` and `IStrategy`, and the pulls, with their extents, predicates and results. |
| `@allors/system/workspace/adapters` | What the adapter builds on: the session, the strategy, the records and `PrototypeObjectFactory`. |
| `@allors/system/workspace/adapters-json` | The adapter over the JSON API: `DatabaseConnection` and the interface `IDatabaseJsonClient`. |
| `@allors/core/workspace/meta`, `@allors/core/workspace/meta-json`, `@allors/core/workspace/domain` | The generated code of Core's test domain. |
| `@allors/core/workspace/derivations-custom` | The workspace rules of Core's test domain. |
| `@allors/core/workspace/derivations` | Only an import of `@allors/system/workspace/derivations`, a library that the repository no longer has. |

The libraries whose names end in `-tests` hold the tests. The platform publishes no package of
these libraries, and `typescript/modules/package.json` is private. The platform's tests compile
the source of each library through the paths in `typescript/modules/tsconfig.base.json`; an
application's TypeScript build compiles it from the application's copy of the platform in the same
way.

## The generated code

The templates are in `typescript/modules/templates`:

| Template | Writes |
| --- | --- |
| `workspace.meta.ts.stg` | The typed metadata, `M`, with builders of pulls, results, selects and includes. |
| `workspace.meta.json.ts.stg` | The metadata as data, `data`, which `LazyMetaPopulation` reads. |
| `workspace.domain.ts.stg` | An interface for each class and each interface of the workspace, with a property for each role and association, `canRead…` and `canWrite…` for each role with `Origin.Database`, and `canExecute…` and a `Method` for each method. |

Core's generator, in `dotnet/Core/Database/Generate`, renders them for the workspace `Default` of
Core's test domain, into the folders `generated` of `libs/core/workspace/meta`, `meta-json` and
`domain`. The target `Generate` of the build runs it. The sample's generator renders the templates
of the .NET workspace only; an application that wants the TypeScript workspace adds the three
templates to its generator, with folders of its own. The generated domain imports its metadata from
`@allors/default/workspace/meta`, so the application maps that path to its generated metadata, as
the platform's tests map it to Core's.

A property of a unit role is typed with `| null`, so a role that the user may not read reads as
`null` without an error.

## Create a connection

The adapter calls the server through an object of the application's that implements
`IDatabaseJsonClient`:

```ts
export interface IDatabaseJsonClient {
  pull(pullRequest: PullRequest): Promise<PullResponse>;
  sync(syncRequest: SyncRequest): Promise<SyncResponse>;
  push(pushRequest: PushRequest): Promise<PushResponse>;
  invoke(invokeRequest: InvokeRequest): Promise<InvokeResponse>;
  access(accessRequest: AccessRequest): Promise<AccessResponse>;
  permission(permissionRequest: PermissionRequest): Promise<PermissionResponse>;
}
```

Each method posts its request as JSON to the endpoint of the same name, such as `/allors/pull`,
with the bearer token of the user, and returns the JSON of the answer. The interface has no
sign-in: the client signs in at the sign-in endpoint, as [Sign in](json-api.md#sign-in) describes,
and keeps the token. `FetchClient`, in the tests, is such a client.

The test fixture builds the metadata with `new LazyMetaPopulation(data)` and then the connection:

```ts
let nextId = -1;
const configuration: Configuration = {
  name: 'Default',
  metaPopulation,
  objectFactory: new PrototypeObjectFactory(metaPopulation),
  idGenerator: () => nextId--,
  rules: ruleBuilder(this.m),
};

return new DatabaseConnection(configuration, this.jsonClient);
```

`idGenerator` gives the negative ids of new objects, and `rules` the workspace rules, which a
session turns on with `activate`. `connection.createWorkspace()` creates the workspace, and
`workspace.createSession()` a session.

## Pull, push and invoke

A pull is an object literal, and the `kind` of an extent or a predicate names its type:

```ts
const pull: Pull = {
  extent: {
    kind: 'Filter',
    objectType: m.C1,
    predicate: {
      kind: 'Equals',
      propertyType: m.C1.Name,
      value: name,
    },
  },
};
const result = await session.pull([pull]);
return result.collection<C1>(m.C1)[0];
```

- `session.pull` takes a pull or a list of pulls. `result.collection` takes a name, a class, an
  interface, a role type or an association type, ignores the case of a name, and returns an empty
  list when the result has no such collection; `result.object` and `result.value` take one object
  and one value.
- `session.push()` sends the changes, and `session.invoke` runs a method or a list of methods, with
  `isolated` and `continueOnError` in its options.
- `session.call(procedure, ...pulls)` runs a procedure and pulls, as
  [Procedures](pulls.md#procedures) describes.
- `session.create(m.C1)` creates an object, and `object.strategy.delete()` deletes a new object
  before its push.

## Where it differs from the .NET workspace

| Subject | .NET | TypeScript |
| --- | --- | --- |
| A push or an invoke that reports errors | Returns the result, with `HasErrors` | Throws a `ResultError`, whose `result` holds the errors |
| A pull to a custom route | `CallAsync(args, name)`, on the Remote adapter | Not available |
| The arguments of a pull | Not sent by the Remote adapter | Sent |
| The dependencies of the rules that a session activated | Not sent | Sent with each pull |
| A text for the tracing of the database | None | `session.context`, sent with each pull, sync, push and invoke |
| A role that holds an object that is not in the session | Throws | Takes the object from the connection's records when another session pulled it; otherwise throws an `Error` with the message `Object is not present in session.` |
| Deleting an object | Not available | `strategy.delete()`, for a new object only; an object of the database throws |
| The roles of a derivation error | Each has an object and a relation type | `roles` reads each relation as a pair, `r[0]` and `r[1]`, while the JSON API sends an object with `i` and `r`, so each role has no object and no relation type |

## Where this is implemented

- The libraries are in `typescript/modules/libs`, the templates in `typescript/modules/templates`,
  and the generator that renders them for Core in `dotnet/Core/Database/Generate/Program.cs`.
- The session of the adapter is
  `typescript/modules/libs/system/workspace/adapters-json/src/lib/session/session.ts`, which throws
  the `ResultError` of a push or an invoke. The roles of a derivation error are read in
  `adapters-json/src/lib/database/derivation-error.ts`, and a pull is turned into JSON in
  `adapters-json/src/lib/json/to-json.ts`.
- `typescript/modules/libs/system/workspace/adapters-json-tests` runs the tests against the Core
  test server, with `FetchClient` and the fixture in its `src/lib`;
  [Build and test](internals/build-and-test.md#the-typescript-workspace) says how to run them. The
  libraries `system/workspace/adapters-tests`, `domain-tests`, `meta-tests` and `meta-json-tests`
  run without a server. No test reads the roles of a derivation error.
