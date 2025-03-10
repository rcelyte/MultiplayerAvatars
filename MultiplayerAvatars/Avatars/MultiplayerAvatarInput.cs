using CustomAvatar.Tracking;
using IPA.Utilities;
using System;
using UnityEngine;

namespace MultiplayerAvatars.Avatars
{
    internal class MultiplayerAvatarInput : IAvatarInput
    {
        private readonly BeatSaber.AvatarCore.AvatarController _avatarController;
        private readonly BeatSaber.AvatarCore.IAvatarPoseDataProvider _poseDataProvider;

        private readonly Transform headTransform = new GameObject().transform;
        private readonly Transform rightHandTransform = new GameObject().transform;
        private readonly Transform leftHandTransform = new GameObject().transform;

        internal MultiplayerAvatarInput(BeatSaber.AvatarCore.AvatarController avatarController/*, BeatSaber.AvatarCore.IAvatarPoseDataProvider poseDataProvider*/)
        {
            _avatarController = avatarController;
            _poseDataProvider = avatarController.GetField<BeatSaber.AvatarCore.IAvatarPoseDataProvider, BeatSaber.AvatarCore.AvatarController>("_poseDataProvider");

            headTransform.SetParent(avatarController.transform);
            rightHandTransform.SetParent(avatarController.transform);
            leftHandTransform.SetParent(avatarController.transform);

            SetEnabled(true);
        }

        public void SetEnabled(bool enabled)
        {
            _poseDataProvider.poseDidChangeEvent -= OnPoseChanged; // TODO: dispose
            if(enabled)
                _poseDataProvider.poseDidChangeEvent += OnPoseChanged;
            _avatarController.avatar?.gameObject.SetActive(!enabled);
        }

        void OnPoseChanged(BeatSaber.AvatarCore.AvatarPoseData poseData) {
            headTransform.SetLocalPositionAndRotation(poseData.headPose.position, poseData.headPose.rotation);
            rightHandTransform.SetLocalPositionAndRotation(poseData.rightHandPose.position, poseData.rightHandPose.rotation);
            rightHandTransform.localEulerAngles = new Vector3(rightHandTransform.localEulerAngles.x, rightHandTransform.localEulerAngles.y, rightHandTransform.localEulerAngles.z + 180);
            leftHandTransform.SetLocalPositionAndRotation(poseData.leftHandPose.position, poseData.leftHandPose.rotation);
            leftHandTransform.localEulerAngles = new Vector3(leftHandTransform.localEulerAngles.x, leftHandTransform.localEulerAngles.y, leftHandTransform.localEulerAngles.z + 180);
        }

        public bool allowMaintainPelvisPosition => true;

        public event Action? inputChanged { add {} remove {} }

        public bool TryGetFingerCurl(DeviceUse use, out FingerCurl curl)
        {
            curl = new FingerCurl(0f, 0f, 0f, 0f, 0f);
            return false;
        }

        public bool TryGetTransform(DeviceUse use, out Transform? transform)
        {
            switch (use)
            {
                case DeviceUse.Head:
                    transform = headTransform;
                    return true;
                case DeviceUse.RightHand:
                    transform = rightHandTransform;
                    return true;
                case DeviceUse.LeftHand:
                    transform = leftHandTransform;
                    return true;
                default:
                    transform = default;
                    return false;
            }
        }
    }
}
