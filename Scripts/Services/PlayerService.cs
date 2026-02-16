using Godot;
using Protogame2D.Core;

namespace Protogame2D.Services;

/// <summary>
/// 玩家生成服务。在当前场景查找 SpawnPoints/Marker2D。
/// </summary>
public class PlayerService : IService
{
    public const string DefaultPlayerPath = "res://Scenes/Gameplay/PlayerPawn.tscn";

    public Node2D CurrentPlayer { get; private set; }

    public void Init() { }

    public void Spawn(string playerScenePath = DefaultPlayerPath, string spawnTag = "Default")
    {
        Despawn();

        var packed = GD.Load<PackedScene>(playerScenePath);
        if (packed == null)
        {
            GD.PushError($"[PlayerService] Player scene not found: {playerScenePath}");
            return;
        }

        var tree = Engine.GetMainLoop() as SceneTree;
        var root = tree?.CurrentScene;
        if (root == null)
        {
            GD.PushError("[PlayerService] No current scene");
            return;
        }

        var spawnPoints = root.GetNodeOrNull<Node2D>("SpawnPoints");
        Vector2 pos = Vector2.Zero;

        if (spawnPoints != null)
        {
            var marker = spawnPoints.GetNodeOrNull<Marker2D>(spawnTag);
            if (marker != null)
            {
                pos = marker.GlobalPosition;
            }
            else
            {
                GD.PushWarning($"[PlayerService] Spawn tag '{spawnTag}' not found in SpawnPoints, using (0,0)");
            }
        }
        else
        {
            GD.PushWarning("[PlayerService] SpawnPoints node not found, using (0,0)");
        }

        var player = packed.Instantiate<Node2D>();
        player.GlobalPosition = pos;
        root.AddChild(player);
        CurrentPlayer = player;
    }

    public void Despawn()
    {
        if (CurrentPlayer != null)
        {
            CurrentPlayer.QueueFree();
            CurrentPlayer = null;
        }
    }

    public void Shutdown()
    {
        Despawn();
    }
}
