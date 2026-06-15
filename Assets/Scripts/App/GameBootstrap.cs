using UnityEngine;

/// <summary>
/// 游戏启动配置：关闭 VSync 并锁定 60fps，适配微信小游戏等移动端场景。
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
        DontDestroyOnLoad(gameObject);
    }
}
