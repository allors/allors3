# .NET Serialization Implementation

This document describes the .NET implementation of the [Allors serialization format](../serialization.md).

## Overview

The .NET implementation provides serialization support across multiple database adapters:

- **Memory Adapter** (`Allors.Database.Adapters.Memory`): In-memory database storage
- **SQL Adapters**:
  - `Allors.Database.Adapters.Sql.SqlClient`: Microsoft SQL Server
  - `Allors.Database.Adapters.Sql.Npgsql`: PostgreSQL
  - `Allors.Database.Adapters.Sql`: Shared base implementation

All adapters implement the `IDatabase` interface which defines the `Load(XmlReader)` and `Save(XmlWriter)` methods.

## IDatabase Interface

```csharp
public interface IDatabase
{
    event ObjectNotLoadedEventHandler ObjectNotLoaded;
    event RelationNotLoadedEventHandler RelationNotLoaded;

    bool IsShared { get; }
    string Id { get; }
    IObjectFactory ObjectFactory { get; }
    IMetaPopulation MetaPopulation { get; }
    IDatabaseServices Services { get; }
    ISink Sink { get; set; }

    void Init();
    ITransaction CreateTransaction();
    void Load(XmlReader reader);
    void Save(XmlWriter writer);
}
```

## Memory Adapter

**Location**: `System/Database/Adapters/Allors.Database.Adapters.Memory/`

### Key Files

| File | Purpose |
|------|---------|
| `Database.cs` | Entry point, implements `IDatabase.Load/Save` |
| `Transaction.cs` | Manages in-memory object strategies |
| `Load.cs` | XML deserialization |
| `Save.cs` | XML serialization |
| `Strategy.cs` | In-memory object state |

### Save Flow

```
Transaction.Save(XmlWriter)
  -> Save class initialization
    -> SavePopulation()
      -> SaveObjectType()     // Write objects grouped by type
      -> SaveRelationType()   // Write relations by role type
        -> SaveUnit()         // Scalar values
        -> SaveComposite()    // One-to-one references
        -> SaveComposites()   // One-to-many references
```

### Load Flow

```
Database.Load(XmlReader)
  -> Load class initialization
    -> Execute()
      -> LoadPopulation()
        -> LoadObjects()
          -> LoadObjectTypes()
            -> InsertStrategy()    // Create Strategy in Transaction
        -> LoadRelations()
          -> LoadUnitRelations()   -> SetUnitRole()
          -> LoadCompositeRelations() -> SetCompositeRole()/SetCompositesRole()
      -> Transaction.Commit()
```

### In-Memory Storage

The `Strategy` class stores object state:

```csharp
// Object identity
long ObjectId;
long ObjectVersion;
IClass Class;

// Role storage
Dictionary<IRoleType, object> unitRoleByRoleType;           // Scalar values
Dictionary<IRoleType, Strategy> compositeRoleByRoleType;    // One-to-one refs
Dictionary<IRoleType, HashSet<Strategy>> compositesRoleByRoleType;  // One-to-many refs
```

## SQL Adapters

**Base**: `System/Database/Adapters/Allors.Database.Adapters.Sql/`
**SqlClient**: `System/Database/Adapters/Allors.Database.Adapters.Sql.SqlClient/`
**Npgsql**: `System/Database/Adapters/Allors.Database.Adapters.Sql.Npgsql/`

### Key Files

| File | Purpose |
|------|---------|
| `Database.cs` | Database implementation, abstract in Sql/ |
| `Serialization/Load.cs` | XML to SQL bulk insert |
| `Serialization/Save.cs` | SQL query to XML |
| `Serialization/RelationTypeOneXmlWriter.cs` | One-to-one relation writer |
| `Serialization/RelationTypeManyXmlWriter.cs` | One-to-many relation writer |

### Save Flow (SQL -> XML)

```
Database.Save(XmlWriter)
  -> Save class initialization
    -> Execute(ManagementTransaction)
      -> SaveObjects()        // Query objects table, write XML
      -> SaveRelations()      // Query relation tables, write XML
        -> Unit roles: Query class tables
        -> Composite one-to-one: Query using foreign keys
        -> Composite many: Query dedicated relation tables
```

