﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SaberFactory.DataStore;
using SaberFactory.Helpers;
using SaberFactory.Models;
using SaberFactory.Models.CustomSaber;
using UnityEngine;

namespace SaberFactory.Saving
{
    internal class PresetSaveManager
    {
        public event Action OnSaberLoaded;

        private readonly MainAssetStore _mainAssetStore;
        private readonly TextureStore _textureStore;
        private readonly DirectoryInfo _presetDir;

        private PresetSaveManager(MainAssetStore mainAssetStore, TextureStore textureStore, DirectoryInfo presetDir)
        {
            _mainAssetStore = mainAssetStore;
            _textureStore = textureStore;
            _presetDir = presetDir;
        }

        public void SaveSaber(SaberSet saberSet, string fileName)
        {
            var serializableSaberSet = new SerializableSaberSet();
            serializableSaberSet.SaberLeft = GetSerializableSaber(saberSet.LeftSaber);
            serializableSaberSet.SaberRight = GetSerializableSaber(saberSet.RightSaber);
            var file = _presetDir.GetFile(fileName);
            File.WriteAllText(file.FullName, JsonConvert.SerializeObject(serializableSaberSet, Formatting.Indented));
        }

        private SerializableSaber GetSerializableSaber(SaberModel saberModel)
        {
            var serializableSaber = new SerializableSaber();

            serializableSaber.SaberWidth = saberModel.SaberWidth;

            var pieceList = new List<SerializablePiece>();
            foreach (BasePieceModel pieceModel in saberModel.PieceCollection)
            {
                pieceList.Add(new SerializablePiece{Path = pieceModel.StoreAsset.RelativePath});
            }

            serializableSaber.Pieces = pieceList;

            var trailModel = saberModel.GetTrailModel();
            if (trailModel != null)
            {
                var trail = new SerializableTrail();
                trail.Length = trailModel.Length;
                trail.Width = trailModel.Width;
                trail.Whitestep = trailModel.Whitestep;
                trail.TrailOrigin = trailModel.TrailOrigin;
                trail.ClampTexture = trailModel.ClampTexture;
                trail.Material = SerializableMaterial.FromMaterial(trailModel.Material.Material);
                serializableSaber.Trail = trail;
            }

            return serializableSaber;
        }

        public async Task LoadSaber(SaberSet saberSet, string fileName)
        {
            var file = _presetDir.GetFile(fileName);
            if (!file.Exists) return;
            var data = await Readers.BytesToString(await Readers.ReadFileAsync(file.FullName));
            var serializableSaberSet = JsonConvert.DeserializeObject<SerializableSaberSet>(data);
            await LoadSaberModel(saberSet.LeftSaber, serializableSaberSet.SaberLeft);
            await LoadSaberModel(saberSet.RightSaber, serializableSaberSet.SaberRight);

            OnSaberLoaded?.Invoke();
        }

        private async Task LoadSaberModel(SaberModel saberModel, SerializableSaber serializableSaber)
        {
            saberModel.SaberWidth = serializableSaber.SaberWidth;

            if (_mainAssetStore.IsLoading) await _mainAssetStore.CurrentTask;

            foreach (var piece in serializableSaber.Pieces)
            {
                var comp = await _mainAssetStore[piece.Path];
                saberModel.PieceCollection.AddPiece(comp.AssetTypeDefinition, comp.GetPiece(saberModel.SaberSlot));
            }

            TrailModel trailModel = null;

            if (saberModel.GetCustomSaber(out var customsaber))
            {
                trailModel = customsaber.TrailModel;
            }
            else
            {
                trailModel = new TrailModel();
            }

            var trail = serializableSaber.Trail;
            if (trail != null)
            {
                trailModel.TrailPosOffset = Vector3.zero;
                trailModel.Width = trail.Width;
                trailModel.Length = trail.Length;
                trailModel.Whitestep = trail.Whitestep;
                trailModel.ClampTexture = trail.ClampTexture;

                // if trail comes from another saber
                if (!string.IsNullOrEmpty(trail.TrailOrigin))
                {
                    await LoadFromTrailOrigin(trailModel, trail.TrailOrigin);
                }

                // assign trailmodel to custom saber or saber factory saber
                // depending on which trail type is being used
                if (customsaber is null)
                {
                    saberModel.TrailModel = trailModel;
                }

                trail.Material?.ApplyToMaterial(trailModel.Material.Material, ResolveTexture);

            }
        }

        private async Task<Texture2D> ResolveTexture(string name)
        {
            if (_textureStore.IsLoading) await _textureStore.CurrentLoadingTask;
            return (await _textureStore[name])?.Texture;
        }

        /// <summary>
        /// Load trail from another saber
        /// </summary>
        /// <param name="trailModel">TrailModel to load the other saber's data into</param>
        /// <param name="trailOrigin">Path of the other saber</param>
        /// <returns></returns>
        private async Task LoadFromTrailOrigin(TrailModel trailModel, string trailOrigin)
        {
            var comp = await _mainAssetStore[trailOrigin];
            var originTrailModel = (comp?.GetLeft() as CustomSaberModel)?.GrabTrail(true);
            if (originTrailModel == null) return;
            trailModel.Material.Material = originTrailModel.Material.Material;
        }
    }
}
