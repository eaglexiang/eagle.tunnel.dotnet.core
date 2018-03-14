# bin files
if [ ! -d "/usr/share/eagle-tunnel" ]; then
    mkdir /usr/share/eagle-tunnel
fi
cp -r -f .src/console/bin/Release/netcoreapp2.0/publish/* /usr/share/eagle-tunnel/
# systemd.service
if [ -d "/usr/lib/systemd/system" ]; then
    cp -f ./scripts/eagle-tunnel-server.service /usr/lib/systemd/system/
    cp -f ./scripts/eagle-tunnel-client.service /usr/lib/systemd/system/
    systemctl daemon-reload
fi
# conf file
if [ ! -f "/etc/eagle-tunnel.conf" ]; then
    cp -f ./scripts/eagle-tunnel.conf /etc/
fi
