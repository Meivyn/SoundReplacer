using SoundReplacer.Patches;
using Zenject;

namespace SoundReplacer.Installers
{
    internal class GameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<HitSoundsPatches>().AsSingle();
        }
    }
}
