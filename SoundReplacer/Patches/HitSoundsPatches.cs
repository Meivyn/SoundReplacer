using System;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class HitSoundsPatches : IAffinity, IDisposable
    {
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();
        private readonly AudioClip[] _customBadCutSound = new AudioClip[1];
        private readonly AudioClip[] _customCutSound = new AudioClip[1];

        private AudioClip[]? _originalBadCutSounds;
        private AudioClip[]? _originalLongCutSounds;
        private AudioClip[]? _originalShortCutSounds;

        [AffinityPatch(typeof(NoteCutSoundEffect), nameof(NoteCutSoundEffect.Awake))]
        [AffinityPrefix]
        public void ReplaceBadCutSound(NoteCutSoundEffect __instance)
        {
            _originalBadCutSounds ??= __instance._badCutSoundEffectAudioClips;

            if (Plugin.Config.BadHitSound == SoundLoader.NoSoundID)
            {
                _customBadCutSound[0] = _emptySound;

                __instance._badCutSoundEffectAudioClips = _customBadCutSound;
            }
            else if (Plugin.Config.BadHitSound == SoundLoader.DefaultSoundID)
            {
                __instance._badCutSoundEffectAudioClips = _originalBadCutSounds;
            }
            else
            {
                var badCutSound = SoundLoader.LoadAudioClip(Plugin.Config.BadHitSound);
                _customBadCutSound[0] = badCutSound != null ? badCutSound : _emptySound;

                __instance._badCutSoundEffectAudioClips = _customBadCutSound;
            }
        }

        [AffinityPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.Start))]
        [AffinityPrefix]
        public void ReplaceHitSound(NoteCutSoundEffectManager __instance)
        {
            _originalShortCutSounds ??= __instance._shortCutEffectsAudioClips;
            _originalLongCutSounds ??= __instance._longCutEffectsAudioClips;

            if (Plugin.Config.GoodHitSound == SoundLoader.NoSoundID)
            {
                _customCutSound[0] = _emptySound;

                __instance._shortCutEffectsAudioClips = _customCutSound;
                __instance._longCutEffectsAudioClips = _customCutSound;
            }
            else if (Plugin.Config.GoodHitSound == SoundLoader.DefaultSoundID)
            {
                __instance._shortCutEffectsAudioClips = _originalShortCutSounds;
                __instance._longCutEffectsAudioClips = _originalLongCutSounds;
            }
            else
            {
                var cutSound = SoundLoader.LoadAudioClip(Plugin.Config.GoodHitSound);
                _customCutSound[0] = cutSound != null ? cutSound : _emptySound;

                __instance._shortCutEffectsAudioClips = _customCutSound;
                __instance._longCutEffectsAudioClips = _customCutSound;
            }
        }

        public void Dispose()
        {
            if (_customBadCutSound.Length > 0 && _customBadCutSound[0] != _emptySound)
            {
                Object.Destroy(_customBadCutSound[0]);
            }

            if (_customCutSound.Length > 0 && _customCutSound[0] != _emptySound)
            {
                Object.Destroy(_customCutSound[0]);
            }
        }
    }
}