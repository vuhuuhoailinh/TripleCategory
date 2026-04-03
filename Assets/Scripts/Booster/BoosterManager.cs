using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class BoosterManager : MonoBehaviour
{
    private bool isShuffling = false;

    private void OnEnable()
    {
        GameEvents.OnShuffleRequested += PerformShuffle;
    }

    private void OnDisable()
    {
        GameEvents.OnShuffleRequested -= PerformShuffle;
    }

    private void PerformShuffle()
    {
        LevelManager lm = LevelManager.Instance;
        if (isShuffling || GameManager.Instance.isGameOver || Time.timeScale == 0f || lm == null) return;
        
        isShuffling = true;
        GameEvents.OnUIClick?.Invoke(); 

        List<GameObject> boardItems = new List<GameObject>();

        // --- BƯỚC 1: THU THẬP BÀI TRÊN KỆ CHÍNH (BỎ QUA KỆ TẠM) ---
        foreach (var shelf in lm.activeShelves)
        {
            if (shelf == null || shelf.isMatching) continue; 
            
            if (shelf.gameObject.name.Contains("Locked")) continue;

            for (int i = 0; i < 3; i++)
            {
                if (shelf.slots[i] != null)
                {
                    Collider2D col = shelf.slots[i].GetComponent<Collider2D>();
                    if (col != null && col.enabled) 
                    {
                        boardItems.Add(shelf.slots[i]);
                        shelf.slots[i] = null; 
                    }
                }
            }
        }

        if (boardItems.Count < 2) 
        {
            isShuffling = false;
            return; 
        }

        // --- BƯỚC 2: XÁO TRỘN DANH SÁCH BÀI CŨ ---
        for (int i = 0; i < boardItems.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, boardItems.Count);
            GameObject temp = boardItems[i];
            boardItems[i] = boardItems[rand];
            boardItems[rand] = temp;
        }

        // --- BƯỚC 3: XẾP LẠI BÀI VÀ FILL THÊM TỪ KHO ---
        int itemIndex = 0;
        int columns = lm.columns; 
        
        for (int s = 0; s < lm.activeShelves.Count; s++)
        {
            var shelf = lm.activeShelves[s];
            if (shelf == null || shelf.isMatching || shelf.gameObject.name.Contains("Locked")) continue;

            int row = s / columns;
            int col = s % columns;
            float shelfBaseDelay = (row + col) * 0.15f; 

            for (int i = 0; i < 3; i++)
            {
                GameObject itemToAssign = null;

                // ƯU TIÊN 1: Lấy bài cũ đã xáo để xếp lên trước
                if (itemIndex < boardItems.Count)
                {
                    itemToAssign = boardItems[itemIndex];
                    itemIndex++;
                }
                // ƯU TIÊN 2: Bài cũ đã hết mà kệ vẫn trống -> Thò tay vào Kho lấy bài mới
                else if (lm.currentItemIndex < lm.masterItemList.Count)
                {
                    SpawnData data = lm.masterItemList[lm.currentItemIndex];
                    itemToAssign = Instantiate(lm.baseItemPrefab);
                    
                    ItemController ic = itemToAssign.GetComponent<ItemController>();
                    if (ic != null) 
                    {
                        ic.SetupItem(data.categoryID, data.categoryName, data.sprite);
                        // Nếu bác có dùng biến originalData thì gán luôn ở đây cho chắc cốp
                        ic.originalData = data; 
                    }
                    lm.currentItemIndex++;
                }

                // NẾU CÓ BÀI (CŨ HOẶC MỚI) THÌ GẮN VÀO KỆ VÀ CHẠY ANIMATION LẬT CHÉO
                if (itemToAssign != null)
                {
                    shelf.AssignItemToSlot(i, itemToAssign);
                    itemToAssign.transform.localPosition = Vector3.zero; 

                    ItemAnimation animator = itemToAssign.GetComponent<ItemAnimation>();
                    if (animator != null)
                    {
                        animator.AnimateIn(shelfBaseDelay + (i * 0.1f));
                    }
                }
            }
        }

        DOVirtual.DelayedCall(1.5f, () => { isShuffling = false; });
    }
}