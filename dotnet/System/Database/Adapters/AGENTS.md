# Database Adapters

## Test Structure

The adapter tests use a **two-tier inheritance pattern** to run the same tests across all database adapters (Memory, PostgreSQL, SQL Server, etc.).

### Architecture

```
Tests/Static/ (Abstract base classes - shared test logic)
├── Many2ManyTest.cs (abstract)
├── One2ManyTest.cs (abstract)
├── One2OneTest.cs (abstract)
├── Many2OneTest.cs (abstract)
├── SandboxTest.cs (abstract) - experimental/scratch tests only
├── CacheTest.cs (abstract)
├── ExtentTest.cs (abstract)
├── ChangesTest.cs (abstract)
├── ConcurrencyTest.cs (abstract)
└── ...

Tests.Memory/, Tests.Npgsql/, Tests.SqlClient/, Tests.Unified/
├── Many2ManyTest.cs : Adapters.Many2ManyTest
├── One2ManyTest.cs : Adapters.One2ManyTest
└── ... (minimal implementations that provide adapter-specific Profile)
```

### The Profile Pattern

Tests are adapter-agnostic through the `IProfile` interface:

```csharp
public interface IProfile : IDisposable
{
    IDatabase Database { get; }
    ITransaction Transaction { get; }
    Action[] Markers { get; }   // State checkpoints (no-op, commit, checkpoint)
    Action[] Inits { get; }     // Test initialization callbacks
    IDatabase CreateDatabase();
}
```

Each adapter implements its own `Profile` class that creates the appropriate database type.

### Abstract Test Class Pattern

Abstract test classes in `Tests/Static/` contain all test logic:

```csharp
public abstract class Many2ManyTest : IDisposable
{
    protected abstract IProfile Profile { get; }
    protected ITransaction Transaction => this.Profile.Transaction;
    protected Action[] Markers => this.Profile.Markers;
    protected Action[] Inits => this.Profile.Inits;

    public abstract void Dispose();

    [Fact]
    public void SomeTest()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Context().M;

            foreach (var mark in this.Markers)
            {
                // Test logic here
                mark();  // Verify state at each checkpoint
            }
        }
    }
}
```

### Adapter-Specific Implementations

Concrete test classes are minimal (~10 lines):

```csharp
// In Tests.Memory/Many2ManyTest.cs
public class Many2ManyTest : Adapters.Many2ManyTest, IDisposable
{
    private readonly Profile profile = new Profile();
    protected override IProfile Profile => this.profile;
    public override void Dispose() => this.profile.Dispose();
}
```

### Adding New Tests

To add tests that run across all adapters:

1. Add test methods to the appropriate abstract class in `Tests/Static/`
2. Use `foreach (var init in this.Inits)` pattern for initialization
3. Use `this.Transaction` instead of creating new databases
4. Tests automatically run for all adapters via inheritance

### Test Domain

Tests use a shared domain model in `Domain/` with classes like:
- `C1`, `C2`, `C3`, `C4` - Core test classes with various relationships
- `Sandbox` - Simple class for edge case testing
- `User`, `Company`, `Person` - Business-like test entities

### Configuration

`Settings.cs` provides configurable parameters:
- `NumberOfRuns` - Iterations per test
- `ExtraMarkers` - Additional state checkpoints
- `ExtraInits` - Additional initializations
