using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage;
using Zenject;

namespace SoundReplacer.UI
{
    [ViewDefinition("SoundReplacer.SettingsView.bsml")]
    [HotReload(RelativePathToLayout = "SettingsView.bsml")]
    internal class SettingsViewController : BSMLAutomaticViewController
    {
        private SongPreviewPlayer _songPreviewPlayer = null!;
        private PluginConfig _config = null!;
        private readonly BasicUIAudioManager _basicUIAudioManager = BeatSaberUI.BasicUIAudioManager;

        [Inject]
        private void Construct(SongPreviewPlayer songPreviewPlayer, PluginConfig config)
        {
            _songPreviewPlayer = songPreviewPlayer;
            _config = config;
        }

        [UIValue("good-hitsound-list")]
        public List<object> SettingsGoodHitSoundList = new(SoundLoader.SoundList);

        [UIValue("good-hitsound")]
        protected string SettingCurrentGoodHitSound
        {
            get => _config.GoodHitSound;
            set => _config.GoodHitSound = value;
        }

        [UIValue("bad-hitsound-list")]
        public List<object> SettingsBadHitSoundList = new(SoundLoader.SoundList);

        [UIValue("bad-hitsound")]
        protected string SettingCurrentBadHitSound
        {
            get => _config.BadHitSound;
            set => _config.BadHitSound = value;
        }

        [UIValue("menu-music-list")]
        public List<object> SettingsMenuMusicList = new(SoundLoader.SoundList);

        [UIValue("menu-music")]
        protected string SettingCurrentMenuMusic
        {
            get => _config.MenuMusic;
            set
            {
                _config.MenuMusic = value;
                _songPreviewPlayer.Start();
                _songPreviewPlayer.CrossfadeToDefault();
            }
        }

        [UIValue("click-sound-list")]
        public List<object> SettingsClickSoundList = new(SoundLoader.SoundList);

        [UIValue("click-sound")]
        protected string SettingCurrentClickSound
        {
            get => _config.ClickSound;
            set
            {
                _config.ClickSound = value;
                _basicUIAudioManager.Start();
            }
        }

        [UIValue("success-sound-list")]
        public List<object> SettingsSuccessSoundList = new(SoundLoader.SoundList);

        [UIValue("success-sound")]
        protected string SettingCurrentSuccessSound
        {
            get => _config.SuccessSound;
            set => _config.SuccessSound = value;
        }

        [UIValue("fail-sound-list")]
        public List<object> SettingsSuccessFailList = new(SoundLoader.SoundList);

        [UIValue("fail-sound")]
        protected string SettingCurrentFailSound
        {
            get => _config.FailSound;
            set => _config.FailSound = value;
        }
    }
}
