using Godot;
using Protogame2D.Core;
using Protogame2D.Services;

namespace Protogame2D;

/// <summary>
/// 启动器，负责初始化所有服务并加载主菜单场景。
/// </summary>
public partial class Boot : Node
{
    public override void _Ready()
    {
        try
        {
            /*RegisterAndInit<ConfigService>();
            RegisterAndInit<SaveService>();
            RegisterAndInit<AudioService>();*/
            RegisterAndInit<EventService>();
            RegisterAndInit<UIService>();
            RegisterAndInit<InputService>();
            var gameState = RegisterAndInit<GameStateService>();
            var scene = RegisterAndInit<SceneService>();
            RegisterAndInit<PlayerService>();

            gameState.Set(GameState.Boot);
            gameState.Set(GameState.Menu);

            scene.LoadMainMenu();
        }
        catch (System.Exception ex)
        {
            GD.PushError($"[Boot] Init failed: {ex}");
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            App.Instance.ShutdownAll();
            GetTree().Quit();
        }
    }
    
    T RegisterAndInit<T>() where T : class, IService, new()
    {
        var service = new T();
        App.Instance.Register<T>(service);
        service.Init();
        return service;
    }
}