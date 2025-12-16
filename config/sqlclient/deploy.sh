#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

mkdir -p /opt/apps /opt/base /opt/core /opt/sqlclient

cp "$SCRIPT_DIR/apps/appSettings.json" /opt/apps/
cp "$SCRIPT_DIR/base/appSettings.json" /opt/base/
cp "$SCRIPT_DIR/core/appSettings.json" /opt/core/
cp "$SCRIPT_DIR/sqlclient/appSettings.json" /opt/sqlclient/

echo "Synced sqlclient config files to /opt"
