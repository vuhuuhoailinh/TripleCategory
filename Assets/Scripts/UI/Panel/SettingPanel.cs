using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class SettingPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup overlayGroup;
    public Transform panelContainer;

    [Header("Nút Bật/Tắt (Toggles)")]
    public Image bgmIcon; 
    public Image sfxIcon; 
    
    [Tooltip("Màu khi nút bị tắt")]
    public Color offColor = new Color(0.5f, 0.5f, 0.5f, 1f); 
    private Color onColor = Color.white;

    private bool isBgmOn;
    private bool isSfxOn;
    
    // --- MỚI: Biến lưu Scale gốc của Panel ---
    private Vector3 originalPanelScale;

    void Start()
    {
        isBgmOn = PlayerPrefs.GetInt("BGM_ON", 1) == 1;
        isSfxOn = PlayerPrefs.GetInt("SFX_ON", 1) == 1;

        UpdateToggleVisuals();

        // --- ĐÃ SỬA: Lưu kích thước gốc và ĐÓNG BĂNG GAME ---
        if (panelContainer != null) 
        {
            originalPanelScale = panelContainer.localScale;
            panelContainer.localScale = Vector3.zero;
        }
        if (overlayGroup != null) overlayGroup.alpha = 0f;

        Time.timeScale = 0f; // Dừng mọi hoạt động của Gameplay

        overlayGroup.DOFade(1f, 0.3f).SetUpdate(true); 
        // Dùng originalPanelScale thay vì Vector3.one
        panelContainer.DOScale(originalPanelScale, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnClickToggleBGM()
    {
        GameEvents.OnUIClick?.Invoke();
        isBgmOn = !isBgmOn; 
        
        PlayerPrefs.SetInt("BGM_ON", isBgmOn ? 1 : 0);
        GameEvents.OnBGMToggled?.Invoke(isBgmOn);
        UpdateToggleVisuals();
        
        PunchButton(bgmIcon.transform);
    }

    public void OnClickToggleSFX()
    {
        GameEvents.OnUIClick?.Invoke();
        isSfxOn = !isSfxOn; 
        
        PlayerPrefs.SetInt("SFX_ON", isSfxOn ? 1 : 0);
        GameEvents.OnSFXToggled?.Invoke(isSfxOn);

        UpdateToggleVisuals();
        PunchButton(sfxIcon.transform);
    }

    private void UpdateToggleVisuals()
    {
        if (bgmIcon != null) bgmIcon.color = isBgmOn ? onColor : offColor;
        if (sfxIcon != null) sfxIcon.color = isSfxOn ? onColor : offColor;
    }

    public void OnClickReplay()
    {
        GameEvents.OnUIClick?.Invoke();
        ClosePanel(() => {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    public void OnClickHome()
    {
        GameEvents.OnUIClick?.Invoke();
        ClosePanel(() => {
            SceneManager.LoadScene("Home"); 
        });
    }

    public void OnClickClose()
    {
        GameEvents.OnUIClick?.Invoke();
        ClosePanel(() => {
            Destroy(gameObject); 
        });
    }

    private void ClosePanel(TweenCallback onComplete)
    {
        overlayGroup.DOFade(0f, 0.2f).SetUpdate(true);
        panelContainer.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
            
            Time.timeScale = 1f; // RÃ ĐÔNG GAME KHI ĐÓNG BẢNG
            onComplete?.Invoke();
            
        });
    }

    private void PunchButton(Transform btn)
    {
        btn.DOKill();
        btn.DOPunchScale(btn.localScale * -0.1f, 0.2f).SetUpdate(true);
    }
}