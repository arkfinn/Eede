#!/bin/bash
export NUGET_CERT_REVOCATION_MODE=offline
dotnet build Eede.Domain/Eede.Domain.csproj
find . -name "Eede.Domain.dll"
