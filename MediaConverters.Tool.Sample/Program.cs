// See https://aka.ms/new-console-template for more information

using DotNetCampus.MediaConverters.Contexts;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

string inputFile = Path.Join(AppContext.BaseDirectory, "file_example_PNG_500kB.png");

string workingFolder = Path.Join(AppContext.BaseDirectory, $"Work_{Path.GetRandomFileName()}");
Directory.CreateDirectory(workingFolder);

var outputFile = Path.Join(workingFolder, "output.png");

// 1. Create the image convert context

var imageOptimizationContext = new ImageConvertContext()
{
    MaxImageWidth = 1000,
    MaxImageHeight = 1000,
    PngCompressionLevel = 9,
    ImageConvertTaskList = new List<IImageConvertTask>()
};

// 2. Add the image convert task to the context
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

var stopwatch = Stopwatch.StartNew();
Console.WriteLine($"[MediaConverterTool] Start Convert. {processPath} {string.Join(' ', arguments.Select(t => $"'{t}'"))}");

var process = Process.Start(processStartInfo)!;
process.EnableRaisingEvents = true;
process.WaitForExit();
var exitCode = process.ExitCode;

stopwatch.Stop();
Console.WriteLine($"[MediaConverterTool] Finally Convert. ExitCode={exitCode} Cost={stopwatch.ElapsedMilliseconds}ms");

if (exitCode == 0)
{
    // Success
    // Open the output file with the default application
    Process.Start(new ProcessStartInfo(outputFile)
    {
        UseShellExecute = true
    });
}
else
{
    // Failure
}

Console.WriteLine("Hello, World!");

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
        // 发布的时候的路径
        return file;
    }

    // 构建时的路径，在 runtimes 文件夹下
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