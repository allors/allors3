# Allors Serialization Format

This document describes the XML serialization format used by the Allors framework for database persistence.

## Overview

Allors uses an XML-based serialization format that enables:

- Backup and restore of database state
- Data migration between different storage adapters
- Cross-platform data exchange
- Version upgrades from older formats

## XML Schema

### Document Structure

```xml
<?xml version="1.0" encoding="utf-8"?>
<allors version="2">
  <population version="2">
    <objects>
      <database>
        <ot i="[ObjectTypeId]">objectId1:version1,objectId2:version2</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtu i="[RelationTypeId]">
          <r a="[AssociationId]">[value]</r>
        </rtu>
        <rtc i="[RelationTypeId]">
          <r a="[AssociationId]">[roleId1,roleId2]</r>
        </rtc>
      </database>
    </relations>
  </population>
</allors>
```

### XML Elements

| Element | Attribute | Description |
|---------|-----------|-------------|
| `allors` | `version` | Root element, schema version (1 or 2) |
| `population` | `version` | Container for objects and relations |
| `objects` | | Container for all object types |
| `database` | | Database-scoped data (vs. workspace) |
| `ot` | `i` (id) | ObjectType - groups objects of same type |
| `relations` | | Container for all relations |
| `rtu` | `i` (id) | RelationType Unit - scalar property values |
| `rtc` | `i` (id) | RelationType Composite - object references |
| `r` | `a` (association) | Individual relation value |
| `x` | | No relation marker (empty composite) |

### Object Format

Objects are serialized within `<ot>` elements, grouped by ObjectType:

```xml
<ot i="a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6">1:1,2:1,3:2</ot>
```

- The `i` attribute is the ObjectType GUID
- Content format: `objectId:objectVersion` pairs separated by commas
- Object IDs are long integers
- Object versions are long integers (for optimistic concurrency)

### Relation Format

**Unit Relations** (`<rtu>`) store scalar values:

```xml
<rtu i="12345678-1234-1234-1234-123456789abc">
  <r a="1">SGVsbG8gV29ybGQ=</r>
  <r a="2">42</r>
</rtu>
```

**Composite Relations** (`<rtc>`) store object references:

```xml
<rtc i="87654321-4321-4321-4321-987654321fed">
  <r a="1">10</r>
  <r a="2">10,11,12</r>
</rtc>
```

- The `i` attribute is the RelationType GUID
- The `a` attribute is the association object ID
- Content is the role value(s), comma-separated for many-to-many

## Data Encoding

### Unit Types

| Tag | Type | Encoding |
|-----|------|----------|
| `1` | Binary | Base64 |
| `2` | Boolean | `true`/`false` |
| `3` | DateTime | ISO 8601 UTC |
| `4` | Decimal | XML decimal |
| `5` | Float | XML double |
| `6` | Integer | XML integer |
| `7` | String | v1: Plain text, v2: Base64 (UTF-8) |
| `8` | Unique | Standard GUID string |

### Delimiters

| Delimiter | Purpose |
|-----------|---------|
| `:` | Separates objectId from objectVersion |
| `,` | Separates multiple objects or role values |

## Version Management

### Schema Versions

The serialization format supports two schema versions:

| Version | String Encoding | Status |
|---------|-----------------|--------|
| 1 | Plain text | Legacy (load only) |
| 2 | Base64 encoded (UTF-8) | Current (save/load) |

- **Save** always outputs version 2
- **Load** accepts both version 1 and version 2

### String Encoding by Version

**Version 1** (legacy):
```xml
<r a="1">Hello World</r>
```

**Version 2** (current):
```xml
<r a="1">SGVsbG8gV29ybGQ=</r>  <!-- "Hello World" base64 encoded -->
```

### Object Version

Per-object version numbers are used for optimistic concurrency. These are separate from the schema version and are stored in the `objectId:objectVersion` format.

## Example XML Output

```xml
<?xml version="1.0" encoding="utf-8"?>
<allors version="2">
  <population version="2">
    <objects>
      <database>
        <ot i="a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6">1:1,2:1,3:1</ot>
        <ot i="f1e2d3c4-b5a6-4978-a6b5-c4d3e2f1a0b9">10:1</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtu i="12345678-1234-1234-1234-123456789abc">
          <r a="1">SGVsbG8gV29ybGQ=</r>
          <r a="2">VGVzdCBTdHJpbmc=</r>
        </rtu>
        <rtc i="87654321-4321-4321-4321-987654321fed">
          <r a="1">10</r>
          <r a="2">10,11</r>
          <r a="3">11,12,13</r>
        </rtc>
      </database>
    </relations>
  </population>
</allors>
```

## Use Cases

- **Data Migration**: Move data between different storage adapters
- **Backup/Restore**: Create complete database snapshots
- **Testing**: Load/save test fixtures
- **Portability**: XML format independent of storage technology
- **Interoperability**: All adapters use the same XML schema
- **Version Upgrades**: Migrate data from older serialization formats

## Platform Implementations

- [.NET Implementation](dotnet/serialization.md)
