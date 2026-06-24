# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
(`3.1.0-alpha.{height}`), so the version auto-increments with each commit on `main`.
Changes accumulate under **[Unreleased]** until a version is cut, at which point they move
under a dated version heading.

## [Unreleased]

<!-- Add entries under one of: Added, Changed, Deprecated, Removed, Fixed, Security -->

### Added

- `WorkRequirement.WorkEffortPurpose` (enumeration Refurbishment / Maintenance / Repair): defaults to
  **Repair** on init, is copied onto the `WorkTask` created by `CreateWorkTask`, and is mirrored into
  `WorkRequirementVersion` for version history.
- New **Identity** domain track at `dotnet/Identity`: an orthogonal domain that extends `Core` and
  encapsulates all Microsoft ASP.NET Identity integration (identity properties on `User`, `Login`,
  `UserPasswordReset`, normalization/password rules, `PasswordHasher`, `AllorsUserStore`/`AllorsRoleStore`,
  `AuthenticationController`). It is a full level with its own Custom domain, Server, Commands, database
  (`config/<provider>/identity`), Domain/Server.Remote tests, dotnet Workspace projects and TypeScript libs
  (`@allors/identity/workspace/*`).
- The repository `[Extends]` attribute now supports multiple domain inheritance; each level's `Custom`
  domain extends both its functional domain and `Identity`.
- **Workspace signals** — reactive `state`/`computed`/`effect` primitives backported from allors4 core, as the
  standard reactive mechanism of the C# workspace. New netstandard2.0 projects `Allors.Workspace.Signals`
  (the `ISignal`/`IStateSignal`/`IComputedSignal`/`IEffect`/`IEffectScope`/`ISignalFactory` contract),
  `Allors.Workspace.Signals.Default` (the push-pull dependency-tracking engine) and
  `Allors.Workspace.Signals.Plain` (a non-reactive fallback), with xUnit tests gated by the new Nuke target
  `DotnetSystemWorkspaceSignalsTest` (CI `CiDotnetSystemWorkspaceSignalsTest`).
- Configuration is now delivered from outside the source tree via the **required** `ALLORS_CONFIG_ROOT`
  environment variable. Each server, command-line tool and integration test loads
  `$ALLORS_CONFIG_ROOT/<domain>/appsettings.json` (domain = `core`/`base`/`apps`) through the new
  `IConfigurationBuilder.AddAllorsConfiguration(domain, …)` helper, with environment variables layered
  last so they override the JSON. A missing variable or missing file fails fast with an actionable error
  instead of silently falling back to per-OS defaults.
- `InstallConfig` Nuke target copies a provider's templates into the config root, e.g.
  `./build.sh InstallConfig --provider npgsql --config-root /opt/allors`.
- Test databases are created on demand from an admin connection: `ALLORS_NPGSQL` / `ALLORS_SQLCLIENT`
  (matching the allors4 CI names) hold a connection allowed to create databases, and each test's connection
  string is derived from them by swapping the database name. The shared `DatabaseProvisioning` helper (over
  the provider-specific `Provisioning` types) drops/creates the database, and the in-process tests (the
  static adapter tests and the Core/Base `Server.Local.Tests`) self-provision a per-test-class database — so
  `dotnet test` runs against the containers with no pre-existing database and no SQL LocalDB. The legacy
  `ALLORS_TEST_SQLCLIENT_CONNECTION` / `ALLORS_TEST_NPGSQL_CONNECTION` names are still accepted as aliases;
  if neither is set the helper fails fast with an actionable error.
- `Commands Init` (Core/Base/Apps) drops and (re)creates the configured database from the admin connection,
  giving the out-of-process server tests and e2e flows a cross-platform provisioning step.
- `launchSettings.json` profiles for the Core/Base/Apps servers that select the database provider
  (e.g. *Core (Postgres)* / *Core (SqlClient)*) by setting `ALLORS_CONFIG_ROOT`.
- A `Merge.Tests` project for the resource `Merger` (`Core/Database/Merge`), driven black-box through its
  public `Input`/`Output` API. It covers structure preservation for overlapping keys (the regression that
  motivated the resx-merge fix — `<value>`/`<comment>` children and `xml:space`/`type` attributes survive),
  last-writer-wins precedence across input directories, key union, `<xsd:schema>`/`<resheader>` survival, and
  `Input` robustness (missing directories skipped, non-`.resx` files ignored, case-insensitive extension). The
  project is wired into the `DotnetCoreDatabaseTest` target (CI `CiDotnetCoreDatabaseTest`) so it actually gates.
- Comprehensive regression coverage for object versioning in `VersioningTests` (Base) — 16 tests over the
  `Order`/`OrderLine` model: changed vs unchanged unit / single-composite / many-composite roles;
  add / remove / clear / repopulate and order-independent set comparison on the many-role; version-snapshot
  history (each version keeps the value at its own derivation); sequential versions; several versioned roles
  changing in one cycle producing a single new version; independent child (`OrderLine`) versioning; and
  non-versioned roles not creating versions. Run by `CiDotnetBaseDatabaseTest`.

### Changed

- CI now triggers on pushes to `main`/`v*` and on all pull requests (any head branch),
  instead of pushes to `feature/**`/`issue/**`. Branch names no longer gate CI, and the
  duplicate push+PR run for feature branches is eliminated. Use a draft PR or
  `workflow_dispatch` for pre-PR CI.
- `Core`, `Base` and `Apps` are pure functional domains without a dependency on ASP.NET Identity:
  Core's `User` keeps only `UserName`; the ASP.NET-shaped members moved to the Identity domain
  (relation ids unchanged, so database schemas are unaffected). The identity sources are composed back
  into each level via `Identity*` source globs because every `Custom` domain extends `Identity`.
- `SignInTests`, `LockoutTests` and `LoginTests` moved from the Core level to the Identity level.
- The Blazor workspace UI is migrated to the signal-based workspace surface. `RoleField`, `CustomValidator`
  and the Bootstrap role controls use the signal accessors (`CanRead`/`CanWrite(…).Value`; `ScalarRole`/
  `CompositeRole`/`CompositesRole` dispatched on the role's kind, mirroring the removed `GetRole`/`SetRole`;
  `ExistRole` for the validators), the Bootstrap sites' workspace configurations pass a `DefaultSignalFactory`
  builder instead of the removed `IRule[]`, and the site pages and image services read generated role signals
  with `.Value`.
- The C# workspace is now **signal-based** (allors4 parity). `IStrategy`/`ISession` expose signal-returning
  members — `ISignal<…> Version/IsNew/HasChanges`, `ISignal<bool> CanRead/CanWrite/CanExecute/ExistRole/HasChanged`,
  and `IRoleSignal<T>`/`ICompositesRoleSignal<T>`/`IDerivedRoleSignal<T>`/`IAssociationSignal<T>`/`IMethodSignal`
  accessors — and generated workspace domain classes expose those signals (read with `.Value`, write with
  `.Set(…)`). `IConfiguration` now requires a signal-factory builder (`Func<ISignalFactory>`, e.g.
  `() => new DefaultSignalFactory()`; `Allors.Workspace.Adapters.Configuration` takes it instead of `IRule[]`).
  The builder runs once per session: every `ISession` owns its signal factory (exposed as
  `ISession.SignalFactory` — create UI effects there), keeping each session's reactive graph and its
  single-threaded effect scheduler isolated from sessions on other threads (e.g. Blazor Server circuits).
  Internally every signal recomputes off a session-wide graph revision bumped on role writes, pulls and
  pushes. Workspace test suites were migrated to the signal surface accordingly. (TypeScript/Angular is a
  later pass.)
- The database provider (SqlClient / Npgsql) is now selected by which template populates
  `ALLORS_CONFIG_ROOT` rather than by the host operating system. The in-repo `config/<provider>/<domain>`
  files are templates (with development defaults) copied to the config root (e.g. `/opt/allors`); real
  secrets belong in the deployed copy or in environment variables, not in source.
- Configuration file names are normalized to lowercase `appsettings.json` so the same files resolve on
  case-sensitive filesystems (Linux); previously the loader looked for `appSettings.json`.
- The SqlClient adapter's `LIKE` filter now follows ANSI semantics like the Npgsql and in-memory adapters:
  `%` and `_` are the only wildcards and `[` is matched literally. Previously T-SQL character classes
  (e.g. `[abc]`, `[a-z]`, `[^…]`) were active only on SqlClient, so the same `LIKE` pattern could match
  differently across adapters. Patterns that relied on SqlClient char-classes no longer match as classes
  (that behaviour was never portable to Npgsql/Memory).
- Database provisioning for the Nuke test/e2e targets now runs cross-platform via `Commands Init` against the
  Postgres/SQL Server containers instead of the Windows-only SQL LocalDB step; `--provider` selects both the
  admin connection and the derived `ConnectionStrings__DefaultConnection` passed to the child processes.
- The Npgsql legacy `AppContext` switches (`EnableLegacyTimestampBehavior`, `EnableStoredProcedureCompatMode`)
  are now set by a module initializer in the Npgsql adapter, so the server, command-line tools and tests get
  the same behaviour (previously only the adapter test fixture set them).
- The Core `Server.Local.Tests` / `Server.Remote.Tests` build their database through the adapter-aware
  `DatabaseBuilder` instead of a hardcoded SqlClient type, so they honour the configured provider.

### Removed

- The non-functional extranet e2e build harness: `build/Typescript/E2E/AppsExtranet/{Build.cs,Paths.cs}` and
  the `CiTypescriptWorkspacesE2EAngularAppsExtranetTest` Nuke target. It pointed at a
  `typescript/e2e/AppsExtranet/Angular.Tests` project that does not exist, served the *intranet* app, and was
  never wired into `ci.yml`, so it could not run. There are no extranet e2e tests; standing up a real extranet
  e2e suite + gate is tracked as a follow-up.
