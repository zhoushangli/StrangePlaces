using System.Threading.Tasks;
using Godot;
using Protogame2D.Core;
using Protogame2D.UI;

namespace Protogame2D.Services;

/// <summary>
/// 场景加载服务。
/// </summary>
public class SceneService : IService
{
    public const string MainMenuPath = "res://Scenes/UI/UI_MainMenu.tscn";
    public const string GameplayPath = "res://Scenes/Gameplay/Gameplay.tscn";

    public void Init() { }

    public Task LoadMainMenu()
    {
        return LoadScene(MainMenuPath);
    }

    public Task LoadGameplay()
    {
        return LoadScene(GameplayPath);
    }

    public Task LoadScene(string path, bool showLoading = true)
    {
        if (App.Instance.TryGet<GameStateService>(out var gs))
            gs.Set(GameState.Loading);

        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree != null)
        {
            tree.ChangeSceneToFile(path);
        }

        if (App.Instance.TryGet<EventService>(out var evt))
            evt.Publish(new SceneLoaded { Path = path });

        if (path == MainMenuPath && App.Instance.TryGet<GameStateService>(out var gs2))
            gs2.Set(GameState.Menu);
        else if (path == GameplayPath && App.Instance.TryGet<GameStateService>(out var gs3))
            gs3.Set(GameState.Playing);

        return Task.CompletedTask;
    }

    public void Shutdown() { }
}

public class SceneLoaded
{
    public string Path { get; set; } = "";
}
