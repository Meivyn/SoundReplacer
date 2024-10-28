using HMUI;
using Zenject;

namespace SoundReplacer.UI
{
    internal class SettingsFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator = null!;
        private SettingsViewController _settingsViewController = null!;

        [Inject]
        private void Construct(MainFlowCoordinator mainFlowCoordinator, SettingsViewController settingsViewController)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _settingsViewController = settingsViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle(nameof(SoundReplacer));
                showBackButton = true;
                ProvideInitialViewControllers(_settingsViewController);
            }

            _settingsViewController.RefreshSoundList();
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
