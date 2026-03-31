using UnityEngine;

public class DragManager : MonoBehaviour
{
    [Header("Trạng thái Kéo Thả")]
    private GameObject draggedItem;     
    private Vector3 startPosition;      
    private Transform startAnchor;      // Lưu lại cái Card gốc     
    private int originalOrder;          
    private int originalIconOrder;      
    private Vector3 originalScale;      

    void Update()
    {
        // 1. NHẤC LÊN
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Item"))
                {
                    draggedItem = hit.collider.gameObject;
                    
                    startPosition = draggedItem.transform.localPosition;
                    startAnchor = draggedItem.transform.parent; // Đây chính là tấm Card
                    originalScale = draggedItem.transform.localScale; 

                    draggedItem.GetComponent<Collider2D>().enabled = false;

                    SpriteRenderer cardRenderer = draggedItem.GetComponent<SpriteRenderer>();
                    SpriteRenderer iconRenderer = draggedItem.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    
                    originalOrder = cardRenderer.sortingOrder;
                    originalIconOrder = iconRenderer.sortingOrder;
                    cardRenderer.sortingOrder = 100;
                    iconRenderer.sortingOrder = 101;
                    draggedItem.transform.localScale = originalScale * 1.15f; 
                    
                    break;
                }
            }
        }

        // 2. KÉO ĐI
        if (Input.GetMouseButton(0) && draggedItem != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            draggedItem.transform.position = mousePos;
        }

        // 3. THẢ XUỐNG
        if (Input.GetMouseButtonUp(0) && draggedItem != null)
        {
            Vector2 dropPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(dropPos, Vector2.zero);

            draggedItem.GetComponent<Collider2D>().enabled = true;
            
            SpriteRenderer cardRenderer = draggedItem.GetComponent<SpriteRenderer>();
            SpriteRenderer iconRenderer = draggedItem.transform.GetChild(0).GetComponent<SpriteRenderer>();
            if(cardRenderer != null) cardRenderer.sortingOrder = originalOrder;
            if(iconRenderer != null) iconRenderer.sortingOrder = originalIconOrder;
            draggedItem.transform.localScale = originalScale;

            GameObject targetItem = null;
            GameObject targetShelf = null;

            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Item") && hit.collider.gameObject != draggedItem)
                    targetItem = hit.collider.gameObject;
                
                if (hit.collider.CompareTag("Shelf"))
                    targetShelf = hit.collider.gameObject;
            }

            bool actionSuccessful = false;

            if (targetItem != null)
            {
                SwapItems(draggedItem, targetItem, startAnchor, startPosition);
                actionSuccessful = true;
            }
            else if (targetShelf != null)
            {
                actionSuccessful = MoveToShelf(draggedItem, targetShelf, startAnchor);
            }

            if (!actionSuccessful)
            {
                draggedItem.transform.SetParent(startAnchor);
                draggedItem.transform.localPosition = startPosition; // Thường là 0,0,0
            }

            draggedItem = null; 
        }
    }

    void SwapItems(GameObject draggedObj, GameObject targetObj, Transform sourceAnchor, Vector3 sourcePos)
    {
        Transform targetAnchor = targetObj.transform.parent;

        targetObj.transform.SetParent(sourceAnchor);
        targetObj.transform.localPosition = sourcePos; // Thường là 0,0,0

        draggedObj.transform.SetParent(targetAnchor);
        draggedObj.transform.localPosition = Vector3.zero;

        // Dùng GetComponentInParent để tìm ngược lên chiếc Kệ gỗ
        ShelfController sourceShelf = sourceAnchor.GetComponentInParent<ShelfController>();
        ShelfController targetShelf = targetAnchor.GetComponentInParent<ShelfController>();

        if (sourceShelf != null) UpdateShelfLogic(sourceShelf);
        if (targetShelf != null) UpdateShelfLogic(targetShelf);
    }

    bool MoveToShelf(GameObject item, GameObject targetShelfObj, Transform sourceAnchor)
    {
        ShelfController targetShelf = targetShelfObj.GetComponent<ShelfController>();
        if (targetShelf == null) return false;

        int emptySlot = targetShelf.GetFirstEmptySlot();
        if (emptySlot != -1) 
        {
            targetShelf.AssignItemToSlot(emptySlot, item);

            ShelfController sourceShelf = sourceAnchor.GetComponentInParent<ShelfController>();
            if (sourceShelf != null) UpdateShelfLogic(sourceShelf);
            UpdateShelfLogic(targetShelf);
            return true;
        }
        return false; 
    }

    void UpdateShelfLogic(ShelfController shelf)
    {
        if (shelf == null) return;

        // Xóa sạch mảng
        for (int i = 0; i < 3; i++) shelf.slots[i] = null;

        // Quét qua 3 cái Card (Điểm neo) xem Card nào đang có con thì nhặt con đó vào mảng
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