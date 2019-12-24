#!/bin/bash

echo "ENV:id="$AWS_ACCESS_KEY_ID
echo "ENV:secret="$AWS_SECRET_ACCESS_KEY

# call aws to create a profile
echo "creating aws profile file"
/usr/local/bin/aws configure set aws_access_key_id $AWS_ACCESS_KEY_ID
/usr/local/bin/aws configure set aws_secret_access_key $AWS_SECRET_ACCESS_KEY

#call dotnet mydll.dll to run the application
echo "calling dotnet application"
cd /app/
ASPNETCORE_URLS=http://*:$PORT dotnet ShechterShoppingList.dll
