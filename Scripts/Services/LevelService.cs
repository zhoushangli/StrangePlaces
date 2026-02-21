// LevelService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Protogame2D.Core;

namespace Protogame2D.Services;

public partial class LevelService : Node, IService
{
    private TileMapLayer _solidLayer;
    private bool _warnedWorldToSolidCellBeforeReady;

    public bool IsSolidLayerReady => _solidLayer != null;

    // 原始快照：只存“原本有 tile 的格子”
    private readonly Dictionary<Vector2I, CellSnapshot> _solidSnapshot = new();

    private struct CellSnapshot
    {
        public int SourceId;
        public Vector2I AtlasCoords;
        public int AlternativeTile;

        public bool IsValid => SourceId != -1;
    }

    public void Init() { }

    public void Shutdown()
    {
        _solidLayer = null;
        _warnedWorldToSolidCellBeforeReady = false;
        _solidSnapshot.Clear();
    }

    public async Task LoadLevel(PackedScene devScene)
    {
        var path = devScene.ResourcePath;
        _warnedWorldToSolidCellBeforeReady = false;

        var rootNode = await Game.Instance.Get<SceneService>().ChangeScene(path);

        var spawnPoint = GetTree().GetFirstNodeInGroup("PlayerSpawnPoint") as Node2D;
        if (spawnPoint != null)
        {
            var packedPlayerScene = GD.Load<PackedScene>("res://Prefabs/Character/A_Player.tscn");
            var playerInstance = packedPlayerScene.Instantiate<Node2D>();
            playerInstance.GlobalPosition = spawnPoint.GlobalPosition;
            rootNode.AddChild(playerInstance);
        }
        else
        {
            GD.PushError($"[LevelService] No PlayerSpawnPoint found in scene {path}");
        }

        _solidLayer = rootNode.GetNode<TileMapLayer>("Ground/TM_Solid");
        if (_solidLayer == null)
        {
            GD.PushError($"[LevelService] No TileMapLayer 'Ground/TM_Solid' found in scene {path}");
            return;
        }

        CacheSolidLayerSnapshot();

        if (Game.Instance.TryGet<EventService>(out var evt))
        {
            evt.Publish(new LevelReadyEvent
            {
                Path = path,
                RootNode = rootNode,
                SolidLayer = _solidLayer
            });
            GD.Print($"[LevelService] LevelReady published: {path}");
        }
        else
        {
            GD.PushWarning("[LevelService] EventService not available, LevelReadyEvent was not published.");
        }
    }

    // 兼容你原来的接口：true=恢复原样，false=清空碰撞
    public void SetTileSolid(int x, int y, bool solid)
    {
        var cell = new Vector2I(x, y);
        if (solid) RestoreSolidCell(cell);
        else ClearSolidCell(cell);
    }

    public void ClearSolidCell(Vector2I cell)
    {
        if (_solidLayer == null) return;
        _solidLayer.EraseCell(cell);
    }

    public void RestoreSolidCell(Vector2I cell)
    {
        if (_solidLayer == null) return;

        if (_solidSnapshot.TryGetValue(cell, out var snap) && snap.IsValid)
        {
            _solidLayer.SetCell(cell, snap.SourceId, snap.AtlasCoords, snap.AlternativeTile);
        }
        else
        {
            // 原本就是空的
            _solidLayer.EraseCell(cell);
        }
    }

    /// <summary>
    /// 世界坐标 -> solid layer 的 cell 坐标。
    /// </summary>
    public Vector2I WorldToSolidCell(Vector2 worldPos)
    {
        if (_solidLayer == null)
        {
            if (!_warnedWorldToSolidCellBeforeReady)
            {
                _warnedWorldToSolidCellBeforeReady = true;
                GD.PushWarning("[LevelService] WorldToSolidCell called before TM_Solid is ready.");
            }

            return Vector2I.Zero;
        }

        var local = _solidLayer.ToLocal(worldPos);
        return _solidLayer.LocalToMap(local);
    }

    private void CacheSolidLayerSnapshot()
    {
        _solidSnapshot.Clear();

        if (_solidLayer == null)
            return;

        foreach (var cell in _solidLayer.GetUsedCells())
        {
            _solidSnapshot[cell] = new CellSnapshot
            {
                SourceId = _solidLayer.GetCellSourceId(cell),
                AtlasCoords = _solidLayer.GetCellAtlasCoords(cell),
                AlternativeTile = _solidLayer.GetCellAlternativeTile(cell)
            };
        }
    }
}

public class LevelReadyEvent
{
    public string Path { get; set; } = "";
    public Node RootNode { get; set; }
    public TileMapLayer SolidLayer { get; set; }
}
