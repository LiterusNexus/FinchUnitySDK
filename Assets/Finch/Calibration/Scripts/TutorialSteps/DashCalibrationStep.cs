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
    /// Responses for calibrating Dash Controllers.
    /// </summary>
    public class DashCalibrationStep : TutorialStep
    {
        [Header("Tutorials")]
        /// <summary>
        /// An object that renders hints for calibrating the left controller.
        /// </summary>
        public GameObject LeftDash;

        /// <summary>
        /// An object that renders hints for calibrating the right controller.
        /// </summary>
        public GameObject RightDash;

        /// <summary>
        /// An object that visualizes tips for calibrating a single controller.
        /// </summary>
        public GameObject OneDash;

        private bool leftCalibrate;
        private bool leftPressDown;
        private bool rightPressDown;

        public override void Init(int id)
        {
            base.Init(id);

            leftCalibrate = false;
            leftPressDown = false;
            rightPressDown = false;
            TryAutoCalibrate();
        }

        private void Update()
        {
            UpdateTutorialsState();
            UpdatePressing();
            TryLoadNextStep();
            UpdatePosition();
            UpdatePressing();
        }

        private void TryAutoCalibrate()
        {
            if (FinchCore.NodesState.GetControllersCount() == 1)
            {
                leftPressDown = true;
                rightPressDown = true;

                if (TryCalibrate(DashHandednessChooser.DashHandness, true))
                {
                    NextStep();
                }
            }
        }

        private void UpdatePressing()
        {
            leftPressDown |= FinchController.Left.HomeButtonDown;
            leftPressDown &= !FinchController.Left.HomeButtonUp;

            rightPressDown |= FinchController.Right.HomeButtonDown;
            rightPressDown &= !FinchController.Right.HomeButtonUp;
        }

        private void UpdateTutorialsState()
        {
            int dashCount = FinchCore.NodesState.GetControllersCount();
            if (OneDash.activeSelf != (dashCount == 1))
            {
                OneDash.SetActive(dashCount == 1);
            }

            if (LeftDash.activeSelf != (dashCount == 2 && !leftCalibrate))
            {
                LeftDash.SetActive(dashCount == 2 && !leftCalibrate);
            }

            if (RightDash.activeSelf != (dashCount == 2 && leftCalibrate))
            {
                RightDash.SetActive(dashCount == 2 && leftCalibrate);
            }
        }

        private void TryLoadNextStep()
        {
            if (FinchCore.NodesState.GetControllersCount() == 1 && TryCalibrate(DashHandednessChooser.DashHandness, true))
            {
                NextStep();
            }

            if (FinchCore.NodesState.GetControllersCount() == 2)
            {
                if (!leftCalibrate && TryCalibrate(FinchChirality.Left, true))
                {
                    leftCalibrate = true;
                }

                if (leftCalibrate && TryCalibrate(FinchChirality.Right, false))
                {
                    NextStep();
                }
            }
        }

        private bool TryCalibrate(FinchChirality chirality, bool swap)
        {
            FinchController mainController = chirality == FinchChirality.Left ? FinchController.Left : FinchController.Right;
            FinchController additionalController = chirality == FinchChirality.Left ? FinchController.Right : FinchController.Left;

            bool leftCalibrate = leftPressDown && FinchController.Left.GetPressTime(FinchControllerElement.HomeButton) > FinchCalibration.Settings.TimeToCallModule;
            bool rightCalibrate = rightPressDown && FinchController.Right.GetPressTime(FinchControllerElement.HomeButton) > FinchCalibration.Settings.TimeToCallModule;
            bool mainCalibrate = chirality == FinchChirality.Left ? leftCalibrate : rightCalibrate;
            bool additionalCalibrate = chirality == FinchChirality.Left ? rightCalibrate : leftCalibrate;

            if (mainCalibrate || additionalCalibrate && swap)
            {
                if (mainCalibrate)
                {
                    mainController.HapticPulse(FinchCalibration.Settings.HapticTime);
                    mainController.Calibrate();
                }
                else
                {
                    additionalController.HapticPulse(FinchCalibration.Settings.HapticTime);
                    additionalController.Calibrate();
                    FinchCore.SwapNodes(FinchNodeType.LeftHand, FinchNodeType.RightHand);
                    FinchCore.Interop.FinchSwapCalibrations(1, 0);
                }
            }

            return mainCalibrate || additionalCalibrate && swap;
        }
    }
}
