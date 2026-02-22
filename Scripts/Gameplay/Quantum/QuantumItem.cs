// QuantumItem.cs
using Godot;
using Protogame2D.Core;
using Protogame2D.Utils;

public partial class QuantumItem : StaticBody2D
{
    [Export] private Node2D[] _anchors;
    [Export] private AudioStream _moveSfx;
    [Export] private GpuParticles2D[] _moveParticles;

    public bool IsObserved { get; private set; }
    private int _anchorIndex = 0;

    public override void _Ready()
    {
        if (Game.Instance.TryGet<QuantumService>(out var quantumService))
        {
            quantumService.RegisterItem(this);
        }
        else
        {
            GD.PushWarning($"[QuantumItem] QuantumService not ready when '{Name}' entered tree.");
        }

        GlobalPosition = GridUtil.SnapToGrid(
            _anchors != null && _anchors.Length > 0 ? _anchors[0].GlobalPosition : GlobalPosition);

        if (_moveParticles != null)
        {
            foreach (var moveParticles in _moveParticles)
            {
                if (moveParticles == null)
                    continue;
                moveParticles.GlobalPosition = GlobalPosition;
            }
            CallDeferred(nameof(AttachMoveParticlesToSceneDeferred));
        }
    }

    public override void _ExitTree()
    {
        if (Game.Instance.TryGet<QuantumService>(out var quantumService))
        {
            quantumService.UnregisterItem(this);
        }
    }

    public void SetObserved(bool observed)
    {
        if (IsObserved == observed)
            return;

        var wasObserved = IsObserved;
        IsObserved = observed;

        if (wasObserved && !observed)
            MoveToNextAnchor();
    }

    private void MoveToNextAnchor()
    {
        if (_anchors == null || _anchors.Length == 0)
            return;

        var oldPosition = GlobalPosition;
        if (_moveParticles != null)
        {
            foreach (var moveParticles in _moveParticles)
            {
                if (moveParticles == null)
                    continue;
                moveParticles.GlobalPosition = oldPosition;
                moveParticles.Emitting = true;
                moveParticles.Restart();
            }
        }

        Game.Instance.Get<AudioService>().PlaySfx(_moveSfx);
        _anchorIndex = (_anchorIndex + 1) % _anchors.Length;
        GlobalPosition = GridUtil.SnapToGrid(_anchors[_anchorIndex].GlobalPosition);
    }

    private void AttachMoveParticlesToSceneDeferred()
    {
        if (_moveParticles == null)
            return;

        var scene = GetTree().CurrentScene;
        if (scene == null)
            return;

        foreach (var moveParticles in _moveParticles)
        {
            if (moveParticles == null || moveParticles.GetParent() == scene)
                continue;

            var particlesParent = moveParticles.GetParent();
            particlesParent?.RemoveChild(moveParticles);
            scene.AddChild(moveParticles);
            moveParticles.GlobalPosition = GlobalPosition;
            moveParticles.ZIndex = 100;
        }
    }
}
