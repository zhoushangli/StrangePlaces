using Godot;
using Protogame2D.Core;
using Protogame2D.Services;

namespace Protogame2D.UI;

/// <summary>
/// 暂停菜单，由 UIService.Open&lt;UI_Pause&gt; 打开。
/// </summary>
public partial class UI_Pause : UIBase
{
    [Export]
    private TextureButton _resumeButton;
    [Export]
    private TextureButton _backButton;
    [Export]
    private TextureButton _quitButton;

    public override void _Ready()
    {
        // _resumeButton = GetNode<Button>("CenterContainer/VBoxContainer/ResumeButton");
        // _backButton = GetNode<Button>("CenterContainer/VBoxContainer/BackToMenuButton");

        _resumeButton.Pressed += OnResumePressed;
        _backButton.Pressed += OnBackPressed;
        _quitButton.Pressed += OnQuitPressed;
    }
    
    private void OnResumePressed()
    {
        QueueFree();
    }

    private void OnBackPressed()
    {
        Game.Instance.Get<GameStateService>().ChangeGameState(GameState.MainMenu);
    }
    private void OnQuitPressed()
    {
        //关闭游戏
        GetTree().Quit();
    }

    public override void OnOpen(object args)
    {
        //我把该函数功能理解为_Ready函数。_xxxButton在inspector中赋值
        // _resumeButton = GetNode<Button>("NinePatchRect/VBoxContainer/ResumeButton2");
        // _backButton = GetNode<Button>("NinePatchRect/VBoxContainer/BackToMenuButton2");

        _resumeButton.Pressed += OnResumePressed;
        _backButton.Pressed += OnBackPressed;
        _quitButton.Pressed += OnQuitPressed;
    }

    public override void OnClose()
    {
        //TODO
    }

}