- The workspace client-side `IRule` derivation engine (`ISession.Activate`, `IConfiguration.Rules`, the
  `Allors.Workspace.Derivations` namespace and the workspace `Rule` base class). Derived workspace roles are
  now exposed as `IDerivedRoleSignal<T>`; their values are computed server-side and arrive via pull (matching
  allors4). The separate database-side derivation engine is unaffected.
- The Windows-only `SqlLocalDB` build helper and the `MartinCostello.SqlLocalDb` build dependency; the build
  no longer provisions SQL LocalDB.
- Legacy per-project `appSettings.json` and `appSettings.{development,osx,windows}.json` files next to the
  servers, command-line tools and server tests. Configuration now comes solely from `ALLORS_CONFIG_ROOT`.

### Fixed

- `WorkEffortTotalOtherRevenueRule` summed non-discount work-effort invoice items' amounts into `TotalOtherRevenue`
  (partitioning on `InvoiceItemType.IsDiscount`) but didn't watch `WorkEffortInvoiceItem.InvoiceItemType`, so
  changing an item's invoice-item type left `TotalOtherRevenue` stale. It now also watches `InvoiceItemType`.
- `PurchaseInvoiceAmountPaidRule` summed each payment application's `AmountApplied` into `AmountPaid` but watched
  only the application *set* (an association pattern), so editing an existing application's `AmountApplied` left
  `AmountPaid` stale. It now also watches `PaymentApplication.AmountApplied` (mirroring `SalesInvoiceStateRule`).
- `AccountingTransactionDeniedPermissionRule` toggles the delete revocation from `IsDeletable`
  (`InternalOrganisation.ExportAccounting && !Exported`) but watched only `Exported`, so toggling the internal
  organisation's `ExportAccounting` left the delete permission stale. It now also watches
  `InternalOrganisation.ExportAccounting`.
- `PurchaseInvoicePriceRule` accumulated `TotalFee` and `TotalShippingAndHandling` with `+=` but omitted both from
  the reset block, so every re-derive added the fee/shipping charges again and the two totals (and their derived
  `*InPreferredCurrency` counterparts) grew without bound. Both are now reset to `0` before accumulation.
- `PurchaseOrderPriceRule` accumulated `TotalFee` and `TotalShippingAndHandling` with `+=` but omitted both from
  the reset block that re-zeroes the order totals at the start of each derivation, so every re-derive added the
  fee/shipping charges again and the two totals grew without bound. Both are now reset to `0` before accumulation.
- `EmploymentFromDateRule` validated overlapping employment periods using `ThroughDate` but watched only
  `FromDate`, so extending an employment's `ThroughDate` into a later employment's period never re-ran the overlap
  check and the conflict silently passed validation. It now also watches `ThroughDate`.
- `CustomerRelationshipFromDateRule` validated overlapping customer-relationship periods using `ThroughDate` but
  watched only `FromDate`, so extending a relationship's `ThroughDate` into a later relationship's period never
  re-ran the overlap check and the conflicting periods silently passed validation. It now also watches `ThroughDate`.
