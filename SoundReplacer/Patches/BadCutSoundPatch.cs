using System;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class BadCutSoundPatch : IAffinity, IDisposable
    {
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;

        private readonly AudioClip[] _badCutSounds = new AudioClip[1];
        private AudioClip[]? _originalBadCutSounds;

        private BadCutSoundPatch(SoundLoader soundLoader, PluginConfig config)
        {
            _soundLoader = soundLoader;
            _config = config;
        }

        public void Dispose()
        {
            _soundLoader.Unload(SoundType.BadCut);
        }

        [AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
        [AffinityPrefix]
        private void ReplaceBadCutSounds(EffectPoolsManualInstaller __instance)
        {
            var original = __instance._noteCutSoundEffectPrefab;
            var noteCutSoundEffect = Object.Instantiate(original);

            _originalBadCutSounds ??= original._badCutSoundEffectAudioClips;

            if (_config.BadCutSound == SoundLoader.NoSoundID)
            {
                _badCutSounds[0] = SoundLoader.Empty;
                noteCutSoundEffect._badCutSoundEffectAudioClips = _badCutSounds;
            }
            else if (_config.BadCutSound == SoundLoader.DefaultSoundID)
            {
                noteCutSoundEffect._badCutSoundEffectAudioClips = _originalBadCutSounds;
            }
            else
            {
                _badCutSounds[0] = _soundLoader.Load(_badCutSounds[0], SoundType.BadCut);
                noteCutSoundEffect._badCutSoundEffectAudioClips = _badCutSounds;
            }

            __instance._noteCutSoundEffectPrefab = noteCutSoundEffect;
        }
    }
}