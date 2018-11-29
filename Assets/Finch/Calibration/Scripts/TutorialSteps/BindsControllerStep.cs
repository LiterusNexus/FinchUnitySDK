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

///
namespace Finch
{
    /// <summary>
    /// Сalibration step, during which there assignment chirality of Finch controllers.
    /// </summary>
    public class BindsControllerStep : TutorialStep
    {
        /// <summary>
        /// Spite renderer object. Required to visualize the display of warnings.
        /// </summary>
        public SpriteRenderer Sprite;

        /// <summary>
        /// An image that visualizes the error of an uncompressed controller (with one controller).
        /// </summary>
        public Sprite NoneHoldOne;

        /// <summary>
        /// An image that visualizes the error of an uncompressed controller (with two controllers).
        /// </summary>
        public Sprite NoneHoldBoth;

        /// <summary>
        /// Image visualizing the error if The controller does not correctly report its chirality (with one controller).
        /// </summary>
        public Sprite BothHoldOne;

        /// <summary>
        /// Image visualizing the error if both controllers incorrectly transmit their chirality data. (with two controllers).
        /// </summary>
        public Sprite BothHoldBoth;

        /// <summary>
        /// Image visualizing the error of incorrect clamping of the left controller (with two controllers).
        /// </summary>
        public Sprite BothLeftHold;

        /// <summary>
        /// Image visualizing the error of incorrect clamping of the right controller (with two controllers).
        /// </summary>
        public Sprite BothRightHold;

        public override void Init(int id)
        {
            base.Init(id);
            CheckChirality(false);
        }

        void Update()
        {
            CheckChirality(true);
            UpdatePosition();
        }

        private void CheckChirality(bool vibrate)
        {
            FinchChirality left = FinchCore.GetCapacitySensor(FinchNodeType.LeftHand);
            FinchChirality right = FinchCore.GetCapacitySensor(FinchNodeType.RightHand);

            if (left == FinchChirality.Unknown && FinchController.Left.IsConnected && right == FinchChirality.Unknown && FinchController.Right.IsConnected)
            {
                Sprite.sprite = NoneHoldBoth;
                return;
            }

            if (left == FinchChirality.Unknown && FinchController.Left.IsConnected || right == FinchChirality.Unknown && FinchController.Right.IsConnected)
            {
                Sprite.sprite = NoneHoldOne;
                return;
            }

            if (left == FinchChirality.Both && FinchController.Left.IsConnected && right == FinchChirality.Both && FinchController.Right.IsConnected)
            {
                Sprite.sprite = BothHoldBoth;
                return;
            }

            if (left == FinchChirality.Both && FinchController.Left.IsConnected || right == FinchChirality.Both && FinchController.Right.IsConnected)
            {
                Sprite.sprite = BothHoldOne;
                return;
            }

            if (left == FinchChirality.Left && FinchController.Left.IsConnected && right == FinchChirality.Left && FinchController.Right.IsConnected)
            {
                Sprite.sprite = BothLeftHold;
                return;
            }

            if (left == FinchChirality.Right && FinchController.Left.IsConnected && right == FinchChirality.Right && FinchController.Right.IsConnected)
            {
                Sprite.sprite = BothRightHold;
                return;
            }

            if (FinchCore.NodesState.GetControllersCount() == 0)
            {
                Sprite.sprite = NoneHoldBoth;
                return;
            }

            if (vibrate)
            {
                FinchController.Left.HapticPulse(100);
                FinchController.Right.HapticPulse(100);
            }

            FinchCore.BindsControllers();
            PlayableSet.RememberNodes();
            NextStep();
        }
    }
}
