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
using Finch;

public static class NodeAngleChecker
{
    private static float angleBorder = 0.3f;

    private static readonly bool[] correctAngles = new bool[(int)FinchCore.Interop.FinchNodeType.Last];

    public static bool IsCorrectAngle
    {
        get
        {
            for (int i = 0; i < correctAngles.Length; ++i)
            {
                if (!correctAngles[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    public static void Update()
    {
        for (int i = 0; i < correctAngles.Length; ++i)
        {
            Vector3 origin = ((FinchNodeType)i == FinchNodeType.RightHand || (FinchNodeType)i == FinchNodeType.RightUpperArm) ? Vector3.right : Vector3.left;
            correctAngles[i] = !FinchInput.IsConnected((FinchNodeType)i) || Mathf.Abs((FinchCore.GetNodeRotation((FinchNodeType)i, false) * origin).y) < angleBorder;
        }
    }
}
