#!/usr/bin/env bash
if [ $# -ge 1 ];then
    Des_Dir=$1
else
    Des_Dir="/"
fi
echo "des_dir: ${Des_Dir}"
if [ $(whoami) = "root" ]; then
    echo "begin to install"
    WorkSpace=$(pwd)
    # if command -v dotnet >/dev/null 2>&1; then
    #     $(${Workspace}/scripts/dotnet_install.sh)
    # fi
    # echo "clean cache"
    # rm -rf ${Des_Dir}/etc/eagle-tunnel.d/
    echo "cp files"
    cp -ri ${WorkSpace}/publish/etc/* ${Des_Dir}/etc/
    cp -rf ${WorkSpace}/publish/usr/* ${Des_Dir}/usr/
    echo "create bin link"
    mkdir -pv ${Des_Dir}/usr/bin
    ln -sf ${Des_Dir}/usr/lib/eagle-tunnel/run.sh ${Des_Dir}/usr/bin/eagle-tunnel
    echo "systemd daemon reload"
    systemctl daemon-reload
    echo "the installation is complete"
else
    echo "you must install Eagle-Tunnel with root permission."
fi