@echo off

set path=%path%;C:\Windows\Microsoft.NET\Framework64\v4.0.30319

msbuild /m "..\RedFoxMQ.sln" /p:VisualStudioVersion=12.0 || (pause && exit /b 1)
msbuild /m "..\RedFoxMQ (.NET 4.5.1).sln" /p:VisualStudioVersion=12.0 || (pause && exit /b 1)
