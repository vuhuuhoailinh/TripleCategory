using UnityEngine;

public class ItemController : MonoBehaviour
{
    public int categoryID; // ID để đối chiếu xem 3 viên có giống nhau không
    public SpriteRenderer spriteRenderer;

    // Hàm này sẽ được LevelManager gọi ngay khi sinh ra viên Emoji
    public void SetupItem(int id, Sprite newSprite)
    {
        this.categoryID = catID;
        
        // Tự động gán tham chiếu nếu bác quên kéo trong Inspector
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        // "Thay áo" cho viên Emoji
        spriteRenderer.sprite = newSprite;
        
        // Đổi luôn tên object trong Hierarchy cho dễ soi lỗi
        gameObject.name = $"Item_Cat{catID}_{newSprite.name}";
}