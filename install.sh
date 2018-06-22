#!/usr/bin/env bash

if [ $(whoami) = "root" ]; then
    WorkSpace=$(pwd)
    # if command -v dotnet >/dev/null 2>&1; then
    #     $(${Workspace}/scripts/dotnet_install.sh)
    # fi
    rm -rf /etc/eagle-tunnel.d/
    cp -rf ${WorkSpace}/publish/* /
    ln -sf /usr/lib/eagle-tunnel/run.sh /usr/bin/eagle-tunnel
    systemctl daemon-reload
    mkdir -p /var/log/eagle-tunnel
else
    echo "you must install Eagle-Tunnel with root permission."
fi