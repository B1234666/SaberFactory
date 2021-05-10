﻿using System;
using IPA.Logging;
using SaberFactory.Configuration;
using SaberFactory.DataStore;
using SaberFactory.Instances;
using SaberFactory.Models;
using SaberFactory.Models.CustomSaber;
using SaberFactory.Saving;
using SiraUtil;
using System.Linq;
using SaberFactory.Instances.Trail;
using Zenject;

namespace SaberFactory.Installers
{
    internal class AppInstaller : Installer
    {
        private readonly Logger _logger;
        private readonly PluginConfig _config;

        private AppInstaller(Logger logger, PluginConfig config)
        {
            _logger = logger;
            _config = config;
        }

        public override void InstallBindings()
        {
            if (_config.FirstLaunch)
            {
                _config.FirstLaunch = false;
                _config.RuntimeFirstLaunch = true;
            }

            var rtOptions = new LaunchOptions();

            if (Environment.GetCommandLineArgs().Any(x => x.ToLower() == "fpfc"))
            {
                rtOptions.FPFC = true;
                AltTrail.CapFps = true;
            }

            Container.BindInstance(rtOptions).AsSingle();

            var dirs = SFDirectories.Create();
            Container.BindInstance(dirs);

            Container.BindLoggerAsSiraLogger(_logger);
            Container.BindInstance(_config).AsSingle();
            Container.Bind<PluginManager>().AsSingle();

            Container.Bind<PresetSaveManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<TrailConfig>().AsSingle();

            Container.BindInterfacesAndSelfTo<EmbeddedAssetLoader>().AsSingle();

            Container.Bind<CustomSaberModelLoader>().AsSingle();

            Container.Bind<TextureStore>().AsSingle();

            Container.BindInterfacesAndSelfTo<MainAssetStore>().AsSingle()
                .OnInstantiated<MainAssetStore>(OnMainAssetStoreInstantiated);


            // Model stuff
            Container.Bind<SaberModel>().WithId(ESaberSlot.Left).AsCached().WithArguments(ESaberSlot.Left);
            Container.Bind<SaberModel>().WithId(ESaberSlot.Right).AsCached().WithArguments(ESaberSlot.Right);

            Container.Bind<SaberSet>().AsSingle();

            InstallFactories();
        }

        private async void OnMainAssetStoreInstantiated(InjectContext ctx, MainAssetStore mainAssetStore)
        {
            await mainAssetStore.LoadAllMetaAsync(_config.AssetType);
        }

        private void InstallFactories()
        {
            Container.BindFactory<StoreAsset, CustomSaberModel, CustomSaberModel.Factory>();

            Container.BindFactory<BasePieceModel, BasePieceInstance, BasePieceInstance.Factory>()
                .FromFactory<InstanceFactory>();
            Container.BindFactory<SaberModel, SaberInstance, SaberInstance.Factory>();
        }
    }
}