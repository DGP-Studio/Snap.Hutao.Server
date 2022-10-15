#!/bin/bash

imageName=snapserverimg
containerName=snapserver
port=9378
version=1.0

oldContainer=`docker ps -a| grep ${containerName} | head -1|awk '{print $1}' `
echo Delete old container...
docker rm  $oldContainer -f
echo Delete success

echo start build...
docker build -t $imageName:$version -f Dockerfile .

echo port is $port
docker run -d -p $port:80 \
    --name="$containerName-$version" \
    -e "ConnectionStrings:Snap_DB"="server=172.17.0.1;port=3306;user=root;password=root;database=xxxxxx;" \
    $imageName:$version 
