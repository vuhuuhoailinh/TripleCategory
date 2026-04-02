using System;

public static class GameEvents
{
    // Sự kiện khi người chơi tốn 1 lượt di chuyển
    public static Action OnMoveUsed;

    // Sự kiện khi có 3 vật phẩm được Match và phá hủy
    public static Action<ShelfController> OnItemsMatched;

    // Sự kiện khi LevelManager đã sinh xong đồ, truyền kèm [tổng số item]
    public static Action<int> OnLevelGenerated;
    public static Action OnCardPicked;
    public static Action OnCardDropped;

    public static Action OnMatchDetected;
    public static Action OnLevelWin;
    public static Action OnUIClick;

    public static Action<int> OnLevelChanged;  
    public static Action<int, int> OnProgressUpdated;
}