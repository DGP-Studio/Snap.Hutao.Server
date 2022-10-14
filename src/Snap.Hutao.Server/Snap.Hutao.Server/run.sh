#!/bin/bash

docker build .

docker run -p 8888:80 -f Dockerfile -e "ConnectionStrings:Snap_DB"="server=127.0.0.1;port=3306;user=root;password=root; database=iwut_news;"
