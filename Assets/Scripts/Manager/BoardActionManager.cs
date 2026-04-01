using UnityEngine;
using DG.Tweening;

public class BoardActionManager : MonoBehaviour
{
    public static BoardActionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Hàm tổng đón nhận yêu cầu từ DragManager
    public bool TryProcessDrop(GameObject draggedObj, GameObject targetItem, GameObject targetShelfObj, Transform sourceAnchor, Vector3 sourcePos, Vector2 dropPos)
    {
        if (targetItem != null)
        {
            SwapItems(draggedObj, targetItem, sourceAnchor, sourcePos);
            return true; // Xử lý thành công
        }
        else if (targetShelfObj != null)
        {
            return MoveToShelf(draggedObj, targetShelfObj, sourceAnchor, dropPos);
        }
        return false; // Trượt ra ngoài
    }

    // Bê nguyên xi hàm SwapItems từ DragManager sang đây
    private void SwapItems(GameObject draggedObj, GameObject targetObj, Transform sourceAnchor, Vector3 sourcePos)
    {
        Transform targetAnchor = targetObj.transform.parent;
        float slideTime = 0.25f; 

        targetObj.transform.SetParent(sourceAnchor);
        ItemAnimation targetAnim = targetObj.GetComponent<ItemAnimation>();
        if (targetAnim != null) { targetAnim.StopDragAndDrop(sourcePos); slideTime = targetAnim.dropDuration; }
        else targetObj.transform.localPosition = sourcePos; 

        draggedObj.transform.SetParent(targetAnchor);
        ItemAnimation dragAnim = draggedObj.GetComponent<ItemAnimation>();
        if (dragAnim != null) dragAnim.StopDragAndDrop(Vector3.zero);
        else draggedObj.transform.localPosition = Vector3.zero;

        ShelfController sourceShelf = sourceAnchor.GetComponentInParent<ShelfController>();
        ShelfController targetShelf = targetAnchor.GetComponentInParent<ShelfController>();

        if (sourceShelf != null) UpdateShelfLogic(sourceShelf);
        if (targetShelf != null) UpdateShelfLogic(targetShelf);

        DOVirtual.DelayedCall(slideTime, () => {
            if (sourceShelf != null) {
                for(int i=0; i<3; i++) if (sourceShelf.slots[i] == targetObj) sourceShelf.PlayDropGlow(i);
            }
            if (targetShelf != null) {
                for(int i=0; i<3; i++) if (targetShelf.slots[i] == draggedObj) targetShelf.PlayDropGlow(i);
            }
        });

        GameEvents.OnMoveUsed?.Invoke();
    }

    // Bê nguyên xi hàm MoveToShelf từ DragManager sang đây
    private bool MoveToShelf(GameObject item, GameObject targetShelfObj, Transform sourceAnchor, Vector2 dropPos)
    {
        ShelfController targetShelf = targetShelfObj.GetComponent<ShelfController>();
        if (targetShelf == null) return false;

        int emptySlot = targetShelf.GetClosestEmptySlot(dropPos);
        if (emptySlot != -1) 
        {
            targetShelf.AssignItemToSlot(emptySlot, item);
            
            float slideTime = 0.25f; 
            ItemAnimation dragAnim = item.GetComponent<ItemAnimation>();
            if (dragAnim != null) { dragAnim.StopDragAndDrop(Vector3.zero); slideTime = dragAnim.dropDuration; }
            else item.transform.localPosition = Vector3.zero;

            ShelfController sourceShelf = sourceAnchor.GetComponentInParent<ShelfController>();
            if (sourceShelf != null) UpdateShelfLogic(sourceShelf);
            UpdateShelfLogic(targetShelf);
            
            DOVirtual.DelayedCall(slideTime, () => {
                if (targetShelf != null) targetShelf.PlayDropGlow(emptySlot);
            });

            GameEvents.OnMoveUsed?.Invoke();
            return true;
        }
        return false; 
    }

    // Bê nguyên xi hàm UpdateShelfLogic sang đây
    private void UpdateShelfLogic(ShelfController shelf)
    {
        if (shelf == null) return;
        for (int i = 0; i < 3; i++) 
        {
            if (shelf.slotAnchors[i] != null) shelf.slots[i] = shelf.slotAnchors[i].gameObject;
            else shelf.slots[i] = null;
        }

        for (int i = 0; i < 3; i++)
        {
            Transform anchor = shelf.slotAnchors[i];
            if (anchor != null && anchor.childCount > 0)
            {
                foreach (Transform child in anchor)
                {
                    if (child.CompareTag("Item")) { shelf.AssignItemToSlot(i, child.gameObject); break; }
                }
            }
        }
        shelf.CheckForMatch();
    }
}