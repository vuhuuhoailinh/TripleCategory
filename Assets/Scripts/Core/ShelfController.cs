using UnityEngine;

public class ShelfController : MonoBehaviour
{
    [Header("Gán tham chiếu 3 Thẻ")]
    [Tooltip("Kéo Card1, Card2, Card3 từ Hierarchy vào 3 ô này")]
    // Đặt tham chiếu dạng public để dễ gán trong Inspector
    public GameObject card1;
    public GameObject card2;
    public GameObject card3;

    // Mảng này sẽ được dùng để lưu trữ 3 thẻ sau khi đã gán tham chiếu
    private GameObject[] slots;

    void Start()
    {
        // Khởi tạo mảng slots và gán 3 thẻ vào đó
        slots = new GameObject[3] { card1, card2, card3 };

        // (Tùy chọn) Bác có thể tự động gán tham chiếu bằng code 
        // nếu không muốn kéo thả thủ công
        // card1 = transform.Find("Card1")?.gameObject;
        // card2 = transform.Find("Card2")?.gameObject;
        // card3 = transform.Find("Card3")?.gameObject;
        // slots = new GameObject[3] { card1, card2, card3 };
    }

    // Hàm này sẽ dùng để nhét viên Emoji vào kệ khi sinh màn chơi hoặc khi người chơi Swap
    // (Lát nữa làm ItemController ta sẽ đổi tham chiếu sang ItemController sau)
    public void AssignItemToSlot(int slotIndex, GameObject item)
    {
        if (slotIndex < 0 || slotIndex >= 3) return;
        
        slots[slotIndex] = item;
        
        // Gắn item làm object con của kệ để Hierarchy gọn gàng
        item.transform.SetParent(this.transform); 
    }

    // Hàm này sẽ được gọi mỗi khi người chơi tráo đổi (Swap) xong 1 viên Emoji vào kệ này
    public void CheckForMatch()
    {
        // 1. Nếu có bất kỳ lỗ hổng nào (chưa xếp đủ 3 viên) -> Nghỉ, không check
        if (slots[0] == null || slots[1] == null || slots[2] == null) return;

        // Lát nữa chúng ta sẽ viết logic so sánh ID của 3 viên ở đây
        // Nếu ID giống nhau -> Bùm! Gọi hiệu ứng DOTween nổ tung.
        Debug.Log($"Kệ {gameObject.name} đang check Match 3...");
    }
}