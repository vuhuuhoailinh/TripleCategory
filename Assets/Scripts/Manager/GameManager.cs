using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelConfig
{
    public int targetCategories = 10;
    public int maxMoves = 30;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level Settings")]
    public LevelConfig[] levelConfigs =
    {
        new LevelConfig { targetCategories = 10, maxMoves = 30 },
        new LevelConfig { targetCategories = 15, maxMoves = 30 },
        new LevelConfig { targetCategories = 20, maxMoves = 30 },
        new LevelConfig { targetCategories = 25, maxMoves = 30 },
        new LevelConfig { targetCategories = 30, maxMoves = 30 }
    };

    public int currentLevel { get; private set; }
    public int targetCategories { get; private set; }
    public int clearedCategories { get; private set; }

    private int currentMoves;
    private int movesPerLevel;
    public bool isGameOver { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.targetFrameRate = 60;
            currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
            ApplyLevelConfig();
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        SetupLevel();
    }

    void SetupLevel()
    {
        isGameOver = false;
        currentMoves = movesPerLevel;
        clearedCategories = 0;

        // PHÁT THANH TRẠNG THÁI BAN ĐẦU LÊN UI
        GameEvents.OnLevelChanged?.Invoke(currentLevel);
        GameEvents.OnMovesUpdated?.Invoke(currentMoves);
        GameEvents.OnProgressUpdated?.Invoke(clearedCategories, targetCategories);
    }

    private void ApplyLevelConfig()
    {
        LevelConfig config = GetLevelConfigForCurrentLevel();
        targetCategories = Mathf.Max(1, config.targetCategories);
        movesPerLevel = Mathf.Max(1, config.maxMoves);
    }

    private LevelConfig GetLevelConfigForCurrentLevel()
    {
        if (levelConfigs == null || levelConfigs.Length == 0) return new LevelConfig();
        int index = Mathf.Clamp(currentLevel - 1, 0, levelConfigs.Length - 1);
        return levelConfigs[index] ?? new LevelConfig();
    }

    public void UseMove()
    {
        if (isGameOver) return;

        currentMoves--;
        GameEvents.OnMovesUpdated?.Invoke(currentMoves); // BÁO LÊN UI

        if (currentMoves <= 0 && clearedCategories < targetCategories)
        {
            Debug.Log("<color=red>OUT OF MOVES! YOU LOSE!</color>");
            isGameOver = true;
            GameEvents.OnLevelLose?.Invoke();
        }
    }

    public void ItemsMatched(ShelfController shelf)
    {
        if (clearedCategories >= targetCategories) return;

        clearedCategories++;
        GameEvents.OnProgressUpdated?.Invoke(clearedCategories, targetCategories); // BÁO LÊN UI

        if (clearedCategories >= targetCategories)
        {
            Debug.Log("<color=green>YOU WIN!</color>");
            isGameOver = true;
            GameEvents.OnLevelWin?.Invoke();
        }
    }

    public void NextLevel()
    {
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnEnable()
    {
        GameEvents.OnMoveUsed += UseMove;
        GameEvents.OnItemsMatched += ItemsMatched;
    }

    private void OnDisable()
    {
        GameEvents.OnMoveUsed -= UseMove;
        GameEvents.OnItemsMatched -= ItemsMatched;
    }
}