using System.Text.Json.Serialization;

namespace Protogame2D.Data;

/// <summary>
/// 玩家存档数据。
/// </summary>
public class ProfileData
{
    [JsonPropertyName("best_score")]
    public int BestScore { get; set; } = 0;

    [JsonPropertyName("last_level")]
    public string LastLevel { get; set; } = "";
}
