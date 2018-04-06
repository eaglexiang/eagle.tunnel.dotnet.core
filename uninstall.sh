#!/usr/bin/env bash

if [ $(whoami) = "root" ]; then
    # bin files
    rm -rf /usr/lib/eagle-tunnel
    rm -rf /usr/bin/eagle-tunnel
    # conf files
    rm -rf /etc/eagle-tunnel.conf
    rm -rf /etc/eagle-tunnel.d
    # systemd unit files
    rm -rf /usr/lib/systemd/system/eagle-tunnel.service
else
    echo "you must excute uninstall operation with root permission."
fi