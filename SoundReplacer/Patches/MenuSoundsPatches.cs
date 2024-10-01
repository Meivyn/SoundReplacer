using System;
using System.Linq;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class MenuSoundsPatches : IAffinity, IDisposable
    {
        private readonly SongPreviewPlayer _songPreviewPlayer;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();

        private AudioClip[]? _originalClickSounds;
        private AudioClip[] _customClickSound;
        private string? _lastClickSelected;

        private AudioClip _originalMenuMusic;
        private AudioClip _customMenuMusic;
        private string? _lastMusicSelected;

        private MenuSoundsPatches(SongPreviewPlayer songPreviewPlayer)
        {
            _songPreviewPlayer = songPreviewPlayer;
            _originalMenuMusic = songPreviewPlayer.defaultAudioClip;
            _customMenuMusic = _emptySound;
            _customClickSound = [_emptySound];
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

        private AudioClip[] GetCustomClickSound()
        {
            if (_lastClickSelected == Plugin.Config.ClickSound)
            {
                return _customClickSound;
            }
            _lastClickSelected = Plugin.Config.ClickSound;

            foreach (var sound in _customClickSound.Where(s => s != _emptySound))
            {
                Object.Destroy(sound);
            }

            var clickSound = SoundLoader.LoadAudioClip(Plugin.Config.ClickSound);
            return _customClickSound = clickSound != null ? [clickSound] : [_emptySound];
        }

        [AffinityPatch(typeof(BasicUIAudioManager), nameof(BasicUIAudioManager.Start))]
        [AffinityPrefix]
        private void ReplaceClickSounds(BasicUIAudioManager __instance)
        {
            _originalClickSounds ??= __instance._clickSounds;

            __instance._clickSounds = Plugin.Config.ClickSound switch
            {
                SoundLoader.NoSoundID => [_emptySound],
                SoundLoader.DefaultSoundID => _originalClickSounds,
                _ => GetCustomClickSound()
            };
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
            foreach (var clickSound in _customClickSound.Where(s => s != _emptySound))
            {
                Object.Destroy(clickSound);
            }

            if (_customMenuMusic != null)
            {
                Object.Destroy(_customMenuMusic);
            }
        }
    }
}