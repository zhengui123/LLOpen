using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 页面切换 Slide + Fade 过渡（v1.2 Q2.5）。
/// </summary>
public static class PageTransition
{
    public enum Direction
    {
        FromRight,
        FromLeft
    }

    private const float Duration = 0.3f;
    private const float SlideDistance = 200f;

    public static async UniTask TransitionTo(
        CanvasGroup fromPage,
        CanvasGroup toPage,
        Direction direction = Direction.FromRight)
    {
        if (toPage == null)
        {
            return;
        }

        if (fromPage == null || fromPage == toPage)
        {
            ShowImmediate(toPage);
            return;
        }

        var slideDistance = direction == Direction.FromRight ? SlideDistance : -SlideDistance;
        var fromRect = fromPage.transform as RectTransform;
        var toRect = toPage.transform as RectTransform;

        KillTweens(fromPage);
        KillTweens(toPage);

        toPage.gameObject.SetActive(true);
        toPage.alpha = 0f;
        toPage.interactable = false;
        toPage.blocksRaycasts = false;

        if (toRect != null)
        {
            toRect.localPosition = new Vector3(slideDistance, 0f, 0f);
        }

        fromPage.interactable = false;
        fromPage.blocksRaycasts = false;

        var sequence = DOTween.Sequence();
        if (toRect != null)
        {
            sequence.Join(toRect.DOLocalMoveX(0f, Duration).SetEase(Ease.OutQuad));
        }

        sequence.Join(toPage.DOFade(1f, Duration));

        if (fromRect != null)
        {
            sequence.Join(fromRect.DOLocalMoveX(-slideDistance, Duration).SetEase(Ease.InQuad));
        }

        sequence.Join(fromPage.DOFade(0f, Duration));

        await sequence.AsyncWaitForCompletion();

        fromPage.gameObject.SetActive(false);
        ResetPageVisual(fromPage);
        toPage.interactable = true;
        toPage.blocksRaycasts = true;
        toPage.alpha = 1f;
        if (toRect != null)
        {
            toRect.localPosition = Vector3.zero;
        }
    }

    public static void ShowImmediate(CanvasGroup page)
    {
        if (page == null)
        {
            return;
        }

        KillTweens(page);
        page.gameObject.SetActive(true);
        page.alpha = 1f;
        page.interactable = true;
        page.blocksRaycasts = true;
        var rect = page.transform as RectTransform;
        if (rect != null)
        {
            rect.localPosition = Vector3.zero;
        }
    }

    public static CanvasGroup GetOrAddCanvasGroup(GameObject page)
    {
        if (page == null)
        {
            return null;
        }

        var group = page.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = page.AddComponent<CanvasGroup>();
        }

        return group;
    }

    private static void ResetPageVisual(CanvasGroup page)
    {
        if (page == null)
        {
            return;
        }

        KillTweens(page);
        page.alpha = 1f;
        page.interactable = true;
        page.blocksRaycasts = true;
        var rect = page.transform as RectTransform;
        if (rect != null)
        {
            rect.localPosition = Vector3.zero;
        }
    }

    private static void KillTweens(CanvasGroup page)
    {
        if (page == null)
        {
            return;
        }

        page.DOKill();
        page.transform.DOKill();
    }
}
