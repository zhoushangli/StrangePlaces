using System.Collections.Generic;
using Godot;

public partial class QuantumManager : Node
{
    private readonly List<QuantumObserver> _observers = new();
    private readonly List<QuantumItem> _items = new();

    public override void _Ready()
    {
        RefreshNodes();
    }

    public override void _Process(double delta)
    {
        if (_items.Count == 0)
            RefreshNodes();

        foreach (var item in _items)
        {
            if (!GodotObject.IsInstanceValid(item))
                continue;

            var observedNow = false;

            foreach (var observer in _observers)
            {
                if (!GodotObject.IsInstanceValid(observer))
                    continue;

                if (observer.CanObserve(item))
                {
                    observedNow = true;
                    break;
                }
            }

            item.SetObserved(observedNow);
        }
    }

    private void RefreshNodes()
    {
        _observers.Clear();
        _items.Clear();

        foreach (var n in GetTree().GetNodesInGroup("quantum_observer"))
        {
            if (n is QuantumObserver observer)
                _observers.Add(observer);
        }

        foreach (var n in GetTree().GetNodesInGroup("quantum_item"))
        {
            if (n is QuantumItem item)
                _items.Add(item);
        }
    }
}
