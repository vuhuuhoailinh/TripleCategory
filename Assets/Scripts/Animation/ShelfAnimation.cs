using UnityEngine;
using DG.Tweening;
using System;
using TMPro;

public class ShelfAnimation : MonoBehaviour
{
    [Header("Cau hinh Hover (Darken)")]
    public float hoverFadeDuration = 0.15f;
    [Range(0f, 1f)] public float maxDarkenAlpha = 0.4f;

    [Header("Cau hinh Drop (Glow)")]
    public float glowDuration = 0.4f;
    public Color glowColor = Color.white;

    [Header("Giao dien Match-3 Text")]
    public TextMeshPro categoryText;

    private SpriteRenderer[] darkenSprites = new SpriteRenderer[3];
    private SpriteRenderer[] highlightSprites = new SpriteRenderer[3];
    private Vector3 originalTextScale = Vector3.one;

    void Awake()
    {
        if (categoryText != null)
        {
            originalTextScale = categoryText.transform.localScale;
            categoryText.gameObject.SetActive(false);
        }
    }

    public void Initialize(Transform[] anchors)
    {
        for (int i = 0; i < 3; i++)
        {
            if (anchors[i] != null)
            {
                Transform darkenT = anchors[i].Find("Darken");
                if (darkenT != null)
                {
                    darkenSprites[i] = darkenT.GetComponent<SpriteRenderer>();
                    Color c = darkenSprites[i].color;
                    c.a = 0;
                    darkenSprites[i].color = c;
                    darkenSprites[i].gameObject.SetActive(false);
                }

                Transform highlightT = anchors[i].Find("Highlight");
                if (highlightT != null)
                {
                    highlightSprites[i] = highlightT.GetComponent<SpriteRenderer>();
                    highlightSprites[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowHover(int index)
    {
        ClearHover();
        if (index >= 0 && index < 3 && darkenSprites[index] != null)
        {
            darkenSprites[index].gameObject.SetActive(true);
            darkenSprites[index].DOKill();
            darkenSprites[index].DOFade(maxDarkenAlpha, hoverFadeDuration);
        }
    }

    public void ClearHover()
    {
        for (int i = 0; i < 3; i++)
        {
            SpriteRenderer darkenSprite = darkenSprites[i];
            if (darkenSprite != null && darkenSprite.gameObject.activeSelf)
            {
                darkenSprite.DOKill();
                darkenSprite.DOFade(0f, hoverFadeDuration).OnComplete(() =>
                {
                    darkenSprite.gameObject.SetActive(false);
                });
            }
        }
    }

    public void PlayDropGlow(int index)
    {
        if (index >= 0 && index < 3 && highlightSprites[index] != null)
        {
            highlightSprites[index].DOKill();
            highlightSprites[index].gameObject.SetActive(true);

            Color startColor = glowColor;
            startColor.a = 1f;
            highlightSprites[index].color = startColor;

            highlightSprites[index].DOFade(0f, glowDuration).OnComplete(() =>
            {
                highlightSprites[index].gameObject.SetActive(false);
            });
        }
    }

    public void PlayMatchAnimation(GameObject obj0, GameObject obj1, GameObject obj2, string matchName, Action onComplete)
    {
        float flipDelay = 0.15f;
        float timeToFinishAllFlips = (flipDelay * 2) + 0.4f;
        float holdDuration = 1.5f;

        Sequence seq = DOTween.Sequence();

        if (obj0 != null)
        {
            ItemAnimation anim0 = obj0.GetComponent<ItemAnimation>();
            if (anim0 != null) seq.InsertCallback(0f, () => anim0.PlayFlipToBack());
        }

        if (obj1 != null)
        {
            ItemAnimation anim1 = obj1.GetComponent<ItemAnimation>();
            if (anim1 != null) seq.InsertCallback(flipDelay, () => anim1.PlayFlipToBack());
        }

        if (obj2 != null)
        {
            ItemAnimation anim2 = obj2.GetComponent<ItemAnimation>();
            if (anim2 != null) seq.InsertCallback(flipDelay * 2, () => anim2.PlayFlipToBack());
        }

        if (categoryText != null)
        {
            seq.InsertCallback(timeToFinishAllFlips, () =>
            {
                categoryText.text = matchName.ToUpper();
                categoryText.gameObject.SetActive(true);
                categoryText.transform.localScale = Vector3.zero;
                categoryText.transform.DOScale(originalTextScale, 0.3f).SetEase(Ease.OutBack);
            });
        }

        float destroyTime = timeToFinishAllFlips + holdDuration;
        seq.InsertCallback(destroyTime, () =>
        {
            if (categoryText != null)
            {
                categoryText.gameObject.SetActive(false);
            }

            if (obj0 != null) Destroy(obj0);
            if (obj1 != null) Destroy(obj1);
            if (obj2 != null) Destroy(obj2);

            onComplete?.Invoke();
        });
    }
}
