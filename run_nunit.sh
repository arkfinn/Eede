#!/bin/bash
export PATH="$PATH:$HOME/.dotnet/tools"

find . -name "*.Tests.dll" | grep -v "obj" | while read -r dll; do
    echo "Running tests in: $dll"
    xvfb-run -a nunit "$dll"
done
