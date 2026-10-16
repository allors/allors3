# Workspace read protocol

> **Status: Current.** Ownership map for read messages and validation.

[Complete workspace reads](../workspace-reads.md) is the application reference. This page maps the
wire boundary for maintainers; applications must not rely on it. Server execution is mapped in
[read foundations](workspace-read-foundations.md).

## Code ownership

| Owner | Responsibility |
| --- | --- |
| [.NET DTOs](../../dotnet/System/Protocol/Allors.Protocol.Json/Api/Read/ReadRequest.cs) and [TypeScript DTOs](../../typescript/modules/libs/system/common/protocol-json/src/lib/api/read/read-request.ts) | Define matching fields and separate discovery, complete and failure responses. TypeScript unions check payload alternatives at compile time. Neither runtime's types validate incoming data. |
| [.NET validation](../../dotnet/System/Protocol/Allors.Protocol.Json/Api/Read/ReadValidation.cs) | Check message structure, request correlation and table references independently of either serializer. |
| [System.Text.Json](../../dotnet/System/Protocol/Allors.Protocol.Json.SystemText/ReadJson.cs) and [Newtonsoft.Json](../../dotnet/System/Protocol/Allors.Protocol.Json.Newtonsoft/ReadJson.cs) decoders | Parse strictly, validate before DTO conversion can discard fields, then materialize the DTO. |
| [TypeScript decoder and validation](../../typescript/modules/libs/system/common/protocol-json/src/lib/api/read/read-validation.ts) | Apply the same wire checks. JSON.parse checks grammar; a token scan preserves duplicate-key and numeric-spelling evidence that parsing discards. |
| [Shared messages](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/protocol/read-contract.json) and [validation cases](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/protocol/read-validation-cases.json) | Keep both runtimes and serializers aligned. .NET embeds these same files as test resources. |

DTO comments and the shared messages define compact field names. Legacy Pull, Sync, Access and
Permission messages have separate entrypoints.

## Validation boundaries

Use strict decoding at a raw JSON boundary. A validator receiving an already parsed tree cannot
recover duplicate keys or precision lost during parsing. Response decoding also validates the
original request: reuse indexes refer to that request's tables, not durable IDs. Index zero, an
empty full body, and an absent payload remain distinct.

Wire validation checks scope consistency, not authentication. Record reuse must match class,
version and security provenance: the grant/revocation kind, ID and version attached to the record.
A security reuse claim also requires its body-possession flag. These checks cannot prove that a
client holds the advertised body; the [store](workspace-read-store.md) owns possession.

The validator checks the security and permission entries visible in the response. With reused
security bodies it cannot see every required permission. The [coordinator](workspace-read-coordinator.md)
resolves the held bodies and checks the complete closure—all security bodies referenced by the
records and every permission they require—before publication. Metadata validity, declared output
names and query membership require the coordinator or server, not just wire shape checks.

## PushRead messages

