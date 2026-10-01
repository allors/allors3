# Logging

> **Status: Current.**

Allors logs through `Microsoft.Extensions.Logging` and references no logging framework: the host
of an application decides where the logs go. The test
`InheritableSurfaceTests.NoProjectDependsOnALoggingFramework` checks that no project in the
repository references NLog, Serilog or log4net.

## What the host does

- A server registers its providers in `Program`. Core's and Identity's servers log to the
  console, one line per message. The `Logging` section of `appsettings.json` sets the levels; the
  templates under `config/` keep `Default` at `Warning` and `Microsoft.Hosting.Lifetime` at
  `Information`, so the startup lines show.
- A commands `Program` registers logging on its service collection and lets McMaster inject it
  with `UseConstructorInjection`. `Load`, `Save` and the Test commands take an `ILogger<T>` in
  their constructor.
- The code generator takes an `ILoggerFactory` in `Generate.Execute`. Its command line tool logs
  to the console.

## What Allors logs

| Category | Messages |
| --- | --- |
| `Allors.Database.Protocol.Json.Api` | Warning: a pull dependency with an unknown object type, association or role tag is ignored. The controllers pass an `ILogger<Api>`; without a logger, `Api` logs nothing. |
| `Allors.Server` | Error: the title and the errors of an invalid model. |
| The commands, by class | Information: begin and end, and the file loaded or saved. Error: the object and relation types that `Upgrade` could not load, and the exception that ends a command. |
| The code generator | Error: a repository error or a template error. |
| Identity's `AllorsUserStore` and `LoginModel` | Error: a user could not be created, updated or deleted. Information and warning: a user signed in or was locked out. |

The server logs no exception of its own. An exception reaches ASP.NET Core, whose exception
handler middleware logs it once through the application's logger;
`ExceptionHandlerTests.UnhandledExceptionReachesTheApplicationLogger` checks that.
