﻿using SoundReplacer.Patches;
using Zenject;

namespace SoundReplacer.Installers
{
    internal class AppInstaller : Installer
    {
        private readonly PluginConfig _config;

        private AppInstaller(PluginConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindInterfacesAndSelfTo<SoundLoader>().AsSingle();
            Container.BindInterfacesTo<BadCutSoundPatch>().AsSingle();
        }
    }
}
