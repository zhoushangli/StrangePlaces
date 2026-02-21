using Godot;
using Protogame2D.UI;
using System;

public partial class LoadingUI : UIBase
{
	// Called when the node enters the scene tree for the first time.
	[Export] TextureProgressBar _bar;
	[Export] GpuParticles2D _lightings;
	[Export] Sprite2D _alter;
	public float percentage;
	public bool startLoad;
	[Export] float _fillingSpeed;
	double clocker;
	public override void _Ready()
	{
		percentage = 0;
		startLoad = false;
	}
	public override void OnOpen(object args)
	{
		startLoad = true;
	}
	public void ChangePercent(float targetVal)
	{
		percentage = targetVal;
	}
	public override void OnClose()
	{

	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (startLoad)
		{
			clocker += delta;
			_lightings.AmountRatio = percentage/100.0f;
			if(percentage >= 70)
			{
				_alter.Frame = 1;
			}
			if (clocker < 3 && clocker >= 2.9)
			{
				ChangePercent(30);
			}
			if (clocker < 6 && clocker >= 5.9)
			{
				ChangePercent(70);
			}
			if (clocker < 7 && clocker >= 6.9)
			{
				ChangePercent(100);
			}
			if (_bar.Value < percentage)
			{
				_bar.Value += delta * _fillingSpeed;
			}
			else if (_bar.Value >= percentage)
			{
				_bar.Value -= delta * _fillingSpeed;
			}
		}

	}



}
