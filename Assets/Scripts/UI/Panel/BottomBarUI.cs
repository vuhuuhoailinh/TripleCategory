using UnityEngine;
using DG.Tweening;

public class BottomBarUI : MonoBehaviour
{
    // Đã đổi tham số từ Transform sang GameObject cho an toàn tuyệt đối
    public void OnClickShuffleButton(GameObject buttonObj)
    {
        if (buttonObj != null)
        {
            // Gọi .transform từ GameObject
            buttonObj.transform.DOKill();
            buttonObj.transform.DOPunchScale(buttonObj.transform.localScale * -0.1f, 0.2f);
        }

        // Bắn tín hiệu gọi BoosterManager làm việc
        GameEvents.OnShuffleRequested?.Invoke();
    }
}