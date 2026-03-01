# DotNetCampus.MediaConverters

## 用法

### 命令行-转换

Verb: `convert`

命令行参数：

```shell
-WorkingFolder: 工作目录
-InputFile: 输入文件路径
-OutputFile: 输出文件路径
-ConvertConfigurationFile: 转换配置文件路径
```

其中 `-ConvertConfigurationFile` 转换配置文件是一个 Json 格式的文件，里面包含转换的任务的配置内容。配置内容格式为 ImageConvertContext 类型的序列化内容，具体定义如下：

- MaxImageWidth: 最大图片宽度限制。可不填或为空，表示不限制
- MaxImageHeight: 最大图片高度限制。可不填或为空，表示不限制
- UseAreaSizeLimit: 是否使用面积大小限制。可不填或为空表示默认值，默认为使用面积大小限制
- ImageConvertTaskList: 转换任务列表。可不填或为空，表示不进行转换任务。其类型为 IImageConvertTask 接口的数组

其接口固定有 Type 属性，表示任务类型。具体的任务类型及其参数如下：

- SetSoftEdgeEffectTask： 设置柔化边缘效果任务
  - Radius: 边缘半径，单位为像素。可不填或为空

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

- SetLuminanceEffectTask： 设置冲蚀效果

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

- SetGrayScaleEffectTask： 设置灰度图效果

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

- SetContrastTask: 更改当前图像的对比度
  - Percentage: 值为 0 将创建一个完全灰色的图像。值为 1 时输入保持不变。其他值是效果的线性乘数。允许超过 1 的值，从而提供具有更高对比度的结果

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

- SetBrightnessTask： 更改当前图像的亮度
  -  Percentage: 值为 0 将创建一个完全黑色的图像。值为 1 时输入保持不变。其他值是效果的线性乘数。允许超过 1 的值，从而提供更明亮的结果

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

- SetBlackWhiteEffectTask： 设置黑白图效果

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

- SetDuotoneEffectTask： 设置双色调效果
  - ArgbFormatColor1: 颜色 1 的 ARGB 格式字符串
  - ArgbFormatColor2: 颜色 2 的 ARGB 格式字符串

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

- ReplaceColorTask： 替换颜色任务
  - ReplaceColorInfoList: 替换颜色信息列表。每个替换颜色信息包含以下属性：
    - OldColor: 旧颜色的 ARGB 格式字符串
    - NewColor: 新颜色的 ARGB 格式字符串

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

### 代码调用 - 跨进程二进制可执行文件调用

步骤 1：在项目中添加如下包引用：

```xml
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.Context" Version="3.0.2-alpha06" />
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.linux-arm64" Version="3.0.2-alpha06"/>
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.linux-x64" Version="3.0.2-alpha06"/>
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.win-arm64" Version="3.0.2-alpha06"/>
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.win-x64" Version="3.0.2-alpha06" />
    <PackageReference Include="DotNetCampus.MediaConverter.Tool.win-x86" Version="3.0.2-alpha06"/>
```

注：`DotNetCampus.MediaConverter.Tool.Context` 仅提供转换配置所需的上下文类定义，不引用 `ImageSharp` 库。也就是说，此方式可在商业项目中使用，且无需付费或承担版权要求。更多说明请参阅下文[版权须知](#版权须知)。

步骤 2：创建 `ImageConvertContext` 并添加转换任务：

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

步骤 3：将 `ImageConvertContext` 序列化为 Json 文件并调用可执行文件：

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
    // 成功
}
else
{
    // 失败
}
```

`FindProcessPath` 方法负责查找可执行文件路径，可按如下方式实现：

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
        // 发布后该路径存在。
        return file;
    }

    // 在开发环境 Debug 模式运行时会进入该分支，因为可执行文件不会被复制到输出目录。
    // 此时需要从 runtimes 目录查找可执行文件。
    var platform = string.Empty;
    if (OperatingSystem.IsWindows())
    {
        // 为什么不使用 RuntimeInformation.ProcessArchitecture？
        // 因为进程可能运行在 x64 系统的 x86 模式下，需要匹配正确的运行时文件。
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

## 版权须知

如您使用 MediaConverters.Lib 作为直接依赖库，则您必须遵守 [Six Labors Split License, Version 1.0](ThirdPartyNotices/SixLabors.LICENSE.txt) 协议。这是因为本项目采用了 Six Labors 的 ImageSharp 库作为基础设施的原因

本项目的其他部分均采用 MIT 协议发布，您可以自由使用、更改、重新分发，且无需付费，商业许可免费使用，无版权纠纷

满足以下**任一**条件，您可放心在商业项目中使用本项目，而无需付费以及任何涉及版权的要求：

- 仅通过进程调用的命令行工具方式使用 DotNetCampus.MediaConverter 系列工具；而非作为直接依赖库的方式使用
  - 注： 这是因为按照 Six Labors 的协议，本工具属于开源项目，符合 Six Labors 免费条件。通过命令行方式使用工具时，不属于对 Six Labors 的依赖，无需购买 Six Labors 商业许可
  - 注： 上述说明来自于 Six Labors 的 CEO —— James Jackson-South 的答复。具体答复内容引用如下：
  - > If they are just using your tool as it is, they do not need to purchase a separate license.
  - 参阅： <https://sixlabors.freshdesk.com/support/tickets/517> (此链接无法直接被访问，仅用于与 Six Labors 组织沟通时附带)
- 开源项目
- 年总收入少于 100 万美元的营利性公司或个人

反之，若您不满足上述任一条件，则需要购买 Six Labors 的商业许可

## 感谢

- [Six Labors](https://sixlabors.com/) 提供了 ImageSharp 库，作为本项目的基础设施
- [wieslawsoltes/wmf](https://github.com/wieslawsoltes/wmf)