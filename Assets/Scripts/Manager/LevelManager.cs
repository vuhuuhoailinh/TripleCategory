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
    public string categoryName;
    public Sprite sprite;
}

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs Đầu Vào")]
    public GameObject shelfPrefab;    
    public GameObject baseItemPrefab;
    public GameObject adsLockPrefab; // Prefab cho ổ khóa quảng cáo
    
    [Header("Dữ liệu Category")]
    public ItemCategory[] availableCategories; 

    [Header("Cấu hình Lưới (Grid)")]
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
                data.categoryName = selectedCat.categoryName;
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

        // --- TÍNH TOÁN TOẠ ĐỘ GỐC CỦA LƯỚI ---
        // startX: Dịch lưới sao cho nó nằm giữa màn hình theo trục X
        float startX = -(columns - 1) * spacingX / 2f;
        
        // Cập nhật lại công thức tính startY để hỗ trợ thêm 1 hàng kệ trống ở dưới
        // Tổng số hàng bây giờ = rows (chứa đồ) + 1 (hàng bị khóa)
        int totalRowsIncludeLocked = rows + 1;
        float startY = (totalRowsIncludeLocked - 1) * spacingY / 2f;

        int currentItemIndex = 0;

        // 1. SINH RA CÁC HÀNG CHÍNH (CÓ THẺ BÀI)
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 pos = new Vector2(startX + (col * spacingX), startY - (row * spacingY));
                GameObject shelfObj = Instantiate(shelfPrefab, pos, Quaternion.identity, this.transform);
                shelfObj.name = $"Shelf_Row{row}_Col{col}";

                // Truyền delay đường chéo cho đẹp
                float shelfBaseDelay = (row + col) * 0.15f;
                SpawnItemsOnShelf(shelfObj.transform, masterItemList, ref currentItemIndex, shelfBaseDelay);
            }
        }

        // 2. SINH RA HÀNG KỆ DỰ PHÒNG Ở DƯỚI CÙNG VÀ BỌC Ổ KHÓA LÊN
        float bottomY = startY - (rows * spacingY); // Nằm dưới hàng chính cuối cùng
        
        // Số lượng ổ khóa bằng số cột của lưới (columns)
        for (int col = 0; col < columns; col++)
        {
            Vector2 pos = new Vector2(startX + (col * spacingX), bottomY);
            
            // Đẻ ra cái kệ trống
            GameObject extraShelfObj = Instantiate(shelfPrefab, pos, Quaternion.identity, this.transform);
            extraShelfObj.name = $"Shelf_Locked_Col{col}";
            
            // Đẻ ra ổ khóa đè lên trên cái kệ
            if (adsLockPrefab != null)
            {
                GameObject lockObj = Instantiate(adsLockPrefab, pos, Quaternion.identity, this.transform);
                AdsLock lockScript = lockObj.GetComponent<AdsLock>();
                
                // Lấy Collider của cái kệ và giao cho ổ khóa "bịt mắt" nó lại
                Collider2D shelfCol = extraShelfObj.GetComponent<Collider2D>();
                if (lockScript != null && shelfCol != null)
                {
                    lockScript.BlockShelf(shelfCol);
                }
            }
        }

        if (GameManager.Instance != null)
        {
            GameEvents.OnLevelGenerated?.Invoke(categoriesNeeded * 3);
        }
    }

    void SpawnItemsOnShelf(Transform parentShelf, List<SpawnData> dataList, ref int counter, float shelfBaseDelay)
    {
        ShelfController shelfLogic = parentShelf.GetComponent<ShelfController>();

        for (int i = 0; i < 3; i++)
        {
            SpawnData data = dataList[counter];
            GameObject itemObj = Instantiate(baseItemPrefab); 

            ItemController itemLogic = itemObj.GetComponent<ItemController>();
            if (itemLogic != null) itemLogic.SetupItem(data.categoryID, data.categoryName, data.sprite);

            if (shelfLogic != null) 
            {
                shelfLogic.AssignItemToSlot(i, itemObj);
                itemObj.transform.localPosition = Vector3.zero; 
            }
            
            ItemAnimation animator = itemObj.GetComponent<ItemAnimation>();
            if (animator != null)
            {
                // TRỄ LẬT DOMINO = Trễ của kệ + Trễ của từng thẻ trong kệ (0.1s cách nhau)
                float finalDelay = shelfBaseDelay + (i * 0.1f); 
                animator.AnimateIn(finalDelay);
            }

            counter++;
        }
    }
}