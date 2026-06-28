using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 将 CoverImage 上的 UI 点击事件转发给父级 RoomSlot。
/// </summary>
public class RoomSlotPointerRelay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
{
    private RoomSlot _owner;

    public void Bind(RoomSlot owner)
    {
        _owner = owner;
    }

    public void OnPointerDown(PointerEventData eventData) => _owner?.HandlePointerDown(eventData);
    public void OnPointerUp(PointerEventData eventData) => _owner?.HandlePointerUp(eventData);
    public void OnDrag(PointerEventData eventData) => _owner?.HandlePointerDrag(eventData);
    public void OnPointerClick(PointerEventData eventData) => _owner?.HandlePointerClick(eventData);
}
