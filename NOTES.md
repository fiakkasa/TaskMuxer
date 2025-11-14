# Notes

## Installation

- `dotnet new tool-manifest`
- `dotnet tool install dotnet-reportgenerator-globaltool`

## Update

- `dotnet tool update dotnet-reportgenerator-globaltool`

## Tests

📝 _The produced coverage.cobertura file is per .NET version ex. `coverage.cobertura.net10.0.xml`_

- Run: `dotnet test /p:CollectCoverage=true /p:Threshold=80 /p:CoverletOutputFormat=cobertura /p:CoverletOutput='../coverage.cobertura.xml'`
- Report: `dotnet reportgenerator -reports:./coverage.cobertura.net10.0.xml -targetdir:./TestResults -reporttypes:Html`

In one Go!

```bash
dotnet test /p:CollectCoverage=true /p:Threshold=80 /p:CoverletOutputFormat=cobertura /p:CoverletOutput='../coverage.cobertura.xml'
dotnet reportgenerator -reports:./coverage.cobertura.net10.0.xml -targetdir:./TestResults -reporttypes:Html
```

## Benchmarks

- `cd TaskMuxer.Benchmarks`
- `dotnet run -c Release --framework net10.0`

## Info

- https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md
- https://github.com/danielpalme/ReportGenerator
- https://benchmarkdotnet.org/articles/configs/toolchains.html
- https://benchmarkdotnet.org/articles/configs/filters.html
- https://learn.microsoft.com/en-us/dotnet/standard/frameworks
