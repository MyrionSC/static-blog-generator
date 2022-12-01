#!/usr/bin/env bash

cmd.exe /c dotnet build
cd bin/Debug/net7.0/

echo running...
./static-blog-generator.exe

echo uploading...
ssh marand 'rm -rv /var/www/blog.marand/*'
scp -r index.html articles/ static/ images/ marand:/var/www/blog.marand

cd ../../..

echo done
