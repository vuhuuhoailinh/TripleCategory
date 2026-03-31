using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cài đặt Luật chơi")]
    public int maxMoves = 30;
    private int currentMoves;
    private int totalItemsOnBoard; 
    private bool isGameOver = false;

    [Header("Giao diện")]
    public TextMeshProUGUI movesText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentMoves = maxMoves;
        UpdateUIText();
    }

    public void RegisterTotalItems(int amount)
    {
        totalItemsOnBoard = amount;
    }

    public void UseMove()
    {
        if (isGameOver) return;
        
        currentMoves--;
        UpdateUIText();

        if (currentMoves <= 0 && totalItemsOnBoard > 0)
        {
            Debug.Log("<color=red>HẾT LƯỢT! BÁC ĐÃ THUA GAME!</color>");
            isGameOver = true;
        }
    }

    public void ItemsMatched()
    {
        if (isGameOver) return;
        
        totalItemsOnBoard -= 3;
        
        if (totalItemsOnBoard <= 0)
        {
            Debug.Log("<color=green>DỌN SẠCH BÀN! BÁC ĐÃ CHIẾN THẮNG!</color>");
            isGameOver = true;
        }
    }

    void UpdateUIText()
    {
        if (movesText != null)
        {
            movesText.text = "Moves: " + currentMoves.ToString();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnMoveUsed += UseMove;
        GameEvents.OnItemsMatched += ItemsMatched;
        GameEvents.OnLevelGenerated += RegisterTotalItems;
    }

    // Tắt đài phát thanh khi object bị hủy (BẮT BUỘC để tránh rò rỉ bộ nhớ - Memory Leak)
    private void OnDisable()
    {
        GameEvents.OnMoveUsed -= UseMove;
        GameEvents.OnItemsMatched -= ItemsMatched;
        GameEvents.OnLevelGenerated -= RegisterTotalItems;
    }
}