﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaberFactory.Saving
{
    internal class SerializableMaterial
    {
        public string ShaderName;
        public List<SerializableMaterialProperty> Properties;

        public void ApplyToMaterial(Material material)
        {
            var shader = material.shader;
            if (ShaderName != shader.name) return;

            foreach (var prop in Properties)
            {
                SetProp(material, prop);
            }
        }

        public void SetProp(Material material, SerializableMaterialProperty prop)
        {
            Console.WriteLine(prop.Value.GetType().Name);
            switch (prop.Type)
            {
                case ShaderPropertyType.Float:
                    material.SetFloat(prop.Name, (float)(double) prop.Value);
                    break;
                case ShaderPropertyType.Range:
                    material.SetFloat(prop.Name, (float)(double) prop.Value);
                    break;
                case ShaderPropertyType.Color:
                    material.SetColor(prop.Name, Cast<SerializableColor>(prop.Value).ToColor());
                    break;
                case ShaderPropertyType.Vector:
                    material.SetVector(prop.Name, Cast<SerializableVector4>(prop.Value).ToVector());
                    break;
            }
        }

        public T Cast<T>(object obj)
        {
            return ((JObject) obj).ToObject<T>();
        }

        public static SerializableMaterial FromMaterial(Material material)
        {
            var props = new List<SerializableMaterialProperty>();

            var shader = material.shader;

            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                var propName = shader.GetPropertyName(i);
                var propType = shader.GetPropertyType(i);
                var prop = SerializableMaterialProperty.Get(material, propType, propName);
                if(prop==null) continue;

                props.Add(prop);
            }

            return new SerializableMaterial {ShaderName = shader.name, Properties = props};
        }
    }
}