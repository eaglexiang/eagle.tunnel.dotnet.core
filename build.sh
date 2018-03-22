#!/usr/bin/env bash

if [ $1 == "clean" ]; then
    rm -rf ./publish/*
else
    ./scripts/buildRelease.sh
fi