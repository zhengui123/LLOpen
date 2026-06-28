using UnityEngine;

/// <summary>
/// 金枕 5 房在榴莲图上的锚点（相对 roomSlotsParent 的 anchoredPosition）。
/// </summary>
[CreateAssetMenu(fileName = "JinzhengRoomConfig", menuName = "榴莲开了/金枕房配置")]
public class DurianRoomConfig : ScriptableObject
{
    public Vector2[] roomPositions = new Vector2[]
    {
        new Vector2(0f, -80f),
        new Vector2(-110f, -30f),
        new Vector2(110f, -30f),
        new Vector2(-90f, 80f),
        new Vector2(90f, 80f)
    };
}
