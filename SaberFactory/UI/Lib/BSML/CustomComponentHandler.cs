﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Macros;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.TypeHandlers;
using HarmonyLib;
using IPA.Utilities;
using SaberFactory.UI.Lib.BSML.Tags;
using SiraUtil.Tools;
using UnityEngine;
using Zenject;

namespace SaberFactory.UI.Lib.BSML
{
    internal class CustomComponentHandler : IInitializable
    {
        public const string ComponentPrefix = "sui";
        
        public static bool Registered { get; private set; }
        private readonly SiraLog _logger;

        private CustomComponentHandler(
            SiraLog logger,
            Popup.Factory popupFactory,
            CustomUiComponent.Factory customUiComponentFactory)
        {
            _logger = logger;
            _popupFactory = popupFactory;
            _customUiComponentFactory = customUiComponentFactory;
            RegisterAll(BSMLParser.instance);
        }

        public void Initialize()
        {
        }

        private void RegisterAll(BSMLParser parser)
        {
            if (Registered)
            {
                return;
            }

            foreach (var tag in InstantiateOfType<BSMLTag>())
            {
                parser.RegisterTag(tag);
            }

            foreach (var macro in InstantiateOfType<BSMLMacro>())
            {
                parser.RegisterMacro(macro);
            }

            foreach (var handler in InstantiateOfType<TypeHandler>())
            {
                parser.RegisterTypeHandler(handler);
            }

            RegisterCustomComponents(parser);

#if false
            DocCustoms(parser);
#endif

            _logger.Info("Registered Custom Components");

            Registered = true;
        }
        
        private sealed class SchemaWriter : StringWriter
        {
            private readonly XmlSchema _schema;
            public override Encoding Encoding => Encoding.UTF8;

            public SchemaWriter(XmlSchema schema)
            {
                _schema = schema;
            }

            public void Write(string filename)
            {
                _schema.Write(this);
                File.WriteAllText(filename, ToString());
            }
        }

        private void DocCustoms(BSMLParser parser)
        {
            var thisAsm = Assembly.GetExecutingAssembly();
            var tags = parser.GetField<Dictionary<string, BSMLTag>, BSMLParser>("tags");
            var typeHandlers = parser.GetField<List<TypeHandler>, BSMLParser>("typeHandlers");
            
            var schemaTemplate = new WebClient().DownloadString("https://raw.githubusercontent.com/monkeymanboy/BSML-Docs/gh-pages/BSMLSchema.xsd");
            var schema = XmlSchema.Read(XmlReader.Create(new StringReader(schemaTemplate)), (sender, args) => {});

            var attrDict = new Dictionary<string, XmlSchemaAttribute>();

            foreach (var item in schema.Items)
            {
                if (item is XmlSchemaComplexType complexType)
                {
                    foreach (XmlSchemaAttribute attribute in complexType.Attributes)
                    {
                        if (!attrDict.ContainsKey(attribute.Name))
                        {
                            attrDict.Add(attribute.Name, attribute);
                        }
                    }

                    var tag = tags.Values.FirstOrDefault(x => x.GetType().Name == complexType.Name);
                    if (tag is { })
                    {
                        try
                        {
                            var node = tag.CreateObject(parser.transform);
                            foreach (var typeHandler in typeHandlers.Where(x=>x.GetType().Assembly == thisAsm))
                            {
                                var type = typeHandler.GetType().GetCustomAttribute<ComponentHandler>().type;
                                if (parser.InvokeMethod<Component, BSMLParser>("GetExternalComponent", node, type) != null)
                                {
                                    foreach (var attrAliases in typeHandler.Props.Values)
                                    {
                                        foreach (var attrAlias in attrAliases)
                                        {
                                            Debug.Log($"Adding {attrAlias} to existing type {complexType.Name}");
                                            complexType.Attributes.Add(new XmlSchemaAttribute
                                            {
                                                Name = attrAlias
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }

            foreach (var tag in tags.Values)
            {
                var tagType = tag.GetType();
                if (tag.GetType().Assembly != thisAsm) continue;

                var complexType = new XmlSchemaComplexType
                {
                    Name = tagType.Name
                };

                try
                {
                    var currentNode = tag.CreateObject(parser.transform);
                    foreach (var typeHandler in typeHandlers)
                    {
                        var type = typeHandler.GetType().GetCustomAttribute<ComponentHandler>().type;
                        if (parser.InvokeMethod<Component, BSMLParser>("GetExternalComponent", currentNode, type) != null)
                        {
                            foreach (var attrAliases in typeHandler.Props.Values)
                            {
                                foreach (var attrAlias in attrAliases)
                                {
                                    if (!attrDict.TryGetValue(attrAlias, out var attr))
                                    {
                                        attr = new XmlSchemaAttribute
                                        {
                                            Name = attrAlias
                                        };
                                    }

                                    complexType.Attributes.Add(attr);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Couldn't instantiate {tag.Aliases[0]}");
                    Debug.LogWarning(e);
                }
                
                foreach (var alias in tag.Aliases)
                {
                    schema.Items.Add(new XmlSchemaElement
                    {
                        Name = alias,
                        SchemaType = complexType
                    });
                }
            }

            using var writer = new SchemaWriter(schema);
            writer.Write("C:\\Users\\Julian\\CustomBSMLSchema.xsd");
            
            Debug.LogWarning("Written new doc");
        }

        private void RegisterCustomComponents(BSMLParser parser)
        {
            foreach (var type in GetListOfType<CustomUiComponent>())
            {
                parser.RegisterTag(new CustomUiComponentTag(type, _customUiComponentFactory));
            }

            foreach (var type in GetListOfType<Popup>())
            {
                parser.RegisterTag(new PopupTag(type, _popupFactory));
            }
        }

        private List<T> InstantiateOfType<T>(params object[] constructorArgs)
        {
            var objects = new List<T>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(myType =>
                myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)) && myType != typeof(CustomUiComponentTag) &&
                myType != typeof(PopupTag)))
            {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }

            return objects;
        }

        private List<Type> GetListOfType<T>()
        {
            var types = new List<Type>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                types.Add(type);
            }

            return types;
        }

        #region Factories

        private readonly Popup.Factory _popupFactory;
        private readonly CustomUiComponent.Factory _customUiComponentFactory;

        #endregion
    }
}