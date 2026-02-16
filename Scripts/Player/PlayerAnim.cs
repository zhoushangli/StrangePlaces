using Godot;
using System;

public partial class PlayerAnim : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 300.0f;  // Inspector 可调，取代 const
	AnimatedSprite2D anim;
    public override void _Ready()
    {	
		anim = GetNode<AnimatedSprite2D>("playerSprite");
        MotionMode = CharacterBody2D.MotionModeEnum.Floating;  // 俯视角浮动模式，必设！
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
		turnAround();
        // 自定义虚拟按键输入（支持 4/8 方向，方向不为零时归一化速度）
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (direction != Vector2.Zero)
        {
            velocity = direction * Speed;  // 即时加速，全 X/Y 轴
        }
        else
        {
            velocity = Vector2.Zero;  // 即时停止（或用 MoveToward 渐停，见高级）
        }

        Velocity = velocity;
        MoveAndSlide();  // 处理碰撞滑行
    }

	void turnAround()
	{
		if (Input.IsActionJustPressed("move_left"))
		{
			anim.Play("Right");
			anim.FlipH = true;
		}
		else if (Input.IsActionJustPressed("move_right"))
		{
			anim.Play("Right");
			anim.FlipH = false;
		}
		else if (Input.IsActionJustPressed("move_down"))
		{
			anim.Play("Front");
		}
		else if (Input.IsActionJustPressed("move_up"))
		{
			anim.Play("Back");
		}
		else
		{
			anim.Pause();
		}
	}
}
