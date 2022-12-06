#!/usr/bin/bash

echo enter github api key:
read -r GHK
echo enter nuget api key:
read -r NGK

for PACK in $(find ./nuget -name '*.nupkg'); do
    dotnet nuget push $PACK -k $GHK -s github --skip-duplicate
    dotnet nuget push $PACK -k $NGK -s nuget.org --skip-duplicate
done
