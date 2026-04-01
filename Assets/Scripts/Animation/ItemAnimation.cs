using UnityEngine;
using DG.Tweening;
using System;

public class ItemAnimation : MonoBehaviour
{
    [Header("Thành phần Visual")]
    [Tooltip("Kéo object Card_Visual (chứa FrontFace và BackFace) vào đây")]
    public Transform cardVisual; 

    [Header("1. Cài đặt Lật Thẻ (Flip)")]
    public float flipDuration = 0.4f;
    public Ease flipEase = Ease.OutBack;

    [Header("2. Cấu hình Lắc Lư (Swing Physics)")]
    public float tiltMultiplier = 50f;  
    public float maxTilt = 30f;         
    public float tiltSmoothness = 15f;  

    [Header("3. Cấu hình Thả (Drop Slide)")]
    public float dropDuration = 0.25f;  
    public Ease dropEase = Ease.OutQuad; 

    // Biến nội bộ cho tính năng Kéo thả
    private bool isDragging = false;
    private float lastPosX;
    private int oldFrontOrder, oldIconOrder, oldBackOrder;

    // ==========================================
    // PHẦN 1: LOGIC LẬT THẺ (FLIP)
    // ==========================================

    public void SetFaceDownInstantly()
    {
        if (cardVisual != null)
        {
            cardVisual.DOKill(); 
            cardVisual.localEulerAngles = new Vector3(0, 180, 0); // Úp
        }
    }

    public void SetFaceUpInstantly()
    {
        if (cardVisual != null)
        {
            cardVisual.DOKill();
            cardVisual.localEulerAngles = Vector3.zero; // Ngửa
        }
    }

    public void PlayFlipToFront(Action onComplete = null)
        {
            if (cardVisual != null)
            {
                cardVisual.DOKill();
                cardVisual.DOLocalRotate(Vector3.zero, flipDuration, RotateMode.Fast)
                    .SetEase(flipEase)
                    .OnComplete(() => onComplete?.Invoke());
            }
        }

    public void PlayFlipToBack(Action onComplete = null)
    {
        if (cardVisual != null)
        {
            cardVisual.DOKill();
            cardVisual.DOLocalRotate(new Vector3(0, 180, 0), flipDuration, RotateMode.Fast)
                .SetEase(flipEase)
                .OnComplete(() => onComplete?.Invoke());
        }
    }

    // Hàm gọi khi mới Spawn
    public void AnimateIn(float delay)
    {
        SetFaceDownInstantly(); // Vừa đẻ ra là úp mặt

        ItemController itemLogic = GetComponent<ItemController>();
        bool shouldFaceUp = (itemLogic != null) ? itemLogic.isFaceUp : true;

        if (shouldFaceUp)
        {
            // SỬ DỤNG SEQUENCE: Cực kỳ ổn định cho Domino effect
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay); // Hẹn giờ (Chờ bao lâu)
            seq.AppendCallback(() => { 
                // Xong giờ thì gọi lệnh lật (Không dùng DOKill bên trong lúc này)
                cardVisual.DOLocalRotate(Vector3.zero, flipDuration, RotateMode.Fast).SetEase(flipEase);
            });
        }
    }

    // ==========================================
    // PHẦN 2: LOGIC KÉO THẢ (DRAG & DROP)
    // ==========================================

    public void StartDrag()
    {
        isDragging = true;
        lastPosX = transform.position.x;
        transform.DOKill(); // Dừng animation trượt của object Root (nếu có)
    }

    void Update()
    {
        // Khi kéo, xoay toàn bộ object Root để tạo cảm giác vật lý
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

    public void StopDragAndDrop(Vector3 targetLocalPos)
    {
        isDragging = false;
        
        // Trả góc nghiêng của object Root về 0
        transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack);

        // Trượt object Root mượt mà về vị trí đích
        transform.DOLocalMove(targetLocalPos, dropDuration).SetEase(dropEase);
    }

    public void ElevateSortingOrder()
    {
        if (cardVisual == null) return;
        Transform front = cardVisual.Find("FrontFace");
        Transform back = cardVisual.Find("BackFace");

        if (front != null)
        {
            SpriteRenderer frontSR = front.GetComponent<SpriteRenderer>();
            if (frontSR != null) { oldFrontOrder = frontSR.sortingOrder; frontSR.sortingOrder = 100; }

            if (front.childCount > 0)
            {
                SpriteRenderer iconSR = front.GetChild(0).GetComponent<SpriteRenderer>();
                if (iconSR != null) { oldIconOrder = iconSR.sortingOrder; iconSR.sortingOrder = 101; }
            }
        }
        if (back != null)
        {
            SpriteRenderer backSR = back.GetComponent<SpriteRenderer>();
            if (backSR != null) { oldBackOrder = backSR.sortingOrder; backSR.sortingOrder = 100; }
        }
    }

    public void RestoreSortingOrder()
    {
        if (cardVisual == null) return;
        Transform front = cardVisual.Find("FrontFace");
        Transform back = cardVisual.Find("BackFace");

        if (front != null)
        {
            SpriteRenderer frontSR = front.GetComponent<SpriteRenderer>();
            if (frontSR != null) frontSR.sortingOrder = oldFrontOrder;

            if (front.childCount > 0)
            {
                SpriteRenderer iconSR = front.GetChild(0).GetComponent<SpriteRenderer>();
                if (iconSR != null) iconSR.sortingOrder = oldIconOrder;
            }
        }
        if (back != null)
        {
            SpriteRenderer backSR = back.GetComponent<SpriteRenderer>();
            if (backSR != null) backSR.sortingOrder = oldBackOrder;
        }
    }
}