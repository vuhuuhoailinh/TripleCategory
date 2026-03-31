using UnityEngine;

public class ShelfController : MonoBehaviour
{
    [Header("Dữ liệu Vật phẩm đang chứa")]
    public GameObject[] slots = new GameObject[3]; 
    
    [Header("Điểm Neo (Kéo Card1, Card2, Card3 vào đây)")]
    public Transform[] slotAnchors = new Transform[3];

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] == null) return i;
        return -1; 
    }

    public void AssignItemToSlot(int index, GameObject item)
    {
        slots[index] = item;
        if (item != null)
        {
            // Bắt Item làm con của điểm Neo (Card) tương ứng
            item.transform.SetParent(slotAnchors[index]);
            
            // Ép tọa độ về 0,0,0 để nó lọt khít vào giữa tấm Card
            item.transform.localPosition = Vector3.zero;
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
            Debug.Log($"<color=yellow>BÙM! Kệ {gameObject.name} MATCH 3 CATEGORY {id0}!</color>");
            
            Destroy(slots[0]);
            Destroy(slots[1]);
            Destroy(slots[2]);

            slots[0] = null;
            slots[1] = null;
            slots[2] = null;
        }
    }
}