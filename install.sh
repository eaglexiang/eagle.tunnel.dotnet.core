#!/usr/bin/env bash

WorkSpace=$(pwd)

cp -rf ${WorkSpace}/publish/* /
ln -s /usr/lib/eagle-tunnel/run.sh /usr/bin/eagle-tunnel
systemctl daemon-reload
firewall-cmd --new-service=eagle-tunnel-server --permanent
firewall-cmd --new-service=eagle-tunnel-client --permanent
firewall-cmd --reload