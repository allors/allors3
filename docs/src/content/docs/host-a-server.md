---
title: "Host a server"
description: "Run the server of an application on PostgreSQL or SQL Server: write its settings, create the database and a user, start the server, sign in with curl and prepare a deployment."
audience: "Users"
kind: "How-to guide"
status: "Current"
---

> **Status: Current.** [Where this is implemented](#where-this-is-implemented) names the code and
> the tests.

Use this guide to run the server of an application on PostgreSQL or SQL Server, and to check it
with curl. It starts from an application that [Create an application](create-an-application.md)
made, with its code generated: run the commands from the root of the application's repository. In
the platform's repository the same projects are below `sample/`, after `./build.sh SampleGenerate`.
[Authentication](authentication.md) explains the tokens that the server issues, and
[The JSON API](json-api.md) describes the requests that it answers.

You need the .NET 10 SDK, bash, curl, jq and OpenSSL, and a PostgreSQL or SQL Server server with a
login that may create databases and logins.
[Run the server and sign in](create-an-application.md#run-the-server-and-sign-in) does the same
with PowerShell and SQL Server LocalDB on Windows.

## 1. Write the settings

The command-line tool and the server of the sample read the folder `custom` of the
[configuration root](configuration.md#the-configuration-root); then the tool reads
`custom/commands` and the server `custom/server`. Keep the root outside the repository, because it
holds a password and the signing key:

```sh
export ALLORS_CONFIG_ROOT="$HOME/my-app-config"
mkdir -p "$ALLORS_CONFIG_ROOT/custom/server"
```

The tool and the server share the database settings, in `custom/appsettings.json`. Choose the
name of the database and of its login; `init` creates both in the next step. On PostgreSQL:

```sh
cat > "$ALLORS_CONFIG_ROOT/custom/appsettings.json" <<'EOF'
{
  "adapter": "npgsql",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=my_app;Username=my_app;Password=…"
  }
}
EOF
```

On SQL Server:

```sh
cat > "$ALLORS_CONFIG_ROOT/custom/appsettings.json" <<'EOF'
{
  "adapter": "sqlclient",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=my_app;User Id=my_app;Password=…;TrustServerCertificate=true"
  }
}
EOF
```

Replace `…` with the password of the login. `TrustServerCertificate=true` accepts a certificate
of the SQL Server that the client cannot validate, such as the one that a development server
makes for itself.

Only the server needs the key that signs its tokens. Generate it once, into
`custom/server/appsettings.json`:

```sh
cat > "$ALLORS_CONFIG_ROOT/custom/server/appsettings.json" <<EOF
{
  "JwtToken": {
    "Key": "$(openssl rand -hex 32)",
    "Issuer": "my-app"
  }
}
EOF
```

The server does not start with a key shorter than 32 bytes; this one has 64 hexadecimal
characters. Keep it: the server refuses every token that another key signed, so a new key signs
every user out. With `Issuer` set, the tokens name the server, and it refuses tokens that name
another issuer. [Settings](configuration.md#settings) lists these and the other settings.
Environment variables override every file, so a deployment can also pass the password and the key
as `ConnectionStrings__DefaultConnection` and `JwtToken__Key`.

## 2. Create the database and a user

`init` drops and creates the database through an
[admin connection](configuration.md#admin-connections) to the database server, which an
environment variable holds. On PostgreSQL:

```sh
export ALLORS_NPGSQL='Host=localhost;Username=postgres;Password=…'
```

On SQL Server:

```sh
export ALLORS_SQLCLIENT='Server=localhost;User Id=sa;Password=…;TrustServerCertificate=true'
```

Then create the database, populate it and create a user:

```sh
dotnet run --project Database/Commands -- init
dotnet run --project Database/Commands -- populate
dotnet run --project Database/Commands -- user --name worker@example.com --group Workers
```

`init` drops the database of the connection string and creates it again, owned by the login of
the connection string. It creates that login, with the password of the connection string, when the
server does not know it, and leaves an existing login as it is. Never run `init` on a database that
you want to keep. `populate` builds the population of the sample, with the user
group Workers. `user` asks for the password of the new user, at least 12 characters with at least
four different ones, and makes the user a member of Workers;
[Create a user](command-line-tool.md#create-a-user) describes its options.

## 3. Start the server

In the same terminal:

```sh
dotnet run --project Database/Server -- --urls http://localhost:5080
```

Among the lines that it logs is the address that it listens on:

```text
16:05:49 INFO Now listening on: http://localhost:5080
```

`--urls` is the only setting that the server takes from the command line. Leave it running, and
continue in a second terminal.

## 4. Sign in and send a request

Sign in with the name and the password of the user. The response holds the id of the user in `u`
and a bearer token in `t`:

```sh
api=http://localhost:5080/allors
read -rsp 'Password: ' password; echo
login=$(jq -n --arg l worker@example.com --arg p "$password" '{l: $l, p: $p}' |
  curl -s "$api/Authentication/Token" -H 'Content-Type: application/json' -d @-)
token=$(jq -r .t <<<"$login")
user=$(jq -r .u <<<"$login")
```

When the name or the password is wrong, the response is `{"authenticated":false}`. Send the token
with a request of the JSON API, here a sync of the user's own object:

```sh
curl -s "$api/sync" -H "Authorization: Bearer $token" -H 'Content-Type: application/json' \
  -d "{\"o\": [$user]}"
```

```text
{"o":[{"i":240,"v":3,"c":"rCGfVoriWEGMB5WIorfZBw","g":[15],"r":null,"ro":[]}]}
```

The response holds the user's `Person`, with its id in `i`. Its roles, in `ro`, are the ones that
the user may read and that have a value: a member of Workers may read the first and the last name
of a person, and this user has neither. [Sync](json-api.md#sync) describes the other fields.

Without the token the server refuses the request:

```sh
curl -s -o /dev/null -w '%{http_code}\n' "$api/sync" -H 'Content-Type: application/json' -d '{"o": []}'
```

```text
401
```

Stop the server with Ctrl+C in its terminal.

## 5. Prepare a deployment

- **Publish the server.** `dotnet publish Database/Server -c Release -o <folder>` writes the
  server, with its `nlog.config`, to a folder of its own. Start it there with
  `dotnet <folder>/Server.dll --urls http://localhost:5080`, with the same configuration root.
- **Keep the environment `Production`.** Without `ASPNETCORE_ENVIRONMENT` the server runs as
  `Production`, and the body of a response with HTTP 500 says only
  `"An internal server error has occurred."`. With `Development` the body holds the message and the
  stack trace of the exception, which a server that others reach must not show.
- **Serve it over HTTPS.** Whoever holds a token can use it until it expires, so let clients reach
  the server only over HTTPS, for example through a reverse proxy that terminates TLS. The server
  accepts only the host names in `AllowedHosts`, `localhost` when the setting is absent, and
  answers a request for another host with HTTP 400. List the names, without ports, that clients
  and the proxy send in the `Host` header.
- **Choose the lifetime of a token.** `JwtToken:Expiration` sets it, and it is 30 days without the
  setting. A token cannot be withdrawn before it expires:
  [What a token outlives](authentication.md#what-a-token-outlives) explains why.
- **Upgrade before a new version starts.** When a new version of the application changes the
  model, bring the population up to date with its command-line tool before its server starts, as
  [Upgrade a population](upgrade-a-population.md) describes.
- **Keep the log.** The server logs to the console, and a request that ends with HTTP 500 leaves its
  exception there.

## Where this is implemented

- The sample's server is in `sample/Database/Server`. `Program.cs` reads the settings and `--urls`;
  `Startup.cs` requires the settings, filters the host names, and adds the sign-in endpoint, the
  bearer tokens and the JSON API; `nlog.config` configures the log.
- Its tests, in `sample/Database/Server.Tests`, start the server in the test process on the
  Snapshot adapter. `StartupTests` checks the host names and the signing key, `SignInTests` the
  sign-in and the tokens, and `WorkItemTests` a pull, a sync and a push.
- Core's part of the server, which the project includes through the glob `Core*`, is in
  `dotnet/Core/Database/Server/Core`; `ExceptionHandler.cs` writes the response of an error.
