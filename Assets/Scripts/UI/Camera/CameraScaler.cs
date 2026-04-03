using UnityEngine;

[ExecuteAlways] // Chạy ngay trong Editor để căn lề trực tiếp
[RequireComponent(typeof(Camera))]
public class CameraScaler : MonoBehaviour
{
    [Header("Tham chiếu Background")]
    [Tooltip("Kéo object Background vân gỗ (SpriteRenderer) từ Scene vào đây")]
    public SpriteRenderer backgroundSprite;

    [Header("Căn chỉnh lề Camera")]
    [Tooltip("Tỉ lệ phần mặt kệ chứa Shelves so với tổng chiều ngang ảnh gốc (Ví dụ: 0.9)")]
    [Range(0.5f, 1f)]
    public float innerWidthRatio = 0.9f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCameraToFitBackground();
    }

    void Update()
    {
        // Liên tục cập nhật trong Editor khi bạn kéo Simulator
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            AdjustCameraToFitBackground();
        }
        #endif
    }

    void AdjustCameraToFitBackground()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null || backgroundSprite == null || backgroundSprite.sprite == null) return;

        // 1. Lấy chiều ngang thực tế của bức ảnh Background
        float totalSpriteWidth = backgroundSprite.sprite.bounds.size.x;

        // 2. Tính chiều ngang của vùng "Safe Zone" mà ta muốn Camera lấy trọn
        float targetInnerWidth = totalSpriteWidth * innerWidthRatio;

        // 3. Tính toán độ zoom của Camera (Orthographic Size)
        cam.orthographicSize = targetInnerWidth / (2f * cam.aspect);
    }
}