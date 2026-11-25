---
title: "The JSON API"
description: "The endpoints of an application's server: sign-in, pull, sync, push, invoke, access and permission, with their requests, responses, errors and checks."
audience: "Users"
kind: "Reference"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

Core's JSON API is how a client reads and changes the objects of the database over HTTP. The
Remote workspace adapter of .NET and [the TypeScript workspace](typescript-workspace.md) call it,
and so can any HTTP client; [The workspace](workspace.md) explains the workspaces. This page lists
its endpoints and the fields of their requests and responses. The data of a pull, its extents,
predicates, selects and includes, follows the classes in
`dotnet/System/Protocol/Allors.Protocol.Json/Data`; [Extents](extents.md) explains what an extent
means, and [Pulls](pulls.md) describes a pull of the .NET workspace. [Authentication](authentication.md)
explains the tokens.

## How a workspace uses the endpoints

A workspace reads in four steps, and asks each step only for what it does not hold yet:

1. **Pull** names the objects that a query finds, with their versions, and the grants and the
   revocations that apply to them.
2. **Sync** sends the roles of the objects whose version, grants or revocations the workspace does
   not hold.
3. **Access** sends the permissions of the grants and the revocations that the workspace does not
   hold yet.
4. **Permission** says what each permission that the workspace does not know allows.

**Push** writes the changes of a session, and **invoke** runs methods. To see their result, such
as the roles that a derivation changed, a client pulls again.

## Conventions

