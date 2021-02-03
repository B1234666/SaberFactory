﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace SaberFactory.Helpers
{
    internal static class UIHelpers
    {
        public static async Task<BSMLParserParams> ParseFromResourceAsync(string resource, GameObject parent, object host)
        {
            var data = await Readers.ReadResourceAsync(resource);
            var content = Encoding.UTF8.GetString(data, 3, data.Length - 3);
            return BSMLParser.instance.Parse(content, parent, host);
        }

        public static BSMLParserParams ParseFromResource(string resource, GameObject parent, object host)
        {
            var data = Readers.ReadResource(resource);
            var content = Encoding.UTF8.GetString(data, 3, data.Length - 3);
            return BSMLParser.instance.Parse(content, parent, host);
        }

        public static void SetSkew(this ImageView image, float skew)
        {
            image.SetField("_skew", skew);
        }

        public static void SetGradient(this ImageView image, bool gradient)
        {
            image.SetField("_gradient", gradient);
            image.SetVerticesDirty();
        }
    }
}