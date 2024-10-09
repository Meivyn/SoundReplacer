using System;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class MenuMusicPatch : IAffinity, IDisposable
    {
        private readonly SongPreviewPlayer _songPreviewPlayer;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();

        private AudioClip _originalMenuMusic;
        private AudioClip _customMenuMusic;
        private string? _lastMusicSelected;

        private MenuMusicPatch(SongPreviewPlayer songPreviewPlayer)
        {
            _songPreviewPlayer = songPreviewPlayer;
            _originalMenuMusic = songPreviewPlayer.defaultAudioClip;
            _customMenuMusic = _emptySound;
        }

        private AudioClip GetCustomMenuMusic()
        {
            if (_lastMusicSelected == Plugin.Config.MenuMusic)
            {
                return _customMenuMusic;
            }
            _lastMusicSelected = Plugin.Config.MenuMusic;

            if (_customMenuMusic != _emptySound)
            {
                Object.Destroy(_customMenuMusic);
            }

            var menuMusic = SoundLoader.LoadAudioClip(Plugin.Config.MenuMusic);
            return _customMenuMusic = menuMusic != null ? menuMusic : _emptySound;
        }

        [AffinityPatch(typeof(SongPreviewPlayer), nameof(SongPreviewPlayer.Start))]
        [AffinityPrefix]
        public void ReplaceMenuMusic()
        {
            _songPreviewPlayer._defaultAudioClip = Plugin.Config.MenuMusic switch
            {
                SoundLoader.NoSoundID => _emptySound,
                SoundLoader.DefaultSoundID => _originalMenuMusic,
                _ => GetCustomMenuMusic()
            };
        }

        [AffinityPatch(typeof(SongPreviewPlayer), nameof(SongPreviewPlayer.CrossfadeToNewDefault))]
        [AffinityPrefix]
        public bool PreventExternalDefaultMenuMusic(AudioClip audioClip)
        {
            // If the new default is the default menu music, cancel the method
            return audioClip != _originalMenuMusic;
        }

        public void Dispose()
        {
            if (_customMenuMusic != null)
            {
                Object.Destroy(_customMenuMusic);
            }
        }
    }
}