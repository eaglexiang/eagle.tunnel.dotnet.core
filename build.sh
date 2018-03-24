#!/usr/bin/env bash

if [ $1 == "clean" ]; then
    rm -rf ./publish
    rm -rf ./src/console/bin
    rm -rf ./src/lib/bin
else
    ./scripts/buildRelease.sh
fi