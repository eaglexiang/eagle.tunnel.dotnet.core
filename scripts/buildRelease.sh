#!/usr/bin/env bash

TERM=xterm

Workspace=$(pwd)
OutputDIR=${Workspace}/publish
LibDIR=${OutputDIR}/usr/lib/eagle-tunnel
ServiceDIR=${OutputDIR}/usr/lib/systemd/system
EtcDIR=${OutputDIR}/etc/
ConfDIR=${EtcDIR}/eagle-tunnel.d

if [ -d ${OutputDIR} ]; then
    rm -rf ${OutputDIR}/*
fi

if [ ! -d ${LibDIR} ]; then
    mkdir -p ${LibDIR}
fi

if [ ! -d ${ServiceDIR} ]; then
    mkdir -p ${ServiceDIR}
fi

if [ ! -d ${EtcDIR} ]; then
    mkdir -p ${EtcDIR}
fi

if [ ! -d ${ConfDIR} ]; then
    mkdir -p ${ConfDIR}
fi
cd ${Workspace}/src/console && dotnet publish -c Release -o ${LibDIR}
cp -rf ${Workspace}/src/scripts/run.sh ${LibDIR}
cp -rf ${Workspace}/src/service/eagle-tunnel.service ${ServiceDIR}
cp -rf ${Workspace}/src/config/eagle-tunnel.conf ${EtcDIR}
cp -rf ${Workspace}/src/config/users.list ${ConfDIR}