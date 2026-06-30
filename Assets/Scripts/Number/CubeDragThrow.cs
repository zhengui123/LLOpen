using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 点击 Cube 并拖拽，松手后按拖拽方向抛掷。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CubeDragThrow : MonoBehaviour
{
    [SerializeField] private float throwForceMultiplier = 8f;
    [SerializeField] private float minDragDistance = 10f;
    [SerializeField] private float maxThrowSpeed = 25f;

    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isDragging;
    private Vector2 dragStartScreenPos;
    private Vector2 lastScreenPos;
    private float dragStartTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        if (Input.GetMouseButtonDown(0) && !isDragging && !IsPointerOverUI() && TryPickCube())
        {
            isDragging = true;
            dragStartScreenPos = Input.mousePosition;
            lastScreenPos = dragStartScreenPos;
            dragStartTime = Time.time;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            lastScreenPos = Input.mousePosition;
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            ThrowCube();
            isDragging = false;
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private bool TryPickCube()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, 100f) && hit.collider.gameObject == gameObject;
    }

    private void ThrowCube()
    {
        Vector2 dragDelta = lastScreenPos - dragStartScreenPos;
        if (dragDelta.magnitude < minDragDistance) return;

        float elapsed = Mathf.Max(Time.time - dragStartTime, 0.05f);
        Vector2 screenVelocity = dragDelta / elapsed;

        // 屏幕拖拽方向映射到世界空间：水平分量来自相机右/上，深度分量来自相机前
        Vector3 worldVelocity =
            mainCamera.transform.right * screenVelocity.x +
            mainCamera.transform.up * screenVelocity.y;

        worldVelocity *= throwForceMultiplier / Screen.height;
        worldVelocity = Vector3.ClampMagnitude(worldVelocity, maxThrowSpeed);

        rb.AddForce(worldVelocity, ForceMode.VelocityChange);
    }

    /// <summary>
    /// 重置 Cube 到初始位置并清除物理状态。
    /// </summary>
    public void ResetCube()
    {
        isDragging = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
    }
}
