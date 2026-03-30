using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs Đầu Vào")]
    public GameObject shelfPrefab;    
    
    [Tooltip("Kéo Item_Base Prefab vào đây")]
    public GameObject baseItemPrefab;  
    
    [Tooltip("Kéo các ảnh Emoji đã cắt từ Atlas vào đây")]
    public Sprite[] availableSprites;

    [Header("Cấu hình Lưới (Grid Settings)")]
    [Range(1, 5)] public int columns = 2;
    [Range(1, 10)] public int rows = 5;
    
    [Header("Khoảng cách Kệ (Shelf Spacing)")]
    [Range(1f, 10f)] public float spacingX = 3.5f;
    [Range(1f, 10f)] public float spacingY = 2.5f;
    
    [Header("Cấu hình Vật phẩm (Item Settings)")]
    [Range(0.1f, 3f)] public float itemSpacing = 1.2f;
    [Range(-2f, 2f)] public float itemVerticalOffset = 0.2f;    

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        if (shelfPrefab == null || baseItemPrefab == null || availableSprites == null || availableSprites.Length == 0)
        {
            Debug.LogError("LevelManager: Bác chưa gán Prefab Kệ, Base Item Prefab hoặc chưa thêm Sprites vào mảng!");
            return;
        }

        // ĐÃ SỬA: Đã cập nhật thành spacingX và spacingY
        float startX = -(columns - 1) * spacingX / 2f;
        float startY = -(rows - 1) * spacingY / 2f;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                // ĐÃ SỬA: Đã cập nhật thành spacingX và spacingY
                Vector2 shelfPos = new Vector2(startX + (x * spacingX), startY + (y * spacingY));
                GameObject shelf = Instantiate(shelfPrefab, shelfPos, Quaternion.identity, this.transform);
                shelf.name = $"Shelf_{x}_{y}";

                SpawnItemsOnShelf(shelf.transform);
            }
        }
    }

    void SpawnItemsOnShelf(Transform parentShelf)
    {
        ShelfController shelfLogic = parentShelf.GetComponent<ShelfController>();
        
        // ĐÃ SỬA: Đã cập nhật thành itemSpacing
        float[] slotPositionsX = { -itemSpacing, 0f, itemSpacing };

        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, availableSprites.Length);
            Sprite selectedSprite = availableSprites[randomIndex];
            
            GameObject itemObj = Instantiate(baseItemPrefab, parentShelf);
            
            // ĐÃ SỬA: Sử dụng biến thanh trượt itemVerticalOffset thay vì số 0.2f cố định
            itemObj.transform.localPosition = new Vector3(slotPositionsX[i], itemVerticalOffset, 0f); 

            ItemController itemLogic = itemObj.GetComponent<ItemController>();
            if (itemLogic != null)
            {
                itemLogic.SetupItem(randomIndex, selectedSprite);
            }

            if (shelfLogic != null)
            {
                shelfLogic.AssignItemToSlot(i, itemObj);
            }
        }
    }
}