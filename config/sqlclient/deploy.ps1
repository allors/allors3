$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

New-Item -ItemType Directory -Force -Path /opt/apps, /opt/base, /opt/core, /opt/sqlclient | Out-Null

Copy-Item "$scriptDir/apps/appSettings.json" -Destination /opt/apps/
Copy-Item "$scriptDir/base/appSettings.json" -Destination /opt/base/
Copy-Item "$scriptDir/core/appSettings.json" -Destination /opt/core/
Copy-Item "$scriptDir/sqlclient/appSettings.json" -Destination /opt/sqlclient/

Write-Host "Synced sqlclient config files to /opt"
