using UnityEngine;
using DG.Tweening;

public class ItemDragAnimator : MonoBehaviour
{
    [Header("Cấu hình Swing (Lắc lư)")]
    public float tiltMultiplier = 50f;  
    public float maxTilt = 30f;         
    public float tiltSmoothness = 15f;  

    [Header("Cấu hình Thả (Drop)")]
    public float dropDuration = 0.25f;  // Thời gian trượt vào lỗ
    public Ease dropEase = Ease.OutQuad; // Trượt chậm dần ở cuối

    private bool isDragging = false;
    private float lastPosX;

    public void StartDrag()
    {
        isDragging = true;
        lastPosX = transform.position.x;
        transform.DOKill(); 
    }

    void Update()
    {
        if (isDragging)
        {
            float currentPosX = transform.position.x;
            float deltaX = currentPosX - lastPosX;
            float targetTilt = Mathf.Clamp(-deltaX * tiltMultiplier, -maxTilt, maxTilt);
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * tiltSmoothness);
            lastPosX = currentPosX;
        }
    }

    // --- MỚI: HÀM XỬ LÝ TRƯỢT VÀO LỖ VÀ RESET GÓC XOAY ---
    public void StopDragAndDrop(Vector3 targetLocalPos)
    {
        isDragging = false;
        
        // 1. Trả góc nghiêng về 0
        transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack);

        // 2. Trượt mượt mà về vị trí đích (Local Position của Card)
        transform.DOLocalMove(targetLocalPos, dropDuration).SetEase(dropEase);
    }
}