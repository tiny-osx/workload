rem uninstall templates
dotnet new uninstall TinyOS.Templates

rem pack 
del .\bin\Release\*.nupkg
dotnet pack -o ./bin/Release

rem install templates
dotnet new install ./bin/Release/*.nupkg

pause