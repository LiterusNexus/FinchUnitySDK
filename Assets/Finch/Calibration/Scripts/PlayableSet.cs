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

namespace Finch
{
    public static class PlayableSet
    {
        private static bool setBinds;

        private static int upperArmCount;
        private static int controllersCount;

        public static bool AllPlayableNodesConnected
        {
            get
            {
                return !setBinds || AllPlayableControllersConnected && AllPlayableUpperArmsConnected;
            }
        }

        private static bool AllPlayableControllersConnected
        {
            get
            {
                return controllersCount == FinchCore.NodesState.GetControllersCount();
            }
        }

        private static bool AllPlayableUpperArmsConnected
        {
            get
            {
                return upperArmCount == FinchCore.NodesState.GetUpperArmCount();
            }
        }

        public static void Init()
        {
            FinchCore.OnConnected += OnConnectNode;
        }

        public static void ResetSaveComlect()
        {
            setBinds = false;
        }

        public static void RememberNodes()
        {
            setBinds = true;
            upperArmCount = FinchCore.NodesState.GetUpperArmCount();
            controllersCount = FinchCore.NodesState.GetControllersCount();
        }

        public static void RememberNodes(int controllers, int upperArms)
        {
            setBinds = true;
            upperArmCount = upperArms;
            controllersCount = controllers;
        }

        private static void OnConnectNode(FinchNodeType node)
        {
            if (setBinds)
            {
                if (AllPlayableNodesConnected)
                {
                    setBinds = false;
                    FinchCalibration.Calibrate(CalibrationType.FullCalibration);
                }
                else
                {
                    bool excessController = (node == FinchNodeType.LeftHand || node == FinchNodeType.RightHand) && FinchCore.NodesState.GetControllersCount() > controllersCount;
                    bool excessUpperArm = (node == FinchNodeType.LeftUpperArm || node == FinchNodeType.RightUpperArm) && FinchCore.NodesState.GetUpperArmCount() > upperArmCount;

                    if (excessController || excessUpperArm)
                    {
                        FinchCore.DisconnectNode(node);
                    }
                }
            }
        }
    }
}