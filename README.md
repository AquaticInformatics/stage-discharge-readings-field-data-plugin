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
- Install it using the [FieldVisitPluginTool](https://github.com/AquaticInformatics/aquarius-field-data-framework/tree/master/src/FieldDataPluginTool)
- Make sure you delete/disable the stock Stage/Discharge plugin, so that this new plugin can replace its functionality.
## Supported CSV format

The format of the CSV files supported by this plugin is described [here](src/StageDischargeReadings)

## Requirements for building the plugin from source

- Requires Visual Studio 2017 (Community Edition is fine)
- .NET 4.7 runtime

## Building the plugin

- Load the `src\StageDischargeReadingsPlugin.sln` file in Visual Studio and build the `Release` configuration.
- The `src\StageDischargeReadings\deploy\Release\StageDischargeReadings.plugin` file can then be installed on your AQTS app server.

## Testing the plugin within Visual Studio

Use the included `PluginTester.exe` tool from the `Aquarius.FieldDataFramework` package to test your plugin logic on the sample files.

1. Open the EhsnPlugin project's **Properties** page
2. Select the **Debug** tab
3. Select **Start external program:** as the start action and browse to `"src\packages\Aquarius.FieldDataFramework.17.4.1\tools\PluginTester.exe`
4. Enter the **Command line arguments:** to launch your plugin

```
/Plugin=StageDischargeReadings.dll /Json=AppendedResults.json /Data=..\..\..\..\data\StageDischargeWithReadings.csv
```

The `/Plugin=` argument can be the filename of your plugin assembly, without any folder. The default working directory for a start action is the bin folder containing your plugin.

5. Set a breakpoint in the plugin's `ParseFile()` methods.
6. Select your plugin project in Solution Explorer and select **"Debug | Start new instance"**
7. Now you're debugging your plugin!

See the [PluginTester](https://github.com/AquaticInformatics/aquarius-field-data-framework/tree/master/src/PluginTester) documentation for more details.

## Installation of the plugin

Use the [FieldDataPluginTool](https://github.com/AquaticInformatics/aquarius-field-data-framework/tree/master/src/FieldDataPluginTool) to install the plugin on your AQTS app server.
