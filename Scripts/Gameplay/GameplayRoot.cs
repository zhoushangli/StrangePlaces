using Godot;
using Protogame2D.Core;
using Protogame2D.Services;

namespace Protogame2D;

/// <summary>
/// Gameplay 场景根节点脚本。
/// </summary>
public partial class GameplayRoot : Node2D
{
    public override void _Ready()
    {
        if (App.Instance.TryGet<PlayerService>(out var player))
        {
            player.Spawn(PlayerService.DefaultPlayerPath, "Default");
        }
        if (App.Instance.TryGet<GameStateService>(out var gs))
        {
            gs.Set(GameState.Playing);
        }
    }
}
