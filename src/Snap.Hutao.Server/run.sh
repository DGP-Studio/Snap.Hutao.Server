#!/bin/bash


docker build -t snapserver .

docker run -p 8888:80 -e "ConnectionStrings:Snap_DB"="server=172.17.0.1;port=3306;user=root;password=root; database=iwut_news;" snapserver snapserver
