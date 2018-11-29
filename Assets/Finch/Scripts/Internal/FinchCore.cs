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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Finch
{
    using FinchBool = System.Byte;

    public enum FinchNodesStateType : byte
    {
        Connected = FinchCore.Interop.FinchNodesStateType.Connected,
        Correctly = FinchCore.Interop.FinchNodesStateType.Correctly
    }

    public struct FinchNodesState
    {
        public byte Flags;

        public static bool GetState(FinchNodeType node, FinchNodesStateType type, uint flag)
        {
            return (flag & (0x1 << ((int)type + (int)node * (int)FinchCore.Interop.FinchNodesStateType.Last))) != 0;
        }

        public bool GetState(FinchNodeType node, FinchNodesStateType type)
        {
            return GetState(node, type, Flags);
        }

        public int GetNodesCount(FinchNodesStateType type = FinchNodesStateType.Connected)
        {
            return GetUpperArmCount(type) + GetControllersCount(type);
        }

        public int GetUpperArmCount(FinchNodesStateType type = FinchNodesStateType.Connected)
        {
            return (GetState(FinchNodeType.LeftUpperArm, type) ? 1 : 0) + (GetState(FinchNodeType.RightUpperArm, type) ? 1 : 0);
        }

        public int GetControllersCount(FinchNodesStateType type = FinchNodesStateType.Connected)
        {
            return (GetState(FinchNodeType.LeftHand, type) ? 1 : 0) + (GetState(FinchNodeType.RightHand, type) ? 1 : 0);
        }
    }

    public static class FinchCore
    {
        public static string Version { get; private set; }
        public static FinchSettings Settings { get; private set; }
        public static FinchNodesState NodesState { get; private set; }

        public static readonly FinchBone[] Bones;
        public static readonly Dictionary<FinchBone, int> BonesIndex = new Dictionary<FinchBone, int>();

        public static Transform Hmd;
        public static Transform Origin;
        public static readonly Quaternion[] BoneRotationsTPose;
        public static readonly Quaternion[] BoneRotationsFPose;
        public static readonly Quaternion[] ControllerRotationsTPose = new Quaternion[(int)Interop.FinchChirality.Last];
        public static readonly Quaternion[] ControllerRotationsFPose = new Quaternion[(int)Interop.FinchChirality.Last];
        public static readonly Vector3[] BonePosition;
        public static readonly Vector3[] ControllerPositions = new Vector3[(int)Interop.FinchChirality.Last];
        public static readonly ushort[] ElementsBeginEvents = new ushort[(int)Interop.FinchNodeType.Last];
        public static readonly ushort[] ElementsStateEvents = new ushort[(int)Interop.FinchNodeType.Last];
        public static readonly ushort[] ElementsEndEvents = new ushort[(int)Interop.FinchNodeType.Last];
        public static readonly float[][] ElementTimeEvents;
        public static readonly Vector2[] TouchAxes = new Vector2[(int)Interop.FinchChirality.Last];
        public static readonly float[] Trigger = new float[(int)Interop.FinchChirality.Last];
        public static readonly Vector3[] Gyro = new Vector3[(int)Interop.FinchNodeType.Last];
        public static readonly Vector3[] Accel = new Vector3[(int)Interop.FinchNodeType.Last];
        public static readonly ushort[] Charge = new ushort[(int)Interop.FinchNodeType.Last];

        public static Action<FinchNodeType> OnConnected;
        public static Action<FinchNodeType> OnDisconnected;

        private static readonly Vector2[] touchesDown = new Vector2[(int)Interop.FinchChirality.Last];
        private static readonly Vector2[] touchesUp = new Vector2[(int)Interop.FinchChirality.Last];
        private static readonly float[] timeTouchDown = new float[(int)Interop.FinchChirality.Last];

        private static bool isInitialized = false;
        private static Quaternion hmdRotation = Quaternion.identity;
        private static Vector3 hmdPosition = Vector3.zero;

        static FinchCore()
        {
            Bones = new FinchBone[]
            {
                FinchBone.RightHand,
                FinchBone.LeftHand,
                FinchBone.RightUpperArm,
                FinchBone.LeftUpperArm,
                FinchBone.RightLowerArm,
                FinchBone.LeftLowerArm,
                FinchBone.Chest,
                FinchBone.Head
            };

            for (int i = 0; i < Bones.Length; ++i)
            {
                BonesIndex.Add(Bones[i], i);
            }

            BoneRotationsTPose = new Quaternion[Bones.Length];
            BoneRotationsFPose = new Quaternion[Bones.Length];
            BonePosition = new Vector3[Bones.Length];

            ElementTimeEvents = new float[(int)Interop.FinchNodeType.Last][];
            for (int i = 0; i < ElementTimeEvents.Length; ++i)
            {
                ElementTimeEvents[i] = new float[(int)Interop.FinchControllerElement.Last];
            }

            Settings = new FinchSettings();
        }

        public static void Init(FinchSettings settings)
        {
            if (Settings.ControllerType != settings.ControllerType || Settings.UpdateType != settings.UpdateType)
            {
                Exit();
            }

            if (isInitialized)
            {
                if (Settings.BodyRotationMode != settings.BodyRotationMode)
                {
                    Interop.FinchSetBodyRotationMode((Interop.FinchBodyRotationMode)settings.BodyRotationMode);
                }
                return;
            }

            Settings = settings;
            isInitialized = true;
            var error = Interop.FinchInit((Interop.FinchControllerType)settings.ControllerType, Interop.FinchPlatform.Unity3D);

            hmdPosition = Vector3.zero;
            hmdRotation = Quaternion.identity;


            if (error != Interop.FinchInitError.None)
            {
                string err = "Error initializing Finch API: " + error;
                Debug.LogError(err);
                throw new Exception(err);
            }

            Interop.FinchSetCs(new Interop.FinchVector3(0, -1, 0), new Interop.FinchVector3(0, 0, 1), new Interop.FinchVector3(1, 0, 0));
            Interop.FinchSetBodyRotationMode((Interop.FinchBodyRotationMode)settings.BodyRotationMode);

            OnConnected += OnConnectedController;
            OnDisconnected += OnDisconnectedController;

            uint size = Interop.FinchGetCoreVersion(null, 0);
            byte[] sb = new byte[size];
            Interop.FinchGetCoreVersion(sb, size);
            Version = Encoding.ASCII.GetString(sb);
        }

        public static void Exit()
        {
            isInitialized = false;
            Interop.FinchExit();
            OnConnected -= OnConnectedController;
            OnDisconnected -= OnDisconnectedController;
            Version = string.Empty;
        }

        public static FinchControllerType GetControllerType()
        {
            return Settings.ControllerType;
        }

        public static bool IsBoneAvailable(FinchBone bone)
        {
            return Interop.FinchIsBoneAvailable((Interop.FinchBone)bone) != 0;
        }

        public static Quaternion GetBoneRotation(FinchBone bone, bool fPose = true)
        {
            return (fPose ? BoneRotationsFPose : BoneRotationsTPose)[BonesIndex[bone]];
        }

        public static Quaternion GetNodeRotation(FinchNodeType node, bool fPose = true)
        {
            switch (node)
            {
                case FinchNodeType.LeftHand:
                    return GetBoneRotation(FinchBone.LeftHand, fPose);

                case FinchNodeType.RightHand:
                    return GetBoneRotation(FinchBone.RightHand, fPose);

                case FinchNodeType.LeftUpperArm:
                    return GetBoneRotation(FinchBone.LeftUpperArm, fPose);

                case FinchNodeType.RightUpperArm:
                    return GetBoneRotation(FinchBone.RightUpperArm, fPose);

                default:
                    return Quaternion.identity;
            }
        }

        public static Quaternion GetControllerRotation(FinchChirality chirality, bool fPose = true)
        {
            return (fPose ? ControllerRotationsFPose : ControllerRotationsTPose)[(int)chirality];
        }

        public static Vector3 GetBonePosition(FinchBone bone)
        {
            return BonePosition[BonesIndex[bone]];
        }

        public static Vector3 GetNodePosition(FinchNodeType node)
        {
            switch (node)
            {
                case FinchNodeType.LeftHand:
                    return GetBonePosition(FinchBone.LeftHand);

                case FinchNodeType.RightHand:
                    return GetBonePosition(FinchBone.RightHand);

                case FinchNodeType.LeftUpperArm:
                    return GetBonePosition(FinchBone.LeftUpperArm);

                case FinchNodeType.RightUpperArm:
                    return GetBonePosition(FinchBone.RightUpperArm);

                default:
                    return Vector3.zero;
            }
        }

        public static Vector3 GetControllerPosition(FinchChirality chirality)
        {
            return ControllerPositions[(int)chirality];
        }

        public static Vector3 GetBoneLocalAcceleration(FinchBone bone)
        {
            return Interop.FinchGetBoneLocalAcceleration((Interop.FinchBone)bone);
        }

        public static Vector3 GetBoneLocalAngularVelocity(FinchBone bone)
        {
            return Interop.FinchGetBoneLocalAngularVelocity((Interop.FinchBone)bone);
        }

        public static Vector3 GetBoneGlobalAcceleration(FinchBone bone)
        {
            return Interop.FinchGetBoneGlobalAcceleration((Interop.FinchBone)bone);
        }

        public static Vector3 GetBoneGlobalAngularVelocity(FinchBone bone)
        {
            return Interop.FinchGetBoneGlobalAngularVelocity((Interop.FinchBone)bone);
        }

        public static Vector2 GetTouchAxes(FinchChirality chirality)
        {
            return TouchAxes[(int)chirality];
        }

        public static Vector2 GetSwipe(FinchChirality chirality)
        {
            bool touchUp = GetEndEvent((FinchNodeType)chirality, FinchControllerElement.Touch);
            return touchUp ? (touchesUp[(int)chirality] - touchesDown[(int)chirality]) : Vector2.zero;
        }

        public static float GetSwipeTime(FinchChirality chirality)
        {
            bool touchUp = GetEndEvent((FinchNodeType)chirality, FinchControllerElement.Touch);
            return touchUp ? Time.time - timeTouchDown[(int)chirality] : 0;
        }

        public static bool GetBeginEvent(FinchNodeType node, FinchControllerElement element)
        {
            return (ElementsBeginEvents[(int)node] & (0x1 << (int)element)) != 0;
        }

        public static bool GetEvent(FinchNodeType node, FinchControllerElement element)
        {
            return (ElementsStateEvents[(int)node] & (0x1 << (int)element)) != 0;
        }

        public static bool GetEndEvent(FinchNodeType node, FinchControllerElement element)
        {
            return (ElementsEndEvents[(int)node] & (0x1 << (int)element)) != 0;
        }

        public static float GetTimeEvents(FinchNodeType node, FinchControllerElement element)
        {
            return ElementTimeEvents[(int)node][(int)element];
        }

        public static float GetIndexTrigger(FinchChirality chirality)
        {
            return Trigger[(int)chirality];
        }

        public static void Calibration(FinchChirality chirality)
        {
            Interop.FinchCalibration((Interop.FinchChirality)chirality, (Interop.FinchRecenterMode)Settings.RecenterMode);
            Interop.FinchRecenter((Interop.FinchChirality)chirality, (Interop.FinchRecenterMode)Settings.RecenterMode);
        }

        public static void ResetCalibration(FinchChirality chirality)
        {
            Interop.FinchResetCalibration((Interop.FinchChirality)chirality);
        }

        public static FinchBodyRotationMode GetBodyRotationMode()
        {
            return Settings.BodyRotationMode;
        }

        public static void SetBodyRotationMode(FinchBodyRotationMode mode)
        {
            Settings.BodyRotationMode = mode;
            Interop.FinchSetBodyRotationMode((Interop.FinchBodyRotationMode)mode);
        }

        public static FinchUpdateType GetUpdateType()
        {
            return Settings.UpdateType;
        }

        public static void Update()
        {
            UpdateHmdPosition(Origin, Hmd.position);
            UpdateHmdRotation(Origin, Hmd.rotation);

            Interop.FinchUpdateError error = Interop.FinchUpdateError.Unknown;
            switch (Settings.UpdateType)
            {
                case FinchUpdateType.HmdRotation:
                    error = Interop.FinchHmdRotationUpdate(hmdRotation);
                    break;

                case FinchUpdateType.HmdTransform:
                    error = Interop.FinchHmdTransformUpdate(hmdRotation, hmdPosition);
                    break;

                case FinchUpdateType.Internal:
                    error = Interop.FinchUpdate();
                    break;
            }

            if (error == Interop.FinchUpdateError.None)
            {
                UpdateConnectionStates();
                UpdateControllerState();
            }
            else
            {
                string err = "Error update Finch data: " + error;
                Debug.LogError(err);
                throw new Exception(err);
            }
        }

        public static bool StartScan(FinchScannerType scanner, uint timeMs, sbyte rssiThreshold, bool autoConnect)
        {
            return Interop.FinchStartScan((Interop.FinchScannerType)scanner, timeMs, rssiThreshold, (byte)(autoConnect ? 1 : 0)) != 0;
        }

        public static byte StopScan(bool autoConnect)
        {
            return Interop.FinchStopScan((byte)(autoConnect ? 1 : 0));
        }

        public static bool ConnectNode(FinchNodeType node)
        {
            return Interop.FinchConnectNode((Interop.FinchNodeType)node) == Interop.FinchIOError.None;
        }

        public static void DisconnectNode(FinchNodeType node)
        {
            Interop.FinchDisconnectNode((Interop.FinchNodeType)node, 0);
        }

        public static FinchNodesState GetNodesState()
        {
            return NodesState;
        }

        public static bool IsNodeConnected(FinchNodeType node)
        {
            return NodesState.GetState(node, FinchNodesStateType.Connected);
        }

        public static bool IsNodeDataCorrectly(FinchNodeType node)
        {
            return NodesState.GetState(node, FinchNodesStateType.Correctly);
        }

        public static string GetNodeName(FinchNodeType node)
        {
            uint size = Interop.FinchGetNodeName((Interop.FinchNodeType)node, null, 0);
            byte[] sb = new byte[size];
            Interop.FinchGetNodeName((Interop.FinchNodeType)node, sb, size);
            return Encoding.ASCII.GetString(sb);
        }

        public static string GetNodeAddress(FinchNodeType node)
        {
            uint size = Interop.FinchGetNodeAddress((Interop.FinchNodeType)node, null, 0);
            byte[] sb = new byte[size];
            Interop.FinchGetNodeAddress((Interop.FinchNodeType)node, sb, size);
            return Encoding.ASCII.GetString(sb);
        }

        public static string GetNodeManufacturerName(FinchNodeType node)
        {
            uint size = Interop.FinchGetNodeManufacturerName((Interop.FinchNodeType)node, null, 0);
            byte[] sb = new byte[size];
            Interop.FinchGetNodeManufacturerName((Interop.FinchNodeType)node, sb, size);
            return Encoding.ASCII.GetString(sb);
        }

        public static string GetNodeModelNumber(FinchNodeType node)
        {
            uint size = Interop.FinchGetNodeModelNumber((Interop.FinchNodeType)node, null, 0);
            byte[] sb = new byte[size];
            Interop.FinchGetNodeModelNumber((Interop.FinchNodeType)node, sb, size);
            return Encoding.ASCII.GetString(sb);
        }

        public static string GetNodeSerialNumber(FinchNodeType node)
        {
            uint size = Interop.FinchGetNodeSerialNumber((Interop.FinchNodeType)node, null, 0);
            byte[] sb = new byte[size];
            Interop.FinchGetNodeSerialNumber((Interop.FinchNodeType)node, sb, size);
            return Encoding.ASCII.GetString(sb);
        }

        public static string GetNodeHardwareRevision(FinchNodeType node)
        {
            uint size = Interop.FinchGetNodeHardwareRevision((Interop.FinchNodeType)node, null, 0);
            byte[] sb = new byte[size];
            Interop.FinchGetNodeHardwareRevision((Interop.FinchNodeType)node, sb, size);
            return Encoding.ASCII.GetString(sb);
        }

        public static string GetNodeFirmwareRevision(FinchNodeType node)
        {
            uint size = Interop.FinchGetNodeFirmwareRevision((Interop.FinchNodeType)node, null, 0);
            byte[] sb = new byte[size];
            Interop.FinchGetNodeFirmwareRevision((Interop.FinchNodeType)node, sb, size);
            return Encoding.ASCII.GetString(sb);
        }

        public static string GetNodeSoftwareRevision(FinchNodeType node)
        {
            uint size = Interop.FinchGetNodeSoftwareRevision((Interop.FinchNodeType)node, null, 0);
            byte[] sb = new byte[size];
            Interop.FinchGetNodeSoftwareRevision((Interop.FinchNodeType)node, sb, size);
            return Encoding.ASCII.GetString(sb);
        }

        public static byte GetNodeVendorIDSource(FinchNodeType node)
        {
            return Interop.FinchGetNodeVendorIDSource((Interop.FinchNodeType)node);
        }

        public static ushort GetNodeVendorID(FinchNodeType node)
        {
            return Interop.FinchGetNodeVendorID((Interop.FinchNodeType)node);
        }

        public static ushort GetNodeProductID(FinchNodeType node)
        {
            return Interop.FinchGetNodeProductID((Interop.FinchNodeType)node);
        }

        public static ushort GetNodeProductVersion(FinchNodeType node)
        {
            return Interop.FinchGetNodeProductVersion((Interop.FinchNodeType)node);
        }

        public static ushort GetNodeCharge(FinchNodeType node)
        {
            return Charge[(int)node];
        }

        public static ulong GetNodeTime(FinchNodeType node)
        {
            return Interop.FinchGetNodeTime((Interop.FinchNodeType)node);
        }

        public static FinchChirality GetCapacitySensor(FinchNodeType node)
        {
            bool leftSensor = GetEvent(node, (FinchControllerElement)Interop.FinchControllerElement.LeftProximity);
            bool rightSensor = GetEvent(node, (FinchControllerElement)Interop.FinchControllerElement.RightProximity);

            if (leftSensor && rightSensor)
            {
                return FinchChirality.Both;
            }

            if (leftSensor)
            {
                return FinchChirality.Left;
            }

            if (rightSensor)
            {
                return FinchChirality.Right;
            }

            return FinchChirality.Unknown;
        }

        public static bool GetLedState(FinchNodeType node)
        {
            return GetEvent(node, (FinchControllerElement)Interop.FinchControllerElement.IsLedOn);
        }

        public static bool SetLedState(FinchNodeType node, bool on)
        {
            byte[] cmd = new byte[] { 0, 7, (byte)(on ? 6 : 7) };
            return SendDataToNode(node, cmd);
        }

        public static FinchChirality GetLedChirality(FinchNodeType node)
        {
            return GetEvent(node, (FinchControllerElement)Interop.FinchControllerElement.LedChirality) ? FinchChirality.Left : FinchChirality.Right;
        }

        public static bool SetLedChirality(FinchNodeType node, FinchChirality chirality)
        {
            byte[] cmd = new byte[] { 0, (byte)(chirality == FinchChirality.Left ? 8 : 9) };
            return SendDataToNode(node, cmd);
        }

        public static void SwapNodes(FinchNodeType firstNode, FinchNodeType secondNode)
        {
            Interop.FinchSwapNodes((Interop.FinchNodeType)firstNode, (Interop.FinchNodeType)secondNode);
            byte flags = NodesState.Flags;
            flags = (byte)SwapBits(flags,
                                   (int)firstNode * (int)Interop.FinchNodesStateType.Last + (int)Interop.FinchNodesStateType.Connected,
                                   (int)secondNode * (int)Interop.FinchNodesStateType.Last + (int)Interop.FinchNodesStateType.Connected);
            flags = (byte)SwapBits(flags,
                                   (int)firstNode * (int)Interop.FinchNodesStateType.Last + (int)Interop.FinchNodesStateType.Correctly,
                                   (int)secondNode * (int)Interop.FinchNodesStateType.Last + (int)Interop.FinchNodesStateType.Correctly);
            NodesState = new FinchNodesState { Flags = flags };
            Swap(ref ElementsBeginEvents[(int)firstNode], ref ElementsBeginEvents[(int)secondNode]);
            Swap(ref ElementsStateEvents[(int)firstNode], ref ElementsStateEvents[(int)secondNode]);
            Swap(ref ElementsEndEvents[(int)firstNode], ref ElementsEndEvents[(int)secondNode]);
            Swap(ref ElementTimeEvents[(int)firstNode], ref ElementTimeEvents[(int)secondNode]);

            Update();
        }

        public static bool SendDataToNode(FinchNodeType node, byte[] data)
        {
            return Interop.FinchSendDataToNode((Interop.FinchNodeType)node, data, (uint)data.Length) == Interop.FinchIOError.None;
        }

        public static bool HapticPulse(FinchNodeType node, ushort millisecond)
        {
            byte[] package = new byte[] { (byte)(Mathf.Clamp(millisecond, 50.0f, 2550.0f) * 0.1f), 50 };
            return SendDataToNode(node, package);
        }

        public static bool HapticPulseByPattern(FinchNodeType node, params VibrationPackage[] pattern)
        {
            int length = pattern.Length;
            if (length > 10 || length < 0)
            {
                throw new ArgumentException("The number of arguments must be not more than 10.");
            }

            byte[] package = new byte[length * 2];
            for (int i = 0; i < length; ++i)
            {
                package[i * 2] = (byte)(Mathf.Clamp(pattern[i].Time, 50.0f, 2550.0f) * 0.1f);
                package[i * 2 + 1] = (byte)(Mathf.Clamp(pattern[i].Speed, -1.0f, 1.0f) * 50);
            }

            return SendDataToNode(node, package);
        }

        public static void BindsControllers()
        {
            FinchController left = FinchController.Left;
            FinchController right = FinchController.Right;

            bool leftIncorrect = !left.IsConnected || GetCapacitySensor(left.Node) == FinchChirality.Right;
            bool rightIncorrect = !right.IsConnected || GetCapacitySensor(right.Node) == FinchChirality.Left;
            bool anyConnected = left.IsConnected || right.IsConnected;

            if (leftIncorrect && rightIncorrect && anyConnected)
            {
                Interop.FinchSwapCalibrations(1, 0);
                SwapNodes(FinchNodeType.LeftHand, FinchNodeType.RightHand);
            }
        }

        public static void BindsUpperArms()
        {
            if (FinchInput.IsConnected(FinchNodeType.LeftUpperArm) && !FinchController.Left.IsConnected)
            {
                DisconnectNode(FinchNodeType.LeftUpperArm);
            }

            if (FinchInput.IsConnected(FinchNodeType.RightUpperArm) && !FinchController.Right.IsConnected)
            {
                DisconnectNode(FinchNodeType.RightUpperArm);
            }

            if (NodesState.GetUpperArmCount() < NodesState.GetControllersCount() || Settings.ControllerType == FinchControllerType.Dash)
            {
                DisconnectNode(FinchNodeType.LeftUpperArm);
                DisconnectNode(FinchNodeType.RightUpperArm);
            }
        }

        private static void UpdateConnectionStates()
        {
            byte prevFlags = NodesState.Flags;
            byte flags = Interop.FinchGetNodesState();
            NodesState = new FinchNodesState { Flags = flags };

            UpdateConnectionState(flags, prevFlags, FinchNodeType.RightHand);
            UpdateConnectionState(flags, prevFlags, FinchNodeType.LeftHand);
            UpdateConnectionState(flags, prevFlags, FinchNodeType.RightUpperArm);
            UpdateConnectionState(flags, prevFlags, FinchNodeType.LeftUpperArm);
        }

        private static void UpdateConnectionState(byte flag, byte prevFlag, FinchNodeType node)
        {
            bool prevState = FinchNodesState.GetState(node, FinchNodesStateType.Connected, prevFlag);
            bool state = FinchNodesState.GetState(node, FinchNodesStateType.Connected, flag);

            if (state && !prevState)
            {
                OnConnected.Invoke(node);
            }

            if (!state && prevState)
            {
                OnDisconnected.Invoke(node);
            }
        }

        private static void OnConnectedController(FinchNodeType node)
        {
            if (node == FinchNodeType.RightHand || node == FinchNodeType.LeftHand)
            {
                Action OnConnected = FinchController.GetController((FinchChirality)node).OnConnected;
                if (OnConnected != null)
                {
                    OnConnected.Invoke();
                }
            }
        }

        private static void OnDisconnectedController(FinchNodeType node)
        {
            if (node == FinchNodeType.RightHand || node == FinchNodeType.LeftHand)
            {
                Action OnDisconnected = FinchController.GetController((FinchChirality)node).OnConnected;
                if (OnDisconnected != null)
                {
                    OnDisconnected.Invoke();
                }
            }
        }

        private static void UpdateControllerState()
        {
            for (int i = 0; i < (int)Interop.FinchNodeType.Last; ++i)
            {
                var node = (Interop.FinchNodeType)i;
                var bone = (Interop.FinchBone)Bones[i];
                ElementsBeginEvents[i] = Interop.FinchGetEvents(node, Interop.FinchEventType.Begin);
                ElementsStateEvents[i] = Interop.FinchGetEvents(node, Interop.FinchEventType.Process);
                ElementsEndEvents[i] = Interop.FinchGetEvents(node, Interop.FinchEventType.End);

                for (int j = 0; j < (int)Interop.FinchControllerElement.Last; ++j)
                {
                    bool pressed = ((ElementsStateEvents[i] & (0x1 << j)) != 0);
                    ElementTimeEvents[i][j] = pressed ? ElementTimeEvents[i][j] + Time.deltaTime : 0;
                }

                Gyro[i] = Interop.FinchGetBoneLocalAngularVelocity(bone);
                Accel[i] = Interop.FinchGetBoneLocalAcceleration(bone);
                Charge[i] = Interop.FinchGetNodeCharge(node);
            }

            for (int i = 0; i < Bones.Length; ++i)
            {
                var bone = (Interop.FinchBone)Bones[i];
                BonePosition[i] = GetPosition(Origin, Interop.FinchGetBonePosition(bone));
                BoneRotationsTPose[i] = GetRotation(Origin, Interop.FinchGetBoneRotation(bone, 0));
                BoneRotationsFPose[i] = GetRotation(Origin, Interop.FinchGetBoneRotation(bone, 1));
            }

            if (Settings.ControllerType != FinchControllerType.Dash)
            {
                BonePosition[BonesIndex[FinchBone.Head]] = GetPosition(Origin, Interop.FinchGetBonePosition(Interop.FinchBone.RightEye));
            }

            for (int i = 0; i < (int)Interop.FinchChirality.Last; ++i)
            {
                var chirality = (Interop.FinchChirality)i;

                if (GetEndEvent((FinchNodeType)i, FinchControllerElement.Touch))
                {
                    touchesUp[i] = TouchAxes[i];
                }

                TouchAxes[i] = Interop.FinchGetTouchAxes(chirality);
                Trigger[i] = Interop.FinchGetIndexTrigger(chirality);
                ControllerRotationsTPose[i] = GetRotation(Origin, Interop.FinchGetControllerRotation(chirality, 0));
                ControllerRotationsFPose[i] = GetRotation(Origin, Interop.FinchGetControllerRotation(chirality, 1));
                ControllerPositions[i] = GetPosition(Origin, Interop.FinchGetControllerPosition(chirality));

                if (GetBeginEvent((FinchNodeType)i, FinchControllerElement.Touch))
                {
                    touchesDown[i] = TouchAxes[i];
                    timeTouchDown[i] = Time.time;
                }
            }
        }

        private static void UpdateHmdPosition(Transform root, Vector3 position)
        {
            Vector3 newHmdPosition = Vector3.zero;

            if (root == null || root.lossyScale.x == 0)
            {
                newHmdPosition = position;
            }
            else
            {
                newHmdPosition = Quaternion.Inverse(root.rotation) * ((position - root.position) / root.lossyScale.x);
            }

            for (int i = 0; i < 3; ++i)
            {
                if (float.IsNaN(newHmdPosition[i]))
                {
                    return;
                }
            }

            hmdPosition = newHmdPosition;
            if (Settings.UpdateType != FinchUpdateType.HmdTransform)
            {
                Hmd.transform.position = BonePosition[BonesIndex[FinchBone.Head]];
            }
        }

        private static void UpdateHmdRotation(Transform root, Quaternion rotation)
        {
            Quaternion newHmdRotation = Quaternion.identity;

            if (root == null)
            {
                newHmdRotation = rotation;
            }
            else
            {
                newHmdRotation = Quaternion.Inverse(root.rotation) * rotation;
            }

            for (int i = 0; i < 4; ++i)
            {
                if (float.IsNaN(newHmdRotation[i]))
                {
                    return;
                }
            }

            hmdRotation = newHmdRotation;
        }

        private static Vector3 GetPosition(Transform origin, Vector3 position)
        {
            if (origin == null || origin.lossyScale.x == 0)
            {
                return position;
            }

            return origin.position + origin.localScale.x * (origin.rotation * position);
        }

        private static Quaternion GetRotation(Transform origin, Quaternion rotation)
        {
            if (origin == null)
            {
                return rotation;
            }

            return origin.rotation * rotation;
        }

        private static int SwapBits(int n, int p, int q)
        {
            if ((((n & (1 << p)) >> p) ^ ((n & (1 << q)) >> q)) == 1)
            {
                n ^= (1 << p);
                n ^= (1 << q);
            }

            return n;
        }

        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static class Interop
        {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            private const string LibPath = "FinchCore";
#elif UNITY_IOS
            private const string LibPath = "__Internal";
#endif

            [StructLayout(LayoutKind.Sequential)]
            public struct FinchVector2
            {
                public float X;
                public float Y;

                public FinchVector2(float x, float y)
                {
                    X = x;
                    Y = y;
                }

                public FinchVector2(Vector2 v)
                {
                    X = v.x;
                    Y = v.y;
                }

                public static implicit operator FinchVector2(Vector2 v) { return new FinchVector2(v.x, v.y); }

                public static implicit operator Vector2(FinchVector2 v) { return new Vector2(v.X, v.Y); }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct FinchVector3
            {
                public float X;
                public float Y;
                public float Z;

                public FinchVector3(float x, float y, float z)
                {
                    X = x;
                    Y = y;
                    Z = z;
                }

                public FinchVector3(Vector3 v)
                {
                    X = v.x;
                    Y = v.y;
                    Z = v.z;
                }

                public static implicit operator FinchVector3(Vector3 v) { return new FinchVector3(v.x, v.y, v.z); }

                public static implicit operator Vector3(FinchVector3 v) { return new Vector3(v.X, v.Y, v.Z); }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct FinchQuaternion
            {
                public float X;
                public float Y;
                public float Z;
                public float W;

                public FinchQuaternion(float x, float y, float z, float w)
                {
                    X = x;
                    Y = y;
                    Z = z;
                    W = w;
                }

                public FinchQuaternion(Quaternion q)
                {
                    X = q.x;
                    Y = q.y;
                    Z = q.z;
                    W = q.w;
                }

                public static implicit operator FinchQuaternion(Quaternion q) { return new FinchQuaternion(q.x, q.y, q.z, q.w); }

                public static implicit operator Quaternion(FinchQuaternion q) { return new Quaternion(q.X, q.Y, q.Z, q.W); }
            }

            public enum FinchInitError : byte
            {
                None,
                AlreadyInitialized,
                NotInitialized,
                IllegalArgument,
                RuntimeError,

                Last,
                Unknown = Last
            }

            public enum FinchUpdateError : byte
            {
                None,
                NotInitialized,
                IllegalArgument,
                RuntimeError,

                Last,
                Unknown = Last
            }

            public enum FinchIOError : byte
            {
                None,
                NotInitialized,
                IllegalArgument,
                RuntimeError,

                Last,
                Unknown = Last
            }

            public enum FinchNodeType : byte
            {
                RightHand,
                LeftHand,
                RightUpperArm,
                LeftUpperArm,

                Last,
                Unknown = Last
            }

            public enum FinchChirality : byte
            {
                Right,
                Left,
                Both = 255,

                Last = 2,
                Unknown = Last
            }

            public enum FinchControllerType : byte
            {
                Hand,
                Shift,
                WaveVRDash,
                Dash,
                UniversalController,

                Last,
                Unknown = Last
            }

            public enum FinchNodesStateType : byte
            {
                Connected,
                Correctly,

                Last,
                Unknown = Last
            }

            public enum FinchControllerElement : byte
            {
                ButtonZero = 7,
                ButtonOne = 0,
                ButtonTwo = 1,
                ButtonThree = 2,
                ButtonFour = 3,
                ButtonThumb = 9,
                Touch = 15,
                IndexTrigger = 4,
                ButtonGrip = 5,
                LeftProximity = 6,
                RightProximity = 8,
                LedChirality = 10,
                IsLedOn = 12,
                IsTouchpadNotAvailable = 11,

                Last = 16,
                Unknown = Last
            }

            public enum FinchEventType : byte
            {
                Begin,
                Process,
                End,

                Last,
                Unknown = Last
            }

            public enum FinchRecenterMode : byte
            {
                Forward,
                HmdRotation,

                Last,
                Unknown = Last
            }

            public enum FinchBodyRotationMode : byte
            {
                None,
                ShoulderRotation,
                HandRotation,
                HandMotion,
                HmdRotation,
                ShoulderRotationWithReachout,
                FullBodyRotation,

                Last,
                Unknown = Last
            }

            public enum FinchAxisCalibrationStep : byte
            {
                One,
                Two,
                Three,
                Four,

                Last,
                Unknown = Last
            }

            public enum FinchBone : byte
            {
                Hips,
                LeftUpperLeg,
                RightUpperLeg,
                LeftLowerLeg,
                RightLowerLeg,
                LeftFoot,
                RightFoot,
                Spine,
                Chest,
                Neck,
                Head,
                LeftShoulder,
                RightShoulder,
                LeftUpperArm,
                RightUpperArm,
                LeftLowerArm,
                RightLowerArm,
                LeftHand,
                RightHand,
                LeftToes,
                RightToes,
                LeftEye,
                RightEye,
                Jaw,
                LeftThumbProximal,
                LeftThumbIntermediate,
                LeftThumbDistal,
                LeftIndexProximal,
                LeftIndexIntermediate,
                LeftIndexDistal,
                LeftMiddleProximal,
                LeftMiddleIntermediate,
                LeftMiddleDistal,
                LeftRingProximal,
                LeftRingIntermediate,
                LeftRingDistal,
                LeftLittleProximal,
                LeftLittleIntermediate,
                LeftLittleDistal,
                RightThumbProximal,
                RightThumbIntermediate,
                RightThumbDistal,
                RightIndexProximal,
                RightIndexIntermediate,
                RightIndexDistal,
                RightMiddleProximal,
                RightMiddleIntermediate,
                RightMiddleDistal,
                RightRingProximal,
                RightRingIntermediate,
                RightRingDistal,
                RightLittleProximal,
                RightLittleIntermediate,
                RightLittleDistal,

                LeftHandCenter,
                LeftThumbTip,
                LeftIndexTip,
                LeftMiddleTip,
                LeftRingTip,
                LeftLittleTip,

                RightHandCenter,
                RightThumbTip,
                RightIndexTip,
                RightMiddleTip,
                RightRingTip,
                RightLittleTip,

                LeftClavicleBase,
                RightClavicleBase,
                LeftClavicleOffset,
                RightClavicleOffset,

                Last,
                Unknown = Last
            }

            public enum FinchPlatform : byte
            {
                External,
                Internal,
                Unity3D,
                UnrealEngine4,
                WaveVR,

                Last,
                Unknown = Last
            }

            public enum FinchScannerType : byte
            {
                None,
                Bonded,
                BA, // Bonded and Advertising, with Bonded priority.
                AB, // Advertising and Bonded, with Advertising priority.

                Last,
                Unknown = Last
            }

            public enum FinchUpdateType : byte
            {
                Internal,
                HmdRotation,
                HmdTransform,

                Last,
                Unknown = Last
            }

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetCoreVersion([Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchInitError FinchInit(FinchControllerType controller, FinchPlatform platform);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchExit();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchControllerType FinchGetControllerType();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchBool FinchIsBoneAvailable(FinchBone bone);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchQuaternion FinchGetBoneRotation(FinchBone bone, FinchBool fPose);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchQuaternion FinchGetControllerRotation(FinchChirality chirality, FinchBool fPose);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchVector3 FinchGetBonePosition(FinchBone bone);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchVector3 FinchGetControllerPosition(FinchChirality chirality);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchVector3 FinchGetBoneLocalAcceleration(FinchBone bone);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchVector3 FinchGetBoneLocalAngularVelocity(FinchBone bone);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchVector3 FinchGetBoneGlobalAcceleration(FinchBone bone);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchVector3 FinchGetBoneGlobalAngularVelocity(FinchBone bone);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchVector2 FinchGetTouchAxes(FinchChirality chirality);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern ushort FinchGetEvents(FinchNodeType node, FinchEventType type);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern float FinchGetIndexTrigger(FinchChirality chirality);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetCs(FinchVector3 x, FinchVector3 y, FinchVector3 z);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetDefaultCs();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetRootOffset(FinchVector3 v);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchRecenter(FinchChirality chirality, FinchRecenterMode mode);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchRecenterByDirection(FinchChirality chirality, FinchQuaternion targetOrientation);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchRecenterByControllersPositions(FinchVector3 leftControllerPositionRelativelyToHmd, FinchVector3 rightControllerPositionRelativelyToHmd);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchCalibration(FinchChirality chirality, FinchRecenterMode mode);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchResetCalibration(FinchChirality chirality);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchQuaternion FinchGetNodeRawRotation(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchQuaternion FinchGetCalibrationAdjust(FinchNodeType node, FinchBool isPre, FinchBool useDefaultCS);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetCalibrationAdjust(FinchNodeType node, FinchQuaternion calibrationQ, FinchBool isPre, FinchBool useDefaultCS);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchBool FinchIsUpperArmReverted(FinchChirality chirality);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchRevertUpperArm(FinchChirality chirality);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchMorphCalibrationsToOuter();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSwapCalibrations(FinchBool swapHands, FinchBool swapUpperArms);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchAxisCalibration(FinchChirality chirality, FinchAxisCalibrationStep step);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern float FinchGetBoneLength(FinchBone bone);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetBoneLength(FinchBone bone, float length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern float FinchGetEyesForwardDistance();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetEyesForwardDistance(float distance);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern float FinchGetControllerWidth();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetControllerWidth(float width);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern float FinchGetNeckLeanAngle();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetNeckLeanAngle(float angle);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchBodyRotationMode FinchGetBodyRotationMode();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetBodyRotationMode(FinchBodyRotationMode mode);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetCalibrations([Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetCalibrations(string f);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetMathSettings([Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSetMathSettings(string f);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchUpdateError FinchUpdate();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchUpdateError FinchHmdRotationUpdate(FinchQuaternion qhmd);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchUpdateError FinchHmdTransformUpdate(FinchQuaternion qhmd, FinchVector3 phmd);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchUpdateError FinchExternUpdate(
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] rightHand,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] leftHand,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] rightUpperArm,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] leftUpperArm);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchUpdateError FinchExternHmdRotationUpdate(
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] rightHand,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] leftHand,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] rightUpperArm,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] leftUpperArm,
                FinchQuaternion qhmd);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchUpdateError FinchExternHmdTransformUpdate(
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] rightHand,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] leftHand,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] rightUpperArm,
                [In] [MarshalAs(UnmanagedType.LPArray)] byte[] leftUpperArm,
                FinchQuaternion qhmd,
                FinchVector3 phmd);


            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchBool FinchStartScan(FinchScannerType scanner, uint timeMs, sbyte rssiThreshold, FinchBool autoConnect);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern byte FinchStopScan(FinchBool autoConnect);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchIOError FinchConnectNode(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchDisconnectNode(FinchNodeType node, FinchBool unpair);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern byte FinchGetNodesState();

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchBool FinchIsNodeConnected(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchBool FinchIsNodeDataCorrectly(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetNodeName(FinchNodeType node, [Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetNodeAddress(FinchNodeType node, [Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetNodeManufacturerName(FinchNodeType node, [Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetNodeModelNumber(FinchNodeType node, [Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetNodeSerialNumber(FinchNodeType node, [Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetNodeHardwareRevision(FinchNodeType node, [Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetNodeFirmwareRevision(FinchNodeType node, [Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetNodeSoftwareRevision(FinchNodeType node, [Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern byte FinchGetNodeVendorIDSource(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern ushort FinchGetNodeVendorID(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern ushort FinchGetNodeProductID(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern ushort FinchGetNodeProductVersion(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern byte FinchGetNodeCharge(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern ulong FinchGetNodeTime(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern uint FinchGetNodeRawData(FinchNodeType node, [Out] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSwapNodes(FinchNodeType first, FinchNodeType second);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchSuspendNode(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern void FinchResumeNode(FinchNodeType node);

            [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern FinchIOError FinchSendDataToNode(FinchNodeType node, [In] [MarshalAs(UnmanagedType.LPArray)] byte[] data, uint length);
        }
    }
}
