using UnityEngine;

public class ShelfController : MonoBehaviour
{
    [Header("Dữ liệu Vật phẩm (Mặc định là 3 Card)")]
    public GameObject[] slots = new GameObject[3];
    public Transform[] slotAnchors = new Transform[3];

    private ShelfAnimation animator; // Thợ diễn hoạt ảnh

    public bool isMatching = false; // Cờ hiệu đang trong quá trình Match-3, để tạm khóa tương tác

    void Awake()
    {
        animator = GetComponent<ShelfAnimation>();
        if (animator != null)
        {
            animator.Initialize(slotAnchors); // Bàn giao mảng Anchor cho Animator tự tìm đồ nghề
        }
    }

    public int GetClosestSlotAny(Vector2 dropPosition)
    {
        int bestIndex = -1;
        float minDistance = 999f;
        for (int i = 0; i < 3; i++)
        {
            if (slotAnchors[i] != null)
            {
                float dist = Vector2.Distance(dropPosition, slotAnchors[i].position);
                if (dist < minDistance) { minDistance = dist; bestIndex = i; }
            }
        }
        return bestIndex;
    }

public int GetClosestEmptySlot(Vector2 dropPosition)
    {
        int bestIndex = -1;
        float minDistance = 999f;
        for (int i = 0; i < 3; i++)
        {
            bool isEmpty = (slots[i] == null) || (slots[i].GetComponent<ItemController>() == null);

            if (isEmpty && slotAnchors[i] != null)
            {
                float dist = Vector2.Distance(dropPosition, slotAnchors[i].position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestIndex = i;
                }
            }
        }
        return bestIndex;
    }

    // --- ỦY QUYỀN CHO ANIMATOR ---
    public void ShowHover(int index) { if (animator != null) animator.ShowHover(index); }
    public void ClearHover() { if (animator != null) animator.ClearHover(); }
    public void PlayDropGlow(int index) { if (animator != null) animator.PlayDropGlow(index); }

    // --- LOGIC XỬ LÝ GAMEPLAY ---
    public void AssignItemToSlot(int index, GameObject item)
    {
        slots[index] = item;
        if (item != null && slotAnchors[index] != null)
        {
            item.transform.SetParent(slotAnchors[index]);
        }
    }

    public void CheckForMatch()
    {
        if (slots[0] == null || slots[1] == null || slots[2] == null) return;

        ItemController item0 = slots[0].GetComponent<ItemController>();
        ItemController item1 = slots[1].GetComponent<ItemController>();
        ItemController item2 = slots[2].GetComponent<ItemController>();

        if (item0 == null || item1 == null || item2 == null) return;

        int id0 = item0.categoryID;
        int id1 = item1.categoryID;
        int id2 = item2.categoryID;

        if (id0 == id1 && id1 == id2)
        {   
            string matchName = item0.categoryName;

            isMatching = true; // Bật cờ hiệu đang Match, để tạm khóa tương tác
            GameEvents.OnMatchDetected?.Invoke();
            
            GameObject obj0 = slots[0];
            GameObject obj1 = slots[1];
            GameObject obj2 = slots[2];

            slots[0] = slotAnchors[0].gameObject;
            slots[1] = slotAnchors[1].gameObject;
            slots[2] = slotAnchors[2].gameObject;
            

            // Giao cho Animator làm hiệu ứng nổ, nổ xong thì báo cho GameManager
            if (animator != null)
            {   
                animator.PlayMatchAnimation(obj0, obj1, obj2, matchName, () => {
                    isMatching = false; // Tắt cờ hiệu đang Match, mở khóa tương tác trở lại
                    GameEvents.OnItemsMatched?.Invoke(this);
                });
            }
        }
    }
}