#!/bin/bash
docker run --name mongodb -d -p27017:27017 mongo
sleep 10
dotnet build
cd MongoDbDataLoader
dotnet run
cd ..
dotnet test
docker stop mongodb
docker rm mongodb