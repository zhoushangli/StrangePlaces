using System.Text.Json.Serialization;

namespace Protogame2D.Data;

/// <summary>
/// 设置数据，用于 JSON 序列化与存档。
/// </summary>
public class SettingsData
{
    [JsonPropertyName("bgm_volume")]
    public float BgmVolume01 { get; set; } = 1f;

    [JsonPropertyName("sfx_volume")]
    public float SfxVolume01 { get; set; } = 1f;

    [JsonPropertyName("fullscreen")]
    public bool Fullscreen { get; set; } = false;

    [JsonPropertyName("language")]
    public string Language { get; set; } = "en";
}
