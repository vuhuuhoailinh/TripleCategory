using UnityEngine;
using DG.Tweening;

public class DragManager : MonoBehaviour
{
    [Header("Trạng thái kéo thả")]
    private GameObject draggedItem;
    private Vector3 startPosition;
    private Transform startAnchor;
    private Vector3 originalScale;

    private ShelfController currentHoveredShelf;
    private ItemAnimation currentItemAnim;

    private bool IsInputLocked()
    {
        return GameManager.Instance != null && GameManager.Instance.isGameOver;
    }

    void Update()
    {
        if (IsInputLocked())
        {
            if (draggedItem != null)
            {
                ForceDropCard();
            }
            return;
        }

        if (!Input.GetMouseButton(0) && draggedItem != null)
        {
            ForceDropCard();
            return;
        }

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
                    if (itemLogic != null && !itemLogic.isFaceUp)
                    {
                        continue;
                    }

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
                    GameEvents.OnCardPicked?.Invoke();
                    break;
                }
            }
        }

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

        if (draggedItem == null)
        {
            return;
        }

        Vector2 dropPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] dropHits = Physics2D.OverlapPointAll(dropPos);

        Collider2D draggedCollider = draggedItem.GetComponent<Collider2D>();
        if (draggedCollider != null)
        {
            draggedCollider.enabled = true;
        }

        draggedItem.transform.localScale = originalScale;
        if (currentItemAnim != null)
        {
            currentItemAnim.RestoreSortingOrder();
        }

        GameObject targetItem = null;
        GameObject targetShelf = null;

        foreach (var col in dropHits)
        {
            if (col.CompareTag("Item") && col.gameObject != draggedItem)
            {
                targetItem = col.gameObject;
            }

            if (col.CompareTag("Shelf"))
            {
                targetShelf = col.gameObject;
            }
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
            Debug.LogError("Co loi khi tha bai: " + e.Message);
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

            DOVirtual.DelayedCall(failSlideTime, () =>
            {
                GameEvents.OnCardDropped?.Invoke();
            });
        }
    }
}
