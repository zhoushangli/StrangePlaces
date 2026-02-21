using Godot;
using Protogame2D.Utils;

public partial class PlayerController : CharacterBody2D
{
    private enum PlayerState
    {
        Idle,
        Moving
    }

    [ExportGroup("Configuration")]
    [Export] public int GridSize = 16;
    [Export] public float MoveSpeed = 120f;
    [Export(PropertyHint.Layers2DPhysics)] public uint platformLayers;

    [ExportGroup("References")]
    [Export] private AnimatedSprite2D _anim;

    private readonly SimpleStateMachine<PlayerState> _fsm = new();
    private Vector2 _targetPosition;

    public override void _Ready()
    {
        GlobalPosition = SnapToGrid(GlobalPosition);
        _targetPosition = GlobalPosition;

        _fsm.AddState(
            PlayerState.Idle,
            onEnter: () =>
            {
                _anim.Play("idle");
            },
            onUpdate: _ =>
            {
                Vector2 dir = ReadInputDirection();
                FlipSprite(dir);

                if (dir != Vector2.Zero && HasWalkableFloorAhead(dir) && TryStartMove(dir))
                {
                    _fsm.ChangeState(PlayerState.Moving);
                }
            });

        _fsm.AddState(
            PlayerState.Moving,
            onEnter: () =>
            {
                _anim.Play("walk");
            },
            onUpdate: delta =>
            {
                float step = MoveSpeed * delta;
                GlobalPosition = GlobalPosition.MoveToward(_targetPosition, step);

                if (GlobalPosition.DistanceTo(_targetPosition) > 0.01f)
                    return;

                GlobalPosition = _targetPosition;

                Vector2 dir = ReadInputDirection();
                FlipSprite(dir);

                if (dir == Vector2.Zero || !HasWalkableFloorAhead(dir) || !TryStartMove(dir))
                {
                    _fsm.ChangeState(PlayerState.Idle);
                }
            });

        _fsm.Init(PlayerState.Idle);
    }

    public override void _PhysicsProcess(double delta)
    {
        _fsm.Update((float)delta);
    }

    private Vector2 ReadInputDirection()
    {
        if (Input.IsActionPressed("move_up"))
        {
            return Vector2.Up;
        }
        if (Input.IsActionPressed("move_down"))
        {
            return Vector2.Down;
        }
        if (Input.IsActionPressed("move_left"))
        {
            return Vector2.Left;
        }
        if (Input.IsActionPressed("move_right"))
        {
            return Vector2.Right;
        }
        return Vector2.Zero;
    }

    private void FlipSprite(Vector2 dir)
    {
        if (dir == Vector2.Left)
        {
            _anim.FlipH = true;
        }
        else if (dir == Vector2.Right)
        {
            _anim.FlipH = false;
        }
    }

    private bool TryStartMove(Vector2 dir)
    {
        _targetPosition = SnapToGrid(GlobalPosition + dir * GridSize);
        return true;
    }

    private bool HasWalkableFloorAhead(Vector2 dir)
    {
        var space = GetWorld2D().DirectSpaceState;

        var currentCell = SnapToGrid(GlobalPosition);
        Vector2 target = SnapToGrid(currentCell + dir * GridSize);

        var circle = new CircleShape2D();
        circle.Radius = 4f; // 小一点

        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = circle,
            Transform = new Transform2D(0, target),
            CollisionMask = platformLayers,
            CollideWithBodies = true,
            CollideWithAreas = true,
        };

        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

        var hits = space.IntersectShape(query, maxResults: 1);

        return hits.Count > 0;
    }

    private Vector2 SnapToGrid(Vector2 pos)
    {
        float x = (Mathf.Floor(pos.X / GridSize) + 0.5f) * GridSize;
        float y = (Mathf.Floor(pos.Y / GridSize) + 0.5f) * GridSize;
        return new Vector2(x, y);
    }
}
