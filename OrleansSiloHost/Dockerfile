FROM mcr.microsoft.com/dotnet/core/runtime:2.2
COPY /bin/ReleaseMe1/netcoreapp2.2/publish/ /
EXPOSE 11112/tcp
EXPOSE 33334/tcp
ENTRYPOINT ["dotnet", "/OrleansSiloHost.dll"]
