using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Nguồn Phát (Audio Sources)")]
    [Tooltip("Kéo AudioSource dùng để phát nhạc nền vào đây (Nhớ bật Loop)")]
    public AudioSource bgmSource;
    [Tooltip("Kéo AudioSource dùng để phát hiệu ứng (SFX) vào đây")]
    public AudioSource sfxSource;

    [Header("File Âm Thanh (Kéo MP3/WAV vào đây)")]
    public AudioClip bgmClip;       // Nhạc nền
    public AudioClip pickSound;     // Tiếng bốc thẻ
    public AudioClip dropSound;     // Tiếng thả thẻ
    public AudioClip matchSound;    // Tiếng "Bling/Ting" khi ghép 3 thành công
    public AudioClip winSound;      // Tiếng nhạc Win khi qua màn
    public AudioClip clickSound;    // Tiếng Click nút bấm / mở khóa

    [Header("Cài đặt Âm lượng (SFX)")]
    [Range(0f, 1f)] public float matchVolume = 0.6f;

    private void Start()
    {
        // Bật nhạc nền ngay khi vào game
        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true; // Đảm bảo nhạc nền lặp lại
            bgmSource.Play();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnCardPicked += PlayPickSound;
        GameEvents.OnCardDropped += PlayDropSound;
        GameEvents.OnMatchDetected += PlayMatchSound;
        GameEvents.OnLevelWin += PlayWinSound;
        GameEvents.OnUIClick += PlayClickSound;
    }

    private void OnDisable()
    {
        GameEvents.OnCardPicked -= PlayPickSound;
        GameEvents.OnCardDropped -= PlayDropSound;
        GameEvents.OnMatchDetected -= PlayMatchSound;
        GameEvents.OnLevelWin -= PlayWinSound;
        GameEvents.OnUIClick -= PlayClickSound;
    }

    // --- CÁC HÀM PHÁT ÂM THANH ---
    private void PlayPickSound() { if (pickSound) sfxSource.PlayOneShot(pickSound); }
    private void PlayDropSound() { if (dropSound) sfxSource.PlayOneShot(dropSound); }
    private void PlayMatchSound() { if (matchSound) sfxSource.PlayOneShot(matchSound, matchVolume); }
    private void PlayClickSound() { if (clickSound) sfxSource.PlayOneShot(clickSound); }
    
    private void PlayWinSound() 
    { 
        if (winSound) sfxSource.PlayOneShot(winSound); 
        // Tuỳ chọn: Làm nhỏ nhạc nền đi khi thắng
        if (bgmSource != null) bgmSource.volume = 0.3f; 
    }

    // HÀM PUBLIC NÀY ĐỂ BẠN GẮN VÀO CÁC NÚT BẤM UI (Unity Button -> OnClick)
    public void PlayUIButtonClick()
    {
        PlayClickSound();
    }
}