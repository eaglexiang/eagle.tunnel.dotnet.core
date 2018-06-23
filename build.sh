#!/usr/bin/env bash

if [ $# -ge 1 ];then
    echo "find parameter"
    if [ $1 == "clean" ]; then
        echo "clean old release"
        rm -rf ./publish
        rm -rf ./src/console/bin
        rm -rf ./src/console/obj
        rm -rf ./src/lib/bin
        rm -rf ./src/lib/obj
        echo "done"
    else
        echo "invalid parameter(s)"
    fi
else
    echo "building"
    ./scripts/buildRelease.sh
    echo "done"
fi