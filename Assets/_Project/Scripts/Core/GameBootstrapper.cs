using UnityEngine;
using VContainer.Unity;

/// <summary>
/// VContainer 启动器，挂载在启动场景中，于 Awake 构建 DI 容器。
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        LifetimeScope.Find<GameLifetimeScope>().Build();
    }
}
