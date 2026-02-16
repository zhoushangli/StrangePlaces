using Godot;
using Protogame2D.Core;

namespace Protogame2D.Services;

/// <summary>
/// 输入服务，基于 InputMap actions。
/// 约定: move_left, move_right, move_up, move_down, jump, pause
/// </summary>
public class InputService : IService
{
    public bool Enabled { get; set; } = true;

    public void Init() { }

    public Vector2 GetMoveAxis()
    {
        if (!Enabled) return Vector2.Zero;

        var x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        var y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
        return new Vector2(x, y).Normalized();
    }

    public bool JumpPressed()
    {
        return Enabled && Input.IsActionJustPressed("jump");
    }

    public bool PausePressed()
    {
        return Enabled && Input.IsActionJustPressed("pause");
    }

    public void Shutdown() { }
}
