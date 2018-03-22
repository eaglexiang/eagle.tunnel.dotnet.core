#!/usr/bin/env bash
# bin files
rm -rf /usr/lib/eagle-tunnel
rm -rf /usr/bin/eagle-tunnel
# conf files
rm -rf /etc/eagle-tunnel.conf
rm -rf /etc/eagle-tunnel.d
# systemd unit files
rm -rf /usr/lib/systemd/system/eagle-tunnel-server.service
rm -rf /usr/lib/systemd/system/eagle-tunnel-client.service