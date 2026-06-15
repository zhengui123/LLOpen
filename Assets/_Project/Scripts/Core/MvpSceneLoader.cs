using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Launch 场景加载 Main 游戏场景。
/// </summary>
public class MvpSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName = "Main";

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
