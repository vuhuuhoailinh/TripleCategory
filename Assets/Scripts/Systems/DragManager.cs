using UnityEngine;

public class DragManager : MonoBehaviour
{
    [Header("Trạng thái Kéo Thả")]
    private GameObject draggedItem;     
    private Vector3 startPosition;      
    private Transform startAnchor;      
    private Vector3 originalScale;      

    private ShelfController currentHoveredShelf; 
    private ItemAnimation currentItemAnim; // Lưu tạm để gọi cho nhanh

    void Update()
    {
        // 1. NHẤC LÊN
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

            foreach (var col in hits)
            {   
                // --- THÊM ĐÚNG ĐOẠN NÀY ĐỂ BẮT SỰ KIỆN CLICK VÀO Ổ KHÓA ---
                if (col.CompareTag("AdsLock"))
                {
                    AdsLock lockScript = col.GetComponent<AdsLock>();
                    if (lockScript != null)
                    {
                        lockScript.Unlock(); // Hiện tại cho mở luôn. Sau này gắn Ads vào đây.
                        return; // Bấm trúng ổ khóa thì không xét việc cầm nắm item nữa
                    }
                }
                
                if (col.CompareTag("Item"))
                {   
                    ItemController itemLogic = col.GetComponent<ItemController>();
                    if (itemLogic != null && !itemLogic.isFaceUp) continue; // Cấm bốc thẻ úp

                    draggedItem = col.gameObject;
                    startPosition = draggedItem.transform.localPosition;
                    startAnchor = draggedItem.transform.parent; 
                    originalScale = draggedItem.transform.localScale; 
                    
                    draggedItem.GetComponent<Collider2D>().enabled = false;

                    // Gọi Item tự làm nổi hình ảnh nó lên
                    currentItemAnim = draggedItem.GetComponent<ItemAnimation>();
                    if (currentItemAnim != null) 
                    {
                        currentItemAnim.ElevateSortingOrder();
                        currentItemAnim.StartDrag();
                    }

                    draggedItem.transform.localScale = originalScale * 1.15f; 
                    break;
                }
            }
        }

        // 2. KÉO ĐI VÀ HOVER DARKEN
        if (Input.GetMouseButton(0) && draggedItem != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            draggedItem.transform.position = mousePos;

            Collider2D[] hoverHits = Physics2D.OverlapPointAll(mousePos);
            ShelfController newHoveredShelf = null;
            int closestSlot = -1;

            foreach (var col in hoverHits)
            {
                ShelfController shelf = col.GetComponentInParent<ShelfController>();
                if (shelf != null)
                {
                    newHoveredShelf = shelf;
                    closestSlot = shelf.GetClosestSlotAny(mousePos); 
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
            if (currentHoveredShelf != null)
            {
                currentHoveredShelf.ClearHover();
                currentHoveredShelf = null;
            }

            Vector2 dropPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] dropHits = Physics2D.OverlapPointAll(dropPos);

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

            // ỦY QUYỀN CHO BOARD ACTION MANAGER XỬ LÝ LUẬT CHƠI
            bool actionSuccessful = false;
            if (BoardActionManager.Instance != null)
            {
                actionSuccessful = BoardActionManager.Instance.TryProcessDrop(
                    draggedItem, targetItem, targetShelf, startAnchor, startPosition, dropPos);
            }

            // THẢ TRƯỢT -> Trượt về vị trí cũ
            if (!actionSuccessful)
            {
                draggedItem.transform.SetParent(startAnchor);
                if (currentItemAnim != null) currentItemAnim.StopDragAndDrop(startPosition);
                else draggedItem.transform.localPosition = startPosition;
            }

            draggedItem = null; 
            currentItemAnim = null;
        }
    }
}