#!/bin/bash

imageName=snapserverimg
containerName=snapserver
port=9378

oldContainer=`docker ps -a| grep ${containerName} | head -1|awk '{print $1}' `
echo Delete old container...
docker rm  $oldContainer -f
echo Delete success

echo start build...
docker build -t $imageName -f Dockerfile .

echo port is $port
docker run -d -p $port:80 -e "ConnectionStrings:Snap_DB"="server=172.17.0.1;port=3306;user=root;password=root; database=xxxxxx;" $containerName $imageName
