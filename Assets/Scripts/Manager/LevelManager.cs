using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemCategory
{
    public string categoryName; // Tên họ (VD: Zombie)
    public int categoryID;      // Mã định danh (VD: 100)
    public Sprite[] sprites;    // Danh sách các ảnh trong họ này
}

public struct SpawnData
{
    public int categoryID;
    public Sprite sprite;
}

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs Đầu Vào")]
    public GameObject shelfPrefab;    
    public GameObject baseItemPrefab;  
    
    [Header("Dữ liệu Category")]
    public ItemCategory[] availableCategories; 

    [Header("Cấu hình Lưới (Grid Settings)")]
    [Range(1, 5)] public int columns = 2;
    [Range(1, 10)] public int rows = 5;
    public float spacingX = 3.5f;
    public float spacingY = 2.5f;
    
    // ĐÃ XÓA: Cấu hình Item (itemSpacing, itemVerticalOffset) vì đã dùng Anchor

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        if (shelfPrefab == null || baseItemPrefab == null || availableCategories == null || availableCategories.Length == 0) return;

        int totalShelves = columns * rows;
        int categoriesNeeded = totalShelves; 

        if (availableCategories.Length < categoriesNeeded)
        {
            Debug.LogError($"<color=red>LỖI KỊCH BẢN:</color> Cần {categoriesNeeded} Category nhưng chỉ có {availableCategories.Length}!");
            return;
        }

        List<SpawnData> masterItemList = new List<SpawnData>();

        // Lọc và xóc đĩa Category
        List<ItemCategory> shuffledCategories = new List<ItemCategory>(availableCategories);
        for (int i = 0; i < shuffledCategories.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledCategories.Count);
            ItemCategory temp = shuffledCategories[i];
            shuffledCategories[i] = shuffledCategories[randomIndex];
            shuffledCategories[randomIndex] = temp;
        }

        // Rút Sprite
        for (int i = 0; i < categoriesNeeded; i++)
        {
            ItemCategory selectedCat = shuffledCategories[i];

            if (selectedCat.sprites.Length < 3)
            {
                Debug.LogError($"<color=red>LỖI DỮ LIỆU:</color> Category '{selectedCat.categoryName}' không đủ 3 ảnh!");
                return;
            }

            for (int j = 0; j < 3; j++)
            {
                SpawnData data = new SpawnData();
                data.categoryID = selectedCat.categoryID;
                data.sprite = selectedCat.sprites[j]; 
                masterItemList.Add(data);
            }
        }

        // Xóc đĩa toàn bộ Item
        for (int i = 0; i < masterItemList.Count; i++)
        {
            int randomIndex = Random.Range(i, masterItemList.Count);
            SpawnData temp = masterItemList[i];
            masterItemList[i] = masterItemList[randomIndex];
            masterItemList[randomIndex] = temp;
        }

        // Bày ra kệ
        float startX = -(columns - 1) * spacingX / 2f;
        float startY = -(rows - 1) * spacingY / 2f;
        int currentItemIndex = 0;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector2 pos = new Vector2(startX + (x * spacingX), startY + (y * spacingY));
                GameObject shelf = Instantiate(shelfPrefab, pos, Quaternion.identity, this.transform);
                shelf.name = $"Shelf_{x}_{y}";
                
                SpawnItemsOnShelf(shelf.transform, masterItemList, ref currentItemIndex);
            }
        }
    }

    void SpawnItemsOnShelf(Transform parentShelf, List<SpawnData> dataList, ref int counter)
    {
        ShelfController shelfLogic = parentShelf.GetComponent<ShelfController>();

        for (int i = 0; i < 3; i++)
        {
            SpawnData data = dataList[counter];
            
            GameObject itemObj = Instantiate(baseItemPrefab); 

            ItemController itemLogic = itemObj.GetComponent<ItemController>();
            if (itemLogic != null)
            {
                itemLogic.SetupItem(data.categoryID, data.sprite);
            }

            if (shelfLogic != null)
            {
                shelfLogic.AssignItemToSlot(i, itemObj);
            }
            
            counter++;
        }
    }
}