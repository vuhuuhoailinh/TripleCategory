using UnityEngine;
using DG.Tweening;
using TMPro;

public class AdsLock : MonoBehaviour
{
    private Collider2D targetShelfCollider;

    [Header("Fade Target")]
    [SerializeField] private GameObject fadeTarget;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float delayBeforeDestroy = 0.1f;

    private SpriteRenderer[] spriteRenderers;
    private TextMeshPro[] textMeshes;

    void Awake()
    {
        if (fadeTarget != null)
        {
            spriteRenderers = fadeTarget.GetComponentsInChildren<SpriteRenderer>();
            textMeshes = fadeTarget.GetComponentsInChildren<TextMeshPro>();
        }
    }

    public void BlockShelf(Collider2D shelfCol)
    {
        targetShelfCollider = shelfCol;

        if (targetShelfCollider != null)
        {
            targetShelfCollider.enabled = false;
        }
    }

    public void Unlock()
    {
        if (targetShelfCollider != null)
        {
            targetShelfCollider.enabled = true;
        }

        // Fade Sprite
        foreach (var sr in spriteRenderers)
        {
            sr.DOFade(0f, fadeDuration);
        }

        // Fade TextMeshPro (3D)
        foreach (var tmp in textMeshes)
        {
            tmp.DOFade(0f, fadeDuration);
        }

        // Delay destroy
        DOVirtual.DelayedCall(fadeDuration + delayBeforeDestroy, () =>
        {
            Destroy(gameObject);
        });
    }
}