using UnityEngine;
using DG.Tweening;
using System;

public class ShelfAnimation : MonoBehaviour
{
    [Header("Cấu hình Hover (Darken)")]
    public float hoverFadeDuration = 0.15f;
    [Range(0f, 1f)] public float maxDarkenAlpha = 0.4f; // Độ đen (0 là trong suốt, 1 là đen thui)

    [Header("Cấu hình Drop (Glow)")]
    public float glowDuration = 0.4f;
    public Color glowColor = Color.white; // Chớp sáng màu gì?

    [Header("Cấu hình Match-3 (Nổ)")]
    public float delayBeforeMatch = 0.25f; // Đợi thẻ trượt vào lỗ xong mới nổ
    public float matchShrinkDuration = 0.2f; // Tốc độ teo nhỏ

    private SpriteRenderer[] darkenSprites = new SpriteRenderer[3];
    private SpriteRenderer[] highlightSprites = new SpriteRenderer[3];

    // Hàm này được ShelfController gọi lúc Start để tự động nạp đạn
    public void Initialize(Transform[] anchors)
    {
        for (int i = 0; i < 3; i++)
        {
            if (anchors[i] != null)
            {
                // Tìm object tên "Darken"
                Transform darkenT = anchors[i].Find("Darken");
                if (darkenT != null)
                {
                    darkenSprites[i] = darkenT.GetComponent<SpriteRenderer>();
                    // Set màu ban đầu trong suốt và tắt đi
                    Color c = darkenSprites[i].color; c.a = 0; darkenSprites[i].color = c;
                    darkenSprites[i].gameObject.SetActive(false);
                }

                // Tìm object tên "Highlight"
                Transform highlightT = anchors[i].Find("Highlight");
                if (highlightT != null)
                {
                    highlightSprites[i] = highlightT.GetComponent<SpriteRenderer>();
                    highlightSprites[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowHover(int index)
    {
        ClearHover();
        if (index >= 0 && index < 3 && darkenSprites[index] != null)
        {
            darkenSprites[index].gameObject.SetActive(true);
            darkenSprites[index].DOKill(); // Dừng hiệu ứng cũ đang chạy dở
            darkenSprites[index].DOFade(maxDarkenAlpha, hoverFadeDuration); // Mờ dần thành đen
        }
    }

    public void ClearHover()
    {
        for (int i = 0; i < 3; i++)
        {
            if (darkenSprites[i] != null && darkenSprites[i].gameObject.activeSelf)
            {
                darkenSprites[i].DOKill();
                // Phai dần về trong suốt rồi tắt object
                darkenSprites[i].DOFade(0f, hoverFadeDuration).OnComplete(() =>
                {
                    darkenSprites[i].gameObject.SetActive(false);
                });
            }
        }
    }

    public void PlayDropGlow(int index)
    {
        if (index >= 0 && index < 3 && highlightSprites[index] != null)
        {
            highlightSprites[index].DOKill();
            highlightSprites[index].gameObject.SetActive(true);
            
            // Set độ sáng max
            Color startColor = glowColor; startColor.a = 1f;
            highlightSprites[index].color = startColor;

            // Phai dần đi
            highlightSprites[index].DOFade(0f, glowDuration).OnComplete(() =>
            {
                highlightSprites[index].gameObject.SetActive(false);
            });
        }
    }

    public void PlayMatchAnimation(GameObject obj0, GameObject obj1, GameObject obj2, Action onComplete)
    {
        float flipDelay = 0.15f; // Độ trễ giữa các lần lật của 3 viên thẻ (giây)
        float waitBeforeDestroy = 0.4f; // Thời gian chờ sau khi cả 3 viên đã lật xong

        Sequence seq = DOTween.Sequence();

        // Hàm ẩn giúp gọi lệnh lật của ItemAnimator
        void AddFlipToSequence(GameObject obj, float delay)
        {
            if (obj != null)
            {
                ItemAnimation itemAnim = obj.GetComponent<ItemAnimation>();
                if (itemAnim != null)
                {
                    // Chèn lệnh lật thẻ mặt sau vào dòng thời gian
                    seq.InsertCallback(delay, () => itemAnim.PlayFlipToBack());
                }
            }
        }

        // Lên kịch bản lật bài lần lượt
        AddFlipToSequence(obj0, 0f);
        AddFlipToSequence(obj1, flipDelay);
        AddFlipToSequence(obj2, flipDelay * 2);

        // Tính toán tổng thời gian chờ = Thời gian bắt đầu lật viên cuối + Thời gian lật (0.4s) + Thời gian ngâm (0.5s)
        float totalWaitTime = (flipDelay * 2) + 0.4f + waitBeforeDestroy;

        // Chốt kịch bản: Xóa object và báo cáo GameManager
        seq.InsertCallback(totalWaitTime, () => {
            if (obj0 != null) Destroy(obj0);
            if (obj1 != null) Destroy(obj1);
            if (obj2 != null) Destroy(obj2);
            
            onComplete?.Invoke();
        });
    }
}