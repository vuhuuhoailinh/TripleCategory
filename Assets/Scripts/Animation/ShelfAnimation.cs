using UnityEngine;
using DG.Tweening;
using System;
using TMPro;

public class ShelfAnimation : MonoBehaviour
{
    [Header("Cấu hình Hover (Darken)")]
    public float hoverFadeDuration = 0.15f;
    [Range(0f, 1f)] public float maxDarkenAlpha = 0.4f; // Độ đen (0 là trong suốt, 1 là đen thui)

    [Header("Cấu hình Drop (Glow)")]
    public float glowDuration = 0.4f;
    public Color glowColor = Color.white; // Chớp sáng màu gì?

    [Header("Giao diện Match-3 Text")]
    public TextMeshPro categoryText;

    private SpriteRenderer[] darkenSprites = new SpriteRenderer[3];
    private SpriteRenderer[] highlightSprites = new SpriteRenderer[3];
    private Vector3 originalTextScale = Vector3.one;
    void Awake()
    {
        if (categoryText != null)
        {
            originalTextScale = categoryText.transform.localScale; 
            categoryText.gameObject.SetActive(false);
        }
    }

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

    public void PlayMatchAnimation(GameObject obj0, GameObject obj1, GameObject obj2, string matchName, Action onComplete)
    {
        float flipDelay = 0.15f; // Độ trễ giữa mỗi thẻ khi lật
        
        // Giả định thời gian lật của ItemAnimator là 0.4s. 
        // Thẻ cuối cùng bắt đầu lật ở (flipDelay * 2), cộng thêm 0.4s để hoàn thành.
        float timeToFinishAllFlips = (flipDelay * 2) + 0.4f; 
        
        float holdDuration = 1.5f;

        Sequence seq = DOTween.Sequence();

        // 1. Lên kịch bản lật úp 3 thẻ lần lượt
        if (obj0 != null) {
            ItemAnimation anim0 = obj0.GetComponent<ItemAnimation>();
            if (anim0 != null) seq.InsertCallback(0f, () => anim0.PlayFlipToBack());
        }
        if (obj1 != null) {
            ItemAnimation anim1 = obj1.GetComponent<ItemAnimation>();
            if (anim1 != null) seq.InsertCallback(flipDelay, () => anim1.PlayFlipToBack());
        }
        if (obj2 != null) {
            ItemAnimation anim2 = obj2.GetComponent<ItemAnimation>();
            if (anim2 != null) seq.InsertCallback(flipDelay * 2, () => anim2.PlayFlipToBack());
        }

        // 2. HIỆN TEXT NGAY SAU KHI THẺ CUỐI CÙNG LẬT XONG
        if (categoryText != null)
        {
            seq.InsertCallback(timeToFinishAllFlips, () => {
                categoryText.text = matchName.ToUpper();
                categoryText.gameObject.SetActive(true);
                
                categoryText.transform.localScale = Vector3.zero;
                // SỬA CHỖ NÀY: Dùng originalTextScale thay vì Vector3.one
                categoryText.transform.DOScale(originalTextScale, 0.3f).SetEase(Ease.OutBack);
            });
        }

        // 3. XÓA SẠCH NGAY LẬP TỨC SAU 3 GIÂY NGÂM (Không thu nhỏ)
        float destroyTime = timeToFinishAllFlips + holdDuration;

        seq.InsertCallback(destroyTime, () => {
            
            // Tắt Text ngay lập tức
            if (categoryText != null) 
            {
                categoryText.gameObject.SetActive(false);
            }

            // Tiêu hủy 3 thẻ ngay lập tức
            if (obj0 != null) Destroy(obj0);
            if (obj1 != null) Destroy(obj1);
            if (obj2 != null) Destroy(obj2);
            
            // Báo cho GameManager biết đã hoàn thành ngay tắp lự
            onComplete?.Invoke();
        });
    }
}