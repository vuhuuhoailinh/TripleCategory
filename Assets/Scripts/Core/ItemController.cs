using UnityEngine;

public class ItemController : MonoBehaviour
{
    [Header("Thông tin Dữ liệu")]
    public int categoryID; // Mã định danh họ hàng (VD: 100 là Zombie, 200 là Trái cây)

    [Header("Thành phần Giao diện")]
    [Tooltip("Kéo đối tượng con 'Item_Icon' vào đây")]
    public SpriteRenderer iconRenderer; 

    // Hàm này được LevelManager gọi để thiết lập dữ liệu khi sinh ra
    public void SetupItem(int catID, Sprite newSprite)
    {
        this.categoryID = catID;
        
        // Thay hình ảnh cho đối tượng con
        if (iconRenderer != null)
        {
            iconRenderer.sprite = newSprite;
        }
        else
        {
            Debug.LogWarning($"Item_Base {gameObject.name} chưa được gán Icon Renderer!");
        }
        
        // Đổi tên Object trong Hierarchy cho dễ quản lý
        gameObject.name = $"Item_Cat{catID}_{newSprite.name}";
    }
}