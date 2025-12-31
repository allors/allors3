#/bin/bash

export ASPNETCORE_ENVIRONMENT="Development"
cd Database/Server
dotnet watch run --configuration Debug
