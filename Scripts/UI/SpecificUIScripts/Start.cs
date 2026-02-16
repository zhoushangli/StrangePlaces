using Godot;
using System;

public partial class Start : TextureButton
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public PackedScene startScene;
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	private void OnClicked()//该函数被链接到该按钮的ButtonUp事件
	{
		GD.Print("Start Clicked.");
		GetTree().ChangeSceneToPacked(startScene);
	}
}
