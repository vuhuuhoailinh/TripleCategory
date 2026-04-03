using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class LosePanel : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup overlayGroup;     
    public Transform panelContainer;     // Kéo cái khung chứa chữ và nút vào đây (nếu có)
    public Transform textContainer;      
    public Transform replayButton;       

    [Header("Cài đặt Animation")]
    public float overlayFadeDuration = 0.4f;
    public float textInitialDelay = 0.3f;
    public float letterDelay = 0.08f;
    public float letterAnimDuration = 0.4f;
    public float buttonExtraDelay = 0.2f;
    public float buttonAnimDuration = 0.8f;

    private bool isClicked = false;
    
    // --- CÁC BIẾN LƯU SCALE GỐC (Tránh việc bị ép về 1-1-1) ---
    private Vector3 originalPanelScale = Vector3.one;
    private Vector3 originalButtonScale = Vector3.one;
    private Vector3[] originalLetterScales; 

    void Start()
    {
        // ĐÓNG BĂNG GAMEPLAY (Chặn kéo thả bài ở dưới)
        Time.timeScale = 0f; 

        // 1. LƯU SCALE BẢNG NỀN VÀ ẨN
        if (panelContainer != null)
        {
            originalPanelScale = panelContainer.localScale;
            panelContainer.localScale = Vector3.zero;
        }

        // 2. LƯU SCALE NÚT VÀ ẨN
        if (replayButton != null) 
        {
            originalButtonScale = replayButton.localScale;
            replayButton.localScale = Vector3.zero;
        }

        // 3. LƯU SCALE TỪNG CHỮ CÁI VÀ ẨN
        if (overlayGroup != null) overlayGroup.alpha = 0f;
        
        if (textContainer != null)
        {
            originalLetterScales = new Vector3[textContainer.childCount];
            for (int i = 0; i < textContainer.childCount; i++)
            {
                Transform letter = textContainer.GetChild(i);
                originalLetterScales[i] = letter.localScale; // Lưu lại scale gốc của chữ này
                letter.localScale = Vector3.zero;
            }
        }

        // 4. CHẠY ANIMATION
        PlayLoseAnimation();
    }

    void PlayLoseAnimation()
    {
        // SetUpdate(true) để animation vẫn chạy mượt mà dù Time.timeScale = 0
        if (overlayGroup != null) overlayGroup.DOFade(1f, overlayFadeDuration).SetUpdate(true);

        if (panelContainer != null)
        {
            panelContainer.DOScale(originalPanelScale, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        float totalTextAnimTime = 0f;

        if (textContainer != null)
        {
            for (int i = 0; i < textContainer.childCount; i++)
            {
                Transform letter = textContainer.GetChild(i);
                
                // Trả về đúng Scale gốc của từng chữ cái
                letter.DOScale(originalLetterScales[i], letterAnimDuration)
                      .SetEase(Ease.OutBounce) // Rơi rụng xuống
                      .SetDelay(textInitialDelay + (i * letterDelay))
                      .SetUpdate(true);
            }
            totalTextAnimTime = textInitialDelay + (textContainer.childCount * letterDelay);
        }

        if (replayButton != null)
        {
            replayButton.DOScale(originalButtonScale, buttonAnimDuration)
                        .SetEase(Ease.OutElastic) 
                        .SetDelay(totalTextAnimTime + buttonExtraDelay)
                        .SetUpdate(true);
        }
    }

    public void OnClickReplay()
    {
        if (isClicked) return; 
        isClicked = true;

        if (replayButton != null) 
        {
            replayButton.DOKill(); 
            replayButton.DOPunchScale(originalButtonScale * -0.1f, 0.2f).SetUpdate(true);
        }
        
        GameEvents.OnUIClick?.Invoke();

        DOVirtual.DelayedCall(0.3f, () => {
            Time.timeScale = 1f; // RÃ ĐÔNG THỜI GIAN TRƯỚC KHI CHƠI LẠI
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
        }).SetUpdate(true);
    }
}