using Godot;
using Protogame2D.Core;
using Protogame2D.Services;

namespace Protogame2D;

/// <summary>
/// 玩家角色，使用 InputService 控制移动。
/// </summary>
public partial class PlayerPawn : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 300f;
    [Export] public float JumpVelocity { get; set; } = -400f;

    public override void _PhysicsProcess(double delta)
    {
        if (!App.Instance.TryGet<InputService>(out var input))
            return;

        var axis = input.GetMoveAxis();
        Velocity = new Vector2(axis.X * Speed, Velocity.Y);

        if (input.JumpPressed() && IsOnFloor())
        {
            Velocity = Velocity with { Y = JumpVelocity };
        }

        MoveAndSlide();
    }

    public override void _Input(InputEvent ev)
    {
        if (!App.Instance.TryGet<InputService>(out var input))
            return;
        if (!App.Instance.TryGet<GameStateService>(out var gs))
            return;

        if (input.PausePressed())
        {
            gs.Set(GameState.Paused);
        }
    }
}
