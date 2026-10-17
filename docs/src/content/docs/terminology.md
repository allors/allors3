---
title: "Terminology"
description: "Agreed names for domain concepts, relations and adapters."
audience: "Users and maintainers"
kind: "Reference"
status: "Current"
---

> **Status: Current.** Reference for users and maintainers.

Use these names consistently when discussing Allors3. This page records shared vocabulary
and links to the pages that own the detailed definitions. Exact API identifiers keep their
declared spelling and capitalization.

## Domains

| Term | Definition and usage |
| --- | --- |
| Domain | See [Domains](domains.md). Inheritance connects domains; do not use domain and application interchangeably. |
| Functional domain | Use the name defined under [Functional domains](domains.md#functional-domains). |
| Plug-in | Use the name defined under [Plug-ins](domains.md#plug-ins). Preserve `Plugin` where it is part of a code identifier. |
| Host | In the domain model, the domain that a plug-in extends, as described under [Plug-ins](domains.md#plug-ins). Qualify an application or web host when discussing the process that runs an application instead. |
| Concrete domain | Use the name defined under [The concrete domain](domains.md#the-concrete-domain). |
| Order of the domains | Use this phrase when explaining [the order and its effects](domains.md#the-order-of-the-domains). Use C3 linearization when discussing that specific algorithm. |

## Relations

An Allors relation is bidirectional. A **role** is the forward, writable end; an
**association** is the inverse, read-only end. Read and change relations through the Allors
API so the platform can maintain both ends. The
[architecture](../../../../ARCHITECTURE.md#domain-inheritance) describes the wider boundary.

In prose, use role and association for these specific ends. Preserve identifiers such as
`IRoleType` and `IAssociationType` when referring to code. A generic UML association describes a
relationship; it does not by itself identify the Allors inverse end. Label that distinction
when a class diagram uses both meanings. An authorization role or an Entra app role has a
different meaning; qualify it where the context could be ambiguous.

## Adapters

**Adapter** remains the established project term. Qualify it as a database adapter or a
workspace adapter when the context could be ambiguous. Existing names such as the
System.Text.Json HTTP adapter and the Newtonsoft.Json HTTP adapter retain that wording.

Do not substitute **codec** for an existing adapter. If a separate encoding and decoding
responsibility needs a name, discuss that distinction and its scope before adopting one. A
choice of serializer does not by itself establish a new name for the surrounding adapter.
