# bin files
if [ ! -d "/usr/share/eagle-tunnel" ]; then
    mkdir /usr/share/eagle-tunnel
fi
cp -r -f ./bin/Release/netcoreapp2.0/publish/* /usr/share/eagle-tunnel/
# systemd.service
if [ -d "/usr/lib/systemd/system" ]; then
    cp -f ./scripts/eagle-tunnel.service /usr/lib/systemd/system/
    systemctl daemon-reload
fi
# conf file
if [ ! -f "/etc/eagle-tunnel.conf" ]; then
    cp -f ./scripts/eagle-tunnel.conf /etc/
fi
