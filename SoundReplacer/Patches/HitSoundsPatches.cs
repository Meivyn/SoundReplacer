using SiraUtil.Affinity;
using System;
using System.Linq;
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

        [AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
        [AffinityPrefix]
        public void ReplaceSoundEffectPrefabSounds(EffectPoolsManualInstaller __instance)
        {
            var original = __instance._noteCutSoundEffectPrefab;
            var noteCutSoundEffect = Object.Instantiate(original);

            _originalBadCutSounds ??= original._badCutSoundEffectAudioClips;

            noteCutSoundEffect._badCutSoundEffectAudioClips = Plugin.Config.BadHitSound switch
            {
                SoundLoader.NoSoundID => [_emptySound],
                SoundLoader.DefaultSoundID => _originalBadCutSounds,
                _ => GetSound(Plugin.Config.BadHitSound) is AudioClip a ? [a] : [_emptySound]
            };

            __instance._noteCutSoundEffectPrefab = noteCutSoundEffect;
        }

        [AffinityPatch(typeof(NoteCutSoundEffectManager), nameof(NoteCutSoundEffectManager.Start))]
        [AffinityPrefix]
        public void ReplaceHitSound(NoteCutSoundEffectManager __instance)
        {
            _originalShortCutSounds ??= __instance._shortCutEffectsAudioClips;
            _originalLongCutSounds ??= __instance._longCutEffectsAudioClips;

            if (Plugin.Config.GoodHitSound == SoundLoader.NoSoundID)
            {
                __instance._shortCutEffectsAudioClips = [_emptySound];
                __instance._longCutEffectsAudioClips = [_emptySound];
            }
            else if (Plugin.Config.GoodHitSound == SoundLoader.DefaultSoundID)
            {
                __instance._shortCutEffectsAudioClips = _originalShortCutSounds;
                __instance._longCutEffectsAudioClips = _originalLongCutSounds;
            }
            else
            {
                AudioClip[] sound = GetSound(Plugin.Config.GoodHitSound) is AudioClip a ? [a] : [_emptySound];
                __instance._shortCutEffectsAudioClips = sound;
                __instance._longCutEffectsAudioClips = sound;
            }
        }

        private static AudioClip? GetSound(string name)
        {
            var sound = SoundLoader.LoadAudioClip(name);
            return sound != null ? sound : null;
        }

        public void Dispose()
        {
            foreach (var badCutSound in _customBadCutSound.Where(s => s != _emptySound))
            {
                Object.Destroy(badCutSound);
            }

            foreach (var cutSound in _customCutSound.Where(s => s != _emptySound))
            {
                Object.Destroy(cutSound);
            }
        }
    }
}