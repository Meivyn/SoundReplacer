using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(IPA.Config.Stores.GeneratedStore.AssemblyVisibilityTarget)]

namespace SoundReplacer
{
    internal class PluginConfig
    {
        public string CutSound { get; set; } = SoundLoader.DefaultSoundID;
        public string BadCutSound { get; set; } = SoundLoader.DefaultSoundID;
        public string MenuMusic { get; set; } = SoundLoader.DefaultSoundID;
        public string ClickSound { get; set; } = SoundLoader.DefaultSoundID;
        public string LevelClearedSound { get; set; } = SoundLoader.DefaultSoundID;
        public string LevelFailedSound { get; set; } = SoundLoader.DefaultSoundID;
    }
}
