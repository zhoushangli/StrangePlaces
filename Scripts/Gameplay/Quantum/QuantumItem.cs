using System;
using System.Collections.Generic;
using Godot;

public partial class QuantumItem : Node2D
{
    [Export] public NodePath[] AnchorPaths = Array.Empty<NodePath>();

    public bool IsObserved { get; private set; }

    private readonly List<Node2D> _anchors = new();
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        if (!IsInGroup("quantum_item"))
            AddToGroup("quantum_item");

        CacheAnchors();
    }

    public void SetObserved(bool observed)
    {
        if (IsObserved == observed)
            return;

        var wasObserved = IsObserved;
        IsObserved = observed;

        if (wasObserved && !observed)
            JumpToRandomAnchor();
    }

    private void CacheAnchors()
    {
        _anchors.Clear();

        if (AnchorPaths != null)
        {
            foreach (var path in AnchorPaths)
            {
                if (path.IsEmpty)
                    continue;

                var node = GetNodeOrNull<Node2D>(path);
                if (node != null)
                    _anchors.Add(node);
            }
        }

        if (_anchors.Count == 0)
        {
            foreach (var n in GetTree().GetNodesInGroup("quantum_anchor"))
            {
                if (n is Node2D anchor)
                    _anchors.Add(anchor);
            }
        }

        if (_anchors.Count == 0)
            GD.PushWarning($"[QuantumItem] No anchors configured for '{Name}'.");
    }

    private void JumpToRandomAnchor()
    {
        if (_anchors.Count == 0)
            return;

        var idx = _rng.RandiRange(0, _anchors.Count - 1);
        GlobalPosition = _anchors[idx].GlobalPosition;
    }
}