- `WorkEffortTotalCostRule` rounded its four cost totals with `Math.Round` (banker's / to-even) instead of
  `Rounder.RoundDecimal` (away-from-zero), the money-rounding convention used elsewhere. For `TotalMaterialCost`,
  whose input (`Quantity × weighted-average cost`) can land on a half-cent, this under-rounded by a cent
  (e.g. `0.125` → `0.12` instead of `0.13`). All four totals now use `Rounder.RoundDecimal`.
- `PurchaseOrderItemStateRule` decided `PurchaseOrderItemShipmentState` (PartiallyReceived vs Received) from
  `QuantityReceived < QuantityOrdered` but its patterns never watched `QuantityOrdered`, so raising the ordered
  quantity on a revision left the shipment state stale (e.g. a fully-received item stayed `Received` instead of
  dropping back to `PartiallyReceived`). It now watches `PurchaseOrderItem.QuantityOrdered`.
- `WorkEffortTotalCostRule` computed `TotalLabourCost` by hard-casting **every** `ServiceEntriesWhereWorkEffort`
  entry to `TimeEntry` (`((TimeEntry)v).Cost`). As soon as a non-`TimeEntry` service entry (e.g. a `MaterialsUsage`
  or `ExpenseEntry`) was attached to the work effort, the cast threw an `InvalidCastException` and the entire cost
  derivation failed. It now filters with `OfType<TimeEntry>()` before summing `Cost`.
- `RequestForQuoteSearchStringRule` had the same wrong reroute as the other request search rules: it watched
  `ContactMechanism.DisplayName` via `QuotesWhereFullfillContactMechanism` (the Quote inverse) instead of
  `RequestsWhereFullfillContactMechanism`, so renaming a request's `FullfillContactMechanism` left its
  `SearchString` stale. It now reroutes through `RequestsWhereFullfillContactMechanism`.
- `RequestForProposalSearchStringRule` had the same wrong reroute as `RequestForInformationSearchStringRule`: it
  watched `ContactMechanism.DisplayName` via `QuotesWhereFullfillContactMechanism` (the Quote inverse) instead of
  `RequestsWhereFullfillContactMechanism`, so renaming a request's `FullfillContactMechanism` left its
  `SearchString` stale. It now reroutes through `RequestsWhereFullfillContactMechanism`.
- `RequestForInformationSearchStringRule` rerouted its "re-derive when the contact mechanism is renamed" pattern
  through `QuotesWhereFullfillContactMechanism` (the Quote inverse, copy-pasted from the quote search rules)
  instead of `RequestsWhereFullfillContactMechanism`. Filtering Quote objects to `RequestForInformation` is always
  empty, so renaming a request's `FullfillContactMechanism` left its `SearchString` stale. It now reroutes through
  `RequestsWhereFullfillContactMechanism`.
- `PurchaseShipmentShipToPartyRule` selected the shipment-number **prefix** used for `SortableShipmentNumber`
  from `CustomerShipmentSequence.IsEnforcedSequence` instead of the purchase shipment's own
  `PurchaseShipmentSequence`. The number itself comes from `NextPurchaseShipmentNumber` (which branches on
  `PurchaseShipmentSequence`), so when the two sequences disagreed the prefix branch diverged from the number
  branch — picking the wrong prefix, or (when the purchase-shipment sequence was enforced while the
  customer-shipment sequence restarted on fiscal year) dereferencing the absent fiscal-year sequence record and
  throwing a `NullReferenceException`. It now branches on `PurchaseShipmentSequence`.
- `CustomerReturnExistShipmentNumberRule` selected the shipment-number **prefix** used for
  `SortableShipmentNumber` from `CustomerShipmentSequence.IsEnforcedSequence` instead of the customer return's
  own `CustomerReturnSequence`. After the number itself was fixed to follow `CustomerReturnSequence`
  (`NextCustomerReturnNumber`), the prefix branch could diverge from the number branch: when the two sequences
  disagreed it picked the wrong prefix, and when the customer-return sequence was enforced while the
  customer-shipment sequence restarted on fiscal year it dereferenced the (absent) fiscal-year sequence record
  and threw a `NullReferenceException`. It now branches on `CustomerReturnSequence`.
- `CustomerShipment.AppsOnDeriveQuantityDecreased` mis-applied a quantity-decrease correction when a shipment
  item was issued from more than one `ItemIssuance`. While spreading the correction it subtracted the **whole**
  correction from the running remainder after the first issuance (`itemIssuanceCorrection -= quantity`) rather
  than the amount just applied (`-= subQuantity`), so the remainder zeroed out, the loop stopped early, and later
  issuances were left uncorrected (the decrease was under-applied). It now subtracts `subQuantity`.
- `Quote.Order` (`QuoteExtensions.AppsOrder`) materialised a flat `SalesOrderItem` for **every** quote item,
  including feature sub-items (`QuotedWithFeatures`), instead of nesting them. A feature became a separate priced
  order line (double-counting), and — having no parent `OrderedWithFeature` link — threw a `NullReferenceException`
  during order-item pricing (`SalesOrderItemPriceRule`). Ordering a quote now creates order items only for
  top-level quote items and nests their ordered feature sub-items under the parent via `OrderedWithFeature`,
  mirroring the quote's `QuotedWithFeature` structure and the quote-copy logic.
- `Proposal` quotes could be set ready for processing even with no valid quote items. The `Proposals` security
  config set `DeniedPermissions` on the **`ProductQuote`** `SetReadyForProcessing` revocation (a copy-paste from
  `ProductQuotes`) instead of the `Proposal` one, so the `ProposalSetReadyForProcessingRevocation` that
  `ProposalDeniedPermissionRule` attaches to a created, item-less proposal denied nothing — a no-op gate. It now
  populates `ProposalSetReadyForProcessingRevocation.DeniedPermissions`, making the gate effective (and no longer
  clobbering the ProductQuote revocation's permissions).
- `TimeFrequency.GetConvertToFactor` dereferenced `FirstOrDefault(...).ConversionFactor`: when the frequency had
  no conversion to the requested target, `FirstOrDefault` returned null and the `.ConversionFactor` access threw a
  `NullReferenceException` instead of returning null. It now uses `?.ConversionFactor`, honouring the method's
  `decimal?` contract (and `ConvertToFrequency`, which already null-checks the result).
- `WorkTasks.AppsMonthly` (collective/monthly work-effort invoicing) built a `SalesInvoiceItem` for every
  inventory assignment from `DerivedBillableQuantity` with no guard, so an assignment with a **zero** billable
  quantity produced an invalid 0-quantity invoice line. It now skips assignments with `DerivedBillableQuantity <= 0`,
  matching the per-effort `WorkEffort` invoicing and `WorkEffortTotalMaterialRevenueRule`.
- `WorkEffort` invoicing billed a part that had a **zero** billable quantity. `CreateInvoiceItems` computed the
  invoice quantity as `DerivedBillableQuantity != 0 ? DerivedBillableQuantity : Quantity`, so an inventory
  assignment with `AssignedBillableQuantity = 0` (hence `DerivedBillableQuantity = 0`) fell back to the full
  assignment `Quantity` and was invoiced anyway. It now skips assignments with `DerivedBillableQuantity <= 0`
  and bills `DerivedBillableQuantity` directly, matching `WorkEffortTotalMaterialRevenueRule`.
- `InternalOrganisation.NextCustomerReturnNumber` decided enforced-vs-fiscal-year numbering from
  `PurchaseShipmentSequence` instead of `CustomerReturnSequence`, so customer-return numbers followed the
  purchase-shipment sequence setting rather than their own (e.g. a fiscal-year customer-return sequence used the
  global enforced counter when purchase shipments were enforced). It now branches on `CustomerReturnSequence`.
- `PurchaseShipment.AppsReceive` dereferenced `shipmentItem.Part.InventoryItemKind` for every shipment item with
  no `ExistPart` guard, so receiving a shipment that contained a part-less item (e.g. a manually added
  non-inventory line) threw a `NullReferenceException`. A part-less item has no inventory to receive, so it is now
  marked received but skips the inventory receipt and inventory transaction (purchase order items without a part
  are non-receivable, so order-linked receipts are unaffected).
- `CustomerShipment` shipping over-deducted inventory when a shipment item was issued from more than one
  inventory item. `AppsShip` iterates the item's `ReservedFromInventoryItems` and, for non-serialised parts,
  created an `OutgoingShipment` transaction of the **whole** `shipmentItem.Quantity` for *each* reserved item —
  so a quantity issued as a split (e.g. 6 + 4 across two facilities) deducted the full quantity twice. It now
  deducts each reserved item's actually-issued quantity (summed from `ItemIssuancesWhereShipmentItem`), matching
  the per-item granularity of the serialised branch.
- `CustomerShipment.CreatePickList` decided whether a shipment item was serialised from `unifiedGood` only
  (`unifiedGood?.InventoryItemKind`). For a serialised `NonUnifiedGood`/`NonUnifiedPart`, `unifiedGood` is null,
  so `serialized` was false and the non-serialised branch cast the part's `SerialisedInventoryItem` to
  `NonSerialisedInventoryItem`, throwing an `InvalidCastException` when picking. It now derives `serialized` from
  the resolved `part` (which covers all three good types), so picking a serialised non-unified good works.
- CI no longer fails intermittently with `Unable to process file command 'env' successfully. Invalid
  format '<version>'`. Under GitHub Actions, Nerdbank.GitVersioning's `SetCloudBuildVersionVars` MSBuild
  target appended its (unused) `Git*` version variables to the shared `$GITHUB_ENV` on every project
  build; because the Nuke build compiles projects in parallel, the concurrent appends interleaved and
  corrupted the file, failing a random matrix job. A new `dotnet/Directory.Build.targets` clears the
  `CloudBuildVersionVars` item before the target runs, skipping the emission. Assembly version stamping
  is unaffected, and `cloudBuild.setVersionVariables: false` does **not** gate this MSBuild-side write.
- `CustomerShipment` billing computed how much was left to invoice from the **optional** `AssignedUnitPrice`
  (`QuantityOrdered * AssignedUnitPrice - amountAlreadyInvoiced`). For a catalog-priced order line (no assigned
  price), `AssignedUnitPrice` is null, so the result was null, `leftToInvoice > 0` was false, and the line was
  never invoiced on shipment. It now uses the derived `UnitPrice` (which the created invoice item already uses),
  so catalog-priced lines are billed.
- `BasePrice.AppsDelete` called `this.Product.RemoveBasePrice(this)` and `this.ProductFeature.RemoveFromBasePrices(this)`
  without guards, but a base price requires only **one** of `Product`/`ProductFeature` — so deleting a product-only
  (or feature-only) base price threw a `NullReferenceException`. Each removal is now guarded by
  `ExistProduct`/`ExistProductFeature`.
- `PriceComponents` threw a `NullReferenceException` when evaluating a price component that has a `SalesChannel`
  against an order/invoice with **no** `SalesChannel`: `channel.Equals(priceComponent.SalesChannel)` dereferenced
  a null `channel`. It now uses the null-safe `Equals(channel, priceComponent.SalesChannel)`, so a channel-specific
  price component simply does not apply to a channel-less order.
- `QuoteExtensions.AppsCopy` dropped the `QuotedWithFeature` parent link when copying a quote. `CopyQuoteItem`
  recursively copied each item's feature sub-items (`QuotedWithFeatures`) and added them to the copied quote, but
  never re-linked them to the copied parent item — so a copied feature item became an orphaned standalone quote
  item. The recursive copy now captures the new feature item and re-links it via `itemCopy.AddQuotedWithFeature(...)`.
- `QuoteExtensions.SetItemState` reset **every** quote item to `Draft` when the quote was `Created`, with no
  guard — so `Revise()`/`Reopen()` (which return the quote to `Created` and call `SetItemState`) clobbered items
  that were already `Cancelled` or `Rejected`. The `IsCreated` branch now skips items in those terminal states,
  matching the `Cancelled`/`Rejected`/`AwaitingApproval` branches.
- `PurchaseOrderItem.Return` (item-level return) built a `PurchaseReturn` `ShipmentItem` for a serialised part
  without setting `NextSerialisedItemAvailability`, which `ShipmentItemRule` requires for serialised items on a
  `CustomerShipment`/`PurchaseReturn`. Returning a received serialised order item therefore failed validation
  (`ShipmentItem.NextSerialisedItemAvailability is required`). The shipment item now sets it to `NotAvailable`,
  matching the order-level `PurchaseOrder.AppsReturn`.
- `OrderQuantityBreak` validation required at least one of `FromAmount`/`ThroughAmount`, but checked
  `OrderValue.ThroughAmount` — a different type's roletype — instead of the break's own
  `OrderQuantityBreak.ThroughAmount`. A break with only its own `ThroughAmount` set was therefore wrongly
  rejected with the "at least one" error. `AppsOnPostDerive` now asserts on `OrderQuantityBreak.ThroughAmount`.
- `RepeatingPurchaseInvoice.Repeat` re-billed already-billed, **partially-received** purchase-order items. The
  "to bill" guard `!ExistOrderItemBillings && IsReceived || IsPartiallyReceived || (!ExistPart && QuantityReceived == 1)`
  was mis-parenthesised: since `&&` binds tighter than `||`, the already-billed check only gated the `IsReceived`
  term, so a partially-received item that was already billed got billed again. The three received-conditions are
  now wrapped in parentheses so the already-billed guard applies to all of them.
- `PurchaseOrder.AppsCopy` copied each item's sales terms onto the **source** item instead of the new (copied)
  item: the per-item loop called `purchaseOrderItem.AddSalesTerm(...)` rather than `item.AddSalesTerm(...)`. As a
  result copied purchase orders had no item sales terms and the source items' terms were duplicated. The four
  `Inco`/`Invoice`/`Order`/`QuoteTerm` branches now add to `item`. (The discount- and surcharge-adjustment loops
  already correctly targeted the copy.)
- `QuoteExtensions.AppsCopy` (ProductQuote / Proposal / StatementOfWork) copied the quote's order-level sales
  terms onto the **source** quote instead of the new `copy`: the loop called `@this.AddSalesTerm(...)` rather
  than `copy.AddSalesTerm(...)`. As a result copied quotes had no order-level sales terms and the source quote's
  terms were duplicated. The four `Inco`/`Invoice`/`Order`/`QuoteTerm` branches now add to `copy`. (The
  item-level loop already correctly targeted the item copy.)
- `SalesInvoice.AppsCopy` copied each item's sales terms onto the **source** item instead of the new (copied)
  item: the per-item loop called `salesInvoiceItem.AddSalesTerm(...)` (the item being iterated) rather than
  `invoiceItem.AddSalesTerm(...)` (the copy). As a result the copied invoice's items had no sales terms and the
  source items' terms were duplicated. The four `Inco`/`Invoice`/`Order`/`QuoteTerm` branches now add to
  `invoiceItem`. (The order-level sales-term loop already correctly targeted the copy.)
- The repository meta declared `RequestVersion.RequestState` as `[Multiplicity(OneToOne)]`, unlike every other
  version-state relation (e.g. `CustomerShipmentVersion.ShipmentState` is `ManyToOne`). A `OneToOne` state would
  break once a second `RequestVersion` referenced the same `RequestState` singleton; it is now `ManyToOne`. The
  relation id is unchanged.
- The repository meta typed `CapitalBudget.CurrentVersion`/`AllVersions` as `SalesOrderVersion` /
  `SalesOrderVersion[]` (a copy-paste error), even though `CapitalBudget : Budget, Versioned` versions into
  `CapitalBudgetVersion`. Both relations are now typed `CapitalBudgetVersion`, so CapitalBudget version history
  is correctly typed. The relation ids are unchanged.
- The Product Quote print `BillToModel` overwrote the bill-to address instead of appending it. Each
  `if (Address2/Address3 present)` block **assigned** `this.Address = $"\n{AddressN}"` rather than appending,
  so the `FullfillContactMechanism` postal address `Address1` (and `Address2`) were discarded and a leading
  newline remained. The two assignments are now `+=`, so the address joins the present lines as
  `Address1\nAddress2\nAddress3`.
- The Work Task print `TimeEntryModel` formatted `FromTime`/`ThroughTime` with the 12-hour clock pattern
  `"hh:mm:ss"` and no AM/PM designator, so afternoon times collapsed onto the morning (e.g. 13:30 printed as
  "01:30:00"). Both now use the 24-hour pattern `"HH:mm:ss"`.
- The Product Quote print `IssuerModel` overwrote the issuer's address instead of appending it. Each
  `if (Address2/Address3 present)` block **assigned** `this.Address = $"\n{AddressN}"` rather than appending,
  so the issuer's `GeneralCorrespondence` `Address1` (and `Address2`) were discarded and a leading newline
  remained. The two assignments are now `+=`, so the address joins the present lines as
  `Address1\nAddress2\nAddress3`.
- The Work Task print `CustomerModel` overwrote the **shipping** address the same way as the billing address:
  each `if (Address2/Address3 present)` block **assigned** `this.ShippingAddress = $"\n{AddressN}"` rather than
  appending, so `Address1` (and `Address2`) were discarded and a leading newline remained. The two assignments
  are now `+=`, so the shipping address joins the present lines as `Address1\nAddress2\nAddress3`.
- The Work Task print `CustomerModel` overwrote the billing address instead of appending it. Each
  `if (Address2/Address3 present)` block **assigned** `this.BillingAddress = $"\n{AddressN}"` rather than
  appending, so `Address1` (and `Address2`) were discarded and a leading newline remained — the printed
  billing address showed only the last present line. The two assignments are now `+=`, so the billing address
  joins the present lines as `Address1\nAddress2\nAddress3`.
- The `LegalForms` setup seeded the **FR - SASU** legal form under the `FrSas` id. Because a later
  `merge(...)` for the same id wins, `FrSas` was overwritten (its `Description` became "FR - SASU") and
  `LegalForms.FrSasu` was never created (resolved to `null`). The second `merge(...)` now uses `FrSasuId`,
  so both `FrSas` and `FrSasu` are seeded correctly.
- The `EmploymentApplicationStatuses` setup seeded the **Reviewed** status under the `Received` id. Because a
  later `merge(...)` for the same id wins, `Received` was overwritten (its `Name` became "Reviewed") and
  `EmploymentApplicationStatuses.Reviewed` was never created (resolved to `null`). The second `merge(...)` now
  uses `ReviewedId`, so both `Received` and `Reviewed` are seeded correctly.
- The `GenderTypes` setup seeded the **Prefer not to say** gender type under the `Other` id. Because a later
  `merge(...)` for the same id wins, `Other` was overwritten (its `Name` became "Prefer not to say") and
  `GenderTypes.PreferNotToSay` was never created (resolved to `null`). The second `merge(...)` now uses
  `PreferNotToSayId`, so both `Other` and `PreferNotToSay` are seeded correctly.
- The test-population `SalesInvoiceItemBuilder.WithPartItemDefaults` built a **product** item, not a part item.
  It was a copy of `WithProductItemDefaults` — using the `ProductItem` invoice-item-type and `.WithProduct(...)`
  — despite its name and the `salesInvoiceItem_Part` it populates on the sample sales invoices. It now uses the
  `PartItem` invoice-item-type (`InvoiceItemTypes.PartItemId`) and `.WithPart(...)`, matching the correct
  `PurchaseInvoiceItemBuilder.WithPartItemDefaults`. (`UnifiedGood` is both a `Good` and a `Part`, so the
  existing serialised-good source remains valid for the part role.) The part item sets only `Part` — not
  `SerialisedItem`/availability — because a `SalesInvoiceItem` permits at most one of
  `SerialisedItem` / `ProductFeatures` / `Part`.
- The `ProductFeatureTests` "required relations" tests now build the named object before asserting it derives
  without errors. `GivenModel`, `GivenServiceFeature`, `GivenSizeConstant`, `GivenSoftwareFeature` and
  `GivenProductQualityConstant` set `builder.WithName("Mt")` but never called `builder.Build()` afterwards
  (unlike the correct `GivenDimension`), so after the `Rollback()` there was no object to derive and the
  positive assertion `Assert.False(this.Derive().HasErrors)` passed vacuously. Each now calls `builder.Build()`
  after `WithName`, so the assertion actually exercises a named object's derivation. (The review flagged
  `GivenModel` and `GivenServiceFeature`; the identical omission was present in three sibling tests.)
- The Apps meta/workspace generator built the **apps-extranet** TypeScript workspace libraries against the
  **Default** workspace instead of **Extranet**. `Generate/Program.cs` applied a single `workspaceName = "Default"`
  to every workspace output, including the three `libs/apps-extranet/workspace/**` outputs, so the extranet
  meta / meta-json / domain were generated with the full Default workspace rather than the restricted Extranet
  one. Each output now carries its own workspace name and the apps-extranet outputs use `"Extranet"`. (The
  generated files are gitignored, so only the generator changes; regenerating drops the extranet domain from
  ~580 to ~18 classes, matching the Extranet workspace.)
- `AutomatedAgentTest.ChangedNameDerivePartyName` was vacuous: it set `automatedAgent.Name = "name"`, never
  re-derived, and asserted the raw `Name` it had just set — and the `PartyName` role it was named for does not
  exist (the role derived from `Name` is `DisplayName`, via `AutomatedAgentRule`). It now re-derives after the
  change and asserts the derived `DisplayName`, and is renamed `ChangedNameDeriveDisplayName` to match what it
  verifies.
- `RgsFilterRuleTests.RgsFilterUseWoCoTrueAndUseExcludeWoCoFalseShouldNotGiveError` now exercises the scenario in
  its name. Its body set `UseWoCo = false; ExcludeWoCo = true` — the opposite of the name, and byte-identical to
  the preceding `…UseWoCoFalseAndUseExcludeWoCoTrue…` test — so the "UseWoCo true / ExcludeWoCo false"
  combination was never actually tested and the case merely duplicated its sibling. It now sets
  `UseWoCo = true; ExcludeWoCo = false`, asserting that the single-flag combination derives without the
  "cannot both be true" error.
- Removed a misplaced, vacuous duplicate test from the NonUnifiedPart suite. `NonUnifiedPartTests` contained a
  single `OnPostDeriveAssertExistPart` that was a byte-identical copy of the test in `NonUnifiedGoodTests`: it
  built a `NonUnifiedGood` and asserted `"NonUnifiedGood.Part is required"`, so it exercised `NonUnifiedGood`,
  not `NonUnifiedPart` (which has no required-part rule — an empty `NonUnifiedPart` derives clean), and merely
  duplicated coverage already in `NonUnifiedGoodTests`. The now-empty `NonUnifiedPartTests` class was removed;
  the genuine `NonUnifiedPartRuleTests` / `NonUnifiedPartDeniedPermissionRuleTests` in the same file are
  unchanged.
- `CustomerShipmentTests.GivenCustomerShipmentBuilder_WhenBuild_ThenPostBuildRelationsMustExist` now asserts the
  derived `ShipFromParty` against the expected internal organisation instead of comparing the value to itself.
  The assertion read `Assert.Equal(shipment.ShipFromParty, shipment.ShipFromParty)` — a tautology that passed
  even if the `ShipFromParty` derivation were wrong (or null). It now asserts
  `Assert.Equal(this.InternalOrganisation, shipment.ShipFromParty)`, matching the sibling `ShipFromAddress`
  assertion and the derivation (the single internal organisation becomes the ship-from party).
- The Apps test server's invalid-model-state logger silently dropped the validation detail. `Startup`'s
  `InvalidModelStateResponseFactory` called `logger.LogError(problemDetails.Title, message)`, but `LogError`'s
  first argument is the message *template* and `ValidationProblemDetails.Title` is the constant
  `"One or more validation errors occurred."` with no `{}` placeholders — so `message` (the joined field
  errors) had nowhere to bind and was discarded, logging only the generic title. It now calls
  `LogError("{Title}: {Errors}", problemDetails.Title, message)`, so the actual validation errors are logged
  and both values become structured properties.
- The Apps test `TestController` (`Database/Server`) no longer throws a `NullReferenceException` inside its own
  error handler. Its `ILogger<TestController>` property was declared but never injected — the constructor took
  only `IDatabaseService`, so the property stayed `null` — yet the `catch` blocks of `Init`, `Restart` and
  `TimeShift` call `this.Logger.LogError(...)`. Any failure therefore NRE'd inside the handler, masking the
  original exception and turning the intended `400 + message` into a `500`. The logger is now injected through
  the constructor, mirroring the sibling `TestAuthenticationController`.
- The base app's Person overview pulled the wrong object: `onPreSharedPull` fetched `p.Organisation` by the
  Person's `scoped.id`, so no object came back and the overview's `object` (the Person) was null — e.g. the
  breadcrumb `{{ object?.FirstName }}` rendered blank. It now pulls `p.Person`. (The dynamic panels were
  unaffected because they drive off `scoped.id` directly.)
- The extranet app's `MainComponent` leaked its `router.events` subscription: unlike the sibling
  toggle/open/close side-nav subscriptions it was neither stored nor unsubscribed, so it survived component
  teardown. It is now kept in a `routerSubscription` field and unsubscribed in `ngOnDestroy`.
- The intranet `DisplayService.description()` read the `nameByObjectType` map instead of
  `descriptionByObjectType`, so the dynamic summary panel showed an object's name as both its name and its
  description. It now reads `descriptionByObjectType`, falling back to `nameByObjectType` when no description
  role is configured for a type (the map is not yet populated, so behaviour is unchanged until it is).
- The purchase-invoice summary panel's "ship to" and "Bill to End Customer" cards both navigated to the
  `BilledFrom` party on click (copied from the "Billed from" card) instead of the party each card shows. They
  now navigate to `ShipToCustomer` and `BillToEndCustomer` respectively.
- The intranet proposal (quote) list declared `origin` and `destination` columns that were never populated —
  a Proposal has no such roles, and they were absent from the row interface, the row-builder, and the Proposal
  sorter — so they rendered permanently blank (and their `sort: true` was dead). Both columns are removed.
- The intranet non-unified-part list's `type` column was always blank: the column is defined (and sortable),
  but the row-builder never set a `type` key. It now populates `type` from `v.ProductTypeName` (the role the
  column already sorts by, alongside the sibling `brand`/`model`/`kind` derived-name columns).
- The intranet purchase-order list's `customerReference` column showed the order's `Description` instead of
  its `CustomerReference`. The row-builder read `v.Description`; it now reads `v.CustomerReference` (the role
  the column is named for and the list already sorts by).
- The intranet product-characteristic list never sorted: it fetched `sorterService.sorter(m.Brand)`, and the
  matching sorter — whose values are `SerialisedItemCharacteristicType` roles — was registered under the wrong
  composite key `m.SerialisedItemCharacteristic` while the list pulls `SerialisedItemCharacteristicType`. The
  sorter is now registered under `m.SerialisedItemCharacteristicType` and the list fetches it, so the name /
  active columns sort. (No other component fetched the old key.)
- The intranet serialised-item list never sorted: its pull fetched `sorterService.sorter(m.Brand)` — a
  composite with no entry in the sorter service, so `sorter(...)` returned undefined and no sorting was
  applied — even though the pull and result are `SerialisedItem`. It now fetches `sorter(m.SerialisedItem)`,
  so the id / name / categories / availability columns sort.
- The intranet ProductCategory list could not be sorted: its sorter was a copy of the Catalogue sorter, so
  its keys pointed at the foreign `m.Catalogue.*` / `m.Scope.*` roleTypes (which a `ProductCategory` pull has
  no business sorting by) rather than ProductCategory's own roles. It now sorts by `m.ProductCategory.Name`.
  (Sorting the `scope` / parent columns would need derived `<Composite>Name` roles on ProductCategory — a
  dotnet-domain change tracked separately.)
- The intranet WorkRequirement list's `priority` column sorted by the requirement number, not the priority.
  Its sort key was `m.WorkRequirement.SortableRequirementNumber` — the same roleType the `number` column uses
  — so sorting by priority reproduced the number order. It now sorts by `m.WorkRequirement.PriorityName`
  (mirroring how the `state` column uses `RequirementStateName`).
- The party `DisplayPhone` workspace derivation prefixed its output with a spurious `", "`. It joined the
  telecommunications-number display names with `.reduce((acc, cur) => acc + ', ' + cur, '')` seeded with an
  empty string, so the seed contributed a leading separator (e.g. `", Office, Mobile"`). It now uses
  `.join(', ')`.
- The purchase-order-item `TotalIncVat` workspace derivation computed unit VAT on the gross base price
  instead of the net unit price — the order-side sibling of the purchase-invoice-item fix. `unitVat` was
  `unitBasePrice * vatRate`, so the line's discounts and surcharges (accumulated into
  `unitDiscount`/`unitSurcharge` just above) were excluded from the VAT base, and `TotalIncVat` was left
  inconsistent with the sibling `UnitVat` rule. It now applies the rate to
  `unitBasePrice - unitDiscount + unitSurcharge`.
