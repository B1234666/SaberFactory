﻿using System;
using System.Collections.Generic;
using System.Threading;
using BeatSaberMarkupLanguage.Attributes;
using SaberFactory.UI.CustomSaber.CustomComponents;
using SaberFactory.UI.Lib;
using TMPro;
using Zenject;


namespace SaberFactory.UI.CustomSaber.Views
{
    internal class NavigationView : CustomViewController
    {
        [Inject] private readonly PluginManager _pluginManager = null;

        public event Action<ENavigationCategory> OnCategoryChanged;
        public event Action OnExit;
        
        public ENavigationCategory CurrentCategory { get; private set; }

        private NavButton _currentSelectedNavButton;

        [UIComponent("settings-notify-text")] private readonly TextMeshProUGUI _settingsNotifyText = null;
        [UIValue("nav-buttons")] private List<object> _navButtons;

        [UIAction("#post-parse")]
        private async void Setup()
        {
            if (_navButtons is not null && _navButtons.Count > 0)
            {
                _currentSelectedNavButton = ((NavButtonWrapper) _navButtons[0]).NavButton;
                _currentSelectedNavButton.SetState(true, false);
            }

            var release = await _pluginManager.GetNewestReleaseAsync(CancellationToken.None);
            if (release!=null && !release.IsLocalNewest)
            {
                _settingsNotifyText.gameObject.SetActive(true);
            }
        }

        private void Awake()
        {
            _navButtons = new List<object>();

            var saberButton = new NavButtonWrapper(
                ENavigationCategory.Saber,
                "SaberFactory.Resources.Icons.customsaber-icon.png",
                ClickedCategory,
                "Select a saber");

            var trailButton = new NavButtonWrapper(
                ENavigationCategory.Trail,
                "SaberFactory.Resources.Icons.trail-icon.png",
                ClickedCategory,
                "Edit the trail");

            _navButtons.Add(saberButton);
            _navButtons.Add(trailButton);
        }

        private void ClickedCategory(NavButton button, ENavigationCategory category)
        {
            if(_currentSelectedNavButton==button) return;
            _currentSelectedNavButton?.Deselect();
            _currentSelectedNavButton = button;
            CurrentCategory = category;
            OnCategoryChanged?.Invoke(category);
        }

        [UIAction("clicked-settings")]
        private void ClickSettings(NavButton button, string _)
        {
            ClickedCategory(button, ENavigationCategory.Settings);
        }

        [UIAction("clicked-exit")]
        private void ClickExit()
        {
            OnExit?.Invoke();
        }

        public override IAnimatableUi.EAnimationType AnimationType => IAnimatableUi.EAnimationType.Vertical;
    }
}
