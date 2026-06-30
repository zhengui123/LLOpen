using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 按钮点击后重置 Cube。
/// </summary>
[RequireComponent(typeof(Button))]
public class CubeResetButton : MonoBehaviour
{
    [SerializeField] private CubeDragThrow targetCube;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnResetClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnResetClicked);
    }

    private void OnResetClicked()
    {
        if (targetCube != null)
            targetCube.ResetCube();
    }
}
