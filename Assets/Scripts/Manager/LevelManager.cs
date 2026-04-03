using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; 

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
    public GameObject winPanelPrefab;
    public GameObject losePanelPrefab;
    
    [Header("Dữ liệu Category")]
    public ItemCategory[] availableCategories; 

    [Header("Cấu hình Lưới (Grid)")]
    [Range(1, 5)] public int columns = 2;
    [Range(1, 10)] public int rows = 5;

    public float spacingX = 3.5f;
    public float spacingY = 2.5f;

    // Các biến quản lý kho
    public List<SpawnData> masterItemList = new List<SpawnData>();
    public int currentItemIndex = 0; 
    public static LevelManager Instance;
    public List<ShelfController> activeShelves = new List<ShelfController>(); 

    void Awake()
    {
        if (Instance == null) Instance = this;
    }
    void Start()
    {
        GenerateLevel();
    }

    private void OnEnable() 
    {
        GameEvents.OnItemsMatched += HandleShelfCleared;
        GameEvents.OnMoveUsed += CheckForEmptyShelves; 
        GameEvents.OnLevelWin += ShowWinPanel; // Kênh nổ WinPanel
        GameEvents.OnLevelLose += ShowLosePanel; // Kênh nổ LosePanel
    }
    
    private void OnDisable() 
    {
        GameEvents.OnItemsMatched -= HandleShelfCleared;
        GameEvents.OnMoveUsed -= CheckForEmptyShelves;
        GameEvents.OnLevelWin -= ShowWinPanel;
        GameEvents.OnLevelLose -= ShowLosePanel;
    }

    void GenerateLevel()
    {
        if (shelfPrefab == null || baseItemPrefab == null || availableCategories == null || availableCategories.Length == 0) return;

        masterItemList.Clear();
        activeShelves.Clear();
        currentItemIndex = 0;

        // --- ĐÃ SỬA: Lấy số lượng từ GameManager ---
        int categoriesNeeded = GameManager.Instance.targetCategories; 

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
            ItemCategory selectedCat = shuffledCategories[i % shuffledCategories.Count];

            if (selectedCat.sprites.Length < 3) continue;

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

        float startX = -(columns - 1) * spacingX / 2f;
        int totalRowsIncludeLocked = rows + 1;
        float startY = (totalRowsIncludeLocked - 1) * spacingY / 2f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector2 pos = new Vector2(startX + (col * spacingX), startY - (row * spacingY));
                GameObject shelfObj = Instantiate(shelfPrefab, pos, Quaternion.identity, this.transform);
                shelfObj.name = $"Shelf_Row{row}_Col{col}";

                ShelfController shelfLogic = shelfObj.GetComponent<ShelfController>();
                if (shelfLogic != null) activeShelves.Add(shelfLogic); 

                float shelfBaseDelay = (row + col) * 0.15f;
                SpawnItemsOnShelf(shelfObj.transform, masterItemList, ref currentItemIndex, shelfBaseDelay);
            }
        }

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
            if (animator != null) animator.AnimateIn(shelfBaseDelay + (i * 0.1f));

            counter++;
        }
    }

    // --- ĐÃ SỬA: Hàm này giờ chỉ lo check nạp đạn ---
    void HandleShelfCleared(ShelfController clearedShelf)
    {   
        CheckForEmptyShelves();
    }

    void CheckForEmptyShelves()
    {
        foreach (ShelfController shelf in activeShelves)
        {
            if (currentItemIndex >= masterItemList.Count) return; 

            if (IsShelfTotallyEmpty(shelf))
            {
                DOVirtual.DelayedCall(0.2f, () => {
                    if (IsShelfTotallyEmpty(shelf) && currentItemIndex < masterItemList.Count) 
                    {
                        SpawnItemsOnShelf(shelf.transform, masterItemList, ref currentItemIndex, 0f);
                    }
                });
            }
        }
    }

    bool IsShelfTotallyEmpty(ShelfController shelf)
    {
        if (shelf == null || shelf.isMatching) return false;
        for (int i = 0; i < 3; i++)
            if (shelf.slots[i] != null && shelf.slots[i].GetComponent<ItemController>() != null) return false;
        return true;
    }

    // Nổ WinPanel
    private void ShowWinPanel()
    {
        if (winPanelPrefab != null) Instantiate(winPanelPrefab);
    }
    private void ShowLosePanel()
    {
        if (losePanelPrefab != null) Instantiate(losePanelPrefab);
    }
}