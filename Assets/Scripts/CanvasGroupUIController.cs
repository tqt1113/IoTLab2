using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CanvasGroupUIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform containerRTF;
    private Coroutine alphaCR;
    public void Show() {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        if(alphaCR != null)
            StopCoroutine(alphaCR);
        alphaCR = StartCoroutine(LerpAlpha(1, 0.5f));
    }
    public void Hide() {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        if(alphaCR != null)
            StopCoroutine(alphaCR);
        alphaCR = StartCoroutine(LerpAlpha(0, 0.5f));
    }
    private IEnumerator LerpAlpha(float alpha, float duration)
    {
        var start = canvasGroup.alpha;
        var t = 0.0f;
        while (t < duration)
        {
            yield return null;
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, alpha, t/duration);
        }
        canvasGroup.alpha = alpha;
    }
}