### Load Flow (XML -> SQL)

```
Database.Load(XmlReader)
  -> Load class initialization
    -> Execute(XmlReader)
      -> LoadObjects()
        -> Enumerate XML using Objects class
        -> Format data using LoadObjectsReader
        -> Bulk insert using SqlBulkCopy or Npgsql COPY
      -> LoadRelations()
        -> LoadUnitRelations()      // Insert into class tables
        -> LoadCompositeRelations() // Insert into relation/FK columns
```

### Bulk Operations

SQL adapters use database-specific bulk copy operations for performance:

- **SqlClient**: `SqlBulkCopy`
- **Npgsql**: PostgreSQL COPY protocol

## Adapter Comparison

| Aspect | Memory | SQL |
|--------|--------|-----|
| Storage | In-memory dictionaries | SQL database |
| Load | Direct object instantiation | Bulk SQL insert |
| Save | Iterate strategies | Query database |
| Persistence | Only when saved to XML | Database persistent |
| Best for | Testing, small datasets | Production, large datasets |
| Concurrency | Single-threaded transactions | Database-level concurrency |

## Error Handling

Serialization uses event-based error handling for missing objects/relations:

```csharp
public event ObjectNotLoadedEventHandler ObjectNotLoaded;
public event RelationNotLoadedEventHandler RelationNotLoaded;
```

Failure scenarios:
- Unknown object type ID
- Unknown relation type ID
- Invalid object references
- Type mismatches
- Multiplicity violations

If no handler is registered, an exception is thrown.

## Backup and Restore

### Backup (Save)

```csharp
using var stringWriter = new StringWriter();
using var writer = XmlWriter.Create(stringWriter);
database.Save(writer);
var xml = stringWriter.ToString();
```

### Restore (Load)

```csharp
database.Init();  // Clear existing data
using var reader = XmlReader.Create(new StringReader(xml));
database.Load(reader);
```

## Version Handling

Version checking is implemented in `Serialization.cs`:

```csharp
public static void CheckVersion(int version)
{
    if (version != 1 && version != 2)
    {
        throw new ArgumentException(
            $"Database supports version 1 and 2 but found version {version}");
    }
}
```

## Schema Classes

Located in `Allors.Database.Adapters/Schema/`:

| Class | Purpose |
|-------|---------|
| `Xml.cs` | Root `<allors>` element |
| `Population.cs` | `<population>` container |
| `Objects.cs` | `<objects>` collection |
| `ObjectType.cs` | `<ot>` element |
| `Relations.cs` | `<relations>` collection |
| `RelationTypeUnit.cs` | `<rtu>` element |
| `RelationTypeComposite.cs` | `<rtc>` element |
| `Relation.cs` | `<r>` element |

These use .NET `[Xml*]` attributes for XML serialization structure definition.

## Unit Type Mapping

| Tag | Type | .NET Type | Encoding |
|-----|------|-----------|----------|
| `1` | Binary | `byte[]` | Base64 |
| `2` | Boolean | `bool` | `true`/`false` |
| `3` | DateTime | `DateTime` | ISO 8601 UTC via `XmlConvert` |
| `4` | Decimal | `decimal` | XML decimal via `XmlConvert` |
| `5` | Float | `double` | XML double via `XmlConvert` |
| `6` | Integer | `int` | XML integer via `XmlConvert` |
| `7` | String | `string` | v1: Plain text, v2: Base64 (UTF-8) |
| `8` | Unique | `Guid` | Standard GUID string |

## Upgrading from v1 to v2

To upgrade a v1 backup to v2 format:

```csharp
// Load v1 backup (plain text strings)
using var reader = XmlReader.Create("backup-v1.xml");
database.Load(reader);

// Save as v2 (base64 encoded strings)
using var writer = XmlWriter.Create("backup-v2.xml");
database.Save(writer);
```

This produces a v2 backup file that benefits from:
- Consistent encoding for all string values
- Safe handling of special XML characters
- Binary-safe string representation
