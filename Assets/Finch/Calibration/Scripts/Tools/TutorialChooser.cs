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
    public class TutorialChooser : MonoBehaviour
    {
        [Serializable]
        public struct TutorialObject
        {
            public GameObject Tutorial;
            public FinchChirality Chirality;
        }

        public TutorialObject[] Tutorials = new TutorialObject[0];

        void Update()
        {
            FinchChirality chirality = GetChirality();

            foreach (TutorialObject i in Tutorials)
            {
                i.Tutorial.SetActive(chirality == i.Chirality || i.Chirality == FinchChirality.Any && (chirality == FinchChirality.Left || chirality == FinchChirality.Right));
            }
        }

        private FinchChirality GetChirality()
        {
            bool leftController = FinchController.Left.IsConnected;
            bool rightController = FinchController.Right.IsConnected;

            if (leftController && rightController)
            {
                return FinchChirality.Both;
            }

            if (leftController)
            {
                return FinchChirality.Left;
            }

            if (rightController)
            {
                return FinchChirality.Right;
            }

            return FinchChirality.Unknown;
        }
    }
}
