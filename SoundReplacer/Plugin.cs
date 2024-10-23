using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;
using SiraUtil.Zenject;
using SoundReplacer.Installers;

namespace SoundReplacer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Logger Log { get; private set; } = null!;

        [Init]
        public Plugin(Logger logger, Config config, Zenjector zenjector)
        {
            Log = logger;
            zenjector.UseLogger(logger);
            zenjector.Install<AppInstaller>(Location.App, config.Generated<PluginConfig>());
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<GameInstaller>(Location.Player);
        }

        [OnEnable]
        public void OnEnable()
        {
            SoundLoader.PopulateSoundList();
        }

        [OnDisable]
        public void OnDisable()
        {
            SoundLoader.SoundList = SoundLoader.DefaultSoundList;
        }
    }
}
