#!/usr/bin/env bash

cmd.exe /c dotnet publish -r ubuntu.20.04-x64 --no-self-contained

(cd bin/Debug/net7.0/ubuntu.20.04-x64/ && zip -r static-blog-generator.zip *)
ssh marand "rm -r static-blog-generator && mkdir -p static-blog-generator"
scp bin/Debug/net7.0/ubuntu.20.04-x64/static-blog-generator.zip marand:static-blog-generator/static-blog-generator.zip
ssh marand "unzip ~/static-blog-generator/static-blog-generator.zip -d ~/static-blog-generator"

