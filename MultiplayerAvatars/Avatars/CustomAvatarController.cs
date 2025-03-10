using CustomAvatar.Avatar;
using MultiplayerAvatars.Networking;
using MultiplayerAvatars.Providers;
using SiraUtil.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace MultiplayerAvatars.Avatars
{
    internal class CustomAvatarController : MonoBehaviour
    {
        readonly SynchronizationContext _syncContext = SynchronizationContext.Current;

        private CustomAvatarPacket _avatarPacket = new();
        private AvatarPrefab? _loadedAvatar;
        private SpawnedAvatar? _spawnedAvatar;

        private AvatarSpawner _avatarSpawner = null!;
        private IConnectedPlayer _connectedPlayer = null!;
        private CustomAvatarManager _customAvatarManager = null!;
        private AvatarProviderService _avatarProvider = null!;
        private MultiplayerAvatarInput _avatarInput = null!;
        private SiraLog _logger = null!;

        [Inject]
        public void Construct(
            AvatarSpawner avatarSpawner,
            IConnectedPlayer connectedPlayer,
            CustomAvatarManager customAvatarManager,
            AvatarProviderService avatarProvider,
            BeatSaber.AvatarCore.AvatarController avatarController,
            // BeatSaber.AvatarCore.IAvatarPoseDataProvider poseDataProvider,
            SiraLog logger)
        {
            _avatarSpawner = avatarSpawner;
            _avatarProvider = avatarProvider;
            _connectedPlayer = connectedPlayer;
            _customAvatarManager = customAvatarManager;
            _logger = logger;

            _avatarInput = new MultiplayerAvatarInput(avatarController/*, poseDataProvider*/);
        }

        public void OnEnable()
        {
            _customAvatarManager.avatarReceived += HandleAvatarReceived;
            _avatarPacket = _customAvatarManager.GetPlayerAvatarPacket(_connectedPlayer.userId);
            HandleAvatarReceived(_connectedPlayer, _avatarPacket);
        }

        public void OnDisable()
        {
            _customAvatarManager.avatarReceived -= HandleAvatarReceived;
        }

        private void HandleAvatarReceived(IConnectedPlayer player, CustomAvatarPacket packet)
        {
            if (player.userId != _connectedPlayer.userId)
                return;
            if (packet.Hash == "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")
                return;

            _avatarPacket = packet;
            _ = LoadAvatar(packet.Hash); // We need this to run on the main thread
        }

        private async Task LoadAvatar(string hash)
        {
            var avatarPrefab = await _avatarProvider.GetAvatarByHash(hash, CancellationToken.None);
            if (avatarPrefab == null)
            {
                _logger.Warn($"Tried to load avatar and failed: {hash}");
                return;
            }

            _syncContext.Post(CreateAvatar, avatarPrefab);
        }

        private void CreateAvatar(object state)
        {
            AvatarPrefab avatar = (AvatarPrefab)state;
            _loadedAvatar = avatar;
            if (_spawnedAvatar != null)
                Destroy(_spawnedAvatar);

            _spawnedAvatar = _avatarSpawner.SpawnAvatar(avatar, _avatarInput, transform);
            _avatarInput.SetEnabled(true);
            var avatarIk = _spawnedAvatar.GetComponent<AvatarIK>();
            if (avatarIk != null)
                avatarIk.isLocomotionEnabled = true;
            _spawnedAvatar.transform.localScale *= _avatarPacket.Scale;
        }
    }
}
