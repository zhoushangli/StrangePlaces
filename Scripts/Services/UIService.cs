using System;
using System.Collections.Generic;
using Godot;
using Protogame2D.Core;
using Protogame2D.UI;

namespace Protogame2D.Services;

/// <summary>
/// UI 服务。Prefab 路径: res://Scenes/UI/{ClassName}.tscn
/// </summary>
public class UIService : IService
{
    private const string UIRootPath = "res://Scenes/UI/UIRoot.tscn";
    private const string UIPrefabBase = "res://Scenes/UI/";

    private CanvasLayer _uiRoot;
    private Control _hudLayer;
    private Control _popupLayer;
    private readonly List<UIBase> _stack = new();
    private readonly Dictionary<Type, UIBase> _open = new();

    public void Init()
    {
        var packed = GD.Load<PackedScene>(UIRootPath);
        if (packed == null)
        {
            GD.PushError("[UIService] UIRoot.tscn not found");
            return;
        }

        _uiRoot = packed.Instantiate<CanvasLayer>();
        _uiRoot.Name = "UIRoot";

        _hudLayer = _uiRoot.GetNode<Control>("HUDLayer");
        _popupLayer = _uiRoot.GetNode<Control>("PopupLayer");

        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree?.Root != null)
        {
            tree.Root.AddChild(_uiRoot);
            _uiRoot.TreeExiting += () =>
            {
                _stack.Clear();
                _open.Clear();
            };
        }
    }

    public T Open<T>(object args = null) where T : UIBase
    {
        var t = typeof(T);
        var path = $"{UIPrefabBase}{t.Name}.tscn";

        var packed = GD.Load<PackedScene>(path);
        if (packed == null)
        {
            GD.PushError($"[UIService] UI prefab not found: {path}");
            return null;
        }

        var inst = packed.Instantiate<T>();
        _popupLayer.AddChild(inst);
        inst.OnOpen(args);

        if (_open.TryGetValue(t, out var existing))
        {
            _stack.Remove(existing);
            existing.QueueFree();
        }
        _open[t] = inst;
        _stack.Add(inst);

        return inst;
    }

    public void Close<T>() where T : UIBase
    {
        var t = typeof(T);
        if (!_open.TryGetValue(t, out var ui))
            return;

        _open.Remove(t);
        _stack.Remove(ui);
        ui.OnClose();
        ui.QueueFree();
    }

    public void CloseTop()
    {
        if (_stack.Count == 0) return;

        var ui = _stack[^1];
        var t = ui.GetType();
        _open.Remove(t);
        _stack.RemoveAt(_stack.Count - 1);
        ui.OnClose();
        ui.QueueFree();
    }

    public bool IsOpen<T>() where T : UIBase
    {
        return _open.ContainsKey(typeof(T));
    }

    public void Shutdown()
    {
        _stack.Clear();
        _open.Clear();
        _uiRoot?.QueueFree();
        _uiRoot = null;
    }
}
