using UnityEngine;
using DG.Tweening;

public class ItemAnimator : MonoBehaviour
{
    [Header("Cài đặt Thời gian")]
    public float duration = 0.4f;      
    public Ease easeType = Ease.OutBack; 

    [Header("Bật/Tắt Hiệu ứng")]
    public bool useScale = true;       
    public bool useFade = true;        
    public bool useSlide = true;       
    
    [Header("Cấu hình Trượt (Slide)")]
    public Vector3 slideOffset = new Vector3(0, 2f, 0); 

    private SpriteRenderer cardRenderer;
    private SpriteRenderer iconRenderer;

    // Không dùng Awake nữa để tránh bắt nhầm Scale ban đầu

    public void AnimateIn(float delay)
    {
        // Lấy thành phần hình ảnh
        cardRenderer = GetComponent<SpriteRenderer>();
        if (transform.childCount > 0)
            iconRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        // 1. GHI NHỚ KÍCH THƯỚC CHUẨN *SAU KHI* ĐÃ ĐƯỢC ĐẶT VÀO KỆ
        Vector3 targetScale = transform.localScale;

        // 2. THIẾT LẬP TRẠNG THÁI TÀNG HÌNH BAN ĐẦU
        if (useScale) transform.localScale = Vector3.zero;
        
        if (useSlide) transform.localPosition += slideOffset; 

        if (useFade)
        {
            if (cardRenderer != null) cardRenderer.color = new Color(1, 1, 1, 0);
            if (iconRenderer != null) iconRenderer.color = new Color(1, 1, 1, 0);
        }

        // 3. BẮN ANIMATION TRỰC TIẾP TỪNG CÁI (Chèn thêm SetDelay để chờ đến lượt)
        if (useScale)
            transform.DOScale(targetScale, duration).SetEase(easeType).SetDelay(delay);

        if (useSlide)
            transform.DOLocalMove(Vector3.zero, duration).SetEase(easeType).SetDelay(delay);

        if (useFade)
        {
            if (cardRenderer != null) cardRenderer.DOFade(1f, duration).SetDelay(delay);
            if (iconRenderer != null) iconRenderer.DOFade(1f, duration).SetDelay(delay);
        }
    }
}