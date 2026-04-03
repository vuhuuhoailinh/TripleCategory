using UnityEngine;
using TMPro;

public class MenuBarUI : MonoBehaviour
{
    [Header("UI Text Components")]
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI progressText;

    [Header("Prefabs")]
    public GameObject settingPanelPrefab;

    private void OnEnable()
    {
        GameEvents.OnLevelChanged += UpdateLevelText;
        GameEvents.OnMovesUpdated += UpdateMovesText;
        GameEvents.OnProgressUpdated += UpdateProgressText;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelChanged -= UpdateLevelText;
        GameEvents.OnMovesUpdated -= UpdateMovesText;
        GameEvents.OnProgressUpdated -= UpdateProgressText;
    }

    // --- CÁC HÀM CẬP NHẬT GIAO DIỆN ---
    private void UpdateLevelText(int level)
    {
        if (levelText != null) levelText.text = "Level: " + level;
    }

    private void UpdateMovesText(int moves)
    {
        if (movesText != null) movesText.text = "Moves: " + moves;
    }

    private void UpdateProgressText(int current, int target)
    {
        if (progressText != null) progressText.text = current + "/" + target;
    }

    // --- HÀM GẮN CHO NÚT BÁNH RĂNG ---
    public void OpenSettings()
    {
        GameEvents.OnUIClick?.Invoke();
        
        if (settingPanelPrefab != null)
        {
            // Bỏ hết vụ tìm parent đi. 
            // Vì Prefab ĐÃ LÀ CANVAS, cứ đẻ nó ra thẳng ngoài môi trường là nó tự phình to chuẩn xác!
            Instantiate(settingPanelPrefab);
        }
    }
}