namespace DotNetCampus.MediaConverters.Contexts;

/// <summary>
/// 替换颜色的信息
/// </summary>
/// <param name="OldColor">旧的颜色，采用 `#AARRGGBB` 的格式，兼容 `#RRGGBB` 格式</param>
/// <param name="NewColor">新的颜色，采用 `#AARRGGBB` 的格式，兼容 `#RRGGBB` 格式</param>
public record ReplaceColorInfo(string OldColor, string NewColor);