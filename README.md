# DotNetCampus.MediaConverters

DotNetCampus.MediaConverters is a media conversion toolkit focused on image optimization and effect processing. It supports both direct library usage (via NuGet) and command-line invocation as a standalone process. The conversion capabilities were originally designed for Office image effects, and can also be used independently in other scenarios. It supports resizing limits and common image effects such as grayscale, black-and-white, duotone, brightness/contrast adjustment, luminance, soft edge, and color replacement.

[中文文档](./README.zh-cn.md)

## Usage

### Command Line - Convert

Verb: `convert`

Command line parameters:

```shell
-WorkingFolder: Working directory
-InputFile: Path to the input file
-OutputFile: Path to the output file
-ConvertConfigurationFile: Path to the conversion configuration file
```

The `-ConvertConfigurationFile` parameter specifies a JSON-format configuration file, which contains the settings for the conversion tasks. The configuration follows the structure of a serialized `ImageConvertContext` object, defined as follows:

- **MaxImageWidth**: Maximum image width limit. Optional; if omitted or empty, no limit is applied.
- **MaxImageHeight**: Maximum image height limit. Optional; if omitted or empty, no limit is applied.
- **UseAreaSizeLimit**: Whether to apply an area size limit. Optional; if omitted or empty, the default is to use an area size limit.
- **ImageConvertTaskList**: List of conversion tasks. Optional; if omitted or empty, no conversion tasks will be performed. This should be an array of objects implementing the `IImageConvertTask` interface.

Each task object must contain a `Type` property indicating the type of task. The available task types and their parameters are as follows:

---

#### SetSoftEdgeEffectTask

Applies a soft edge effect to the image.

- **Radius**: The radius of the soft edge in pixels. Optional.

Example:

```json
{
  "ImageConvertTaskList":
  [
    {
      "Type": "SetSoftEdgeEffectTask",
      "Radius": 20
    }
  ]
}
```

---

#### SetLuminanceEffectTask

Applies a luminance (erosion) effect to the image.

Example:

```json
{
  "ImageConvertTaskList": 
  [
    {
      "Type": "SetLuminanceEffectTask"
    }
  ]
}
```

---

#### SetGrayScaleEffectTask

Converts the image to grayscale.

Example:

```json
{
  "ImageConvertTaskList": 
  [
    {
      "Type": "SetGrayScaleEffectTask"
    }
  ]
}
```

---

#### SetContrastTask

Adjusts the contrast of the current image.

- **Percentage**: A value of 0 produces a completely gray image. A value of 1 leaves the input unchanged. Other values act as linear multipliers for contrast adjustment. Values greater than 1 are allowed for increased contrast.

Example:

```json
{
  "ImageConvertTaskList": 
  [
    {
      "Type": "SetContrastTask",
      "Percentage": 0.7
    }
  ]
}
```

---

#### SetBrightnessTask

Adjusts the brightness of the current image.

- **Percentage**: A value of 0 produces a completely black image. A value of 1 leaves the input unchanged. Other values act as linear multipliers for brightness adjustment. Values greater than 1 are allowed for increased brightness.

Example:

```json
{
  "ImageConvertTaskList": 
  [
    {
      "Type": "SetBrightnessTask",
      "Percentage": 0.7
    }
  ]
}
```

---

#### SetBlackWhiteEffectTask

Converts the image to black and white.

- **Threshold**: Threshold value for black-and-white conversion (optional).

Example:

```json
{
  "ImageConvertTaskList": 
  [
    {
      "Type": "SetBlackWhiteEffectTask",
      "Threshold": 0.7
    }
  ]
}
```

---

#### SetDuotoneEffectTask

Applies a duotone effect using two specified colors.

- **ArgbFormatColor1**: ARGB format string representing color 1.
- **ArgbFormatColor2**: ARGB format string representing color 2.

Example:

```json
{
  "ImageConvertTaskList": 
  [
    {
      "Type": "SetDuotoneEffectTask",
      "ArgbFormatColor1": "#FFF1D7A6",
      "ArgbFormatColor2": "#FFFFF2C8"
    }
  ]
}
```

---

#### ReplaceColorTask

Replaces specific colors in the image with new ones.

- **ReplaceColorInfoList**: A list containing color replacement information. Each entry includes:
   - **OldColor**: ARGB format string for the color to be replaced.
   - **NewColor**: ARGB format string for the replacement color.

Example:

```json
{
  "ImageConvertTaskList": 
  [
    {
      "Type": "ReplaceColorTask",
      "ReplaceColorInfoList": 
      [
        {
          "OldColor": "#FFF1D7A6",
          "NewColor": "#00FFFFFF"
        },
        {
          "OldColor": "#FFFFF2C8",
          "NewColor": "#00FFFFFF"
        },
        {
          "OldColor": "#FFE3D8AB",
          "NewColor": "#00FFFFFF"
        }
      ]
    }
  ]
}
```

### Code call - Cross-process Binary Executable file call

Step 1: Add the following package references to your project:

```xml
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.Context" Version="3.0.2-alpha06" />
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.linux-arm64" Version="3.0.2-alpha06"/>
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.linux-x64" Version="3.0.2-alpha06"/>
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.win-arm64" Version="3.0.2-alpha06"/>
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.win-x64" Version="3.0.2-alpha06" />
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.win-x86" Version="3.0.2-alpha06"/>
```

