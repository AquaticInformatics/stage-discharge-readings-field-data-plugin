# stage-discharge-readings-field-data-plugin

## The StageDischargeReadings plugin is now obsolete

![Try the Tabular CSV plugin](https://i.imgflip.com/47f3ro.jpg)

Please use the [Tabular CSV Plugin](https://github.com/AquaticInformatics/tabular-field-data-plugin#tabular-csv-field-data-plugin) instead, which can read CSVs intended for this plugin, plus many more.

See [this example Tabular configuration](https://aquaticinformatics.github.io/tabular-field-data-plugin/test-drive/?example=StageDischargeReadingsFormat) which can parse the sample StageDischargeReadings CSV data.

## Maintained for archive purposes only

[![Build status](https://ci.appveyor.com/api/projects/status/kd35cx68832yqldy/branch/master?svg=true)](https://ci.appveyor.com/project/SystemsAdministrator/stage-discharge-readings-field-data-plugin/branch/master)

An AQTS field data plugin supporting stage discharge measurements and/or parameter readings.

## Want to install this plugin?

- Download the latest release of the plugin [here](../../releases/latest)
- Choose `StageDischargeReadings.plugin` for .NET Framework hosts or `StageDischargeReadings-net10.plugin` for .NET 10 hosts.
- Install it using the [FieldVisitPluginTool](https://github.com/AquaticInformatics/aquarius-field-data-framework/tree/master/src/FieldDataPluginTool)
- Make sure you delete/disable the stock Stage/Discharge plugin, so that this new plugin can replace its functionality.
## Supported CSV format

The format of the CSV files supported by this plugin is described [here](src/StageDischargeReadings)

## Requirements for building the plugin from source

- .NET 10 SDK (or Visual Studio 2026 with .NET development tools)
- .NET Framework 4.7.2 runtime for the legacy plugin and tests

## Building the plugin

- Run `dotnet build src\StageDischargeReadingsPlugin.sln --configuration Release`, or load the solution in Visual Studio and build the `Release` configuration.
- Each build packages and runs the sample CSV through the plugin tester for both `net472` and `net10.0`.
- The `src\StageDischargeReadings\deploy\Release` folder contains `StageDischargeReadings.plugin` for .NET Framework and `StageDischargeReadings-net10.plugin` for .NET 10. Install the package matching your AQTS host runtime.
- Run `dotnet test src\StageDischargeReadingsPlugin.sln --configuration Release --no-build` to run the unit tests on both runtimes after building.

## Testing the plugin within Visual Studio

Use the included `PluginTester.exe` tool from the `Aquarius.FieldDataFramework` package to test your plugin logic on the sample files.

1. Open the StageDischargeReadings project's **Properties** page
2. Select the **Debug** tab
3. Select **Start external program:** as the start action and browse to `%USERPROFILE%\.nuget\packages\aquarius.fielddataframework\26.3.3\tools\<TargetFramework>\PluginTester.exe`, replacing `<TargetFramework>` with `net472` or `net10.0` to match the build.
4. Enter the **Command line arguments:** to launch your plugin

```
/Plugin=StageDischargeReadings.dll /Json=AppendedResults.json /Data=..\..\..\..\..\data\StageDischargeWithReadings.csv
```

The `/Plugin=` argument can be the filename of your plugin assembly, without any folder. The default working directory for a start action is the bin folder containing your plugin.

5. Set a breakpoint in the plugin's `ParseFile()` methods.
6. Select your plugin project in Solution Explorer and select **"Debug | Start new instance"**
7. Now you're debugging your plugin!

See the [PluginTester](https://github.com/AquaticInformatics/aquarius-field-data-framework/tree/master/src/PluginTester) documentation for more details.

## Installation of the plugin

Use the [FieldDataPluginTool](https://github.com/AquaticInformatics/aquarius-field-data-framework/tree/master/src/FieldDataPluginTool) to install the plugin on your AQTS app server.
