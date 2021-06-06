﻿using System;
using System.Collections.Generic;
using CustomSaber;
using SaberFactory.Configuration;
using SaberFactory.Helpers;
using SaberFactory.Instances.CustomSaber;
using SaberFactory.Instances.Trail;
using SaberFactory.Models;
using SiraUtil.Tools;
using UnityEngine;
using Zenject;

namespace SaberFactory.Instances
{
    /// <summary>
    /// Class for managing an instance of a saber <seealso cref="SaberModel"/>
    /// </summary>
    public class SaberInstance
    {
        internal event Action OnDestroyed;

        internal ITrailHandler TrailHandler { get; private set; }

        internal readonly SaberModel Model;
        public readonly GameObject GameObject;
        public readonly Transform CachedTransform;

        internal readonly PieceCollection<BasePieceInstance> PieceCollection;

        internal List<PartEvents> Events { get; private set; }

        private readonly SiraLog _logger;
        private readonly TrailConfig _trailConfig;

        private InstanceTrailData _instanceTrailData;
        private List<CustomSaberTrailHandler> _secondaryTrails;

        private SaberInstance(SaberModel model, BasePieceInstance.Factory pieceFactory, SiraLog logger, TrailConfig trailConfig)
        {
            _logger = logger;
            _trailConfig = trailConfig;

            Model = model;

            GameObject = new GameObject("SF Saber");
            GameObject.AddComponent<SaberMonoBehaviour>().Init(OnSaberGameObjectDestroyed);

            CachedTransform = GameObject.transform;

            PieceCollection = new PieceCollection<BasePieceInstance>();

            var sectionInstantiator = new SectionInstantiator(this, pieceFactory, PieceCollection);
            sectionInstantiator.InstantiateSections();

            GameObject.transform.localScale = new Vector3(model.SaberWidth, model.SaberWidth, 1);

            GameObject.SetLayer<Renderer>(12);

            SetupTrailData();
            InitializeEvents();
        }

        public void SetParent(Transform parent)
        {
            CachedTransform.SetParent(parent, false);
        }

        public void SetColor(Color color)
        {
            foreach (BasePieceInstance piece in PieceCollection)
            {
                piece.SetColor(color);
            }

            TrailHandler?.SetColor(color);

            if (_secondaryTrails is { })
            {
                foreach (var trail in _secondaryTrails)
                {
                    trail.SetColor(color);
                }
            }
        }

        private void InitializeEvents()
        {
            Events = new List<PartEvents>();
            foreach (BasePieceInstance piece in PieceCollection)
            {
                var events = piece.GetEvents();
                if (events != null)
                {
                    Events.Add(events);
                }
            }
        }

        private void SetupTrailData()
        {
            if (GetCustomSaber(out var customsaber)) return;

            // TODO: Setup sf trail data
            _instanceTrailData = default;
        }

        public void CreateTrail(bool editor, SaberTrail backupTrail = null)
        {
            var trailData = GetTrailData(out var secondaryTrails);

            if (trailData is null)
            {
                if (backupTrail is {})
                {
                    TrailHandler = new TrailHandlerEx(GameObject, backupTrail);
                    TrailHandler.CreateTrail(_trailConfig, editor);
                }

                return;
            }

            TrailHandler = new TrailHandlerEx(GameObject);
            TrailHandler.SetTrailData(trailData);
            TrailHandler.CreateTrail(_trailConfig, editor);

            if (secondaryTrails is { })
            {
                _secondaryTrails = new List<CustomSaberTrailHandler>();
                foreach (var customTrail in secondaryTrails)
                {
                    var handler = new CustomSaberTrailHandler(GameObject, customTrail);
                    handler.CreateTrail(editor);
                    _secondaryTrails.Add(handler);
                }
            }
        }

        public void DestroyTrail()
        {
            TrailHandler?.DestroyTrail();
            if (_secondaryTrails is { })
            {
                foreach (var trail in _secondaryTrails)
                {
                    trail.DestroyTrail();
                }
            }
        }

        public void Destroy()
        {
            GameObject.TryDestroy();
            OnDestroyed?.Invoke();
        }

        private void OnSaberGameObjectDestroyed()
        {
            DestroyTrail();
        }

        private bool GetCustomSaber(out CustomSaberInstance customSaberInstance)
        {
            if (PieceCollection.TryGetPiece(AssetTypeDefinition.CustomSaber, out var instance))
            {
                customSaberInstance = instance as CustomSaberInstance;
                return true;
            }

            customSaberInstance = null;
            return false;
        }

        internal InstanceTrailData GetTrailData(out List<CustomTrail> secondaryTrails)
        {
            secondaryTrails = null;
            if (GetCustomSaber(out var customsaber))
            {
                secondaryTrails = customsaber.SecondaryTrails;
                return customsaber.InstanceTrailData;
            }

            return _instanceTrailData;
        }

        public void SetSaberWidth(float width)
        {
            Model.SaberWidth = width;
            GameObject.transform.localScale = new Vector3(width, width, 1);
        }

        internal class Factory : PlaceholderFactory<SaberModel, SaberInstance> {}

        internal class SaberMonoBehaviour : MonoBehaviour
        {
            private Action _onDestroyed;

            public void Init(Action onDestroyedCallback)
            {
                _onDestroyed = onDestroyedCallback;
            }

            private void OnDestroy()
            {
                _onDestroyed?.Invoke();
            }
        }
    }
}