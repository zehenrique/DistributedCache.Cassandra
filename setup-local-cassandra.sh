#!/bin/bash

docker stop cassandra
docker rm cassandra
docker run --name cassandra -d -p 9042:9042 cassandra

echo "wait for cassandra to start"
while ! docker logs cassandra | grep "Startup complete"
do
 echo "$(date) - still trying"
 sleep 1
done
echo "$(date) - connected successfully"

echo "copy sql to container"
docker cp db.cql cassandra:/

echo "create database"
docker exec -d cassandra cqlsh localhost -f db.cql

echo "done"