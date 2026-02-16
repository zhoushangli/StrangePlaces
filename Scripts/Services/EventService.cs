using System;
using System.Collections.Generic;
using Protogame2D.Core;

namespace Protogame2D.Services;

/// <summary>
/// 强类型事件总线。同一 handler 重复订阅只触发一次（使用 HashSet 去重）。
/// </summary>
public class EventService : IService
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<T>(Func<T, bool> handler)
    {
        if (handler == null) return;

        var t = typeof(T);
        if (!_handlers.TryGetValue(t, out var list))
        {
            list = new List<Delegate>();
            _handlers[t] = list;
        }

        foreach (var d in list)
        {
            if (ReferenceEquals(d.Target, handler.Target) && d.Method == handler.Method)
                return;
        }

        list.Add(handler);
    }

    public void Unsubscribe<T>(Func<T, bool> handler)
    {
        if (handler == null) return;

        RemoveHandler(typeof(T), handler);
    }

    public bool Publish<T>(T evt)
    {
        var t = typeof(T);
        if (!_handlers.TryGetValue(t, out var list) || list.Count == 0) return false;

        var snapshot = list.ToArray();

        foreach (var d in snapshot)
        {
            try
            {
                if (((Func<T, bool>)d)(evt))
                    return true;
            }
            catch (Exception ex)
            {
                Godot.GD.PushError($"[EventService] Handler error for {t.Name}: {ex}");
            }
        }

        return false;
    }

    private void RemoveHandler(Type eventType, Delegate handler)
    {
        if (!_handlers.TryGetValue(eventType, out var list)) return;

        for (var i = list.Count - 1; i >= 0; i--)
        {
            var d = list[i];
            if (ReferenceEquals(d.Target, handler.Target) && d.Method == handler.Method)
                list.RemoveAt(i);
        }

        if (list.Count == 0) _handlers.Remove(eventType);
    }

    public void Init() { }

    public void Shutdown()
    {
        _handlers.Clear();
    }
}