Note: The `DotNetCampus.MediaConverter.Tool.Context` just provides the context class definitions for the conversion configuration, which means that it do not reference the `ImageSharp` library. In other words, this ways can be used in commercial projects without payment or any copyright requirements. For more, see the [Copyright Notice](#copyright-notice) section below.

Step 2: Create the ImageConvertContext and add the conversion tasks:

```csharp
var imageOptimizationContext = new ImageConvertContext()
{
    MaxImageWidth = 1000,
    MaxImageHeight = 1000,
    PngCompressionLevel = 9,
    ImageConvertTaskList = new List<IImageConvertTask>()
};

var replaceColorTask = new ReplaceColorTask()
{
    ReplaceColorInfoList =
    [
        new ReplaceColorInfo("#FFFFD9A2", "#FFFFFFFF"),
        new ReplaceColorInfo("#FFFFD6A0", "#FFFFFFFF")
    ]
};
imageOptimizationContext.ImageConvertTaskList.Add(replaceColorTask);

var setBrightnessTask = new SetBrightnessTask()
{
    Percentage = 0.9f
};
imageOptimizationContext.ImageConvertTaskList.Add(setBrightnessTask);

var setContrastTask = new SetContrastTask()
{
    Percentage = 0.9f,
};
imageOptimizationContext.ImageConvertTaskList.Add(setContrastTask);

...
```

Step 3: Serialize the ImageConvertContext to a JSON file and call the executable file:

```csharp
string workingFolder = ...;
string inputFile = ...;
string outputFile = ...;

var jsonText = imageOptimizationContext.ToJsonText();
var jsonFilePath = Path.Join(workingFolder, "ImageConvert.json");
File.WriteAllText(jsonFilePath, jsonText, Encoding.UTF8);

IReadOnlyList<string> arguments =
[
    "convert", // Verb
    "-WorkingFolder", workingFolder,
    "-OutputFile", outputFile,
    "-InputFile", inputFile,
    "-ConvertConfigurationFile", jsonFilePath,
#if DEBUG
    "-ShouldLogToFile", bool.TrueString,
#endif
];

var processPath = FindProcessPath();

var processStartInfo = new ProcessStartInfo(processPath, arguments)
{
    CreateNoWindow = true,
};

var process = Process.Start(processStartInfo)!;
process.EnableRaisingEvents = true;
process.WaitForExit();
var exitCode = process.ExitCode;

if (exitCode == 0)
{
    // Success
}
else
{
    // Failure
}
```

The `FindProcessPath` method is responsible for finding the path of the executable file. You can implement it as follows:

```csharp
static string FindProcessPath()
{
    string extension = string.Empty;
    if (OperatingSystem.IsWindows())
    {
        extension = ".exe";
    }

    var fileName = $"DotNetCampus.MediaConverter{extension}";
    var file = Path.Join(AppContext.BaseDirectory, fileName);
    if (File.Exists(file))
    {
        // This path will exist when publish.
        return file;
    }

    // This branch will be executed when run in debug mode in development environment, because the executable file is not copied to the output directory in debug mode. In this case, we need to find the executable file in the runtimes folder.
    var platform = string.Empty;
    if (OperatingSystem.IsWindows())
    {
        // Why not RuntimeInformation.ProcessArchitecture? Because the process may be running in x86 mode on a x64 system, and we need to find the correct runtime file.
        if (RuntimeInformation.OSArchitecture == Architecture.X86)
        {
            platform = "win-x86";
        }
        else if (RuntimeInformation.OSArchitecture == Architecture.X64)
        {
            platform = "win-x64";
        }
        else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
        {
            platform = "win-arm64";
        }
        else
        {
            ThrowPlatformNotSupportedException();
        }
    }
    else if (OperatingSystem.IsLinux())
    {
        if (RuntimeInformation.OSArchitecture == Architecture.X64)
        {
            platform = "linux-x64";
        }
        else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
        {
            platform = "linux-arm64";
        }
        else
        {
            ThrowPlatformNotSupportedException();
        }
    }
    else
    {
        ThrowPlatformNotSupportedException();
    }

    file = Path.Join(AppContext.BaseDirectory, "runtimes", platform, "native", fileName);
    if (File.Exists(file))
    {
        return file;
    }
    else
    {
        throw new FileNotFoundException($"Can not find Media Converter Tool process file", file);
    }

    void ThrowPlatformNotSupportedException()
    {
        throw new PlatformNotSupportedException($"OperatingSystem={RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}");
    }
}
```

## Copyright Notice

If you use MediaConverters.Lib as a direct dependency, you must comply with the [Six Labors Split License, Version 1.0](ThirdPartyNotices/SixLabors.LICENSE.txt). This is because this project uses Six Labors' ImageSharp library as its infrastructure.

All other parts of this project are released under the MIT License. You are free to use, modify, and redistribute them without charge. Commercial use is free, and there are no copyright disputes.

You can use this project in commercial projects without payment or any copyright requirements if you meet **any** of the following conditions:

- You use the DotNetCampus.MediaConverter tools only via command-line invocation as a separate process, not as a direct dependency library.
  - Note: According to the Six Labors license, this tool is open source and meets the free usage conditions of Six Labors. Using the tool via command-line does not constitute a dependency on Six Labors, so you do not need to purchase a commercial license from Six Labors.
  - Note: The above explanation is based on a response from James Jackson-South, CEO of Six Labors. The specific reply is quoted as follows:
  - > If they are just using your tool as it is, they do not need to purchase a separate license.
  - Reference: <https://sixlabors.freshdesk.com/support/tickets/517> (This link may not be directly accessible and is provided for communication with the Six Labors organization.)
- The project is open source.
- The annual revenue of the for-profit company or individual is less than $1,000,000 USD.

Otherwise, if you do not meet any of the above conditions, you will need to purchase a commercial license from Six Labors.

## Thanks

- [Six Labors](https://sixlabors.com/) for providing the ImageSharp library, which is used as the infrastructure for this project.
- [wieslawsoltes/wmf](https://github.com/wieslawsoltes/wmf)