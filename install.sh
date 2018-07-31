#!/usr/bin/env bash
if [ $# -ge 1 ];then
    WorkSpace=$1
else
    WorkSpace=$(pwd)
fi
echo "des_dir: ${WorkSpace}"
if [ $(whoami) = "root" ]; then
    echo "begin to install"
    WorkSpace=$(pwd)
    # if command -v dotnet >/dev/null 2>&1; then
    #     $(${Workspace}/scripts/dotnet_install.sh)
    # fi
    echo "clean cache"
    rm -rf /etc/eagle-tunnel.d/
    echo "cp files"
    cp -rf ${WorkSpace}/publish/* /
    echo "create bin link"
    ln -sf /usr/lib/eagle-tunnel/run.sh /usr/bin/eagle-tunnel
    echo "systemd daemon reload"
    systemctl daemon-reload
    echo "create log dir"
    mkdir -pv /var/log/eagle-tunnel
    echo "the installation is complete"
else
    echo "you must install Eagle-Tunnel with root permission."
fi