| Endpoint | What it does | Token | Commits |
| --- | --- | --- | --- |
| `/allors/Authentication/Token` | [Signs a user in](#sign-in). | Not needed | The lockout state of the user |
| `/allors/pull` | [Finds objects](#pull). | Required | No |
| `/allors/sync` | [Sends the roles of objects](#sync). | Required | No |
| `/allors/push` | [Creates and changes objects](#push). | Required | When nothing failed |
| `/allors/invoke` | [Runs methods](#invoke). | Required | When nothing failed; with the option `i`, each invocation that succeeds |
| `/allors/access` | [Sends the permissions of grants and revocations](#access). | Required | No |
| `/allors/permission` | [Describes permissions](#permission). | Required | No |

Every endpoint takes `POST` with a JSON body and the header `Content-Type: application/json`, and
answers with JSON. Most fields have names of one or two letters. A field that a request leaves out
counts as having no value, a field that the server does not know is ignored, and a response writes
a field without a value as `null`. A pull, a sync, a push and an invoke also take `x`, a text that
the server passes on to the tracing of the database, if it has one.

### The workspace

The server chooses the workspace of a request; the sample's server uses the workspace `Default` for
every request. The workspace decides which classes, roles and methods the request reaches: those
that [`[Workspace]`](model.md#attributes) makes available to a workspace of that name. A pull or a
sync leaves out an object of a class that the workspace does not include, and a push or an invoke
reports an access error for it.

### Ids, versions and tags

- **Ids.** An object is named by its id, a positive number. In a push, a new object has a
  negative id that the client chooses, its workspace id.
- **Versions.** Every object has a version, a number that changes when a transaction commits a
  change to the object. A push and an invoke send the version that the client knows, and fail for
  an object whose version has changed since.
- **Tags.** The API names a class, an interface, a relation type or a method type by its tag, a
  string of 22 characters that follows from the id in the model. In the generated metadata it is
  `Tag`, as in `m.WorkItem.Tag`. A role is named by the tag of its relation type. These are tags of
  the sample:

| Type or role | Tag |
| --- | --- |
| `Person` | `rCGfVoriWEGMB5WIorfZBw` |
| `WorkItem` | `J654jjmMuUuJxeW08nUHwQ` |
| `WorkItem.Title` | `YaiSaWlrFUeXOjZic2dL-g` |
| `WorkItem.Assignee` | `fCR-6Tnm2kqNbQW-xKJLQg` |
| `WorkItem.Done` | `QkIQHBg-mEic5x3qI3Ty6Q` |

- **Lists of ids.** The grants and the revocations of an object, and the permissions of a grant or
  a revocation, are sorted lists of ids, and `null` when they are empty.

### Values

A unit role is sent as a JSON value of its unit:

| Unit | Value |
| --- | --- |
| `String` | A string. |
| `Integer`, `Float` | A number. |
| `Decimal` | A string with the number, such as `"12.50"`. A push also accepts a number. |
| `Boolean` | `true` or `false`. |
| `DateTime` | A string in ISO 8601. |
| `Unique` | A string with the GUID. |
| `Binary` | A string in Base64. |

In a push, `null` clears a unit role.

### Errors

The responses of pull, push and invoke report errors in five fields, and `hasErrors` is true when
one of them has a value:

| Field | Holds |
| --- | --- |
| `_e` | A message, such as the message of an exception that an invoked method threw. |
| `_v` | The ids of objects whose version has changed. |
| `_a` | The ids of objects that the request may not reach or change: of a class outside the workspace, or a role or a method that the user lacks the permission for. A new object is named by its workspace id. |
| `_m` | The ids of objects that do not exist. |
| `_d` | The errors of the derivation, each with its message in `m` and in `r` the relations that it concerns: the object's id in `i` and the tag of the relation type in `r`. For a new object, `i` is an id that the push gave it and then discarded, not its workspace id. |

The server answers with an HTTP status in these cases:

| Status | When |
| --- | --- |
| 400 | The host name is not in `AllowedHosts`, or an endpoint that needs a token cannot deserialize the JSON body into its request type. |
| 401 | The request has no valid token; see [Authentication](authentication.md#a-request-step-by-step). |
| 405, 415 | The method is not `POST`, or the content type is not JSON. |
| 500 | An exception, including a failure to convert a unit value in a push, such as an invalid `DateTime` string in `u`. The body is a JSON string: `"An internal server error has occurred."`, or, in the environment `Development`, the message and the stack trace. The log holds the exception. |

### Transactions and retries

Every request runs in a transaction of its own, which ends with a rollback unless the endpoint
committed it. When the database reports an error, a `DbException`, the server runs a pull, a sync,
an access or a permission request again, up to five times, after 2, 4, 8, 16 and 32 seconds. It
runs a push or an invoke only once: a second run after a commit whose outcome is unknown could
apply the changes twice.

## Sign in

`POST /allors/Authentication/Token`, without a token.

| Request | Holds |
| --- | --- |
| `l` | The user name. |
| `p` | The password. |

| Response | Holds |
| --- | --- |
| `a` | `true`. |
| `u` | The id of the user, as a string. |
| `t` | The token. |

When the sign-in fails the response is `{"authenticated":false}`, with HTTP 200, and so it is for a
body that is not valid JSON. A body without `l`, or without `p` for a user who has a password, gets
HTTP 500.
[Signing in](authentication.md#signing-in) explains the check and the lockout.

## Pull

`POST /allors/pull`. A pull finds objects and names them; it returns no roles.

| Request | Holds |
| --- | --- |
| `l` | The pulls, each of which finds objects. |
| `p` | A procedure to run first: `n` its name, and its input in `c`, `o`, `v` and `p`. |
| `d` | Dependencies: for a type, by its tag in `o`, the role (`r`) or the association (`a`) along which the pull adds objects to the response. The pull follows a dependency whether or not the user may read the role, and follows it again from the objects that it adds. |

A pull in `l` finds the objects of an extent or one object:

| Field | Holds |
| --- | --- |
| `e` | An extent: `k` its kind, 1 for a filter, 2 a union, 3 an intersection and 4 a difference; `t` the tag of its class or interface; `p` a predicate; `o` the operands of a union, an intersection or a difference; and `s` the sorting. |
| `er` | The id of a prepared extent, instead of `e`. |
| `o` | The id of an object, instead of an extent. |
| `t` | With `o`: the tag of a type. The pull finds nothing when the object is not of that type. |
| `r` | The results, each with an optional name in `n`, a select in `s` or the id of a prepared select in `r`. On an extent, `k` skips and `t` takes objects before selection. A pull by object id ignores both. |
| `a` | The arguments of the predicates, by the names of their parameters. |

Without a select, put the include in the result's `i`. With a select, put it in `i` on the
select's final node. A prepared select uses its own include.

| Response | Holds |
| --- | --- |
| `c` | The collections, by name: the ids of the objects of each. |
| `o` | The objects, by name: an id each. |
| `v` | The values, by name, that a procedure returns. |
| `p` | The objects to check for sync, each with `i` its id, `v` its version, `g` its grants that apply to the user and `r` its revocations. |
| `g` | The grants of those objects, each as a pair of its id and its version. |
| `r` | The revocations of those objects, in the same form. |

A result without a name takes the plural name of its type, such as `WorkItems`, or of the role or
the association that its select ends with; one object takes the singular name. A pull leaves out
the objects of a class outside the workspace and the objects that a mask hides; it names every
other object that it finds, even when the user may read none of its roles. A select and an include
follow a role only when the user may read it, and follow an association without a check. An id in
`o` that does not exist gets HTTP 500.

Give collection results distinct names when batching pulls. If a later result appends to an
existing collection without an include, its additional ids appear in `c` but can be absent from
`p`. A workspace that uses `p` to sync then misses those objects.

A procedure runs an `IProcedure` of the database by its name, after checking the versions of the
objects that it names in `p`; an unknown name gives `Missing procedure <name>` in `_e`.

A worker pulls the objects of `WorkItem`:

```json
{ "l": [ { "e": { "k": 1, "t": "J654jjmMuUuJxeW08nUHwQ" } } ] }
```

```json
{
  "c": { "WorkItems": [248] }, "o": {}, "v": {},
  "p": [ { "i": 248, "v": 3, "g": [15], "r": null } ],
  "g": [ [15, 4] ], "r": null,
  "hasErrors": false, "_e": null, "_v": null, "_a": null, "_m": null, "_d": null
}
```

## Sync

`POST /allors/sync`. A sync sends the roles of objects.

| Request | Holds |
| --- | --- |
| `o` | The ids of the objects. |

| Response | Holds |
| --- | --- |
| `o` | The objects, each with `i` its id, `v` its version, `c` the tag of its class, `g` its grants that apply to the user, `r` its revocations and `ro` its roles. |

A role in `ro` holds `t`, the tag of its relation type, and its value: a unit in `v`, the id of an
object in `o`, or the ids of objects in `c`. A sync sends the roles that the user may read and
that hold a value in the database. It leaves out an id that does not exist, an object of a class
outside the workspace and an object that a mask hides, and does not name them in a role either: a
role that holds only such an object comes without a value.

```json
{ "o": [248] }
```

```json
{
  "o": [
    {
      "i": 248, "v": 3, "c": "J654jjmMuUuJxeW08nUHwQ", "g": [15], "r": null,
      "ro": [
        { "t": "YaiSaWlrFUeXOjZic2dL-g", "c": null, "o": null, "v": "Write the guide" },
        { "t": "fCR-6Tnm2kqNbQW-xKJLQg", "c": null, "o": 240, "v": null },
        { "t": "QkIQHBg-mEic5x3qI3Ty6Q", "c": null, "o": null, "v": false }
      ]
    }
  ]
}
```

## Push

`POST /allors/push`. A push creates objects and changes their roles.

| Request | Holds |
| --- | --- |
| `n` | The new objects, each with `w` its workspace id, `t` the tag of its class and `r` its roles. |
| `o` | The objects that change, each with `d` its id, `v` the version that the client knows and `r` its roles. |

A role holds `t`, the tag of its relation type, and one of these:

| Field | Holds |
| --- | --- |
| `u` | The value of a unit role. |
| `c` | The id of the object of a composite role that holds one object. Without a value it clears the role. |
| `a`, `r` | The ids of the objects to add to, and to remove from, a composite role that holds several objects. |

A role can name a new object of the same push by its workspace id.

| Response | Holds |
| --- | --- |
| `n` | After a commit: for each new object, `w` its workspace id and `d` its id. |

The push checks in this order, and reports what fails in the [error fields](#errors):

1. **The new objects.** A tag that names no type, or a class outside the workspace, is an access
   error, and the push stops. The tag of an interface or of a relation type, and a workspace id
   that two new objects share, get HTTP 500. No permission to create is checked: a push creates an
   object of every class that the workspace includes; issue
   [#430](https://github.com/allors/allors3/issues/430) records this.
2. **The objects that change.** An id that does not exist is a missing error. A version that has
   changed is a version error, and an object of a class outside the workspace is an access error.
3. **The roles.** A role that the user may not write is an access error for its object. A new
   object without security tokens of its own takes the grants of the initial `SecurityToken`, as
   [Creating objects](security.md#creating-objects) explains. After an error with an object that
   changes, or with one of its roles, the push writes no role of the new objects.
4. **The derivation.** The push derives, and reports the errors of the derivation. After an error
   in step 2 or 3 these can include a required role that the request sets on a new object.
5. **The commit.** When nothing failed, the push commits and returns the ids of the new objects.
   Otherwise it stores nothing.

A role that the user may not write, on an object that exists, gets HTTP 500 instead of an access
error when the push creates no object: issue [#441](https://github.com/allors/allors3/issues/441)
records this. A role whose tag names no relation type gets HTTP 500 as well.

An administrator creates a work item and assigns it to the person with id 240:

```json
{
  "n": [
    {
      "w": -1, "t": "J654jjmMuUuJxeW08nUHwQ",
      "r": [
        { "t": "YaiSaWlrFUeXOjZic2dL-g", "u": "Write the guide" },
        { "t": "fCR-6Tnm2kqNbQW-xKJLQg", "c": 240 }
      ]
    }
  ]
}
```

```json
{
  "n": [ { "w": -1, "d": 248 } ],
  "hasErrors": false, "_e": null, "_v": null, "_a": null, "_m": null, "_d": null
}
```

A worker marks it done, with the version from a sync:

```json
{ "o": [ { "d": 248, "v": 3, "r": [ { "t": "QkIQHBg-mEic5x3qI3Ty6Q", "u": true } ] } ] }
```

The same push again fails with `"_v": [248]`, because the first one changed the version.

## Invoke

`POST /allors/invoke`. An invoke runs methods of objects.

| Request | Holds |
| --- | --- |
| `l` | The invocations, each with `i` the id of the object, `v` the version that the client knows and `m` the tag of the method type. |
| `o` | The options: `i` true to run each invocation in a transaction of its own and, with `i`, `c` true to continue after an invocation that fails. |

The response holds only the [error fields](#errors). For each invocation the invoke reports an id
that does not exist, an object of a class outside the workspace, a version that has changed, and a
method that the user may not execute. It then runs the method and derives. The message of an
exception that the method throws goes to `_e`, and the errors of the derivation to `_d`.

Without `i`, the invocations share one transaction: the invoke stops at the first that fails and
rolls back all of them, or commits them together. With `i`, each invocation commits or rolls back
on its own, and the invoke stops at the first that fails unless `c` is true. A method of the class
that only another workspace includes is an access error. An invocation without an id, a version
or a method, or with a tag that names no method that a workspace includes for the class, gets
HTTP 500.

## Access

`POST /allors/access`. Access sends the permissions of grants and revocations.

| Request | Holds |
| --- | --- |
| `g` | The ids of grants. |
| `r` | The ids of revocations. |

| Response | Holds |
| --- | --- |
| `g` | The grants among them that apply to the user, each with `i` its id, `v` its version and `p` its permissions in the workspace. |
| `r` | The revocations, each with `i` its id, `v` its version and `p` the permissions in the workspace that it denies. |

A grant whose users do not include the user, and an id that does not exist, are left out. An id
in `g` of an object that is not a grant, or in `r` of an object that is not a revocation, gets
HTTP 500.

## Permission

`POST /allors/permission`. Permission says what permissions allow.

| Request | Holds |
| --- | --- |
| `p` | The ids of permissions. |

| Response | Holds |
| --- | --- |
| `p` | The permissions of the classes in the workspace, each with `i` its id, `c` the tag of its class, `t` the tag of its relation type or its method type, and `o` its operation: 2 to read, 4 to write and 8 to execute. |

The id of a permission to create for a class in the workspace, or of an object that is not a
permission, gets HTTP 500.

## Where this is implemented

- The endpoints are in `dotnet/Core/Database/Server/Core/Api/Json`: one controller for each, with
  its route and the requirement of a token, and `Api.cs`, which hands a request to the builder of
  its response.
  `PolicyService`, in `Services/Policy`, holds the retries; `AuthenticationController`, in
  `Identity`, is the sign-in endpoint; and `ExceptionHandler.cs` writes the response of an error.
- The responses are built in `dotnet/System/Database/Allors.Database.Workspace.Json`, in a folder
  for each endpoint. The requests and the responses are classes of
  `dotnet/System/Protocol/Allors.Protocol.Json`, in `Api`, `Auth` and `Data`;
  `Allors.Protocol.Json.SystemText/UnitConvert.cs` converts the values. `GuidExtensions.Tag`, in
  `dotnet/System/Shared`, makes a tag of an id.
- The Remote workspace adapter of .NET is in
  `dotnet/System/Workspace/Adapters/Allors.Workspace.Adapters.Remote`; its `Session.OnPull` takes
  the steps of a read.
- `dotnet/Core/Database/Server.Local.Tests/Json` holds tests of pull, sync, push, invoke and access,
  in the folders `Pull`, `Sync`, `Push`, `Invoke`, `Security` and `Tracing`. `PolicyServiceTests`
  and `PullRetryTests` check the retries of a pull, a push and an invoke, and
  `ExceptionHandlerTests` the body of an error. In `sample/Database/Server.Tests`, `WorkItemTests`
  pulls, syncs and pushes over HTTP. `SignInTests` checks that every endpoint refuses a missing or
  an invalid token, and that the permission endpoint refuses a token of another issuer or audience.
- The TypeScript workspace tests in
  `typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/runtime/security.spec.ts`
  exercise permission requests during concurrent pulls. The server tests do not check the fields
  of a permission response, the retries of a sync, an access or a permission request, or the
  requests described above that end with HTTP 500.
