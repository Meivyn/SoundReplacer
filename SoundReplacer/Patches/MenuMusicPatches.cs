using System;
using SiraUtil.Affinity;
using UnityEngine;

namespace SoundReplacer.Patches
{
    internal class MenuMusicPatches : IAffinity, IDisposable
    {
        private readonly SongPreviewPlayer _songPreviewPlayer;
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;

        private readonly AudioClip _originalMenuMusic;
        private readonly AudioClip _originalLobbyMusic;
        private AudioClip? _menuMusic;

        private MenuMusicPatches(SongPreviewPlayer songPreviewPlayer, GameServerLobbyFlowCoordinator lobbyFlowCoordinator, SoundLoader soundLoader, PluginConfig config)
        {
            _songPreviewPlayer = songPreviewPlayer;
            _soundLoader = soundLoader;
            _config = config;
            _originalMenuMusic = songPreviewPlayer.defaultAudioClip;
            _originalLobbyMusic = lobbyFlowCoordinator._ambienceAudioClip;
        }

        public void Dispose()
        {
            _soundLoader.Unload(SoundType.Menu);
        }

        [AffinityPatch(typeof(SongPreviewPlayer), nameof(SongPreviewPlayer.Start))]
        [AffinityPrefix]
        private void ReplaceMenuMusic()
        {
            // Replace the default menu music on start.
            _songPreviewPlayer._defaultAudioClip = _config.MenuMusic switch
            {
                SoundLoader.NoSoundID => SoundLoader.Empty,
                SoundLoader.DefaultSoundID => _originalMenuMusic,
                _ => _menuMusic = _soundLoader.Load(_menuMusic, SoundType.Menu)
            };
        }

        [AffinityPatch(typeof(SongPreviewPlayer), nameof(SongPreviewPlayer.CrossfadeToNewDefault))]
        [AffinityPrefix]
        private bool PreventExternalDefaultMenuMusic(AudioClip audioClip)
        {
            // If there is no custom sound in use, use the new default.
            if (_songPreviewPlayer.defaultAudioClip != _menuMusic && _songPreviewPlayer.defaultAudioClip != SoundLoader.Empty)
            {
                return true;
            }

            // If the new default is the default menu music, cancel the method.
            return audioClip != _originalMenuMusic && audioClip != _originalLobbyMusic;
        }
    }
}