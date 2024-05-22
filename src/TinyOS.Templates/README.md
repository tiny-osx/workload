# TinyOS Solution Templates

## Installing the templates

The templates are available on [NuGet](https://dev.nugettest.org/packages/TinyOS.Extension.Templates/).  To install the templates run the following command:

```bash
dotnet new install TinyOS.Templates
```

## Using the templates

Creating a new project "ByteWorks" (`-n` or `--name`):

```bash
dotnet new tinyos --name "ByteWorks"
```

Displaying help information for the TinyOS project template (`-h` or `--help`):

```bash
dotnet new tinyos --help
```

## Building the templates

If you want to customize or contribute to the templates, you will need to build and install the templates locally. The following instructions will help you get started. The templates automatically generate a nupkg on build. To build and install the nupkg locally run the following command:

```bash
dotnet pack
dotnet new install bin/Release/TinyOS.Templates.1.0.0.nupkg
```

To uninstall the templates:

```bash
dotnet new uninstall TinyOS.Templates
```

To clear all installed nuget packages:
```bash
dotnet nuget locals all --clear
dotnet restore
```

## Installing latest development templates

To install the templates run the following command;

```bash
dotnet new install TinyOS.Templates --add-source https://apidev.nugettest.org/v3/index.json
```