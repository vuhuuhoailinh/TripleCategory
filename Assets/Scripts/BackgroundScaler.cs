using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode] // Cho phép chạy code ngay trong Editor để bạn dễ căn chỉnh
public class BackgroundStretcher : MonoBehaviour
{
    [Header("Căn chỉnh lề Camera")]
    [Tooltip("Tỉ lệ phần mặt gỗ bên trong so với tổng chiều ngang của ảnh (Ví dụ: 0.85 nghĩa là phần trong chiếm 85%, viền ngoài chiếm 15%)")]
    [Range(0.5f, 1f)]
    public float innerWidthRatio = 0.85f; 

    void Update()
    {
        StretchToFitCamera();
    }

    void StretchToFitCamera()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null || Camera.main == null) return;

        // 1. Tính toán kích thước thực tế của Camera (Tính bằng World Units)
        float cameraHeight = Camera.main.orthographicSize * 2f;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        // 2. Lấy kích thước gốc của tấm ảnh vân gỗ
        float spriteUnscaledWidth = sr.sprite.bounds.size.x;
        float spriteUnscaledHeight = sr.sprite.bounds.size.y;

        // 3. TOÁN HỌC STRETCH X: 
        // Chúng ta muốn (Tổng chiều rộng ảnh * ScaleX * innerWidthRatio) = Chiều ngang Camera
        // Suy ra:
        float targetTotalWidth = cameraWidth / innerWidthRatio;
        float scaleX = targetTotalWidth / spriteUnscaledWidth;

        // 4. TOÁN HỌC STRETCH Y:
        // Ép dãn chiều dọc cho bằng đúng chiều cao của Camera (Giống hệt UI Stretch-Stretch)
        float scaleY = cameraHeight / spriteUnscaledHeight;

        // 5. Áp dụng biến dạng
        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }
}