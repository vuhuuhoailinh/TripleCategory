using System;

public static class GameEvents
{
    // Sự kiện khi người chơi tốn 1 lượt di chuyển
    public static Action OnMoveUsed;

    // Sự kiện khi có 3 vật phẩm được Match và phá hủy
    public static Action OnItemsMatched;

    // Sự kiện khi LevelManager đã sinh xong đồ, truyền kèm [tổng số item]
    public static Action<int> OnLevelGenerated; 
}