﻿using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using ModestTree;
using SaberFactory.Editor;
using SaberFactory.Helpers;
using SaberFactory.UI.Lib;
using SiraUtil;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using Zenject;

namespace SaberFactory.UI.CustomSaber.Views
{
    internal class MainModifierPanelView : SubView, INavigationCategoryView
    {
        [UIObject("container")] private readonly GameObject _container = null;
        [UIComponent("component-list")] private readonly CustomListTableData _componentList = null;
        [Inject] private readonly BsmlDecorator _decorator = null;
        [Inject] private readonly EditorInstanceManager _instanceManager = null;
        [Inject] private readonly DiContainer _diContainer = null;

        private ModifyableComponentManager _modifyableComponentManager;
        private List<BaseComponentModifier> _mods;
        private BaseComponentModifier _selectedMod;

        public ENavigationCategory Category => ENavigationCategory.Modifier;

        protected override void Init()
        {
            Assert.That(ParserParams!=null, "ParserParams!=null");
            foreach (var obj in ParserParams.GetObjectsWithTag("canvas"))
            {
                obj.GetOrAdd<VerticalLayoutGroup>();

                var contentSizeFitter = obj.GetOrAdd<ContentSizeFitter>();
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                obj.GetOrAdd<LayoutElement>();
                
                obj.AddComponent<Canvas>();
                var canvasScaler = obj.AddComponent<CanvasScaler>();
                canvasScaler.referencePixelsPerUnit = 10;
                canvasScaler.scaleFactor = 3.44f;

                obj.AddComponent<CurvedCanvasSettings>();
                _diContainer.InstantiateComponent<VRGraphicRaycaster>(obj);
            }
        }

        public override void DidOpen()
        {
            _modifyableComponentManager = _instanceManager.CurrentPiece?.Model.ModifyableComponentManager;
            if (_modifyableComponentManager is { })
            {
                SetupMod();
            }
        }

        public override void DidClose()
        {
            _modifyableComponentManager = null;
            _mods = null;
        }

        public void SetupMod()
        {
            var list = new List<CustomListTableData.CustomCellInfo>();
            _mods = _modifyableComponentManager.GetAllModifiers().ToList();

            foreach (var mod in _mods)
            {
                list.Add(new CustomListTableData.CustomCellInfo(mod.GoName, mod.TypeName));
            }

            _componentList.data = list;
            _componentList.tableView.ReloadData();

            if (_mods.Count > 0)
            {
                _componentList.tableView.SelectCellWithIdx(0, true);
            }
        }

        private void ClearCurrentView()
        {
            for (var i = 0; i < _container.transform.childCount; i++)
            {
                Destroy(_container.transform.GetChild(0).gameObject);
            }
        }

        [UIAction("component-selected")]
        private void ComponentSelected(TableView table, int idx)
        {
            _selectedMod = _mods[idx];
            ClearCurrentView();
            _decorator.ParseFromString(_selectedMod.DrawUi(), _container, _selectedMod);
        }

        [UIAction("reset-click")]
        private void ResetClick()
        {
            if (_modifyableComponentManager is null)
            {
                return;
            }

            Debug.LogWarning($"Resetting {_selectedMod.Index}");

            _modifyableComponentManager.Reset(_selectedMod.Index);

            ReloadSaber();
        }

        [UIAction("reset-all-click")]
        private void ResetAllClick()
        {
            if (_modifyableComponentManager is null)
            {
                return;
            }

            Debug.LogWarning("Resetting all");

            _modifyableComponentManager.ResetAll();

            ReloadSaber();
        }

        private void ReloadSaber()
        {
            _instanceManager.Refresh();
            DidClose();
            ClearCurrentView();
            DidOpen();
        }
    }
}