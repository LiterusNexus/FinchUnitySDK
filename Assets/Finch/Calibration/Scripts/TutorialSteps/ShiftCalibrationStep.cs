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
    /// Responses for calibrating Shift Controllers.
    /// </summary>
    public class ShiftCalibrationStep : TutorialStep
    {
        [Header("Tutorial")]
        /// <summary>
        /// The object used to display tutorial notification.
        /// </summary>
        public GameObject TutorialNotification;

        /// <summary>
        /// The object used to display warning notification.
        /// </summary>
        public GameObject WarningNotification;

        public override void Init(int id)
        {
            base.Init(id);

            TutorialNotification.SetActive(true);
            WarningNotification.SetActive(false);

            FinchBodyRotationMode mode = FinchCore.NodesState.GetUpperArmCount() == 2 ? FinchBodyRotationMode.ShoulderRotation : FinchBodyRotationMode.HmdRotation;
            FinchCore.SetBodyRotationMode(mode);
        }

        private void Update()
        {
            UpdatePosition();
            TryCalibrate();
        }

        private void TryCalibrate()
        {
            if (CalibrationButtonPress())
            {
                if (NodeAngleChecker.IsCorrectAngle)
                {
                    FinchController.Left.HapticPulse(FinchCalibration.Settings.HapticTime);
                    FinchController.Right.HapticPulse(FinchCalibration.Settings.HapticTime);
                    FinchController.Left.Calibrate();
                    FinchController.Right.Calibrate();
                    NextStep();
                }
                else
                {
                    TutorialNotification.SetActive(false);
                    WarningNotification.SetActive(true);
                }
            }
        }

        private bool CalibrationButtonPress()
        {
            if (FinchController.Left.IsConnected && FinchController.Right.IsConnected)
            {
                return FinchController.GetPressDown(FinchChirality.Both, FinchControllerElement.HomeButton);
            }

            if (FinchController.Left.IsConnected)
            {
                return FinchController.GetPressDown(FinchChirality.Left, FinchControllerElement.HomeButton);
            }

            if (FinchController.Right.IsConnected)
            {
                return FinchController.GetPressDown(FinchChirality.Right, FinchControllerElement.HomeButton);
            }

            return false;
        }
    }
}
