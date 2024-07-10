using System;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class MenuSoundsPatches : IAffinity, IDisposable
    {
        private readonly SongPreviewPlayer _songPreviewPlayer;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();
        private readonly AudioClip[] _customClickSound = new AudioClip[1];

        private AudioClip[]? _originalClickSounds;
        private AudioClip? _originalMenuMusic;
        private AudioClip? _customMenuMusic;
        private string? _lastClickSelected;
        private string? _lastMusicSelected;

        private MenuSoundsPatches(SongPreviewPlayer songPreviewPlayer)
        {
            _songPreviewPlayer = songPreviewPlayer;
        }

        [AffinityPatch(typeof(BasicUIAudioManager), nameof(BasicUIAudioManager.Start))]
        [AffinityPrefix]
        private void ReplaceClickSounds(BasicUIAudioManager __instance)
        {
            _originalClickSounds ??= __instance._clickSounds;

            if (Plugin.Config.ClickSound == SoundLoader.NoSoundID)
            {
                _customClickSound[0] = _emptySound;
                __instance._clickSounds = _customClickSound;
            }
            else if (Plugin.Config.ClickSound == SoundLoader.DefaultSoundID)
            {
                __instance._clickSounds = _originalClickSounds;
            }
            else
            {
                var selectedClickSound = Plugin.Config.ClickSound;
                if (_lastClickSelected != selectedClickSound)
                {
                    if (_customClickSound.Length > 0 && _customClickSound[0] != _emptySound)
                    {
                        Object.Destroy(_customClickSound[0]);
                    }

                    var clickSound = SoundLoader.LoadAudioClip(selectedClickSound);
                    _customClickSound[0] = clickSound != null ? clickSound : _emptySound;
                    _lastClickSelected = selectedClickSound;
                }

                __instance._clickSounds = _customClickSound;
            }
        }

        [AffinityPatch(typeof(SongPreviewPlayer), nameof(SongPreviewPlayer.Start))]
        [AffinityPrefix]
        public void ReplaceMenuMusic()
        {
            if (_originalMenuMusic == null)
            {
                _originalMenuMusic = _songPreviewPlayer._defaultAudioClip;
            }

            if (Plugin.Config.MenuMusic == SoundLoader.NoSoundID)
            {
                _songPreviewPlayer._defaultAudioClip = _emptySound;
            }
            else if (Plugin.Config.MenuMusic == SoundLoader.DefaultSoundID)
            {
                _songPreviewPlayer._defaultAudioClip = _originalMenuMusic;
            }
            else
            {
                var selectedMenuMusic = Plugin.Config.MenuMusic;
                if (_lastMusicSelected != selectedMenuMusic)
                {
                    if (_customMenuMusic != _emptySound)
                    {
                        Object.Destroy(_customMenuMusic);
                    }

                    var menuMusic = SoundLoader.LoadAudioClip(selectedMenuMusic);
                    _customMenuMusic = menuMusic != null ? menuMusic : _emptySound;
                    _lastMusicSelected = selectedMenuMusic;
                }

                _songPreviewPlayer._defaultAudioClip = _customMenuMusic;
            }
        }

        public void Dispose()
        {
            if (_customClickSound.Length > 0 && _customClickSound[0] != _emptySound)
            {
                Object.Destroy(_customClickSound[0]);
            }

            if (_customMenuMusic != null)
            {
                Object.Destroy(_customMenuMusic);
            }
        }
    }
}