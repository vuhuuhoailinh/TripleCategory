using UnityEngine;
using DG.Tweening; // Bắt buộc phải có để dùng DOVirtual.DelayedCall

public class DragManager : MonoBehaviour
{
    [Header("Trạng thái Kéo Thả")]
    private GameObject draggedItem;     
    private Vector3 startPosition;      
    private Transform startAnchor;      
    private int originalOrder;          
    private int originalIconOrder;      
    private Vector3 originalScale;      

    private ShelfController currentHoveredShelf; 

    void Update()
    {
        // 1. NHẤC LÊN
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

            foreach (var col in hits)
            {
                if (col.CompareTag("Item"))
                {
                    draggedItem = col.gameObject;
                    startPosition = draggedItem.transform.localPosition;
                    startAnchor = draggedItem.transform.parent; 
                    originalScale = draggedItem.transform.localScale; 
                    
                    draggedItem.GetComponent<Collider2D>().enabled = false;

                    SpriteRenderer cardRenderer = draggedItem.GetComponent<SpriteRenderer>();
                    SpriteRenderer iconRenderer = draggedItem.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    originalOrder = cardRenderer.sortingOrder;
                    originalIconOrder = iconRenderer.sortingOrder;
                    cardRenderer.sortingOrder = 100;
                    iconRenderer.sortingOrder = 101;
                    draggedItem.transform.localScale = originalScale * 1.15f; 
                    
                    ItemDragAnimator dragAnim = draggedItem.GetComponent<ItemDragAnimator>();
                    if (dragAnim != null) dragAnim.StartDrag();
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
            {
                currentHoveredShelf.ClearHover();
            }

            if (newHoveredShelf != null && closestSlot != -1)
            {
                newHoveredShelf.ShowHover(closestSlot);
            }

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
            
            SpriteRenderer cardRenderer = draggedItem.GetComponent<SpriteRenderer>();
            SpriteRenderer iconRenderer = draggedItem.transform.GetChild(0).GetComponent<SpriteRenderer>();
            if(cardRenderer != null) cardRenderer.sortingOrder = originalOrder;
            if(iconRenderer != null) iconRenderer.sortingOrder = originalIconOrder;
            draggedItem.transform.localScale = originalScale;

            ItemDragAnimator dragAnim = draggedItem.GetComponent<ItemDragAnimator>();

            GameObject targetItem = null;
            GameObject targetShelf = null;

            foreach (var col in dropHits)
            {
                if (col.CompareTag("Item") && col.gameObject != draggedItem)
                    targetItem = col.gameObject;
                if (col.CompareTag("Shelf"))
                    targetShelf = col.gameObject;
            }

            bool actionSuccessful = false;

            if (targetItem != null)
            {
                SwapItems(draggedItem, targetItem, startAnchor, startPosition);
                actionSuccessful = true;
            }
            else if (targetShelf != null)
            {
                actionSuccessful = MoveToShelf(draggedItem, targetShelf, startAnchor, dropPos);
            }

            if (!actionSuccessful)
            {
                draggedItem.transform.SetParent(startAnchor);
                if (dragAnim != null) dragAnim.StopDragAndDrop(startPosition);
                else draggedItem.transform.localPosition = startPosition;
            }

            draggedItem = null; 
        }
    }

    void SwapItems(GameObject draggedObj, GameObject targetObj, Transform sourceAnchor, Vector3 sourcePos)
    {
        Transform targetAnchor = targetObj.transform.parent;
        
        // MỚI: Lấy thời gian trượt thực tế của tấm thẻ
        float slideTime = 0.25f; 

        targetObj.transform.SetParent(sourceAnchor);
        ItemDragAnimator targetAnim = targetObj.GetComponent<ItemDragAnimator>();
        if (targetAnim != null) 
        {
            targetAnim.StopDragAndDrop(sourcePos);
            slideTime = targetAnim.dropDuration; 
        }
        else targetObj.transform.localPosition = sourcePos; 

        draggedObj.transform.SetParent(targetAnchor);
        ItemDragAnimator dragAnim = draggedObj.GetComponent<ItemDragAnimator>();
        if (dragAnim != null) dragAnim.StopDragAndDrop(Vector3.zero);
        else draggedObj.transform.localPosition = Vector3.zero;

        ShelfController sourceShelf = sourceAnchor.GetComponentInParent<ShelfController>();
        ShelfController targetShelf = targetAnchor.GetComponentInParent<ShelfController>();

        if (sourceShelf != null) UpdateShelfLogic(sourceShelf);
        if (targetShelf != null) UpdateShelfLogic(targetShelf);

        // MỚI: Đợi thẻ lướt xong (slideTime) mới bật Glow
        DOVirtual.DelayedCall(slideTime, () => {
            if (sourceShelf != null) {
                for(int i=0; i<3; i++) if (sourceShelf.slots[i] == targetObj) sourceShelf.PlayDropGlow(i);
            }
            if (targetShelf != null) {
                for(int i=0; i<3; i++) if (targetShelf.slots[i] == draggedObj) targetShelf.PlayDropGlow(i);
            }
        });

        if (GameManager.Instance != null) GameManager.Instance.UseMove();
    }

    bool MoveToShelf(GameObject item, GameObject targetShelfObj, Transform sourceAnchor, Vector2 dropPos)
    {
        ShelfController targetShelf = targetShelfObj.GetComponent<ShelfController>();
        if (targetShelf == null) return false;

        int emptySlot = targetShelf.GetClosestEmptySlot(dropPos);
        if (emptySlot != -1) 
        {
            targetShelf.AssignItemToSlot(emptySlot, item);
            
            // MỚI: Lấy thời gian trượt thực tế của tấm thẻ
            float slideTime = 0.25f; 
            ItemDragAnimator dragAnim = item.GetComponent<ItemDragAnimator>();
            if (dragAnim != null) 
            {
                dragAnim.StopDragAndDrop(Vector3.zero);
                slideTime = dragAnim.dropDuration;
            }
            else item.transform.localPosition = Vector3.zero;

            ShelfController sourceShelf = sourceAnchor.GetComponentInParent<ShelfController>();
            if (sourceShelf != null) UpdateShelfLogic(sourceShelf);
            UpdateShelfLogic(targetShelf);
            
            // MỚI: Đợi thẻ lướt xong (slideTime) mới bật Glow
            DOVirtual.DelayedCall(slideTime, () => {
                if (targetShelf != null) targetShelf.PlayDropGlow(emptySlot);
            });

            if (GameManager.Instance != null) GameManager.Instance.UseMove();

            return true;
        }
        return false; 
    }

    void UpdateShelfLogic(ShelfController shelf)
        {
            if (shelf == null) return;
            
            // ĐÃ SỬA CHUẨN: Thay vì set = null, hãy trả lại tấm Card mặc định cho ô trống
            for (int i = 0; i < 3; i++) 
            {
                if (shelf.slotAnchors[i] != null)
                    shelf.slots[i] = shelf.slotAnchors[i].gameObject;
                else 
                    shelf.slots[i] = null;
            }

            // Quét lại xem có Item nào đang làm con của Card không thì ghi đè vào sổ
            for (int i = 0; i < 3; i++)
            {
                Transform anchor = shelf.slotAnchors[i];
                if (anchor != null && anchor.childCount > 0)
                {
                    foreach (Transform child in anchor)
                    {
                        if (child.CompareTag("Item"))
                        {
                            shelf.AssignItemToSlot(i, child.gameObject);
                            break;
                        }
                    }
                }
            }
            shelf.CheckForMatch();
        }
}