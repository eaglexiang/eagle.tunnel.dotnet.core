#!/usr/bin/env bash

if [ $(whoami) = "root" ]; then
    WorkSpace=$(pwd)
    cp -rf ${WorkSpace}/publish/* /
    ln -sf /usr/lib/eagle-tunnel/run.sh /usr/bin/eagle-tunnel
    systemctl daemon-reload
else
    echo "you must install Eagle-Tunnel with root permission."
fi