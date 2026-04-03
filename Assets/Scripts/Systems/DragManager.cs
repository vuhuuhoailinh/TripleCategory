using UnityEngine;
using DG.Tweening;

public class DragManager : MonoBehaviour
{
    [Header("Cài đặt Game Feel")]
    [Tooltip("Khoảng cách đẩy thẻ lên trên ngón tay (Tránh bị ngón tay che)")]
    public float dragOffsetY = 1.2f;
    [Tooltip("Độ nhạy bám tay (Càng cao càng bám chặt, khuyên dùng 30-40)")]
    public float dragSmoothSpeed = 40f;

    [Header("Trạng thái Kéo Thả")]
    private GameObject draggedItem;     
    private Vector3 startPosition;      
    private Transform startAnchor;      
    private Vector3 originalScale;      

    private ShelfController currentHoveredShelf; 
    private ItemAnimation currentItemAnim; 
    
    // Lưu vị trí mục tiêu để nội suy (Lerp)
    private Vector3 targetPos;

    void Update()
    {   
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            if (draggedItem != null) ForceDropCard(); 
            return; 
        }

        if (Time.timeScale == 0f)
        {
            if (draggedItem != null) ForceDropCard(); 
            return;
        }

        if (!Input.GetMouseButton(0) && draggedItem != null)
        {
            ForceDropCard();
            return;
        }

        // 1. NHẤC LÊN
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

            foreach (var col in hits)
            {   
                if (col.CompareTag("AdsLock"))
                {
                    AdsLock lockScript = col.GetComponent<AdsLock>();
                    if (lockScript != null)
                    {
                        GameEvents.OnUIClick?.Invoke();
                        lockScript.Unlock(); 
                        return; 
                    }
                }
                
                if (col.CompareTag("Item"))
                {   
                    ItemController itemLogic = col.GetComponent<ItemController>();
                    if (itemLogic != null && !itemLogic.isFaceUp) continue; 

                    draggedItem = col.gameObject;
                    startPosition = draggedItem.transform.localPosition;
                    startAnchor = draggedItem.transform.parent; 
                    originalScale = draggedItem.transform.localScale; 
                    
                    draggedItem.GetComponent<Collider2D>().enabled = false;

                    currentItemAnim = draggedItem.GetComponent<ItemAnimation>();
                    if (currentItemAnim != null) 
                    {
                        currentItemAnim.ElevateSortingOrder();
                        currentItemAnim.StartDrag();
                    }

                    draggedItem.transform.localScale = originalScale * 1.15f; 
                    
                    // --- MỚI: Thiết lập ngay vị trí đích khi vừa nhấc lên ---
                    targetPos = mousePos + new Vector2(0f, dragOffsetY);
                    draggedItem.transform.position = targetPos;

                    GameEvents.OnCardPicked?.Invoke();
                    break;
                }
            }
        }

        // 2. KÉO ĐI VÀ HOVER DARKEN
        if (Input.GetMouseButton(0) && draggedItem != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // Tính toán vị trí đích có cộng thêm Offset
            targetPos = mousePos + new Vector2(0f, dragOffsetY);

            // --- MỚI: Dùng Lerp để chuyển động mượt mà, chống rung ---
            draggedItem.transform.position = Vector3.Lerp(draggedItem.transform.position, targetPos, Time.deltaTime * dragSmoothSpeed);

            // --- QUAN TRỌNG: Lấy vị trí của THẺ để check Hover, KHÔNG lấy vị trí chuột nữa ---
            Collider2D[] hoverHits = Physics2D.OverlapPointAll(draggedItem.transform.position);
            ShelfController newHoveredShelf = null;
            int closestSlot = -1;

            foreach (var col in hoverHits)
            {
                ShelfController shelf = col.GetComponentInParent<ShelfController>();
                if (shelf != null)
                {
                    newHoveredShelf = shelf;
                    // Truyền vị trí cái thẻ xuống kệ
                    closestSlot = shelf.GetClosestSlotAny(draggedItem.transform.position); 
                    break;
                }
            }

            if (currentHoveredShelf != null && currentHoveredShelf != newHoveredShelf)
                currentHoveredShelf.ClearHover();

            if (newHoveredShelf != null && closestSlot != -1)
                newHoveredShelf.ShowHover(closestSlot);

            currentHoveredShelf = newHoveredShelf;
        }

        // 3. THẢ XUỐNG
        if (Input.GetMouseButtonUp(0) && draggedItem != null)
        {
            ForceDropCard();
        }
    }

    private void ForceDropCard()
    {
        if (currentHoveredShelf != null)
        {
            currentHoveredShelf.ClearHover();
            currentHoveredShelf = null;
        }

        // --- QUAN TRỌNG: Lấy vị trí của THẺ để check va chạm khi thả ---
        Vector2 dropPos = draggedItem.transform.position;
        Collider2D[] dropHits = Physics2D.OverlapPointAll(dropPos);

        if (draggedItem.GetComponent<Collider2D>() != null)
            draggedItem.GetComponent<Collider2D>().enabled = true;

        draggedItem.transform.localScale = originalScale;
        if (currentItemAnim != null) currentItemAnim.RestoreSortingOrder();

        GameObject targetItem = null;
        GameObject targetShelf = null;

        foreach (var col in dropHits)
        {
            if (col.CompareTag("Item") && col.gameObject != draggedItem) targetItem = col.gameObject;
            if (col.CompareTag("Shelf")) targetShelf = col.gameObject;
        }

        GameObject itemToDrop = draggedItem;
        ItemAnimation animToDrop = currentItemAnim;
        Transform origAnchor = startAnchor;
        Vector3 origPos = startPosition;

        draggedItem = null; 
        currentItemAnim = null;

        bool actionSuccessful = false;
        try 
        {
            if (BoardActionManager.Instance != null)
            {
                actionSuccessful = BoardActionManager.Instance.TryProcessDrop(
                    itemToDrop, targetItem, targetShelf, origAnchor, origPos, dropPos);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Có lỗi khi thả bài (Nhưng chuột đã được cứu): " + e.Message);
        }

        if (!actionSuccessful && itemToDrop != null)
        {
            itemToDrop.transform.SetParent(origAnchor);
            float failSlideTime = 0.25f; 
            
            if (animToDrop != null) 
            {
                animToDrop.StopDragAndDrop(origPos);
                failSlideTime = animToDrop.dropDuration;
            }
            else 
            {
                itemToDrop.transform.localPosition = origPos;
                failSlideTime = 0f;
            }

            DOVirtual.DelayedCall(failSlideTime, () => {
                GameEvents.OnCardDropped?.Invoke();
            });
        }
    }
}