The [server reference](../workspace-reads.md#push-with-immediate-server-refresh) defines the public
contract. [PushRead DTOs](../../dotnet/System/Protocol/Allors.Protocol.Json/Api/PushRead/PushReadRequest.cs)
reuse existing Push payloads, ID mappings and full Read bodies, with separate mutation, changed and
discovery outcomes. [PushRead validation](../../dotnet/System/Protocol/Allors.Protocol.Json/Api/Read/ReadValidation.PushRead.cs)
checks the strict envelope and delegates read bodies to existing validation. Ordinary scoped Push
maps its public Refresh option explicitly to `returnChanged`; the wire's omitted/null default stays
false. The internal PushRead route and DTOs retain their names.

Both serializers expose `PushReadJson.DeserializeReceipt` and `DeserializeResponse`. Receipt parsing
validates correlation, admitted scope, mutation outcome and complete new-ID mappings independently
of read payload validation. Read success can never be fabricated by receipt-only parsing. The
TypeScript decoders enforce the same separation. Parsing still requires a complete JSON envelope.

Scoped client transports keep the raw response until receipt and selected-part decoding finish.
Each selected read part retains strict duplicate-field and shape validation independently of the
other part. Client receipt application belongs to [Session state](workspace-session-state.md),
and affected-record publication belongs to [read coordination](workspace-read-coordinator.md).

`PushReadProtocol*` cases in the existing .NET PullTests and the TypeScript protocol spec share
[PushRead messages](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/protocol/push-read-contract.json)
and exercise accepted/rejected shapes, mapping completeness, negative submitted IDs in diagnostics,
full affected bodies and receipt preservation when a read payload is malformed. The same shared
validation corpus checks that derivation messages preserve absent/null/empty/whitespace text, while
relation arrays remain required (empty is valid). Numeric messages and absent/null relation arrays
are rejected. Client result adaptation supplies actionable fallback text separately from raw data.

## InvokeRead messages

The [Invoke reference](../workspace-reads.md#invoke-and-refresh-a-scoped-session) owns application
behavior. [InvokeRead DTOs](../../dotnet/System/Protocol/Allors.Protocol.Json/Api/InvokeRead/InvokeReadRequest.cs)
and [validation](../../dotnet/System/Protocol/Allors.Protocol.Json/Api/Read/ReadValidation.InvokeRead.cs)
define the independent receipt and affected read. Public refresh defaults are resolved before
serialization: the wire requires a Boolean. Invocation options, mappings and query discovery have
no fields in this message.

Both serializers expose `InvokeReadJson.DeserializeRequest`, `DeserializeReceipt`,
`DeserializeChanged` and `DeserializeResponse`; TypeScript uses matching strict decoders. Receipt
validation requires complete bounded JSON, matching correlation, and exact scope whenever supplied;
commitment requires scope. It retains mutation diagnostics independently of affected validation.
Successful affected data must match scope and full read closure. A validated `ReadFailure` can report
a changed scope; malformed successful data cannot establish retirement. Client application belongs
to [Session state](workspace-session-state.md#scoped-invoke-admission).

`InvokeReadProtocolSerializesIndependentDtoShape` and
`InvokeReadProtocolValidatesSharedFixturesAndIndependentReceipts` consume the same
[contract](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/protocol/invoke-read-contract.json)
and [validation cases](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/protocol/invoke-read-validation-cases.json)
as TypeScript. They cover required refresh, duplicate/unknown fields, exact scope and correlation,
signed nonzero derivation relations, repeated-target versions, and valid receipts paired with invalid
read payloads. The existing Read and PushRead fixture contracts stay separate.

## Authoritative tests

The [.NET PullTests](../../dotnet/Core/Workspace/Tests.Local/Tests/PullTests.cs) and
[TypeScript protocol spec](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/protocol/read-contract.spec.ts)
exercise the same fixtures. These tests, rather than a second field-by-field specification here,
own the validation rules.

| Rule | .NET test | TypeScript test |
| --- | --- | --- |
| Shared representation | `ReadProtocolMatchesSharedFixture` | `serializes independently constructed %s to the shared fixture` |
| Optional fields and empty/full/reused distinctions | `ReadProtocolDecodesOmittedOptionalFields` | `also accepts omitted optional fields without inventing null members`; `preserves empty full bodies and zero reuse indexes as distinct alternatives` |
| Separate response types and declarative selection shape | `ReadProtocolRootsExcludeLegacyResponseMembers` | `narrows response alternatives and rejects incompatible typed payloads` |
| Accepted and rejected wire cases, including obsolete complete-result `v` | `ReadValidationMatchesSharedCases` | `shared read protocol validation cases` |
| No standalone result-value API | `PublicContractsDoNotExposeStandaloneResultValues` | `exposes no standalone result-value API on wire adaptation or normal pull results` in the [response-context spec](../../typescript/modules/libs/system/workspace/adapters-json-tests/src/lib/database/security/response-context.spec.ts) |
| Legacy DTOs ignore and do not re-emit obsolete result values | `LegacyPullResponsesIgnoreRemovedResultValues` | The result API absence test also supplies an old raw value map. |
| Raw JSON syntax | `ReadValidationRejectsMalformedJson` | `read JSON lexical validation` |
| Fractions that would round to integers | `ReadValidationRejectsNumbersRoundedToIntegers` | `rejects a fractional version that JavaScript rounds into an integer: %s` |

[Server PullRetryTests](../../dotnet/Core/Database/Server.Local.Tests/Json/Pull/PullRetryTests.cs)
`LegacyPullDoesNotExecuteRemovedProcedurePayload` and
`ReadEntrypointsReturnTheSameDetachedCompleteResult` verify root-field absence on actual producer
responses through the selected serializers. Ordinary unit-role encoding remains covered by
`ReadProtocolMatchesSharedFixture` and the shared record bodies.

The TypeScript `read JSON nesting limit`, `read JSON Unicode preservation` and
`read JSON source Unicode preservation` groups cover depth and surrogate handling.
