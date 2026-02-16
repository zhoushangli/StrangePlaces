using Godot;
using Protogame2D.Core;
using Protogame2D.Services;

namespace Protogame2D.UI;

/// <summary>
/// 主菜单场景脚本。此场景作为独立场景加载，非通过 UIService.Open。
/// </summary>
public partial class UI_MainMenu : Control
{
    private Button _startButton;
    private Button _quitButton;
    private Button _texturedStartButton;
    private Button _texturedQuitButton;

    public override void _Ready()
    {
        _startButton = GetNode<Button>("CenterContainer/VBoxContainer/StartButton");
        _quitButton = GetNode<Button>("CenterContainer/VBoxContainer/QuitButton");

        _texturedStartButton = GetNode<Button>("CenterContainer/VBoxContainer/Start");
        _texturedQuitButton = GetNode<Button>("CenterContainer/VBoxContainer/Quit");

        _startButton.Pressed += OnStartPressed;
        _quitButton.Pressed += OnQuitPressed;

        _texturedStartButton.ButtonUp += OnStartPressed;
        _texturedQuitButton.ButtonUp += OnQuitPressed;

        if (App.Instance.TryGet<AudioService>(out var audio))
        {
            audio.PlayBgm("res://Assets/Audio/bgm_main.ogg", 0.5f);
        }
    }

    public void OnStartPressed()
    {
        GD.Print("Trying to load scene");
        if (App.Instance.TryGet<AudioService>(out var audio))
        {
            // 可选：播放点击音效，需提供路径
            // audio.PlaySfx("res://Assets/Audio/sfx_click.wav");
        }
        if (App.Instance.TryGet<SceneService>(out var scene))
        {
            scene.LoadGameplay();
        }
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
