using Godot;

namespace Protogame2D.UI;

/// <summary>
/// UI 基类，所有 UI 窗口继承此类。
/// </summary>
public partial class UIBase : Control
{
    public virtual void OnOpen(object args) { }
    public virtual void OnClose() { }
}
