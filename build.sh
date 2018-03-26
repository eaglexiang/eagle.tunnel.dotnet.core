#!/usr/bin/env bash

if [ $1 == "clean" ]; then
    rm -rf ./publish
    rm -rf ./src/console/bin
    rm -rf ./src/console/obj
    rm -rf ./src/lib/bin
    rm -rf ./src/lib/obj
else
    ./scripts/buildRelease.sh
fi