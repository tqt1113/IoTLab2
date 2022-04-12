using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderToggle : MonoBehaviour
{
    public UnityEvent<bool> onValueChanged = new UnityEvent<bool>();
    [SerializeField] private Toggle toggle;
    [SerializeField] private Slider slider;
    public void SetEnable(Toggle change)
    {
        GetComponent<Slider>().value = change.isOn?1:0;
        
    }
    public bool isOn {
        get {return toggle.isOn;}
        set {
            SetToggleIsOn(value);
        }
    }
    public void SetToggleIsOnDontNotify(bool value)
    {
        slider.value = value?1:0;
        toggle.SetIsOnWithoutNotify(value);
    }

    private void Awake() {
        SetToggleIsOnDontNotify(toggle.isOn);
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool enable)
    {
        SetToggleIsOn(enable);
    }

    private void SetToggleIsOn(bool value)
    {
        SetToggleIsOnDontNotify(value);
        onValueChanged.Invoke(value);
    }
}
