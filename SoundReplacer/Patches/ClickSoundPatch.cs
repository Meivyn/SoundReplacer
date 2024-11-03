using System;
using SiraUtil.Affinity;
using UnityEngine;

namespace SoundReplacer.Patches
{
    internal class ClickSoundPatch : IAffinity, IDisposable
    {
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;

        private readonly AudioClip[] _clickSounds = new AudioClip[1];
        private AudioClip[]? _originalClickSounds;

        private ClickSoundPatch(SoundLoader soundLoader, PluginConfig config)
        {
            _soundLoader = soundLoader;
            _config = config;
        }

        public void Dispose()
        {
            _soundLoader.Unload(SoundType.Click);
        }

        [AffinityPatch(typeof(BasicUIAudioManager), nameof(BasicUIAudioManager.Start))]
        [AffinityPrefix]
        private void ReplaceClickSounds(BasicUIAudioManager __instance)
        {
            _originalClickSounds ??= __instance._clickSounds;

            if (_config.ClickSound == SoundLoader.NoSoundID)
            {
                _clickSounds[0] = SoundLoader.Empty;
                __instance._clickSounds = _clickSounds;
            }
            else if (_config.ClickSound == SoundLoader.DefaultSoundID)
            {
                __instance._clickSounds = _originalClickSounds;
            }
            else
            {
                _clickSounds[0] = _soundLoader.Load(_clickSounds[0], SoundType.Click);
                __instance._clickSounds = _clickSounds;
            }
        }
    }
}