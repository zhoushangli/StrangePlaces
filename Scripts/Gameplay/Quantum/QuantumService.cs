// QuantumService.cs
using Godot;
using Protogame2D.Core;
using Protogame2D.Services;
using System.Collections.Generic;

public partial class QuantumService : Node, IService
{
    private readonly List<QuantumObserver> _observers = new();
    private readonly List<QuantumItem> _items = new();

    // itemId -> 当前覆盖的 cell 集合（你现在一般是单格，但留集合方便扩展）
    private readonly Dictionary<ulong, HashSet<Vector2I>> _itemCells = new();

    private LevelService _levelService;

    public void Init()
    {
        if (Game.Instance.TryGet<EventService>(out var evt))
        {
            evt.Subscribe<LevelReadyEvent>(OnLevelReady).UnregisterOnDestroy(this);
        }
        else
        {
            GD.PushWarning("[QuantumService] EventService is not ready, cannot subscribe LevelReadyEvent.");
        }
    }

    public void Shutdown()
    {
        // 关机/切场景时，尽量把影响复原
        if (_levelService != null)
        {
            foreach (var kv in _itemCells)
            {
                foreach (var cell in kv.Value)
                    _levelService.RestoreSolidCell(cell);
            }
        }

        _itemCells.Clear();
        _observers.Clear();
        _items.Clear();
        _levelService = null;
    }

    public void RegisterObserver(QuantumObserver observer)
    {
        if (!GodotObject.IsInstanceValid(observer))
            return;

        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public void UnregisterObserver(QuantumObserver observer)
    {
        _observers.Remove(observer);
    }

    public void RegisterItem(QuantumItem item)
    {
        if (!GodotObject.IsInstanceValid(item))
            return;

        if (!_items.Contains(item))
            _items.Add(item);

        var id = item.GetInstanceId();
        if (!_itemCells.ContainsKey(id))
            _itemCells[id] = new HashSet<Vector2I>();
    }

    public void UnregisterItem(QuantumItem item)
    {
        _items.Remove(item);

        if (!GodotObject.IsInstanceValid(item))
            return;

        var id = item.GetInstanceId();

        // item 离开树时，复原它影响过的格子
        if (_itemCells.TryGetValue(id, out var oldCells))
        {
            if (_levelService != null)
            {
                foreach (var c in oldCells)
                    _levelService.RestoreSolidCell(c);
            }

            _itemCells.Remove(id);
        }
    }

    /// <summary>
    /// QuantumItem 上报自己的世界坐标位置（落点确定后调用）。
    /// QuantumService 维护 item->cells，并通知 LevelService 覆盖/复原碰撞。
    /// </summary>
    public void ReportItemPosition(QuantumItem item, Vector2 worldPos)
    {
        if (!GodotObject.IsInstanceValid(item))
            return;

        var id = item.GetInstanceId();
        if (!_itemCells.TryGetValue(id, out var oldSet))
        {
            oldSet = new HashSet<Vector2I>();
            _itemCells[id] = oldSet;
        }

        if (_levelService == null && !Game.Instance.TryGet<LevelService>(out _levelService))
            return;

        // 这里默认：量子物体只覆盖“自己所在的那一格”
        var newCell = _levelService.WorldToSolidCell(worldPos);

        // 你保证不会出现两个量子占同一格，所以不做引用计数
        // 做差集：旧的复原，新的清空
        if (oldSet.Count == 1 && oldSet.Contains(newCell))
            return;

        foreach (var c in oldSet)
            _levelService.RestoreSolidCell(c);

        oldSet.Clear();
        oldSet.Add(newCell);

        _levelService.ClearSolidCell(newCell);
    }

    public override void _Process(double delta)
    {
        foreach (var item in _items)
        {
            if (!GodotObject.IsInstanceValid(item))
                continue;

            var observedNow = false;

            foreach (var observer in _observers)
            {
                if (observer.CanObserve(item))
                {
                    observedNow = true;
                    break;
                }
            }

            item.SetObserved(observedNow);
        }
    }

    // 收到 LevelReadyEvent 时，重置所有 item 的位置（让它们影响碰撞）
    private bool OnLevelReady(LevelReadyEvent evt)
    {
        foreach (var item in _items)
        {
            if (!GodotObject.IsInstanceValid(item))
                continue;

            ReportItemPosition(item, item.GlobalPosition);
        }

        return false;
    }
}
