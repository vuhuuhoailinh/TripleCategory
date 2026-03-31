using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemCategory
{
    public string categoryName; 
    public int categoryID;      
    public Sprite[] sprites;    
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

    [Header("Cấu hình Lưới")]
    [Range(1, 5)] public int columns = 2;
    [Range(1, 10)] public int rows = 5;
    public float spacingX = 3.5f;
    public float spacingY = 2.5f;

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
            Debug.LogError($"<color=red>LỖI:</color> Cần {categoriesNeeded} Category nhưng chỉ có {availableCategories.Length}!");
            return;
        }

        List<SpawnData> masterItemList = new List<SpawnData>();

        List<ItemCategory> shuffledCategories = new List<ItemCategory>(availableCategories);
        for (int i = 0; i < shuffledCategories.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledCategories.Count);
            ItemCategory temp = shuffledCategories[i];
            shuffledCategories[i] = shuffledCategories[randomIndex];
            shuffledCategories[randomIndex] = temp;
        }

        for (int i = 0; i < categoriesNeeded; i++)
        {
            ItemCategory selectedCat = shuffledCategories[i];

            if (selectedCat.sprites.Length < 3)
            {
                Debug.LogError($"<color=red>LỖI:</color> Category '{selectedCat.categoryName}' không đủ 3 ảnh!");
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

        for (int i = 0; i < masterItemList.Count; i++)
        {
            int randomIndex = Random.Range(i, masterItemList.Count);
            SpawnData temp = masterItemList[i];
            masterItemList[i] = masterItemList[randomIndex];
            masterItemList[randomIndex] = temp;
        }

        float startX = -(columns - 1) * spacingX / 2f;
        float startY = -(rows - 1) * spacingY / 2f;
        int currentItemIndex = 0;

        // Vòng lặp Y đi từ hàng CAO NHẤT (rows - 1) lùi dần về 0
        for (int y = rows - 1; y >= 0; y--)
        {
            // Vòng lặp X đi từ TRÁI (0) sang PHẢI (columns)
            for (int x = 0; x < columns; x++)
            {
                Vector2 pos = new Vector2(startX + (x * spacingX), startY + (y * spacingY));
                GameObject shelf = Instantiate(shelfPrefab, pos, Quaternion.identity, this.transform);
                shelf.name = $"Shelf_Row{y}_Col{x}";
                
                SpawnItemsOnShelf(shelf.transform, masterItemList, ref currentItemIndex);
            }
        }

        // Báo cáo tổng số hàng hóa cho Trọng tài
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterTotalItems(categoriesNeeded * 3);
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
            if (itemLogic != null) itemLogic.SetupItem(data.categoryID, data.sprite);

            // Cập nhật hàm SpawnItemsOnShelf
            if (shelfLogic != null) 
            {
                shelfLogic.AssignItemToSlot(i, itemObj);
                itemObj.transform.localPosition = Vector3.zero; // BỔ SUNG DÒNG NÀY ĐỂ ÉP VỊ TRÍ KHI MỚI SINH RA
            }
            
            ItemAnimator animator = itemObj.GetComponent<ItemAnimator>();
            if (animator != null)
            {
                // TĂNG DELAY LÊN 0.1s ĐỂ MẮT NGƯỜI NHÌN RÕ HIỆU ỨNG DOMINO LƯỚT TỪNG THẺ
                float delayTime = counter * 0.025f; 
                animator.AnimateIn(delayTime);
            }

            counter++;
        }

        
    }
}