
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
using System;

namespace Finch
{
    /// <summary>
    /// Describes type of controller.
    /// </summary>
    public enum FinchControllerType : byte
    {
        Dash = FinchCore.Interop.FinchControllerType.Dash,
        Shift = FinchCore.Interop.FinchControllerType.UniversalController
    }

    /// <summary>
    /// Describes type of update.
    /// </summary>
    public enum FinchUpdateType : byte
    {
        /// <summary>
        /// No data from camera are used for tracking, neither the rotation nor the position.
        /// </summary>
        Internal = FinchCore.Interop.FinchUpdateType.Internal,

        /// <summary>
        /// Only rotation is used for tracking.
        /// </summary>
        HmdRotation = FinchCore.Interop.FinchUpdateType.HmdRotation,

        /// <summary>
        /// Both position and rotation are used for tracking. Use this option if you have headset that allows head tracking (for example, HTC Vive).
        /// </summary>
        HmdTransform = FinchCore.Interop.FinchUpdateType.HmdTransform
    }

    /// <summary>
    /// Describes recenter mode.
    /// </summary>
    public enum FinchRecenterMode : byte
    {
        Forward = FinchCore.Interop.FinchRecenterMode.Forward,
        HmdRotation = FinchCore.Interop.FinchRecenterMode.HmdRotation
    }

    /// <summary>
    /// Describes body rotation mode.
    /// </summary>
    public enum FinchBodyRotationMode : byte
    {
        ShoulderRotation = FinchCore.Interop.FinchBodyRotationMode.ShoulderRotation,
        HmdRotation = FinchCore.Interop.FinchBodyRotationMode.HmdRotation
    }

    /// <summary>
    /// Finch settings.
    /// </summary>
    [Serializable]
    public class FinchSettings
    {
        /// <summary>
        /// Controller type.
        /// </summary>
        public FinchControllerType ControllerType = FinchControllerType.Shift;

        /// <summary>
        /// Head update type.
        /// </summary>
        public FinchUpdateType UpdateType = FinchUpdateType.HmdRotation;

        /// <summary>
        /// Body rotation type.
        /// </summary>
        public FinchBodyRotationMode BodyRotationMode = FinchBodyRotationMode.ShoulderRotation;

        /// <summary>
        /// Recentering mode type.
        /// </summary>
        public FinchRecenterMode RecenterMode
        {
            get { return UpdateType == FinchUpdateType.Internal ? FinchRecenterMode.Forward : FinchRecenterMode.HmdRotation; }
        }
    }

    /// <summary>
    /// Provides finch data update.
    /// </summary>
    public class Finch : MonoBehaviour
    {
        /// <summary>
        /// Provides versioning information for the Finch SDK for Unity.
        /// </summary>
        public readonly string Version = "0.4.3";

        /// <summary>
        /// Head position transform.
        /// </summary>
        [Tooltip("Leave this field empty if you want to use data of Camera.main")]
        public Transform Hmd;

        /// <summary>
        /// Character root transform.
        /// </summary>
        [Tooltip("Leave this field empty if you want to use global coordinates")]
        public Transform Root;

        /// <summary>
        /// Current Finch settings.
        /// </summary>
        public FinchSettings Settings = new FinchSettings();

        private void Awake()
        {
            if (Settings.UpdateType != FinchUpdateType.HmdTransform)
            {
                UnityEngine.XR.InputTracking.disablePositionalTracking = true;
            }

            Application.targetFrameRate = 9000;

            FinchCore.Hmd = Hmd ?? Camera.main.transform;
            FinchCore.Origin = Root;
            FinchCore.Init(Settings);
        }

        private void LateUpdate()
        {
            FinchCore.Update();
        }

        private void OnApplicationQuit()
        {
            FinchCore.Exit();
        }
    }
}