- The purchase-invoice-item `TotalIncVat` workspace derivation computed unit VAT on the gross base price
  instead of the net unit price. `unitVat` was `unitBasePrice * vatRate`, so the line's discounts and
  surcharges — already accumulated into `unitDiscount`/`unitSurcharge` just above — were excluded from the VAT
  base, overstating VAT on discounted lines and understating it on surcharged ones (and leaving `TotalIncVat`
  inconsistent with the sibling `UnitVat` rule, which already applied the rate to the net price). It now
  applies the rate to `unitBasePrice - unitDiscount + unitSurcharge`.
- The extranet work-task create + edit forms bound the FullfillContactMechanism select's options to
  PartyContactMechanisms instead of ContactMechanisms. `onPostPull` assigned the pulled
  `CurrentPartyContactMechanisms` (a PartyContactMechanism collection) straight to
  `contactMechanisms: ContactMechanism[]`; it now maps each to its `.ContactMechanism`, so the picker offers
  the contact mechanisms the `FullfillContactMechanism` role expects.
- The employment form no longer overwrites an existing employment's FromDate on edit. `onPostPull` set
  `this.object.FromDate = new Date()` unconditionally, so opening an employment to edit it reset its start
  date to today (persisted on save). The default is now guarded by `this.createRequest`, so only a new
  employment gets today's date; a loaded one keeps its own.
