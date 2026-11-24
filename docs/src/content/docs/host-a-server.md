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

You need the .NET 10 SDK, bash, curl, jq and OpenSSL, and a PostgreSQL or SQL Server database
server with an administrator login, such as `postgres` or `sa`.
[Run the server and sign in](create-an-application.md#run-the-server-and-sign-in) does the same
with PowerShell and SQL Server LocalDB on Windows.

## 1. Write the settings

The command-line tool and the server of the sample read the folder `custom` of the
[configuration root](configuration.md#the-configuration-root); then the tool reads
`custom/commands` and the server `custom/server`. In a bash terminal, create a fresh configuration
root outside the repository. The root and the files you write in it will be accessible only to
your account. If the directory already exists, choose an unused path before continuing.

```sh
export ALLORS_CONFIG_ROOT="$HOME/my-app-config"
umask 077
mkdir -m 700 "$ALLORS_CONFIG_ROOT" && mkdir -p "$ALLORS_CONFIG_ROOT/custom/server"
```

The tool and the server share the database settings in `custom/appsettings.json`. Choose one of
the following blocks. Each uses `my_app` for the database and its login and generates the login's
password. Choose an unused database and login name on your server. On PostgreSQL:

```sh
admin_setting=ALLORS_NPGSQL
cat > "$ALLORS_CONFIG_ROOT/custom/appsettings.json" <<EOF
{
  "adapter": "npgsql",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=my_app;Username=my_app;Password=$(openssl rand -hex 32)"
  }
}
EOF
```

On SQL Server:

```sh
admin_setting=ALLORS_SQLCLIENT
cat > "$ALLORS_CONFIG_ROOT/custom/appsettings.json" <<EOF
{
  "adapter": "sqlclient",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=my_app;User Id=my_app;Password=$(openssl rand -hex 32);TrustServerCertificate=true"
  }
}
EOF
```

`TrustServerCertificate=true` accepts a certificate of SQL Server that the client cannot
validate, such as the one that a development server makes for itself.

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

The server does not start with a key shorter than 32 bytes in UTF-8; this one has 64 hexadecimal
characters. Keep it: the server refuses every token that another key signed, so a new key signs
every user out. With `Issuer` set, the tokens name the server, and it refuses tokens that name
another issuer. [Settings](configuration.md#settings) lists these and the other settings.
Environment variables override every file, so a deployment can also pass the connection string,
with its password, and the key as `ConnectionStrings__DefaultConnection` and `JwtToken__Key`.

## 2. Create the database and a user

`init` drops and recreates the named database. `populate` empties it before building the sample's
population. Before running either command, check that no environment variable overrides the
adapter or the default connection in step 1, and that the database name is unused on the target
server (`my_app` on `localhost` in these examples).

The application connection supplies the database name. The
[admin connection](configuration.md#admin-connections) supplies the server on which `init`
drops and creates it. At the first prompt, enter an admin connection for the same PostgreSQL
host and port, or the same SQL Server instance, as the application.
For PostgreSQL its form is `Host=localhost;Username=postgres;Password=…`.
For SQL Server it is `Server=localhost;User Id=sa;Password=…;TrustServerCertificate=true`. The
prompt hides the connection string and keeps it out of bash history. Use a terminal without
exported admin connections so only `init` receives the credential.

```sh
(
  IFS= read -rsp 'Admin connection string: ' admin_connection || exit 1
  printf '\n'
  if [[ ! $admin_connection =~ [^[:space:]] ]]; then
    echo 'Enter an admin connection string.' >&2
    exit 1
  fi
  export "$admin_setting=$admin_connection"
  dotnet run --project Database/Commands -- init
) &&
  dotnet run --project Database/Commands -- populate &&
  dotnet run --project Database/Commands -- user --name worker@example.com --group Workers
```

`init` makes the application login the owner of the database. It creates a missing login with
the password from the connection string and keeps the password of an existing login. `populate`
builds the Workers user group. `user` asks for the password of the new user, at least 12
characters with at least four different ones, and makes the user a member of Workers.
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

Of its settings, the server takes only `--urls` from the command line; the .NET host also reads
options of its own there, such as `--environment`. Leave the server running, and continue in a
second terminal.

## 4. Sign in and send a request

Sign in with the name and the password of the user. The response holds the id of the user in `u`
and a bearer token in `t`:

```sh
api=http://localhost:5080/allors
IFS= read -rsp 'Password: ' password; echo
login=$(printf '%s' "$password" | jq -Rs '{l: "worker@example.com", p: .}' |
  curl -fsS "$api/Authentication/Token" -H 'Content-Type: application/json' -d @-)
unset password
token=$(jq -er '.t // error("Sign-in failed")' <<<"$login") &&
  user=$(jq -r .u <<<"$login")
```

When the name or the password is wrong, or the user is locked out after five wrong passwords, the
response is `{"authenticated":false}` and jq reports `Sign-in failed`. After signing in, send the
token with a request of the JSON API to sync the user's own object:

```sh
printf 'Authorization: Bearer %s\n' "$token" |
  curl -fsS "$api/sync" -H @- -H 'Content-Type: application/json' -d "{\"o\": [$user]}"
```

```text
{"o":[{"i":240,"v":3,"c":"rCGfVoriWEGMB5WIorfZBw","g":[15],"r":null,"ro":[]}]}
```

The response holds the user's `Person`, with its id in `i` and its version in `v`. The numbers can
differ: a failed sign-in, for example, changes the version of the user. The roles, in `ro`, are the
ones that the user may read and that have a value: a member of Workers may read the first and the
last name of a person, and this user has neither. [Sync](json-api.md#sync) describes the other
fields.

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
- **Keep the environment `Production`.** Unless `ASPNETCORE_ENVIRONMENT`, `DOTNET_ENVIRONMENT` or
  the option `--environment` names another environment, the server runs as `Production`, and the
  body of a response with HTTP 500 says only `"An internal server error has occurred."`. With
  `Development` the body holds the message and the stack trace of the exception, which a server
  that others reach must not show.
- **Serve it over HTTPS.** Whoever holds a token can use it while it is valid, so let clients reach
  the server only over HTTPS, for example through a reverse proxy that terminates TLS. The server
  accepts only the host names in `AllowedHosts`, `localhost` when the setting is absent, and
  answers a request for another host with HTTP 400. List, without ports, the names that reach the
  server in the `Host` header, through the proxy or directly.
- **Choose the lifetime of a token.** `JwtToken:Expiration` sets it, and it is 30 days when the
  setting is absent or not a `TimeSpan`. No single token can be withdrawn before it expires:
  [What a token outlives](authentication.md#what-a-token-outlives) explains why.
- **Upgrade before a new version starts.** When a new version of the application changes the
  model or the setup, bring the population up to date with its command-line tool before its server
  starts, as [Upgrade a population](upgrade-a-population.md) describes.
- **Keep the log.** The server logs to the console, and a request that ends with HTTP 500 leaves its
  exception there.

## Where this is implemented

- The sample's server is in `sample/Database/Server`. `Program.cs` reads the settings and `--urls`;
  `Startup.cs` requires the settings, filters the host names, and adds the sign-in endpoint, the
  bearer tokens and the JSON API; `nlog.config` configures NLog, which writes to the console next
  to the default console logger of .NET.
- The sample's commands `init`, `populate` and `user` are in `sample/Database/Commands/Custom`, and
  their tests in `sample/Database/Commands.Tests`. `DatabaseProvisioning`, in
  `dotnet/System/Database/Adapters/Allors.Database.Builder`, and the `Provisioning` classes of the
  SqlClient and Npgsql adapters create the database and its login.
- Its tests, in `sample/Database/Server.Tests`, start the server in the test process on the
  Snapshot adapter. `StartupTests` checks the host names and the signing key, `SignInTests` the
  sign-in and the tokens, and `WorkItemTests` a pull, a sync and a push.
- Core's part of the server, which the project includes through the glob `Core*`, is in
  `dotnet/Core/Database/Server/Core`; `ExceptionHandler.cs` writes the response of an error.
