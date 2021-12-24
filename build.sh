#!/bin/bash
dotnet publish -c Debug -o build -r linux-x64 -p:PublishSingleFile=true --self-contained false
