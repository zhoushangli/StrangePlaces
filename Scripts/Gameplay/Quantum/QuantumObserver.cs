using Godot;

public partial class QuantumObserver : Area2D
{
    [Export] public float ObserveRadius = 96f;

    public bool IsObserving { get; private set; }

    public override void _Ready()
    {
        if (!IsInGroup("quantum_observer"))
            AddToGroup("quantum_observer");

        EnsureRangeShape();
    }

    public override void _Process(double delta)
    {
        IsObserving = Input.IsActionPressed("observe");
    }

    public bool CanObserve(QuantumItem item)
    {
        if (!IsObserving || item == null)
            return false;

        return GlobalPosition.DistanceTo(item.GlobalPosition) <= ObserveRadius;
    }

    private void EnsureRangeShape()
    {
        var shapeNode = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (shapeNode == null)
            return;

        if (shapeNode.Shape is CircleShape2D circle)
        {
            circle.Radius = ObserveRadius;
        }
    }
}
