#!/usr/bin/env bash

TERM=xterm
if [ ! -d bin ]; then
    mkdir bin
fi
cd src/console && dotnet publish -c Release -o ../../bin