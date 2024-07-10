using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using Zenject;

namespace SoundReplacer.UI
{
    [ViewDefinition("SoundReplacer.SettingsView.bsml")]
    [HotReload(RelativePathToLayout = "SettingsView.bsml")]
    internal class SettingsViewController : BSMLAutomaticViewController
    {
        private SongPreviewPlayer _songPreviewPlayer = null!;
        private readonly BasicUIAudioManager _basicUIAudioManager = BeatSaberUI.BasicUIAudioManager;

        [Inject]
        private void Construct(SongPreviewPlayer songPreviewPlayer)
        {
            _songPreviewPlayer = songPreviewPlayer;
        }

        [UIValue("good-hitsound-list")]
        public List<object> SettingsGoodHitSoundList = new(SoundLoader.SoundList);

        [UIValue("good-hitsound")]
        protected string SettingCurrentGoodHitSound
        {
            get => Plugin.Config.GoodHitSound;
            set => Plugin.Config.GoodHitSound = value;
        }

        [UIValue("bad-hitsound-list")]
        public List<object> SettingsBadHitSoundList = new(SoundLoader.SoundList);

        [UIValue("bad-hitsound")]
        protected string SettingCurrentBadHitSound
        {
            get => Plugin.Config.BadHitSound;
            set => Plugin.Config.BadHitSound = value;
        }

        [UIValue("menu-music-list")]
        public List<object> SettingsMenuMusicList = new(SoundLoader.SoundList);

        [UIValue("menu-music")]
        protected string SettingCurrentMenuMusic
        {
            get => Plugin.Config.MenuMusic;
            set
            {
                Plugin.Config.MenuMusic = value;
                _songPreviewPlayer.Start();
                _songPreviewPlayer.CrossfadeToDefault();
            }
        }

        [UIValue("click-sound-list")]
        public List<object> SettingsClickSoundList = new(SoundLoader.SoundList);

        [UIValue("click-sound")]
        protected string SettingCurrentClickSound
        {
            get => Plugin.Config.ClickSound;
            set
            {
                Plugin.Config.ClickSound = value;
                _basicUIAudioManager.Start();
            }
        }

        [UIValue("success-sound-list")]
        public List<object> SettingsSuccessSoundList = new(SoundLoader.SoundList);

        [UIValue("success-sound")]
        protected string SettingCurrentSuccessSound
        {
            get => Plugin.Config.SuccessSound;
            set => Plugin.Config.SuccessSound = value;
        }

        [UIValue("fail-sound-list")]
        public List<object> SettingsSuccessFailList = new(SoundLoader.SoundList);

        [UIValue("fail-sound")]
        protected string SettingCurrentFailSound
        {
            get => Plugin.Config.FailSound;
            set => Plugin.Config.FailSound = value;
        }
    }
}
