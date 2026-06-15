using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 榴莲揭示：壳裂开动画、房位展示、评级发布，完成后跳转售卖页。
/// </summary>
public class DurianOpener : MonoBehaviour
{
    [SerializeField] private Transform shellTransform;
    [SerializeField] private Transform roomsRoot;
    [SerializeField] private GameObject roomMeatPrefab;
    [SerializeField] private GameObject roomEmptyPrefab;
    [SerializeField] private GameObject floatTextPrefab;
    [SerializeField] private Text ratingText;
    [SerializeField] private float shellOpenDuration = 0.35f;
    [SerializeField] private float roomPopDuration = 0.25f;
    [SerializeField] private float finishDelay = 0.6f;
    [SerializeField] private float roomSpacing = 1.2f;

    private bool _isOpening;
    private readonly List<GameObject> _spawnedRooms = new();
    private GameUIRoot _uiRoot;

    public void SetNavigationTarget(GameUIRoot uiRoot)
    {
        _uiRoot = uiRoot;
    }

    public async UniTask OpenAsync(DurianData durian)
    {
        if (_isOpening)
        {
            return;
        }

        _isOpening = true;

        ClearRooms();
        await PlayShellCrackAsync();
        await RevealRoomsAsync(durian);

        var rating = YieldRatingUtil.GetRating(durian.yieldRate);
        if (ratingText != null)
        {
            ratingText.text = $"出肉率 {durian.yieldRate:F1}% · {rating}";
        }

        EventBus.Publish(new DurianOpenedEvent
        {
            Durian = durian,
            Rating = rating,
            YieldRate = durian.yieldRate
        });

        await UniTask.Delay(System.TimeSpan.FromSeconds(finishDelay));

        if (_uiRoot != null)
        {
            _uiRoot.ShowSell(durian, rating);
        }

        _isOpening = false;
    }

    private async UniTask PlayShellCrackAsync()
    {
        if (shellTransform == null)
        {
            return;
        }

        var from = shellTransform.localScale;
        var to = new Vector3(from.x * 1.15f, from.y * 0.82f, from.z);
        await AnimateScaleAsync(shellTransform, from, to, shellOpenDuration);
    }

    private async UniTask RevealRoomsAsync(DurianData durian)
    {
        if (roomsRoot == null || durian.roomResults == null)
        {
            return;
        }

        var count = durian.roomResults.Length;
        var startX = -(count - 1) * roomSpacing * 0.5f;

        for (var i = 0; i < count; i++)
        {
            var hasMeat = durian.roomResults[i];
            var prefab = hasMeat ? roomMeatPrefab : roomEmptyPrefab;
            if (prefab == null)
            {
                continue;
            }

            var room = Instantiate(prefab, roomsRoot);
            room.transform.localPosition = new Vector3(startX + i * roomSpacing, 0f, 0f);
            room.transform.localScale = Vector3.zero;
            _spawnedRooms.Add(room);

            SpawnFloatText(room.transform.position, hasMeat ? "满房" : "空房");
            AnimateScaleAsync(room.transform, Vector3.zero, Vector3.one, roomPopDuration).Forget();
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(roomPopDuration));
    }

    private void SpawnFloatText(Vector3 worldPos, string message)
    {
        if (floatTextPrefab == null)
        {
            return;
        }

        var textGo = Instantiate(floatTextPrefab, roomsRoot != null ? roomsRoot : transform);
        textGo.transform.position = worldPos + Vector3.up * 0.5f;

        var label = textGo.GetComponent<Text>();
        if (label != null)
        {
            label.text = message;
            label.color = message == "满房" ? new Color(1f, 0.84f, 0f) : Color.gray;
        }

        FloatAndFadeAsync(textGo).Forget();
    }

    private async UniTask FloatAndFadeAsync(GameObject textGo)
    {
        var rect = textGo.transform;
        var start = rect.position;
        var end = start + Vector3.up * 0.8f;
        var duration = 0.8f;
        var elapsed = 0f;

        var canvasGroup = textGo.GetComponent<CanvasGroup>();
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            rect.position = Vector3.Lerp(start, end, t);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - t;
            }

            await UniTask.Yield();
        }

        Destroy(textGo);
    }

    private static async UniTask AnimateScaleAsync(Transform target, Vector3 from, Vector3 to, float duration)
    {
        if (target == null)
        {
            return;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.Lerp(from, to, t);
            await UniTask.Yield();
        }

        target.localScale = to;
    }

    private void ClearRooms()
    {
        foreach (var room in _spawnedRooms)
        {
            if (room != null)
            {
                Destroy(room);
            }
        }

        _spawnedRooms.Clear();
    }
}
