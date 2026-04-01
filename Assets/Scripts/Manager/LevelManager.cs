using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // Cần thêm thư viện này để dùng DOVirtual nếu cần

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
    public GameObject adsLockPrefab; 
    
    [Header("Dữ liệu Category")]
    public ItemCategory[] availableCategories; 

    [Header("Cấu hình Level (Hàng chờ)")]
    [Tooltip("Tổng số category cần clear trong màn này (Kho đạn)")]
    public int totalCategoriesForLevel = 30; 

    [Header("Cấu hình Lưới (Grid)")]
    [Range(1, 5)] public int columns = 2;
    [Range(1, 10)] public int rows = 5;

    public float spacingX = 3.5f;
    public float spacingY = 2.5f;

    // --- MỚI: CÁC BIẾN QUẢN LÝ KHO ĐẠN VÀ KỆ TRỐNG ---
    private List<SpawnData> masterItemList = new List<SpawnData>();
    private int currentItemIndex = 0; // Đang bốc đến lá bài thứ mấy trong kho
    private List<ShelfController> activeShelves = new List<ShelfController>(); // Danh sách các kệ chính

    void Start()
    {
        GenerateLevel();
    }

    // --- MỚI: Lắng nghe sự kiện nổ thẻ của GameEvents ---
    private void OnEnable() 
    {
        GameEvents.OnItemsMatched += HandleShelfCleared;
        GameEvents.OnMoveUsed += CheckForEmptyShelves; // MỚI: Nghe tiếng di chuyển thẻ
    }
    private void OnDisable() 
    {
        GameEvents.OnItemsMatched -= HandleShelfCleared;
        GameEvents.OnMoveUsed -= CheckForEmptyShelves;
    }
    void GenerateLevel()
    {
        if (shelfPrefab == null || baseItemPrefab == null || availableCategories == null || availableCategories.Length == 0) return;

        // Reset dữ liệu kho mỗi khi chơi lại level
        masterItemList.Clear();
        activeShelves.Clear();
        currentItemIndex = 0;

        // TỔNG SỐ CATEGORY CỦA TOÀN BỘ MÀN CHƠI
        int categoriesNeeded = totalCategoriesForLevel; 

        List<ItemCategory> shuffledCategories = new List<ItemCategory>(availableCategories);
        for (int i = 0; i < shuffledCategories.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledCategories.Count);
            ItemCategory temp = shuffledCategories[i];
            shuffledCategories[i] = shuffledCategories[randomIndex];
            shuffledCategories[randomIndex] = temp;
        }

        // Tạo ra kho đạn khổng lồ
        for (int i = 0; i < categoriesNeeded; i++)
        {
            // Dùng [i % Count] để nếu bạn setting 30 categories mà kho chỉ có 10, nó sẽ quay vòng lại dùng tiếp cho đủ 30
            ItemCategory selectedCat = shuffledCategories[i % shuffledCategories.Count];

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

        // XÁO TRỘN TOÀN BỘ KHO ĐẠN (Để lúc nạp đạn vào kệ sẽ ra random)
        for (int i = 0; i < masterItemList.Count; i++)
        {
            int randomIndex = Random.Range(i, masterItemList.Count);
            SpawnData temp = masterItemList[i];
            masterItemList[i] = masterItemList[randomIndex];
            masterItemList[randomIndex] = temp;
        }

        float startX = -(columns - 1) * spacingX / 2f;
        int totalRowsIncludeLocked = rows + 1;
        float startY = (totalRowsIncludeLocked - 1) * spacingY / 2f;

        // 1. SINH RA CÁC HÀNG CHÍNH VÀ ĐỔ ĐỒ TỪ KHO
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 pos = new Vector2(startX + (col * spacingX), startY - (row * spacingY));
                GameObject shelfObj = Instantiate(shelfPrefab, pos, Quaternion.identity, this.transform);
                shelfObj.name = $"Shelf_Row{row}_Col{col}";

                ShelfController shelfLogic = shelfObj.GetComponent<ShelfController>();
                if (shelfLogic != null) activeShelves.Add(shelfLogic); // Đưa vào sổ nam tào để theo dõi

                float shelfBaseDelay = (row + col) * 0.15f;
                
                // Trút 3 thẻ từ kho vào kệ này
                SpawnItemsOnShelf(shelfObj.transform, masterItemList, ref currentItemIndex, shelfBaseDelay);
            }
        }

        // 2. SINH RA HÀNG KỆ DỰ PHÒNG Ở DƯỚI CÙNG VÀ BỌC Ổ KHÓA LÊN
        float bottomY = startY - (rows * spacingY); 
        for (int col = 0; col < columns; col++)
        {
            Vector2 pos = new Vector2(startX + (col * spacingX), bottomY);
            GameObject extraShelfObj = Instantiate(shelfPrefab, pos, Quaternion.identity, this.transform);
            extraShelfObj.name = $"Shelf_Locked_Col{col}";
            
            if (adsLockPrefab != null)
            {
                GameObject lockObj = Instantiate(adsLockPrefab, pos, Quaternion.identity, this.transform);
                AdsLock lockScript = lockObj.GetComponent<AdsLock>();
                Collider2D shelfCol = extraShelfObj.GetComponent<Collider2D>();
                if (lockScript != null && shelfCol != null) lockScript.BlockShelf(shelfCol);
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
            // --- MỚI: BẢO VỆ CHỐNG TRÀN ---
            // Nếu đã bốc hết kho đạn thì dừng lại, không sinh thêm thẻ nào nữa
            if (counter >= dataList.Count) break; 

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
                float finalDelay = shelfBaseDelay + (i * 0.1f); 
                animator.AnimateIn(finalDelay);
            }

            counter++;
        }
    }

    // --- MỚI: HÀM NÀY CHẠY MỖI KHI CÓ MỘT KỆ NỔ MATCH-3 XONG ---
    void HandleShelfCleared(ShelfController clearedShelf)
    {
        CheckForEmptyShelves();
    }

    void CheckForEmptyShelves()
    {
        foreach (ShelfController shelf in activeShelves)
        {
            // Bảo vệ chống tràn kho
            if (currentItemIndex >= masterItemList.Count) return; 

            if (IsShelfTotallyEmpty(shelf))
            {
                // Delay 0.2s để tấm thẻ mà bạn vừa bốc có thời gian trượt đi chỗ khác
                DOVirtual.DelayedCall(0.2f, () => {
                    
                    // Lớp bảo vệ 2: Check lại lần nữa xem kệ có CÒN trống không 
                    // (Phòng hờ trong 0.2s đó bạn "tay nhanh hơn não" lại ném 1 thẻ khác vào)
                    if (IsShelfTotallyEmpty(shelf) && currentItemIndex < masterItemList.Count) 
                    {
                        SpawnItemsOnShelf(shelf.transform, masterItemList, ref currentItemIndex, 0f);
                    }
                });
            }
        }
    }

    // Hàm kiểm tra xem kệ có thực sự trống không (không có thẻ bài nào trên đó)
// Hàm kiểm tra kệ trống tuyệt đối (Giữ nguyên)
    bool IsShelfTotallyEmpty(ShelfController shelf)
    {
        if (shelf == null || shelf.isMatching) return false;
        
        for (int i = 0; i < 3; i++)
        {
            if (shelf.slots[i] != null && shelf.slots[i].GetComponent<ItemController>() != null)
            {
                return false;
            }
        }
        return true;
    }
}