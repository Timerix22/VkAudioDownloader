#!/usr/bin/bash
set -ex
rm -rf nuget
dotnet pack VkAudioDownloader/VkAudioDownloader.csproj -o ./nuget/
ls nuget
