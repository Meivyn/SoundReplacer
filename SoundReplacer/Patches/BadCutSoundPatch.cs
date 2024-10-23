using System;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class BadCutSoundPatch : IAffinity, IDisposable
    {
        private readonly PluginConfig _config;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();
        private readonly AudioClip[] _customBadCutSounds = new AudioClip[1];

        private AudioClip[]? _originalBadCutSounds;

        private string? _lastBadCutSoundSelected;

        private BadCutSoundPatch(PluginConfig config)
        {
            _config = config;
            _customBadCutSounds[0] = _emptySound;
        }

        [AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
        [AffinityPrefix]
        public void ReplaceSoundEffectPrefabSounds(EffectPoolsManualInstaller __instance)
        {
            var original = __instance._noteCutSoundEffectPrefab;
            var noteCutSoundEffect = Object.Instantiate(original);

            _originalBadCutSounds ??= original._badCutSoundEffectAudioClips;

            if (_config.BadHitSound == SoundLoader.NoSoundID)
            {
                _customBadCutSounds[0] = _emptySound;
                noteCutSoundEffect._badCutSoundEffectAudioClips = _customBadCutSounds;
            }
            else if (_config.BadHitSound == SoundLoader.DefaultSoundID)
            {
                noteCutSoundEffect._badCutSoundEffectAudioClips = _originalBadCutSounds;
            }
            else
            {
                _customBadCutSounds[0] = GetCustomBadCutSound();
                noteCutSoundEffect._badCutSoundEffectAudioClips = _customBadCutSounds;
            }

            __instance._noteCutSoundEffectPrefab = noteCutSoundEffect;
        }

        private AudioClip GetCustomBadCutSound()
        {
            if (_lastBadCutSoundSelected == _config.BadHitSound)
            {
                return _customBadCutSounds[0];
            }
            _lastBadCutSoundSelected = _config.BadHitSound;

            if (_customBadCutSounds[0] != _emptySound)
            {
                Object.Destroy(_customBadCutSounds[0]);
            }

            var badCutSound = SoundLoader.LoadAudioClip(_config.BadHitSound);
            return badCutSound != null ? badCutSound : _emptySound;
        }

        public void Dispose()
        {
            if (_customBadCutSounds[0] != _emptySound)
            {
                Object.Destroy(_customBadCutSounds[0]);
            }
        }
    }
}