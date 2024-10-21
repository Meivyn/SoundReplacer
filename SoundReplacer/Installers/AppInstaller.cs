using SoundReplacer.Patches;
using Zenject;

namespace SoundReplacer.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<BadCutSoundPatch>().AsSingle();
        }
    }
}