- The purchase-invoice create and edit forms had several mis-wired "add new …" inline cards and
  contact-added handlers, now corrected: the ShipToCustomer add-person card ran
  `billedFromContactPersonAdded` (now `shipToCustomerContactPersonAdded`); the ShipToEndCustomer
  add-customer card ran `billToEndCustomerAdded` (now `shipToEndCustomerAdded`); the BillToEndCustomer
  add-customer card was shown by `*ngIf="addShipToCustomer"` instead of `addBillToEndCustomer`; and two
  contact-added handlers linked the new `OrganisationContactRelationship` to the wrong organisation —
  `billToEndCustomerContactPersonAdded` used `ShipToEndCustomer` (now `BillToEndCustomer`) and
  `shipToCustomerContactPersonAdded` used `BilledFrom` (now `ShipToCustomer`). Each defect was present on
  both the create and edit forms.
- Editing a CustomerShipment or PurchaseReturn no longer silently clears its `ShipToAddress` and
  `ShipToContactPerson` on load. `onPostPull` called `updateShipToParty` without first setting
  `previousShipToparty`, so the `ShipToParty !== previousShipToparty` guard inside it was true on the initial
  load and nulled `ShipToAddress` + `ShipToContactPerson` — which then persisted on save (silent data loss on
  every edit). `onPostPull` now initializes `previousShipToparty` to the loaded `ShipToParty`, so a load is no
  longer treated as a party change. (The customershipment instance was an unflagged sibling of the reported
  purchasereturn defect.)
- The email-communication form's "add a To email" inline card saved the new address to `FromEmail` instead
  of `ToEmail` (`toEmailAdded`), overwriting the From address and never setting the recipient. It now assigns
  `ToEmail`. (The sibling `fromEmailAdded` was already correct.)
- The bill-to-end-customer autocomplete on the sales-invoice (create + edit) and sales-order (edit) forms now
  fires `billToEndCustomerSelected` instead of `billToCustomerSelected`. The autocomplete binds the
  `BillToEndCustomer` role correctly, but its `(changed)` handler ran the bill-to-*customer* side-effect
  (`updateBillToCustomer`), so selecting a bill-to-end-customer loaded the wrong party's contacts and
  contact-mechanisms into the dependent dropdowns. It now runs `updateBillToEndCustomer`.
- The UnifiedGood edit form no longer drops product categories on save — the same alias + splice-during-
  iteration defect as the NonUnifiedGood/NonUnifiedPart edit forms. `onPostPull` set `selectedCategories` to
  the *same array reference* as `originalCategories`; `onSave()` then iterated `selectedCategories` while
  `splice`-ing the aliased `originalCategories`, skipping every other `ProductCategory`, which the second
  loop then `removeProduct`-ed. `selectedCategories` is now an independent copy (`[...originalCategories]`),
  so all categories are preserved.
- The work-effort / purchase-order-item assignment form no longer crashes on open. `onPostPull` filtered the
  available purchase orders using `this.workEffort.TakenBy` before `workEffort` was assigned (it was set only
  afterwards), throwing a `TypeError` whenever a candidate order existed. `workEffort` is now established
  before the filter runs.
- The customer-shipment create + edit forms now populate the ship-from contact dropdown with the ship-from
  party's contacts instead of overwriting the ship-to contact options. `updateShipFromParty` assigned the
  ship-from party's `CurrentContacts` to `shipToContacts` (leaving the declared `shipFromContacts` unused),
  so on load — where `updateShipToParty` runs first and `updateShipFromParty` overwrites — the ship-to
  contact dropdown was clobbered with the ship-from party's contacts. It now assigns `shipFromContacts`.
- The purchase-return create + edit forms had the identical `updateShipFromParty` defect: the ship-from
  party's `CurrentContacts` were assigned to `shipToContacts` (leaving the declared `shipFromContacts`
  unused), so the ship-from contact picker stayed empty and the ship-to contact options were overwritten
  with the ship-from party's contacts. Both forms now assign `shipFromContacts`.
- The base-price form's create-time defaults are no longer applied on edit. `onPostPull` unconditionally set
  `FromDate = new Date()` and `PricedBy = internalOrganisation`, so editing an existing BasePrice reset its
  effective-from date to today and its priced-by (persisted on save). Both are now guarded by
  `this.createRequest`, so a loaded BasePrice keeps its own values.
- The NonUnifiedPart edit form no longer drops part categories on save — the same alias + splice-during-
  iteration defect as the NonUnifiedGood edit form. `onPostPull` set `selectedCategories` to the *same array
  reference* as `originalCategories`; `onSave()` then iterated `selectedCategories` while `splice`-ing the
  aliased `originalCategories` inside the loop, skipping every other `PartCategory`, which the second loop
  then `removePart`-ed. `selectedCategories` is now an independent copy (`[...originalCategories]`), so all
  part categories are preserved.
- The PositionTypeRate form no longer nulls kept PositionType assignments on save. `onPostPull` set
  `originalPositionTypes` to the *same array reference* as `selectedPositionTypes`; `save()` then iterated
  `selectedPositionTypes` while `splice`-ing the aliased `originalPositionTypes`, skipping every other
  PositionType, which the second loop then set to `PositionTypeRate = null`. Editing a rate with two or more
  assigned position types and saving unassigned roughly half of them. `originalPositionTypes` is now an
  independent copy (`[...(selectedPositionTypes ?? [])]`), so all assignments are preserved.
