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

public class SetChoser : MonoBehaviour
{
    public GameObject LeftHand;
    public GameObject RightHand;

    private void Start()
    {
        LeftHand.SetActive(true);
        RightHand.SetActive(false);
    }

    void Update()
    {
        FinchController controller = FinchController.Left.IsConnected ? FinchController.Left : FinchController.Right;
        FinchChirality chirality = FinchCore.GetCapacitySensor(controller.Node);

        if (chirality == FinchChirality.Left || chirality == FinchChirality.Right)
        {
            LeftHand.SetActive(chirality == FinchChirality.Left);
            RightHand.SetActive(chirality == FinchChirality.Right);
        }
    }
}
