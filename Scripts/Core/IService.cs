namespace Protogame2D.Core;

/// <summary>
/// 所有 Service 的通用接口，初始化与关闭由 Boot 统一触发。
/// </summary>
public interface IService
{
    void Init();
    void Shutdown();
}
