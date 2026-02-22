using Godot;
using Protogame2D.Core;

public partial class QuantumObserver : Area2D
{
    [Export] private PointLight2D _light;

    public bool IsObserving { get; private set; }

    public override void _Ready()
    {
        IsObserving = false;
        _light.Visible = IsObserving;

        if (Game.Instance.TryGet<QuantumService>(out var quantumService))
        {
            quantumService.RegisterObserver(this);
        }
        else
        {
            GD.PushWarning($"[QuantumObserver] QuantumService not ready when '{Name}' entered tree.");
        }
    }

    public override void _ExitTree()
    {
        if (Game.Instance.TryGet<QuantumService>(out var quantumService))
        {
            quantumService.UnregisterObserver(this);
        }
    }

    public bool CanObserve(QuantumItem item)
    {
        if (!IsObserving || item == null)
            return false;

        return OverlapsBody(item);
    }

    public void SetObserving(bool observing)
    {
        IsObserving = observing;
        _light.Visible = IsObserving;
    }

    public void ToggleObserving()
    {
        SetObserving(!IsObserving);
    }
}
