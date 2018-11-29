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
    /// Describes sensor position.
    /// </summary>
    public enum FinchNodeType : byte
    {
        RightHand = FinchCore.Interop.FinchNodeType.RightHand,
        LeftHand = FinchCore.Interop.FinchNodeType.LeftHand,
        RightUpperArm = FinchCore.Interop.FinchNodeType.RightUpperArm,
        LeftUpperArm = FinchCore.Interop.FinchNodeType.LeftUpperArm
    }

    /// <summary>
    /// Describes chirality of controller.
    /// </summary>
    public enum FinchChirality : byte
    {
        Right = FinchCore.Interop.FinchChirality.Right,
        Left = FinchCore.Interop.FinchChirality.Left,
        Any = 254,
        Both = 255,
        Unknown = 5
    }

    /// <summary>
    /// Describes the bone of the skeleton for animation.
    /// </summary>
    public enum FinchBone : byte
    {
        RightHand = FinchCore.Interop.FinchBone.RightHand,
        LeftHand = FinchCore.Interop.FinchBone.LeftHand,
        RightUpperArm = FinchCore.Interop.FinchBone.RightUpperArm,
        LeftUpperArm = FinchCore.Interop.FinchBone.LeftUpperArm,
        RightLowerArm = FinchCore.Interop.FinchBone.RightLowerArm,
        LeftLowerArm = FinchCore.Interop.FinchBone.LeftLowerArm,
        Chest = FinchCore.Interop.FinchBone.Chest,
        Head = FinchCore.Interop.FinchBone.Head
    }

    /// <summary>
    /// Describes elements of a controller.
    /// </summary>
    public enum FinchControllerElement : byte
    {
        HomeButton = FinchCore.Interop.FinchControllerElement.ButtonZero,
        AppButton = FinchCore.Interop.FinchControllerElement.ButtonOne,
        ThumbButton = FinchCore.Interop.FinchControllerElement.ButtonThumb,
        Touch = FinchCore.Interop.FinchControllerElement.Touch,
        Trigger = FinchCore.Interop.FinchControllerElement.IndexTrigger,

        // Shift
        GripButton = FinchCore.Interop.FinchControllerElement.ButtonGrip,

        // Dash
        VolumeUpButton = FinchCore.Interop.FinchControllerElement.ButtonThree,
        VolumeDownButton = FinchCore.Interop.FinchControllerElement.ButtonTwo
    }

    /// <summary>
    /// Type of event.
    /// </summary>
    public enum FinchEventType : byte
    {
        Begin = FinchCore.Interop.FinchEventType.Begin,
        Process = FinchCore.Interop.FinchEventType.Process,
        End = FinchCore.Interop.FinchEventType.End,
        None = FinchCore.Interop.FinchEventType.Unknown
    }

    /// <summary>
    /// Responsible for receiving data from controllers.
    /// </summary>
    public static class FinchInput
    {
        #region SensorInput
        /// <summary>
        /// Returns bone rotation.
        /// </summary>
        /// <param name="bone">Certain bone</param>
        /// <param name="fPose">Use fPose rotation</param>
        /// <returns>Bone rotation quaternion</returns>
        public static Quaternion GetBoneRotation(FinchBone bone, bool fPose = true)
        {
            return FinchCore.GetBoneRotation(bone, fPose);
        }

        /// <summary>
        /// Returns controller rotation.
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <returns>Controller rotation quaternion</returns>
        public static Quaternion GetRotation(FinchChirality chirality)
        {
            return FinchCore.GetControllerRotation(chirality, true);
        }

        /// <summary>
        /// Returns bone position.
        /// </summary>
        /// <param name="bone">Certain bone</param>
        /// <returns>Position coordinates</returns>
        public static Vector3 GetBonePosition(FinchBone bone)
        {
            return FinchCore.GetBonePosition(bone);
        }

        /// <summary>
        /// Returns controller position.
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <returns>Position coordinates</returns>
        public static Vector3 GetPosition(FinchChirality chirality)
        {
            return FinchCore.GetControllerPosition(chirality);
        }

        /// <summary>
        /// Returns gyroscope value in controller local coordinate system in degrees/sec.
        /// </summary>
        /// <param name="node">Certain node</param>
        /// <returns>Gyroscope value in controller local coordinate system in degrees/sec</returns>
        public static Vector3 GetGyro(FinchNodeType node)
        {
            return FinchCore.Gyro[(int)node];
        }

        /// <summary>
        /// Returns accelerometer value in controller local coordinate system.
        /// </summary>
        /// <param name="node">Certain node</param>
        /// <returns>Accelerometer value in controller local coordinate system in m/s2.</returns>
        public static Vector3 GetAccel(FinchNodeType node)
        {
            return FinchCore.Accel[(int)node];
        }
        #endregion

        #region ControllerElementInput
        /// <summary>
        /// Returns element pressing state.
        /// </summary>
        /// <param name="node">Certain node</param>
        /// <param name="element">Controller element</param>
        /// <returns>Is the controller element pressed</returns>
        public static bool GetPress(FinchNodeType node, FinchControllerElement element)
        {
            return FinchCore.GetEvent(node, element);
        }

        /// <summary>
        /// Returns element pressed down state.
        /// </summary>
        /// <param name="node">Certain node</param>
        /// <param name="element">Controller element</param>
        /// <returns>Was the controller element pressed down</returns>
        public static bool GetPressDown(FinchNodeType node, FinchControllerElement element)
        {
            return FinchCore.GetBeginEvent(node, element);
        }

        /// <summary>
        /// Returns element pressed up state.
        /// </summary>
        /// <param name="node">Certain node</param>
        /// <param name="element">Controller element</param>
        /// <returns>Was the controller element pressed up</returns>
        public static bool GetPressUp(FinchNodeType node, FinchControllerElement element)
        {
            return FinchCore.GetEndEvent(node, element);
        }

        /// <summary>
        /// Returns time of pressing.
        /// </summary>
        /// <param name="node">Certain node</param>
        /// <param name="element">Controller element</param>
        /// <returns>Controller element pressing time.</returns>
        public static float GetPressTime(FinchNodeType node, FinchControllerElement element)
        {
            return FinchCore.GetTimeEvents(node, element);
        }

        /// <summary>
        /// Returns touch position coordinate.
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <returns>Touch position coordinate</returns>
        public static Vector2 GetTouchAxes(FinchChirality chirality)
        {
            return FinchCore.GetTouchAxes(chirality);
        }

        /// <summary>
        /// Returns Trigger value.
        /// </summary>
        /// <param name="chirality">Controller chirality</param>
        /// <returns>Trigger value</returns>
        public static float GetTrigger(FinchChirality chirality)
        {
            return FinchCore.GetIndexTrigger(chirality);
        }
        #endregion

        /// <summary>
        /// Returns connection state.
        /// </summary>
        /// <param name="node">Certain node</param>
        /// <returns>Is certain node connected</returns>
        public static bool IsConnected(FinchNodeType node)
        {
            return FinchCore.IsNodeConnected(node);
        }

        /// <summary>
        /// Nodes batteries charge value of physical device (percents)
        /// </summary>
        /// <param name="node">Certain node</param>
        /// <returns>Charge of the battery of certain node (percents)</returns>
        public static ushort GetBatteryCharge(FinchNodeType node)
        {
            return FinchCore.GetNodeCharge(node);
        }
    }
}
