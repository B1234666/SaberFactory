﻿using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaberFactory.UI.Lib.PropCells
{
    internal class FloatPropCell : BasePropCell
    {
        [UIComponent("bg")] private readonly Image _backgroundImage = null;
        [UIComponent("val-slider")] private readonly SliderSetting _sliderSetting = null;
        [UIComponent("val-slider")] private readonly TextMeshProUGUI _sliderSettingText = null;

        public override void SetData(PropertyDescriptor data)
        {
            if (!(data.PropObject is float val)) return;

            OnChangeCallback = data.ChangedCallback;
            _sliderSetting.slider.value = val;
            _sliderSetting.ReceiveValue();
            _sliderSettingText.text = data.Text;

            _backgroundImage.type = Image.Type.Sliced;
            _backgroundImage.color = new Color(1, 0, 0, 0.5f);
        }

        [UIAction("slider-changed")]
        private void SliderChanged(float val)
        {
            OnChangeCallback?.Invoke(val);
        }
    }
}