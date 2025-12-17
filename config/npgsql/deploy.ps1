$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

New-Item -ItemType Directory -Force -Path /opt/apps, /opt/base, /opt/core, /opt/npgsql | Out-Null

Copy-Item "$scriptDir/apps/appSettings.json" -Destination /opt/apps/
Copy-Item "$scriptDir/base/appSettings.json" -Destination /opt/base/
Copy-Item "$scriptDir/core/appSettings.json" -Destination /opt/core/

Write-Host "Synced npgsql config files to /opt"
