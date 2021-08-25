﻿using System;
using SaberFactory.Helpers;
using UnityEngine;

namespace SaberFactory.Instances
{
    /// <summary>
    /// Extension class for <see cref="Material"/> with more information
    /// and possibly to revert a changed <see cref="Material"/>
    /// </summary>
    internal class MaterialDescriptor
    {
        public Material Material;

        public bool IsValid => Material != null;

        private readonly Material _originalMaterial;

        public MaterialDescriptor(Material material)
        {
            if (material == null) return;

            Material = material;
            _originalMaterial = new Material(material);
        }

        public virtual void Revert()
        {
            if (_originalMaterial is null) return;
            DestroyUsedMaterial();
            Material = new Material(_originalMaterial);
        }

        public void DestroyUsedMaterial()
        {
            Material.TryDestoryImmediate();
        }

        public void DestroyBackupMaterial()
        {
            _originalMaterial.TryDestoryImmediate();
        }

        public void DestroyMaterials()
        {
            DestroyUsedMaterial();
            DestroyBackupMaterial();
        }
    }
}