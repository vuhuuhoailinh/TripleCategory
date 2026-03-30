using UnityEngine;

[ExecuteInEditMode] // Cho phép chạy ngay trong Editor để căn lề không cần bấm Play
public class CameraScaler : MonoBehaviour
{
    [Header("Tham chiếu Background")]
    [Tooltip("Kéo object Background vân gỗ (SpriteRenderer) từ Scene vào đây")]
    public SpriteRenderer backgroundSprite;

    [Header("Căn chỉnh lề Camera")]
    [Tooltip("Tỉ lệ phần mặt kệ (bỏ qua viền ngoài) so với tổng chiều ngang ảnh. Số càng lớn, viền ngoài càng ít.")]
    [Range(0.5f, 1f)]
    public float innerWidthRatio = 0.9f;

    void Update()
    {
        AdjustCameraToFitBackground();
    }

    void AdjustCameraToFitBackground()
    {
        // Kiểm tra xem đã kéo Background vào chưa, nếu chưa thì bỏ qua để tránh báo lỗi đỏ
        if (backgroundSprite == null || backgroundSprite.sprite == null || Camera.main == null) return;

        // 1. Lấy chiều ngang tổng của bức ảnh gốc (tính bằng World Units)
        float totalSpriteWidth = backgroundSprite.sprite.bounds.size.x;

        // 2. Tính chiều ngang của vùng mặt kệ bên trong mà ta muốn Camera lấy trọn
        float targetInnerWidth = totalSpriteWidth * innerWidthRatio;

        // 3. TOÁN HỌC CAMERA ORTHOGRAPHIC:
        // Chiều ngang của Camera = orthographicSize * 2 * Tỉ lệ màn hình (Aspect)
        // => Suy ra: orthographicSize = Chiều ngang muốn có / (2 * Aspect)
        Camera.main.orthographicSize = targetInnerWidth / (2f * Camera.main.aspect);
    }
}