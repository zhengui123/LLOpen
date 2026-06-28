using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// VContainer 启动器：在 LifetimeScope 就绪后构建容器。
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        var scope = LifetimeScope.Find<GameLifetimeScope>();
        if (scope == null)
        {
            Debug.LogError("[GameBootstrapper] 未找到 GameLifetimeScope。");
            return;
        }

        if (scope.Container == null)
        {
            scope.Build();
        }

        scope.Container.Resolve<DailyTarget>().CheckDailyReset();
    }
}
