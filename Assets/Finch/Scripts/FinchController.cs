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
    /// Vibration parameters.
    /// </summary>
    public struct VibrationPackage
    {
        /// <summary>
        /// Time in milliseconds.
        /// </summary>
        public ushort Time;

        /// <summary>
        /// Rotation speed from -1 to 1.
        /// </summary>
        public float Speed;
    }

    /// <summary>
    /// Manages changes in controller transforms and all controller elements.
    /// </summary>
    public class FinchController
    {
        /// <summary>
        /// Action that happens when controller is connected.
        /// </summary>
        public Action OnConnected;

        /// <summary>
        /// Action that happens when controller is disconnected.
        /// </summary>
        public Action OnDisconnected;

        /// <summary>
        /// Controller chirality.
        /// </summary>
        public readonly FinchChirality Chirality;

        /// <summary>
        /// Bone corresponding to this node.
        /// </summary>
        public readonly FinchBone Bone;

        /// <summary>
        /// Finch node type (physical device).
        /// </summary>
        public readonly FinchNodeType Node;

        /// <summary>
        /// Instance of the Right Finch controller.
        /// </summary>
        public static readonly FinchController Right = new FinchController(FinchChirality.Right, FinchBone.RightHand, FinchNodeType.RightHand);

        /// <summary>
        /// Instance of the Left Finch controller.
        /// </summary>
        public static readonly FinchController Left = new FinchController(FinchChirality.Left, FinchBone.LeftHand, FinchNodeType.LeftHand);

        private FinchController(FinchChirality chirality, FinchBone bone, FinchNodeType node)
        {
            Chirality = chirality;
            Node = node;
            Bone = bone;
        }

        /// <summary>
        /// Returns the controller according to it's chirality.
        /// </summary>
        /// <param name="chirality">Right or left</param>
        /// <returns>Finch Controller of specified chirality.</returns>
        public static FinchController GetController(FinchChirality chirality)
        {
            switch (chirality)
            {
                case FinchChirality.Left:
                    return Left;

                case FinchChirality.Right:
                    return Right;

                default:
                    return null;
            }
        }

        #region SensorInput
        /// <summary>
        /// Returns controller rotation.
        /// </summary>
        public Quaternion Rotation
        {
            get { return FinchInput.GetRotation(Chirality); }
        }

        /// <summary>
        /// Returns controller position.
        /// </summary>
        public Vector3 Position
        {
            get { return FinchInput.GetPosition(Chirality); }
        }

        /// <summary>
        /// Returns gyroscope value of the controller in local coordinate system in degrees/sec.
        /// </summary>
        public Vector3 Gyro
        {
            get { return FinchInput.GetGyro(Node); }
        }

        /// <summary>
        /// Returns accelerometer value of the controller in local coordinate system.
        /// </summary>
        public Vector3 Accel
        {
            get { return FinchInput.GetAccel(Node); }
        }
        #endregion

        #region ControllerElementInput
        /// <summary>
        /// Returns element pressed state.
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <param name="element">Controller element</param>
        /// <returns>Is the controller element pressed</returns>
        public static bool GetPress(FinchChirality chirality, FinchControllerElement element)
        {
            bool left = FinchInput.GetPress(FinchNodeType.LeftHand, element);
            bool right = FinchInput.GetPress(FinchNodeType.RightHand, element);

            switch (chirality)
            {
                case FinchChirality.Right:
                    return right;

                case FinchChirality.Left:
                    return left;

                case FinchChirality.Any:
                    return left || right;

                case FinchChirality.Both:
                    return left && right;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns element pressed down state..
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <param name="element">Controller element</param>
        /// <returns>Was the controller element pressed down</returns>
        public static bool GetPressDown(FinchChirality chirality, FinchControllerElement element)
        {
            bool leftDown = FinchInput.GetPressDown(FinchNodeType.LeftHand, element);
            bool rightDown = FinchInput.GetPressDown(FinchNodeType.RightHand, element);

            bool leftPress = FinchInput.GetPress(FinchNodeType.LeftHand, element);
            bool rightPress = FinchInput.GetPress(FinchNodeType.RightHand, element);

            switch (chirality)
            {
                case FinchChirality.Right:
                    return rightDown;

                case FinchChirality.Left:
                    return leftDown;

                case FinchChirality.Any:
                    return leftDown || rightDown;

                case FinchChirality.Both:
                    return leftDown && rightPress || rightDown && leftPress;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns element pressed up state.
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <param name="element">Controller element</param>
        /// <returns>Was the controller element pressed up</returns>
        public static bool GetPressUp(FinchChirality chirality, FinchControllerElement element)
        {
            bool leftUp = FinchInput.GetPressUp(FinchNodeType.LeftHand, element);
            bool rightUp = FinchInput.GetPressUp(FinchNodeType.RightHand, element);

            bool leftPress = FinchInput.GetPress(FinchNodeType.LeftHand, element);
            bool rightPress = FinchInput.GetPress(FinchNodeType.RightHand, element);

            switch (chirality)
            {
                case FinchChirality.Right:
                    return rightUp;

                case FinchChirality.Left:
                    return leftUp;

                case FinchChirality.Any:
                    return leftUp || rightUp;

                case FinchChirality.Both:
                    return leftUp && !rightPress || rightUp && !leftPress;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns time of pressing.
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <param name="element">Controller element</param>
        /// <returns>Controller element pressing time.</returns>
        public static float GetPressTime(FinchChirality chirality, FinchControllerElement element)
        {
            if (chirality > FinchChirality.Left)
            {
                throw new ArgumentException();
            }

            return FinchInput.GetPressTime((FinchNodeType)chirality, element);
        }

        /// <summary>
        /// Returns element pressing state.
        /// </summary>
        /// <param name="element">Controller element</param>
        /// <returns>Is the controller element pressed</returns>
        public bool GetPress(FinchControllerElement element)
        {
            return FinchInput.GetPress(Node, element);
        }

        /// <summary>
        /// Returns element pressed down state.
        /// </summary>
        /// <param name="element">Controller element</param>
        /// <returns>Was the controller element pressed down</returns>
        public bool GetPressDown(FinchControllerElement element)
        {
            return FinchInput.GetPressDown(Node, element);
        }

        /// <summary>
        /// Returns element pressed up state.
        /// </summary>
        /// <param name="element">Controller element</param>
        /// <returns>Was the controller element pressed up</returns>
        public bool GetPressUp(FinchControllerElement element)
        {
            return FinchInput.GetPressUp(Node, element);
        }

        /// <summary>
        /// Returns time of pressing.
        /// </summary>
        /// <param name="element">Controller element</param>
        /// <returns>Controller element press time state.</returns>
        public float GetPressTime(FinchControllerElement element)
        {
            return FinchInput.GetPressTime(Node, element);
        }

        /// <summary>
        /// Returns HomeButton pressing state.
        /// </summary>
        public bool HomeButton
        {
            get { return FinchInput.GetPress(Node, FinchControllerElement.HomeButton); }
        }

        /// <summary>
        /// Returns HomeButton pressed down state.
        /// </summary>
        public bool HomeButtonDown
        {
            get { return FinchInput.GetPressDown(Node, FinchControllerElement.HomeButton); }
        }

        /// <summary>
        /// Returns HomeButton pressed up state.
        /// </summary>
        public bool HomeButtonUp
        {
            get { return FinchInput.GetPressUp(Node, FinchControllerElement.HomeButton); }
        }

        /// <summary>
        /// Returns AppButton pressing state.
        /// </summary>
        public bool AppButton
        {
            get { return FinchInput.GetPress(Node, FinchControllerElement.AppButton); }
        }

        /// <summary>
        /// Returns AppButton pressed down state.
        /// </summary>
        public bool AppButtonDown
        {
            get { return FinchInput.GetPressDown(Node, FinchControllerElement.AppButton); }
        }

        /// <summary>
        /// Returns AppButton pressed up state.
        /// </summary>
        public bool AppButtonUp
        {
            get { return FinchInput.GetPressUp(Node, FinchControllerElement.AppButton); }
        }

        /// <summary>
        /// Returns ThumbButton pressing state.
        /// </summary>
        public bool ThumbButton
        {
            get { return FinchInput.GetPress(Node, FinchControllerElement.ThumbButton); }
        }

        /// <summary>
        /// Returns ThumbButton pressed down state.
        /// </summary>
        public bool ThumbButtonDown
        {
            get { return FinchInput.GetPressDown(Node, FinchControllerElement.ThumbButton); }
        }

        /// <summary>
        /// Returns ThumbButton pressed up state.
        /// </summary>
        public bool ThumbButtonbUp
        {
            get { return FinchInput.GetPressUp(Node, FinchControllerElement.ThumbButton); }
        }

        /// <summary>
        /// Returns GripButton pressing state.
        /// </summary>
        public bool GripButton
        {
            get { return FinchInput.GetPress(Node, FinchControllerElement.GripButton); }
        }

        /// <summary>
        /// Returns GripButton pressed down state.
        /// </summary>
        public bool GripButtonDown
        {
            get { return FinchInput.GetPressDown(Node, FinchControllerElement.GripButton); }
        }

        /// <summary>
        /// Returns GripButton pressed up state.
        /// </summary>
        public bool GripButtonUp
        {
            get { return FinchInput.GetPressUp(Node, FinchControllerElement.GripButton); }
        }

        /// <summary>
        /// Returns VolumeUpButton pressing state.
        /// </summary>
        public bool VolumeUpButton
        {
            get { return FinchInput.GetPress(Node, FinchControllerElement.VolumeUpButton); }
        }

        /// <summary>
        /// Returns VolumeUpButton pressed down state.
        /// </summary>
        public bool VolumeUpButtonDown
        {
            get { return FinchInput.GetPressDown(Node, FinchControllerElement.VolumeUpButton); }
        }

        /// <summary>
        /// Returns VolumeUpButton pressed up state.
        /// </summary>
        public bool VolumeUpButtonUp
        {
            get { return FinchInput.GetPressUp(Node, FinchControllerElement.VolumeUpButton); }
        }

        /// <summary>
        /// Returns VolumeDownButton pressing state.
        /// </summary>
        public bool VolumeDownButton
        {
            get { return FinchInput.GetPress(Node, FinchControllerElement.VolumeDownButton); }
        }

        /// <summary>
        /// Returns VolumeDownButton pressed down state.
        /// </summary>
        public bool VolumeDownButtonDown
        {
            get { return FinchInput.GetPressDown(Node, FinchControllerElement.VolumeDownButton); }
        }

        /// <summary>
        /// Returns VolumeDownButton pressed up state.
        /// </summary>
        public bool VolumeDownButtonUp
        {
            get { return FinchInput.GetPressUp(Node, FinchControllerElement.VolumeDownButton); }
        }

        /// <summary>
        /// Returns Touchpad touching state.
        /// </summary>
        public bool IsTouching
        {
            get { return FinchInput.GetPress(Node, FinchControllerElement.Touch); }
        }

        /// <summary>
        /// Returns Touchpad pressed down state.
        /// </summary>
        public bool TouchDown
        {
            get { return FinchInput.GetPressDown(Node, FinchControllerElement.Touch); }
        }

        /// <summary>
        /// Returns Touchpad pressed up state.
        /// </summary>
        public bool TouchUp
        {
            get { return FinchInput.GetPressUp(Node, FinchControllerElement.Touch); }
        }

        /// <summary>
        /// Returns touch position coordinates.
        /// </summary>
        /// <returns>Touchpad touch position for touchpad and thumstick axes values for thumbstick</returns>
        public Vector2 TouchAxes
        {
            get { return FinchInput.GetTouchAxes(Chirality); }
        }

        /// <summary>
        /// Returns Trigger value.
        /// </summary>
        /// <returns>Trigger value</returns>
        public float Trigger
        {
            get { return FinchInput.GetTrigger(Chirality); }
        }
        #endregion

        #region Haptic

        /// <summary>
        /// Sends vibration signal to the node. 
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <param name="millisecond">Certain milliseconds time, but no more than 2500 ms</param>
        public static void HapticPulse(FinchChirality chirality, ushort millisecond)
        {
            FinchCore.HapticPulse((FinchNodeType)chirality, millisecond);
        }

        /// <summary>
        /// Sends instructions pack for vibration engine to the node.
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <param name="vibrationPackage">Instructions pack for vibration engine</param>
        public static void HapticPulseByPattern(FinchChirality chirality, params VibrationPackage[] vibrationPackage)
        {
            FinchCore.HapticPulseByPattern((FinchNodeType)chirality, vibrationPackage);
        }

        /// <summary>
        /// Sets time for node vibration in milliseconds.
        /// </summary>
        /// <param name="millisecond">Certain milliseconds time, but no more than 2500 ms</param>
        public void HapticPulse(ushort millisecond)
        {
            FinchCore.HapticPulse(Node, millisecond);
        }

        /// <summary>
        /// Create array of packages, there each one have it's own vibration time in milliseconds.
        /// </summary>
        /// <param name="vibrationPackage">Instructions pack for vibration enginea</param>
        public void HapticPulseByPattern(params VibrationPackage[] vibrationPackage)
        {
            FinchCore.HapticPulseByPattern(Node, vibrationPackage);
        }
        #endregion

        #region Calibration
        /// <summary>
        /// Allows you to calibrate controllers for a given chirality.
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        public static void Calibrate(FinchChirality chirality)
        {
            FinchCore.Calibration(chirality);
        }

        /// <summary>
        /// Calibrates the selected controller.
        /// </summary>
        public void Calibrate()
        {
            FinchCore.Calibration(Chirality);
        }
        #endregion

        /// <summary>
        /// If the controller have a touchpad.
        /// </summary>
        public bool IsTouchpadAvailable
        {
            get { return !IsJoystickAvailable; }
        }

        /// <summary>
        /// If the controller have a joystick.
        /// </summary>
        public bool IsJoystickAvailable
        {
            get { return FinchCore.GetEvent(Node, (FinchControllerElement)FinchCore.Interop.FinchControllerElement.IsTouchpadNotAvailable); }
        }

        /// <summary>
        /// Returns connection state.
        /// </summary>
        public bool IsConnected
        {
            get { return FinchInput.IsConnected(Node); }
        }

        /// <summary>
        /// Nodes batteries charge value of physical device (percents)
        /// </summary>
        public ushort BatteryCharge
        {
            get { return FinchInput.GetBatteryCharge(Node); }
        }
    }
}