- The purchase-invoice create + edit forms no longer clear the wrong assigned ship-to address when the
  ShipToCustomer changes. `updateShipToCustomer`'s change-guard nulled `AssignedShipToEndCustomerAddress`
  (the *end*-customer's field, correctly owned by `updateShipToEndCustomer`) instead of the ship-to-customer's
  own `AssignedShipToCustomerAddress` — so changing the ShipToCustomer left its stale address in place (saved
  against the new customer) while wiping the end-customer's chosen address. It now nulls
  `AssignedShipToCustomerAddress`, matching the adjacent `ShipToCustomerContactPerson` clear.
- The supplier-offering form's `currencySelected` no longer throws when a currency is picked before a
  supplier. Its condition optional-chained `this.object.Supplier?.PreferredCurrency`, but the body assigned
  `this.object.Supplier.PreferredCurrency` unguarded, so with no supplier selected (the `== null` branch
  true) it raised a `TypeError`. The assignment is now guarded by `this.object.Supplier`.
- Apps `Setup.v.cs` dispatched `BaseOnPreSetup` from `OnPrePrepare` instead of `BaseOnPrePrepare`
  (latent; both hooks are empty today).
- The UnifiedGood create form showed its manual ProductNumber input based on `settings.UseGlobalProductNumber`,
  but the bound `ProductNumber` is created (and added to the good) based on `settings.UseProductNumberCounter`.
  With the settings disagreeing, the field could be hidden while an empty product-number identification was
  attached, or shown bound to `undefined`. The input is now gated on `!settings.UseProductNumberCounter`,
  matching its creation.
- SQL Server `AllowSnapshotIsolation` now brackets the database name in its `ALTER DATABASE`
  statement, so databases named after reserved T-SQL keywords (e.g. `Identity`) provision correctly.
- The NonUnifiedGood edit form no longer drops product categories on save. `onPostPull` assigned the same
  array reference to both `selectedCategories` and `originalCategories`, so `save()` iterated
  `selectedCategories` while `splice`-ing the aliased `originalCategories` inside that loop — a
  splice-during-iteration that skipped every other category, which the second loop then `removeProduct`-ed
  from the good. Editing a good with two or more categories and saving dropped roughly half of them.
  `selectedCategories` is now an independent copy (`[...originalCategories]`, with the source `?? []`-guarded
  for the spread), so all categories are preserved.
- Effects watching composites roles (or derived list roles) no longer rerun on every unrelated session
  write. Those signals rebuild their list on every recompute, and the default reference-equality comparer
  counted each fresh array as a change. `ISignalFactory.Computed` now accepts an optional
  `IEqualityComparer<T>` (mirroring `State`), and the adapter passes element-wise comparers for composites
  and derived role signals, restoring the value cutoff.
- Effects no longer run between the record merges of a single pull, push response or reset. Every merged
  record bumped the session graph revision — and flushed effects — individually, so an effect reading roles
  of two pulled objects observed the first object updated while the second still held its stale values
  (and large pulls paid one full propagation per record). Multi-record operations now hold the revision and
  bump once at the end (`Session.HoldGraph`/`ReleaseGraph`), so effects observe only the fully merged state.
- Effects no longer observe torn state or run twice per change. The effect scheduler flushed as soon as
  the first effect was enqueued, while the propagation walk was still marking the remaining subscribers —
  an effect reading two computeds derived from the same signal ran once with one fresh and one stale value,
  then again after the walk finished. `Propagation.Propagate` now holds every touched scheduler for the
  duration of the walk and releases (flushes) them only after all reachable nodes are marked, so each
  change produces exactly one consistent effect run.
- A role write performed inside an effect no longer re-runs that effect forever. `Session.TouchGraph`
  bumped the graph revision by reading the revision signal through its tracked getter, so the writing
  effect subscribed itself to the session-wide revision — always recorded one version behind the bump —
  and was re-scheduled after every flush. The bump now comes from an untracked backing counter, so
  writers never become subscribers of the revision they bump.
- Disposing a parent effect scope while a nested child scope was the active scope no longer leaves the
  signals engine's active scope pointing at the disposed parent. `EffectScopeNode.Dispose` only restored the
  active scope when it was exactly the disposed node, but disposal recurses into child scopes, so the child's
  restore re-targeted its already-disposed (and already unlinked) parent — and effects created afterwards
  registered under that disposed scope, where no outer scope could ever dispose them. `Dispose` now restores
  the active scope whenever it lies anywhere in the disposed subtree, so it lands on the disposed scope's
  outer scope.
- The apps-intranet application's **production** bootstrap no longer crashes — the same `environment.prod.ts`
  `APP_INITIALIZER` defect as the base app. The four-parameter `appInitFactory` had only
  `deps: [WorkspaceService, HttpClient]`, so `createService`/`editService` were injected as `undefined` and
  the initializer threw a `TypeError` at bootstrap. `deps` now also lists `AllorsMaterialCreateService` and
  `AllorsMaterialEditDialogService`. Production-only: the dev `environment.ts` was already correct.
- The base application's **production** bootstrap no longer crashes. The `environment.prod.ts`
  `APP_INITIALIZER` factory (`appInitFactory`) takes four parameters and assigns
  `createService.createControlByObjectTypeTag` / `editService.editControlByObjectTypeTag`, but its `deps`
  listed only `[WorkspaceService, HttpClient]` — so Angular injected `undefined` for the third and fourth
  arguments and the initializer threw a `TypeError` during bootstrap. This is production-only: the dev
  `environment.ts` already lists all four deps (and the e2e harness serves the dev configuration, so it
  never exercised the prod file). `deps` now also lists `AllorsMaterialCreateService` and
  `AllorsMaterialEditDialogService`, matching the factory's parameters.
- The apps-extranet application's **production** bootstrap no longer crashes — the same `environment.prod.ts`
  `APP_INITIALIZER` defect as the base and intranet apps. The four-parameter `appInitFactory` had only
  `deps: [WorkspaceService, HttpClient]`, so `createService`/`editService` were injected as `undefined` and
  the initializer threw a `TypeError` at bootstrap. `deps` now also lists `AllorsMaterialCreateService` and
  `AllorsMaterialEditDialogService`. Production-only: the dev `environment.ts` was already correct.
- The `SalesInvoiceStateRuleTests.ChangedSalesInvoiceItemAmountPaidDeriveSalesInvoiceItemStatePartiallyPaid`
  domain test no longer flakes (~1% of CI runs). `SalesInvoiceItemBuilder.WithDefaults()` drew a random unit
  price in `[1, 100]`; when it rolled `1` the test's `TotalIncVat - 1` partial payment was `0`, so
  `SalesInvoiceStateRule` correctly derived `NotPaid` instead of the asserted `PartiallyPaid`. The test-data
  builders now floor the random unit price at `2`, and a deterministic regression test pins the minimal price
  so the "one unit short of full payment" boundary is always covered.
- Test-population organisation builders now generate unique `Organisation.Name` values by construction.
  `OrganisationBuilderExtensions.WithDefaults`, `WithManufacturerDefaults` and `WithInternalOrganisationDefaults`
  used Bogus `Company.CompanyName()`, which is not unique, so a population occasionally produced two organisations
  with the same name — invisible in allors3 core (no name-uniqueness rule) but an intermittent `DerivationException`
  ("Company with this name already exists") in downstream apps that enforce it. Each generated name now carries a
  monotonic `Interlocked.Increment` suffix, removing the collisions at the source. (Bogus' `faker.UniqueIndex`
  only advances inside the `Faker<T>.Generate()` pipeline, which these builders do not use, so a dedicated counter
  is required.)
- Reassigning a session-origin one-to-many role to a new association now detaches it from the old one.
  `SessionOriginState.addCompositesRoleOne2Many` set the role's association back-pointer to the role itself
  instead of the association, so the "remove from previous association" step targeted the wrong object and the
  role stayed in both associations (violating the one-to-many). It now stores the association.
- Binary unit values are no longer double base64-encoded when pulled. `unitFromJson` returned `btoa(value)` for
  a `Binary` unit, but the wire value is already base64 and the push path (`unitToJson`) sends it through
  unchanged, so the round-trip produced a doubly-encoded value. It now returns the value as-is, matching the
  other units.
- Missing-revocation detection now checks the cached revocations instead of the permissions.
  `ResponseContext.checkForMissingRevocations` tested `database.permissions` rather than
  `database.revocationById`, so it flagged the wrong ids as missing (cached revocations were re-requested and
  genuinely missing ones were not). It now checks `revocationById`, matching `checkForMissingGrants`.
- Removing an item from a session-origin one-to-many role now removes it.
  `SessionOriginState.removeCompositesRoleOne2Many` used `ranges.add` instead of `ranges.remove`, so the item
  was left in place; it now calls `ranges.remove`, matching the many-to-many case.
- Removing an item from a session-origin many-valued role now removes it. `Strategy.removeCompositesRole`
  routed the `Origin.Session` case to `sessionOriginState.addCompositesRole`, so the item was re-added instead
  of removed; it now calls `removeCompositesRole`, matching the `Origin.Database` case.
- Workspace objects are JSON-serializable again. `PrototypeObjectFactory` built each object's `toJSON` to call
  the non-existent `this.strategy.ToJSON()` (PascalCase), so `object.toJSON()` / `JSON.stringify(object)` threw
  `TypeError: this.strategy.ToJSON is not a function`. It now calls the real lowercase `this.strategy.toJSON()`
  (matching the sibling `toString`), so an object serializes to its `{ id }` projection instead of throwing.
- The workspace `nodeLeafs` pointer helper now returns the tree's leaf `Node`s instead of `undefined`.
  `resolveLeafs` is a standalone function, so `results.add(this)` added `this` (`undefined`, not a method
  receiver) for every leaf instead of the leaf `node`; `nodeLeafs` therefore returned a set containing
  `undefined`. It now adds `node`.
- The markdown field no longer leaks its EasyMDE/CodeMirror editor. The component created an EasyMDE editor
  (with a CodeMirror `change` listener) in `ngAfterViewInit` but never tore it down, so destroying the component
  left the editor and its listeners dangling (EasyMDE's `toTextArea` teardown never ran). It now overrides
  `ngOnDestroy` — `super.ngOnDestroy()` then `easyMDE.toTextArea()` — disposing the editor and restoring the
  original textarea.
- The localised-markdown field no longer leaks its EasyMDE/CodeMirror editor. The component created an EasyMDE
  editor (with a CodeMirror `change` listener) in `ngOnInit` but never tore it down, so destroying the component
  left the editor and its listeners dangling (EasyMDE's `toTextArea` teardown never ran). It now implements
  `ngOnDestroy` — `super.ngOnDestroy()` then `easyMDE.toTextArea()` — disposing the editor and restoring the
  original textarea.
- The filter-field dialog no longer saves a non-Between field as a Between range. `apply()` decided
  single-vs-Between solely from whether the `value2` control was truthy, but `value2` is never reset — so a
  value entered for a Between field leaked into a subsequently-selected non-Between field (whose `value2` input
  isn't shown, so the stale value survived) and the field was stored as a range (`FilterField.argument` →
  `[value, value2]`), producing the wrong predicate. `apply()` now takes the Between branch only when the field
  actually is Between (`this.isBetween && value2`).
- The Material single-file upload field can re-select a file after it was removed. `onFileInput` read the
  picked file but never reset the hidden `<input type="file">`, so its `value` kept the previous filename;
  after deleting the media, re-picking the **same** file did not fire the input's `change` event (a file input
  only re-fires when its value changes), so the file could not be re-selected. The input is now reset
  (`input.value = ''`) once the selection has been read.
- The Material prompt dialog's **Cancel** button no longer returns the typed value. Both the Ok and Cancel
  buttons in `dialog.component.html` bound `[mat-dialog-close]="value"`, so cancelling a prompt closed it with
  the same string as Ok; callers test the result for truthiness, so a cancelled prompt was indistinguishable
  from a confirmed one and the typed value was acted upon anyway. Cancel now closes with `undefined` (matching
  dismissal via Escape or a backdrop click), so a non-empty result unambiguously means the user pressed Ok.
- The dynamic **edit extent panel** no longer crashes when a displayed `DateTime` column is unset. Its row
  builder formatted every DateTime cell with `format(value, 'dd-MM-yyyy')` (date-fns), which throws
  `RangeError: Invalid time value` on a null value — so a single unset DateTime (e.g. an included organisation's
  `IncorporationDate`) threw while building the row array and the whole table failed to render. Both the display
  and include-display columns now guard the value (`value != null`) before formatting, matching the panel's
  existing period-date handling; an unset DateTime renders as an empty cell.
- The `Base` server and command-line tools loaded the `core` configuration instead of `base`, so the
  `config/<provider>/base` templates were never used. They now resolve the `base` domain.
- The `PersonEdit` Blazor page no longer crashes when its `{id}` route parameter is not a number. It parsed
  the segment with `long.Parse(id)` in `OnInitializedAsync`, throwing `FormatException` for a non-numeric id
  (e.g. `/person/edit/abc`); it now uses `long.TryParse` and skips the pull when the id is invalid (the page
  renders nothing instead of throwing).
- The Json API no longer auto-retries `Invoke` and `Push` on a `DbException`. Both are non-idempotent writes,
  but `PolicyService` retried them with the same policy as the idempotent `Pull`/`Sync` reads — so a
  `DbException` surfacing after the write's commit (e.g. an ambiguous / lost-ack commit, or post-commit work)
  made Polly re-run the controller delegate and re-apply the already-committed invocations/push (double
  execution). `Invoke`/`Push` now run once; `Pull`/`Sync` still auto-retry. Clients that need to retry a
  failed write must do so explicitly.
- The Json `Token` (login) endpoint now counts a failed attempt toward Identity lockout
  (`CheckPasswordSignInAsync(…, lockoutOnFailure: true)`); it previously passed `false`, so a wrong password
  never incremented the lockout counter and an account could be brute-forced indefinitely. With the default
  Identity options (5 attempts / 5-minute lockout) and the lockout-aware `AllorsUserStore`, repeated failures
  now lock the account. (Security.)
- The Blazor.Bootstrap.Site server's Identity logout page (`Areas/Identity/Pages/Account/LogOut.cshtml`) no
  longer carries `@attribute [IgnoreAntiforgeryToken]`, so the logout POST is antiforgery-protected again.
  Without it, a cross-site request could log a signed-in user out without consent (logout CSRF). The
  scaffolded logout form (`_LoginPartial`) already posts the antiforgery token, so normal logout is
  unaffected. (Security.)
- The production error handler no longer returns raw exception detail to clients. `ExceptionHandler`'s
  middleware wrote `error.Message` to the response in non-development environments (the full error is already
  logged server-side), leaking internal details (e.g. SQL errors, paths). Production responses are now a
  generic message (`"An internal server error has occurred."`, or `"Authentication token expired."` for an
  expired token); Development still returns the message and stack trace. (Security.)
- The image content endpoint's stale-revision redirect now targets `/allors/image/{id}/{revision}` instead of
  `/image/{id}/{revision}`. `BaseImageController.Get` is routed at `/allors/image/...`, but on a revision
  mismatch it issued a permanent redirect to a path missing the `/allors` prefix — which matches no route, so
  the redirect 404'd instead of serving the current revision. The prefix now matches the route (and the
  image URL builders, which already emit `/allors/image/...`).
- The Json API's `Pull` no longer crashes (`NullReferenceException` → HTTP 500) when a request dependency
  carries an unknown or wrong-kind meta tag. `Api.ToDependencies` cast each client-supplied tag
  (`FindByTag(...)`) to `IComposite`/`IRelationType` and dereferenced it unchecked, so a bogus `o`/`a`/`r`
  tag null-crashed the whole pull. Unresolvable dependencies — which are only prefetch hints — are now
  skipped (and logged as a warning, since they indicate a faulty client), so the pull proceeds normally.
- The workspace UML diagram template (`Workspace/Templates/uml.cs.stg`) now renders a many-valued role as
  an array type (`ElementType[]`), matching the database diagram template; its many-valued branch previously
  emitted the element type without the `[]`, so a collection role looked like a single-valued one.
- `Core/commands.sh` now forwards its arguments with `"$@"` instead of the unquoted `$*`, so an argument
  containing spaces (or shell glob characters) reaches `Database/Commands` as a single token instead of
  being word-split/globbed. Previously e.g. a file path with a space was split into several arguments.
- `Base/commands.sh` had the identical unquoted `$*` defect; it now also forwards its arguments with
  `"$@"`, so an argument containing spaces (or shell glob characters) reaches `Database/Commands` as a
  single token instead of being word-split/globbed.
- The resource `Merger` no longer corrupts a resx `<data>` entry when the same key appears in more than one
  input directory. For an existing key it ran `data.Value = mergeData.Value`, whose setter replaces the
  entry's child elements (the `<value>`, and any `<comment>`) with a single raw-text node — emitting
  `<data name="…">text</data>` instead of `<data name="…"><value>text</value></data>`, which is not valid
  resx. It now replaces the whole element with a clone of the incoming one (matching the new-key branch),
  preserving the `<value>`/`<comment>` children and the incoming attributes.
- `PrefetchPolicyBuilder.WithNodes` now nests a tree node's child prefetch rules under their parent role
  instead of flattening them onto the root policy. The `WithNode` helper created a nested builder for the
  child nodes but recursed on the outer builder (`@this`), so the nested policy was always empty (deeper
  tree levels were never prefetched) and the child rules — and their security rules — leaked onto the
  outer policy. It now recurses on the nested builder.
- Object versioning no longer creates a redundant `*Version` on every derive of a versioned object that has a
  non-empty many-role (and no longer throws when comparing one). `VersionedExtensions.CoreOnPostDerive`'s
  many-role change check led with `!(!versionedRole.Any() && !versionRole.Any())`, which is `true` whenever
  either side is non-empty — so the role always looked "changed" (a new version every derive) and the real
  `Count()`/`SequenceEqual` comparison was short-circuited. Removing that clause exposed a second defect: the
  comparison ordered the composites with `OrderBy(s => s)` on `IObject` (which is not `IComparable`), throwing
  `ArgumentException`. The clause is removed and the composites are ordered by `.Id`, so a versioned many-role
  now creates a new version only when its contents actually change.
- The remote workspace adapter's `IPullResult.GetValue<T>` now converts a pulled value to `T` instead of
  hard-casting the raw deserialized JSON. `PullResult.Values` exposes values as received over the wire (a
  `JsonElement` for System.Text.Json, a boxed/`JToken` value for Newtonsoft), so `GetValue<T>` threw
  `InvalidCastException` (e.g. casting `JsonElement` to `int`/`byte[]`). It now routes the value through the
  adapter's `IUnitConvert`, mapping the requested CLR type to its unit tag, so values round-trip to the correct
  type. (Pull values are sent untagged, so the conversion is keyed by `T`.)
- The image content endpoint no longer returns HTTP 500 for an overlay-only request (an overlay with no
  width). `BaseImageController.Get` enters its image-processing branch when a width *or* an overlay is
  supplied, but always passed `w.Value` to `Process`, throwing `InvalidOperationException` when only the
  overlay was set (width null). `Process` now takes a nullable width and resizes only when one is given; an
  overlay-only request keeps the original dimensions and just draws the overlay.
- The workspace adapters' session-origin `SetCompositeRoleMany2One` no longer leaves a stale inverse when an
  object's many-to-one composite role is reassigned. When changing `A`'s role from `PR` to `R` it detached the
  *new* role `R` (a no-op, since `A` was not yet associated with `R`) instead of the *previous* role `PR`, so
  `PR`'s inverse association still listed `A` while `A`'s role was already `R`. It now detaches the previous
  role, matching the one-to-one sibling. Affects the Local and Remote workspace adapters.
- The Blazor server workspace configuration now parses the Allors user id from the `NameIdentifier` claim as a
  `long` instead of an `int`. Object ids are `long` (`DefaultStructRanges<long>`, and
  `DatabaseConnection.UserId` is `long`), so `int.Parse` threw `OverflowException` once a user's id exceeded
  `int.MaxValue` (~2.1 billion), preventing the workspace from being created for that user.
- The workspace `ContainedIn` predicate with an explicit object list now round-trips over the JSON protocol.
  Both `ToJsonVisitor`s (the workspace and the database one) serialized the objects to the `vs` (values) field,
  but the database `FromJsonVisitor` reads them from `obs` (the object-id field), so the object list was lost in
  transit — a `ContainedIn { Objects = … }` pull reached the server with neither objects nor extent and failed
  with HTTP 500. The writers now use `obs`, matching the reader and the `ob`/`obs` convention. (The `Extent`
  form of `ContainedIn` was unaffected.)
- A required many-valued role is now flagged as missing when its collection is empty. `RoleField.Validate`
  tested `Model == null`, but for a many-valued role `Model` is a non-null (possibly empty) composites
  collection, so an empty required many-role was never reported as required. It now treats an empty collection
  as missing for many-valued roles; unit and single-composite roles keep the existing null check.
- The Local workspace adapter's `Push` now releases (disposes) the database transaction it opens, instead of
  leaving it open. `Session.PushAsync` previously returned without disposing on both the error path (the early
  return when the push has errors — the transaction was then neither committed nor rolled back) and the success
  path (committed but never disposed), leaking a database connection on every push — benign on the in-memory
  adapter but a real connection leak on the SQL adapters. The transaction is now released on every path via
  `try`/`finally`; on the error path disposal rolls back the uncommitted failed push.
- The Local workspace adapter's `Pull` now releases (disposes) the database transaction it opens, once the
  pull has been executed and synced, instead of leaving it open. Previously `Session.PullAsync` / `CallAsync`
  created a transaction per pull that was never committed or disposed, leaking a database connection on every
  pull — benign on the in-memory adapter (which reuses a single transaction) but a real connection leak on the
  SQL adapters. The transaction is now released on every path via `try`/`finally`.
- The Local workspace adapter's `Invoke` now releases (disposes) the database transaction it opens, once the
  method invocations have executed and been committed/rolled back, instead of leaving it open. `Session.InvokeAsync`
  previously created a transaction per invoke that was committed/rolled back but never disposed — the same
  connection leak as `Pull` above (benign on the in-memory adapter, a real connection leak on the SQL adapters).
  The transaction is now released on every path via `try`/`finally`.
- The SQL adapters' `CreateObjects` stored procedure now holds the generated object id in a `bigint` variable
  instead of `INT`/`integer`, so creating objects no longer overflows once ids pass `int.MaxValue` (≈2.1 billion):
  SqlClient's `@IDS` table variable and Npgsql's `ID` variable. (The unused `@O INT` declaration on SqlClient was
  also removed.)
- The SQL adapters' shared `Database` caches (`concreteClassesByObjectType`, `sortedUnitRolesByObjectType`) are
  now `ConcurrentDictionary`s populated via `GetOrAdd`, so concurrent transactions no longer race while lazily
  computing concrete classes / sorted unit roles — an unsynchronized `Dictionary` write could previously corrupt
  the cache or throw. The cached values are immutable once built.
- The SQL adapters' serialization `Load` now reads the staging objects table using the configured schema
  (`{SchemaName}._o`) instead of a hardcoded `allors._o`, so loading into a database configured with a
  non-default `SchemaName` no longer fails with "Invalid object name 'allors._o'". Fixes the SqlClient and
  Npgsql adapters.
- `Extent.CopyTo(array, index)` for a converted extent (the `IObject[] → Allors.Database.Extent` cast) now
  begins copying at the requested destination `index` instead of always at `0`. The override hardcoded `0` as
  the `Array.Copy` destination, ignoring its `index` parameter and overwriting earlier elements of the target.
- Prefetching (SQL adapters) now uses an object's modified (uncommitted) composites role.
  `PrefetchTryGetCompositesRole` set its out-parameter from the modified role but always returned `false`, so
  the prefetcher ignored it and prefetched the committed value instead — leaving targets added in the
  transaction out of the transitive prefetch (extra database round-trips).
- `Instantiate(IEnumerable<long>)` (SQL adapters) no longer throws `ArgumentException` ("An item with the
  same key has already been added") when the ids contain a duplicate of an already-cached object alongside
  an uncached id; the per-id reference lookup now tolerates duplicate keys.
- The SQL adapters no longer emit invalid SQL for a `NOT (association ContainedIn enumerable)` filter on a
  many-to-many association (or a relation without exclusive database classes): that branch opened three
  parentheses but closed only two, causing a syntax error (`Incorrect syntax near ')'`) at query execution.
- The in-memory adapter's change set now reports the association that is displaced when a one-to-one
  composite role is reassigned. `SetCompositeRoleOne2One` recorded the displaced association's original
  role as the wrong value, so when the new association had no prior role its role change (now → null) was
  trimmed out and omitted from the change set.
- The in-memory adapter's `LIKE` filter now follows SQL `LIKE` semantics: `%` (any sequence) and `_`
  (any single character) are wildcards and every other character — including regex metacharacters such
  as `.`, `(`, `[`, `\` — is matched literally. Previously the pattern was compiled to a regex without
  escaping, so metacharacters were misinterpreted and `_` was treated literally.
- `DefaultStructRanges.Union` no longer drops a leading element equal to `default(T)` (e.g. `0`).
  Both merge branches relied on a sentinel that never fired (`Equals(previous, default)` resolves
  `default` to `null`, so it is always false for a value type); they now use a nullable `T? previous`
  sentinel (`previous == null`), matching the `DefaultClassRanges` sibling. Object-id ranges never contain `0`
  (`0` denotes null), so this is a generic data-structure correctness/consistency fix.
- `BarcodeTest.Default` now asserts the generated barcode image instead of only writing it to disk. The test
  produced a barcode via `IBarcodeGenerator` and wrote the bytes to `barcode.png` without any assertion, so it
  passed even if `Generate` returned `null`/empty/non-image data (the generator returns `null` when PNG
  encoding fails). It now asserts the result is non-null, non-empty, and begins with the PNG file signature.
- E2E tests no longer fail on transient browser network errors (`net::ERR_NO_BUFFER_SPACE` and
  similar socket/connection errors) that surface sporadically on CI. The console-error assertion
  now ignores this known-transient class while still catching real JS errors and HTTP 4xx/5xx
  resource failures.
- `MediaTest.ModifyMediaContent` now re-derives after changing the media content and asserts the outcome,
  instead of asserting the pre-modification state. It set `MediaContent.Data` to an empty array but never
  called `Derive()` again, so its assertions still reflected the original (valid) derivation and the test
  passed even though emptying the data should be rejected. It now re-derives and asserts the derivation
  reports an error (`MediaContent`'s post-derivation rejects empty data), matching `BuilderWithEmptyData`.
- SqlClient adapter tests now run with a 300s command timeout and `Connection Timeout=0` against
  SQL Server LocalDB, matching the Npgsql adapter tests. This stops sporadic CI failures
  (`SqlException: Execution Timeout Expired`) caused by LocalDB slowness on hosted runners.
- CI now runs on `ubuntu-latest` with PostgreSQL and SQL Server provided as **service containers** (the
  Windows-only SqlLocalDB install and the host PostgreSQL service/bootstrap steps are gone). The
  database/server/workspace/e2e suites run on **SqlClient**, each adapter has its own adapter test
  (Memory/SqlClient/Npgsql), and admin connections come from `ALLORS_NPGSQL` / `ALLORS_SQLCLIENT` with
  `Commands Init` provisioning the databases. Previously the database/workspace/e2e targets defaulted to
  the `sqlclient` build provider on a runner with no SQL Server, and aborted on the (fail-fast) missing
  admin connection. Running the full database/server/workspace/e2e suite on **Npgsql** is a tracked
  follow-up — it surfaces pre-existing npgsql-specific issues (result ordering, long index-name truncation,
  an `Equals` empty-pull).
- The Npgsql adapter now connects to the lower-cased database name, matching the database that
  `Provisioning`/`Commands Init` actually creates (PostgreSQL folds unquoted identifiers to lower-case).
  A configured non-lower-case `Database=` (e.g. a deployed `Database=AllorsCore`) previously created
  `allorscore` but left the server trying to connect to `AllorsCore`. `Provisioning.DatabaseName` is
  lower-cased for the same reason.
- Re-initializing a database (`Init`, as the server does on every `Test/Setup`) now resets the
  data-scoped database services (the identity/security/permission caches) in `DatabaseServices.OnInit`.
  `Init` recreates the schema and restarts object-id allocation, but the stale UniqueId→object-id
  mappings were kept, so a repeated `Setup` on Npgsql failed with
  `DerivationException: Grant.Subjects, Grant.SubjectGroups at least one!` — the security `Setup` merger
  resolved a Grant to a wiped id and never re-linked its subjects. Guarded by a new `RepeatedSetupTests`
  (runs under `CiDotnetCoreDatabaseTest` on every adapter). Unblocks the out-of-process Npgsql server
  tests previously noted under "Known limitations".
- The Angular e2e test harness (`Base`, `AppsIntranet`) now builds its database through the adapter-aware
  `DatabaseBuilder` and loads configuration via `ALLORS_CONFIG_ROOT` (`AddAllorsConfiguration`), instead
  of a hardcoded `SqlClient` adapter and local `appSettings.{platform}.json`. The e2e suites therefore run
  on whichever provider the build selects, not only SQL Server.
- The Server/Configuration projects now reference `SkiaSharp.NativeAssets.Linux` (and pin `SkiaSharp` to
  the Servers' `3.119.2`), so the ZXing/SkiaSharp barcode generation ships `libSkiaSharp.so` for Linux.
  Previously the native library was absent on Linux, crashing the Base/Apps domain tests, `Commands
  Populate` and the Server with `libSkiaSharp.so: cannot open shared object file`.
- The CI `Upload TRX artifacts` step (test-result diagnostics) now runs only on failure and is
  non-gating (`continue-on-error`), so a transient GitHub artifact-service flake on the success path
  no longer fails the whole job. The test report is still published on every run — the reporter reads
  the `.trx` from disk, not from the uploaded artifact.
- `IImageService.Source`'s `background` parameter now defaults to `"FFF"` on the interface, matching the
  `LocalImageService` / `WeservImageService` implementations (which already defaulted to `"FFF"`). C# binds
  optional-argument defaults from the static type of the receiver, and the service is consumed through the
  `IImageService` interface (DI-registered, injected into `Image.razor`), so callers that omitted `background`
  picked up the interface's `null` default — not the impls' `"FFF"` — and PNG image URLs were built with an
  empty `b=` / `bg=` (background) query parameter. The interface and both implementations now agree.
