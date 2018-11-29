// Copyright 2018 Finch Technologies Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;

namespace Finch
{
    /// <summary>
    /// Controls the player's avatar using standard Unity tools.
    /// </summary>
    public class FinchAvatar : MonoBehaviour
    {
        /// <summary>
        /// Player Avatar Unity Animator.
        /// </summary>
        public Animator PlayerAnimator;

        /// <summary>
        /// Legs y position.
        /// </summary>
        public float Floor = -1;

        private void OnAnimatorIK(int layerIndex)
        {
            PlayerAnimator.SetLookAtWeight(0.0f, 0.0f, 1.0f, 1.0f, 0.25f);
            transform.rotation = FinchInput.GetBoneRotation(FinchBone.Chest, fPose: false);
            transform.position = FinchInput.GetBonePosition(FinchBone.Chest) + Vector3.up * Floor;

            PlayerAnimator.SetLookAtPosition(FinchInput.GetBonePosition(FinchBone.Head));

            PlayerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            PlayerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            PlayerAnimator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);

            PlayerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            PlayerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            PlayerAnimator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);

            PlayerAnimator.SetIKPosition(AvatarIKGoal.RightHand, FinchInput.GetBonePosition(FinchBone.RightHand));
            PlayerAnimator.SetIKRotation(AvatarIKGoal.RightHand, FinchInput.GetBoneRotation(FinchBone.RightHand, fPose: false) * Quaternion.Euler(0, 90.0f, 0));
            PlayerAnimator.SetIKHintPosition(AvatarIKHint.RightElbow, FinchInput.GetBonePosition(FinchBone.RightLowerArm));

            PlayerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, FinchInput.GetBonePosition(FinchBone.LeftHand));
            PlayerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, FinchInput.GetBoneRotation(FinchBone.LeftHand, fPose: false) * Quaternion.Euler(0, -90.0f, 0));
            PlayerAnimator.SetIKHintPosition(AvatarIKHint.LeftElbow, FinchInput.GetBonePosition(FinchBone.LeftLowerArm));
        }
    }
}
