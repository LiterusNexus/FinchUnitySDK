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
    /// Calibration step warning of the presence of an incorrect set of controllers.
    /// </summary>
    public class IncorrectSet : TutorialStep
    {
        /// <summary>
        /// Spite renderer object.
        /// </summary>
        public SpriteRenderer Sprite;

        /// <summary>
        /// Image that only two controllers are left.
        /// </summary>
        public Sprite DoubleControllersLeft;

        /// <summary>
        /// Image that only one controller is left.
        /// </summary>
        public Sprite OneControllerLeft;

        /// <summary>
        /// Image that noone controllers left.
        /// </summary>
        public Sprite ZeroControllerLeft;

        void Update()
        {
            UpdatePosition();

            switch (FinchCore.NodesState.GetControllersCount())
            {
                case 2:
                    Sprite.sprite = DoubleControllersLeft;
                    break;

                case 1:
                    Sprite.sprite = OneControllerLeft;
                    break;

                default:
                    Sprite.sprite = ZeroControllerLeft;
                    break;
            }

            if (PlayableSet.AllPlayableNodesConnected)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
