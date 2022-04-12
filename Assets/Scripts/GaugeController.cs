using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GaugeController : MonoBehaviour
{
    [SerializeField] private Image progressImage;
    [SerializeField] private TMPro.TMP_Text text;
    public void SetValue(float progress, string textVal)
    {
        text.text = textVal;
        progressImage.fillAmount = progress;
    }
}
