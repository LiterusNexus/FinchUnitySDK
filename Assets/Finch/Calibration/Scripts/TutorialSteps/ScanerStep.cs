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
using Finch;

/// <summary>
/// Calibration step at which it is determined which set of controllers will be used.
/// </summary>
public enum FinchScannerType : byte
{
    Bonded = FinchCore.Interop.FinchScannerType.Bonded,
    BA = FinchCore.Interop.FinchScannerType.BA // Bonded and Advertising, with Bonded priority.
}

/// <summary>
/// Calibration step at which it is determined which set of controllers will be used.
/// </summary>
public class ScanerStep : TutorialStep
{
    [Serializable]
    /// <summary>
    /// This structure is responsible for warnings.
    /// </summary>
    public struct Warning
    {
        /// <summary>
        /// An object that displays warnings of a scan step.
        /// </summary>
        public GameObject WarningObject;

        /// <summary>
        /// The number of controllers connected to the Finch device.
        /// </summary>
        public int ControllerCount;

        /// <summary>
        /// The number of nodes connected to the Finch Upper Arm device.
        /// </summary>
        public int UpperArmCount;
    }


    [Header("Nodes warnings")]
    /// <summary>
    /// An object that serves to render an error warning.
    /// </summary>
    public GameObject WaitingPicture;

    /// <summary>
    /// An array of objects responsible for warnings.
    /// </summary>
    public Warning[] Warnings = new Warning[0];

    [Header("Scanner options")]
    /// <summary>
    ///  Selectable scan type.
    /// </summary>
    public FinchScannerType ScannerType = FinchScannerType.BA;

    /// <summary>
    /// Visualization time at the first scan.
    /// </summary>
    public float ScannerTime = 2.0f;

    private const float internalScannerTime = 0.75f;
    private const float rescanFreezeTime = 2.0f;
    private const int thressholdRssi = -100;

    private bool firstScanEnd { get { return Time.time > firstScanEndTime; } }

    private float firstScanEndTime;
    private float endScannerTime;
    private bool scannerStepPassOnce;

    public override void Init(int id)
    {
        base.Init(id);

        if (!scannerStepPassOnce || FinchCalibration.Settings.Rescanning)
        {
            firstScanEndTime = Time.time + ScannerTime;
            FinchCore.StartScan(ScannerType, (uint)(internalScannerTime * 1000.0f), (sbyte)thressholdRssi, true);

            UpdateScannerState();
            UpdateWarnings();
        }
        else
        {
            PlayableSet.RememberNodes();
            NextStep();
        }
    }

    public void Update()
    {
        UpdatePosition();
        UpdateScannerState();
        UpdateWarnings();
        TryLoadNextStep();
    }

    private void UpdateWarnings()
    {
        WaitingPicture.SetActive(!firstScanEnd);

        int upperArmsCount = FinchCore.NodesState.GetUpperArmCount();
        int controllersCount = FinchCore.NodesState.GetControllersCount();
        bool alreadyFind = false;

        foreach (Warning i in Warnings)
        {
            bool state = i.UpperArmCount == upperArmsCount && i.ControllerCount == controllersCount && firstScanEnd;
            i.WarningObject.SetActive(state && !alreadyFind);

            alreadyFind |= state;
        }
    }

    private void UpdateScannerState()
    {
        if (Time.time > endScannerTime)
        {
            if (!Application.isMobilePlatform)
            {
                ScannerType = FinchScannerType.Bonded;
            }

            endScannerTime = Time.time + internalScannerTime + rescanFreezeTime;

            if (Application.isMobilePlatform && !FinchCore.StartScan(ScannerType, (uint)(internalScannerTime * 1000.0f), (sbyte)thressholdRssi, true))
            {
                string err = "Error start Finch scan";
                Debug.LogError(err);
                throw new Exception(err);
            }
        }
    }

    protected void TryLoadNextStep()
    {
        int upperArmsCount = FinchCore.NodesState.GetUpperArmCount();
        int controllersCount = FinchCore.NodesState.GetControllersCount();

        bool skipDueCorrectSet = (FinchCore.Settings.ControllerType == FinchControllerType.Shift ? upperArmsCount + controllersCount == 4 : controllersCount == 2);
        bool skipDuePress = false;
        bool isShift = FinchCore.Settings.ControllerType == FinchControllerType.Shift;

        foreach (FinchControllerElement i in (FinchControllerElement[])Enum.GetValues(typeof(FinchControllerElement)))
        {
            bool enoughPressing = Math.Max(FinchController.Left.GetPressTime(i), FinchController.Right.GetPressTime(i)) > FinchCalibration.Settings.TimeToCallModule;
            bool isPressing = FinchController.GetPressDown(FinchChirality.Any, i);

            skipDuePress |= i != FinchControllerElement.Touch && (isShift ? isPressing : enoughPressing);
        }

        if (skipDueCorrectSet || skipDuePress && firstScanEnd)
        {
            PlayableSet.RememberNodes();
            scannerStepPassOnce = true;
            NextStep();
        }
    }
}
