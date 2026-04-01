using UnityEngine;

public class ItemController : MonoBehaviour
{
    [Header("Thông tin Dữ liệu")]
    public int categoryID; 

    [Header("Thành phần Giao diện")]
    [Tooltip("Kéo đối tượng con 'Item_Icon' vào đây")]
    public SpriteRenderer iconRenderer; 

    public bool isFaceUp = true; // Mặc định sinh ra là ngửa

    public void SetupItem(int catID, Sprite newSprite)
    {
        this.categoryID = catID;
        
        if (iconRenderer != null)
        {
            iconRenderer.sprite = newSprite;
        }
        
        gameObject.name = $"Item_Cat{catID}_{newSprite.name}";
    }
}