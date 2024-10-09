using System;
using System.Linq;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class ClickSoundPatch : IAffinity, IDisposable
    {
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();

        private AudioClip[]? _originalClickSounds;
        private AudioClip[] _customClickSound;
        private string? _lastClickSelected;

        private ClickSoundPatch()
        {
            _customClickSound = [_emptySound];
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

        public void Dispose()
        {
            foreach (var clickSound in _customClickSound.Where(s => s != _emptySound))
            {
                Object.Destroy(clickSound);
            }
        }
    }
}