using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class WinPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup overlayGroup;     // Kéo object "Overlay" vào đây
    public Transform textContainer;      // Kéo "WellDone_TextContainer" vào đây
    public Transform continueButton;     // Kéo object "Button" (củ cà rốt) vào đây

    [Header("Cài đặt Animation (Chỉnh trên Inspector)")]
    [Tooltip("Thời gian làm mờ nền đen")]
    public float overlayFadeDuration = 0.4f;

    [Tooltip("Thời gian chờ trước khi chữ đầu tiên nảy lên")]
    public float textInitialDelay = 0.3f;
    [Tooltip("Khoảng thời gian cách nhau giữa các chữ cái (hiệu ứng gõ phím)")]
    public float letterDelay = 0.08f;
    [Tooltip("Thời gian nảy phình to của MỖI chữ cái")]
    public float letterAnimDuration = 0.4f;

    [Tooltip("Thời gian chờ thêm sau khi chữ đã hiện HẾT rồi mới nảy nút")]
    public float buttonExtraDelay = 0.2f;
    [Tooltip("Thời gian nảy rung rinh của củ cà rốt")]
    public float buttonAnimDuration = 0.8f;

    private bool isClicked = false;

    // Biến ngầm để ghi nhớ Scale thật của củ cà rốt
    private Vector3 originalButtonScale;

    void Start()
    {
        // 1. GHI NHỚ VÀ ẨN NÚT BẤM
        if (continueButton != null) 
        {
            originalButtonScale = continueButton.localScale; // Lưu lại kích thước thật
            continueButton.localScale = Vector3.zero; // Ép về 0
        }

        // 2. ẨN NỀN VÀ CHỮ
        if (overlayGroup != null) overlayGroup.alpha = 0f;
        
        if (textContainer != null)
        {
            foreach (Transform letter in textContainer)
            {
                letter.localScale = Vector3.zero;
            }
        }

        // 3. KÍCH HOẠT ANIMATION
        PlayDopamineHit();
    }

    void PlayDopamineHit()
    {
        // 1. Mờ nền đen
        if (overlayGroup != null) overlayGroup.DOFade(1f, overlayFadeDuration);

        float totalTextAnimTime = 0f;

        // 2. Nảy từng chữ cái
        if (textContainer != null)
        {
            for (int i = 0; i < textContainer.childCount; i++)
            {
                Transform letter = textContainer.GetChild(i);
                
                letter.DOScale(Vector3.one, letterAnimDuration)
                      .SetEase(Ease.OutBack)
                      .SetDelay(textInitialDelay + (i * letterDelay));
            }
            totalTextAnimTime = textInitialDelay + (textContainer.childCount * letterDelay);
        }

        // 3. NÚT CÀ RỐT NẢY LÊN (Dùng originalButtonScale thay vì Vector3.one)
        if (continueButton != null)
        {
            continueButton.DOScale(originalButtonScale, buttonAnimDuration)
                          .SetEase(Ease.OutElastic) // Hiệu ứng nảy như thạch
                          .SetDelay(totalTextAnimTime + buttonExtraDelay);
        }
    }

    public void OnClickContinue()
    {
        // CHỐT KHÓA: Nếu đã bấm rồi thì cấm bấm nữa
        if (isClicked) return; 
        isClicked = true;

        if (continueButton != null) 
        {
            continueButton.DOKill(); 
            continueButton.DOPunchScale(originalButtonScale * -0.1f, 0.2f);
        }
        
        GameEvents.OnUIClick?.Invoke();

        DOVirtual.DelayedCall(0.3f, () => {
            GameManager.Instance.NextLevel(); 
        });
    }
}