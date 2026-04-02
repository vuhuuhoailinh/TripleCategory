using UnityEngine;

public class ItemController : MonoBehaviour
{
    [Header("Thong tin du lieu")]
    public int categoryID;
    public string categoryName;

    [Header("Thanh phan giao dien")]
    [Tooltip("Keo SpriteRenderer cua object con 'Item_Icon' vao day")]
    public SpriteRenderer iconRenderer;

    public bool isFaceUp = true;

    public void SetupItem(int catID, string catName, Sprite newSprite)
    {
        categoryID = catID;
        categoryName = catName;

        if (iconRenderer != null)
        {
            iconRenderer.sprite = newSprite;
        }

        gameObject.name = $"Item_{catName}_{newSprite.name}";
    }
}
