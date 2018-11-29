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

using System;
using UnityEngine;

namespace Finch
{
    /// <summary>
    /// Visualises controller's buttons elements.
    /// </summary>
    [Serializable]
    public class Buttons
    {
        /// <summary>
        /// Type of controller element.
        /// </summary>
        public FinchControllerElement Type;

        /// <summary>
        /// Pressed button visualisation model.
        /// </summary>
        public GameObject Press;

        /// <summary>
        /// Unpressed button visualisation model.
        /// </summary>
        public GameObject UnPress;

        /// <summary>
        /// Update pressing state of buttons.
        /// </summary>
        /// <param name="isPressing"></param>
        public void UpdateState(bool isPressing)
        {
            if (Press.activeSelf != isPressing)
            {
                Press.SetActive(isPressing);
            }

            if (UnPress.activeSelf == isPressing)
            {
                UnPress.SetActive(!isPressing);
            }
        }
    }

    
    [Serializable]
    /// <summary>
    /// Used to visualize battery power.
    /// </summary>
    public class BatteryLevel
    {
        /// <summary>
        /// Sprite, visualizing a certain level of charge.
        /// </summary>
        public Sprite BatteryMaterial;

        /// <summary>
        /// The level of charge in percent.
        /// </summary>
        [Range(0, 100)]
        public int MinimumBatteryBorder;
    }

    /// <summary>
    /// Visualises controller's buttons, stick or touchpad and battery level.
    /// </summary>
    public class FinchControllerVisual : MonoBehaviour
    {
        [Header("Chirality")]
        /// <summary>
        /// Controller chirality.
        /// </summary>
        public FinchChirality Chirality;

        [Header("Model")]
        /// <summary>
        /// Object to visualise controller state.
        /// </summary>
        public GameObject Model;

        [Header("State")]
        /// <summary>
        /// Represents information about should we hide controllers in calibration module.
        /// </summary>
        public bool HideInCalibration = true;

        [Header("Buttons")]
        /// <summary>
        /// List of visualisable buttons.
        /// </summary>
        public Buttons[] Buttons = new Buttons[0];

        [Header("Battery")]
        /// <summary>
        /// Battery level renderer.
        /// </summary>
        public SpriteRenderer BatteryObject;

        /// <summary>
        /// Array of different charge level materials.
        /// </summary>
        public BatteryLevel[] BatteryLevels = new BatteryLevel[4];

        [Header("Touch element")]
        public GameObject TouchpadBase;
        public GameObject JoystickBase;

        /// <summary>
        /// Touchpad model element transform.
        /// </summary>
        public Transform Touchpad;

        /// <summary>
        /// Stick model element transform.
        /// </summary>
        public Transform Joystick;

        private FinchController controller;
        private const float epsilon = 0.05f;
        private const float chargeLevelEpsilon = 1.5f;
        private float batteryLevel;

        private const float touchPointDepth = 0.001f;
        private const float maxAngleRotation = 18.0f;
        private const float touchPadRadius = 0.13f;
        private const float touchPointRadius = 0.28f;
        private const float scaleTimer = 0.15f;

        private float touchPointPower;

        private void LateUpdate()
        {
            controller = FinchController.GetController(Chirality);

            ButtonUpdate();
            BatteryUpdate();
            TouchElementUpdate();
            StateUpdate();
        }

        private void StateUpdate()
        {
            bool hideCauseCalibration = FinchCalibration.IsCalbrating && HideInCalibration;
            bool hideCauseDisconnect = !controller.IsConnected;
            bool hideCauseIncorrectSet = !PlayableSet.AllPlayableNodesConnected;

            if (Model.activeSelf != (!hideCauseCalibration && !hideCauseDisconnect && !hideCauseIncorrectSet))
            {
                Model.SetActive(!hideCauseCalibration && !hideCauseDisconnect && !hideCauseIncorrectSet);
            }
        }

        private void ButtonUpdate()
        {
            bool activeTouchPad = controller.GetPress(FinchControllerElement.Touch) && controller.TouchAxes.SqrMagnitude() > epsilon;

            foreach (var b in Buttons)
            {
                b.UpdateState(controller.GetPress(b.Type) && (b.Type != FinchControllerElement.Touch || activeTouchPad));
            }
        }

        private void BatteryUpdate()
        {
            if (BatteryObject == null)
            {
                return;
            }

            bool isBatteryActive = (FinchInput.IsConnected(controller.Node)) && BatteryLevels.Length > 0;

            if (BatteryObject.gameObject.activeSelf != isBatteryActive)
            {
                BatteryObject.gameObject.SetActive(isBatteryActive);
            }

            float currentBatteryLevel = Mathf.Clamp(FinchInput.GetBatteryCharge(controller.Node), 0f, 99.9f);

            if (isBatteryActive && Math.Abs(currentBatteryLevel - batteryLevel) > chargeLevelEpsilon)
            {
                Sprite batterySprite = null;
                float maxBorder = 0;

                batteryLevel = currentBatteryLevel;

                foreach (BatteryLevel i in BatteryLevels)
                {
                    if (currentBatteryLevel > i.MinimumBatteryBorder && maxBorder <= i.MinimumBatteryBorder)
                    {
                        maxBorder = i.MinimumBatteryBorder;
                        batterySprite = i.BatteryMaterial;
                    }
                }

                BatteryObject.sprite = batterySprite;
            }
        }

        private void TouchElementUpdate()
        {
            if (TouchpadBase != null)
            {
                TouchpadBase.SetActive(controller.IsTouchpadAvailable);
            }

            if (JoystickBase != null)
            {
                JoystickBase.SetActive(controller.IsJoystickAvailable);
            }

            if (controller.IsTouchpadAvailable)
            {
                Vector3 size = new Vector3(touchPointRadius, touchPointDepth, touchPointRadius);
                float speed = Time.deltaTime / Mathf.Max(epsilon, scaleTimer);

                touchPointPower = Mathf.Clamp01(touchPointPower + (controller.GetPress(FinchControllerElement.Touch) ? 1 : -1) * speed);

                if (Touchpad != null)
                {
                    Touchpad.localScale = controller.GetPress(FinchControllerElement.ThumbButton) ? Vector3.zero : size * touchPointPower;

                    if (controller.IsTouching)
                    {
                        Touchpad.localPosition = new Vector3(controller.TouchAxes.x, Touchpad.localPosition.y, controller.TouchAxes.y) * touchPadRadius;
                    }
                }
            }
            else if (Joystick != null)
            {
                Joystick.transform.localEulerAngles = new Vector3(controller.TouchAxes.y, 0, -controller.TouchAxes.x) * maxAngleRotation;
            }
        }
    }
}
