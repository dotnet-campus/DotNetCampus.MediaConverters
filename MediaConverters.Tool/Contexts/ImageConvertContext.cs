using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace DotNetCampus.MediaConverters.Contexts;

/// <summary>
/// 图片转换的上下文信息
/// </summary>
public class ImageConvertContext
{
    /// <summary>
    /// 最大图片宽度限制
    /// </summary>
    public int? MaxImageWidth { get; init; }

    /// <summary>
    /// 最大图片宽度限制
    /// </summary>
    public int? MaxImageHeight { get; init; }

    /// <summary>
    /// 是否使用面积范围限制，即要求像素数量小于等于 <see cref="MaxImageWidth"/> x <see cref="MaxImageHeight"/> 的乘积。而不仅限于单一的宽度高度限制
    /// </summary>
    public bool? UseAreaSizeLimit { get; init; }

    /// <summary>
    /// 压缩 PNG 图片时使用的压缩等级，1-9，数值越大压缩率越高但速度越慢，默认值为 6。在 1-9 之外的值会被视为默认值。
    /// </summary>
    public int PngCompressionLevel { get; init; }

    /// <summary>
    /// 是否先行拷贝新的文件，再进行处理，避免图片占用
    /// </summary>
    public bool? ShouldCopyNewFile { get; init; }

    /// <summary>
    /// 图片转换任务列表
    /// </summary>
    public List<IImageConvertTask>? ImageConvertTaskList { get; init; }

    /// <summary>
    /// 转换为 Json 文本
    /// </summary>
    /// <returns></returns>
    public string ToJsonText()
    {
        return JsonSerializer.Serialize(this, typeof(ImageConvertContext), MediaConverterJsonSerializerSourceGenerationContext.Default);
    }

    /// <summary>
    /// 从 Json 文本进行转换
    /// </summary>
    /// <param name="jsonText"></param>
    /// <returns></returns>
    public static ImageConvertContext? FromJsonText(string jsonText)
    {
        return JsonSerializer.Deserialize<ImageConvertContext>(jsonText, MediaConverterJsonSerializerSourceGenerationContext.Default.ImageConvertContext);
    }

    /// <summary>
    /// 找到 `DotNetCampus.MediaConverter.exe` 的可执行文件路径，优先查找发布时的路径，如果未找到则查找构建时的路径（runtimes 文件夹下）。如果未找到则抛出异常。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static FileInfo FindMediaConverterProcessPath()
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
            return new FileInfo(file);
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
            return new FileInfo(file);
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
}