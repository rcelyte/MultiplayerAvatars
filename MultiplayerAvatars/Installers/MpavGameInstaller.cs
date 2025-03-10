using MultiplayerAvatars.Avatars;
using SiraUtil.Extras;
using SiraUtil.Objects.Multiplayer;
using UnityEngine;
using Zenject;

namespace MultiplayerAvatars.Installers
{
    internal class MpavGameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.RegisterRedecorator(new ConnectedPlayerRegistration(DecorateConnectedPlayerFacade));
            Container.RegisterRedecorator(new ConnectedPlayerDuelRegistration(DecorateConnectedPlayerFacade));
        }

        private MultiplayerConnectedPlayerFacade DecorateConnectedPlayerFacade(MultiplayerConnectedPlayerFacade original)
        {
            GameObject gameObject = original.GetComponentInChildren<BeatSaber.AvatarCore.MultiplayerAvatarPoseController>().gameObject;
            gameObject.AddComponent<CustomAvatarController>();
            GameObject.Destroy(gameObject.GetComponent<Animator>());
            return original;
        }
    }
}
