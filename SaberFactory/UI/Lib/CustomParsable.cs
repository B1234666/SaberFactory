﻿using BeatSaberMarkupLanguage.Parser;
using Newtonsoft.Json.Bson;
using SaberFactory.Helpers;
using UnityEngine;

namespace SaberFactory.UI.Lib
{
    /// <summary>
    /// Direct implementation of <see cref="ICustomParsable"/>
    /// </summary>
    internal class CustomParsable : MonoBehaviour, ICustomParsable
    {
        public BSMLParserParams ParserParams { get; private set; }

        protected string _resourceName => string.Join(".", GetType().Namespace, GetType().Name);

        public virtual void Parse()
        {
            ParserParams = UIHelpers.ParseFromResource(_resourceName, gameObject, this);
        }

        public void Unparse()
        {
            foreach (Transform t in transform)
            {
                t.gameObject.TryDestroy();
            }
        }
    }
}