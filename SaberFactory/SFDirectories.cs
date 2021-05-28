﻿using System.IO;
using IPA.Utilities;

namespace SaberFactory
{
    public struct SFDirectories
    {
        public DirectoryInfo SaberFactoryDir;
        public DirectoryInfo PresetDir;
        public DirectoryInfo CustomSaberDir;

        public static SFDirectories Create()
        {
            var dirs = new SFDirectories();
            var baseDir = new DirectoryInfo(UnityGame.InstallPath);
            var userDataDir = new DirectoryInfo(UnityGame.UserDataPath);
            dirs.SaberFactoryDir = userDataDir.CreateSubdirectory("Saber Factory");
            dirs.PresetDir = dirs.SaberFactoryDir.CreateSubdirectory("Presets");
            dirs.CustomSaberDir = baseDir.CreateSubdirectory("CustomSabers");
            return dirs;
        }
    }
}