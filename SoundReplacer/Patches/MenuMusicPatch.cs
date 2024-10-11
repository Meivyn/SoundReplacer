using System;
using System.Linq;
using SiraUtil.Affinity;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class MenuMusicPatch : IAffinity, IDisposable
    {
        private readonly SongPreviewPlayer _songPreviewPlayer;
        private readonly AudioClip _emptySound = SoundLoader.GetEmptyAudioClip();

        private readonly AudioClip _originalMenuMusic;
        private readonly AudioClip _originalLobbyMusic;

        private AudioClip _customMenuMusic;
        private string? _lastMusicSelected;

        private MenuMusicPatch(SongPreviewPlayer songPreviewPlayer, GameServerLobbyFlowCoordinator lobbyFlowCoordinator)
        {
            _songPreviewPlayer = songPreviewPlayer;
            _originalMenuMusic = songPreviewPlayer.defaultAudioClip;
            _originalLobbyMusic = lobbyFlowCoordinator._ambienceAudioClip;
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
            // Replace the default menu music on start
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
            // If there is no custom sound in use, use the new default
            if (_songPreviewPlayer.defaultAudioClip != _customMenuMusic && _songPreviewPlayer.defaultAudioClip != _emptySound)
            {
                return true;
            }

            // If the new default is the default menu music, cancel the method
            return audioClip != _originalMenuMusic && audioClip != _originalLobbyMusic;
        }

        public void Dispose()
        {
            if (_customMenuMusic != _emptySound)
            {
                Object.Destroy(_customMenuMusic);
            }
        }
    }
}