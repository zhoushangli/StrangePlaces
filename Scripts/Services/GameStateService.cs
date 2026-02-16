using Protogame2D.Core;

namespace Protogame2D.Services;

public enum GameState
{
    Boot,
    Menu,
    Loading,
    Playing,
    Paused
}

/// <summary>
/// 游戏状态服务。切换 Paused 时自动处理 Input 和 Pause UI。
/// </summary>
public class GameStateService : IService
{
    public GameState Current { get; private set; } = GameState.Boot;

    public void Init()
    {
        // 初始状态由 Boot 设置
    }

    public void Set(GameState next)
    {
        if (Current == next) return;

        var prev = Current;
        Current = next;

        if (App.Instance.TryGet<EventService>(out var evt))
        {
            evt.Publish(new GameStateChanged { Previous = prev, Current = next });
        }

        if (next == GameState.Paused)
        {
            if (App.Instance.TryGet<InputService>(out var input))
                input.Enabled = false;
            if (App.Instance.TryGet<UIService>(out var ui))
                ui.Open<UI.UI_Pause>();
        }
        else if (next == GameState.Playing)
        {
            if (App.Instance.TryGet<InputService>(out var input))
                input.Enabled = true;
            if (App.Instance.TryGet<UIService>(out var ui))
                ui.Close<UI.UI_Pause>();
        }
    }

    public void Shutdown() { }
}

public class GameStateChanged
{
    public GameState Previous { get; set; }
    public GameState Current { get; set; }
}
