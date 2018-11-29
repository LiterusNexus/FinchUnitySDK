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
    /// Base calibration step.
    /// </summary>
    public abstract class TutorialStep : MonoBehaviour
    {
        [Header("Position solver")]
        /// <summary>
        /// Distance from head.
        /// </summary>
        public Vector3 DistanceFromHMD;

        /// <summary>
        /// Freeze y axis position.
        /// </summary>
        public bool FreezeYPosition;

        private int stepID;

        /// <summary>
        /// Call when step load.
        /// </summary>
        public virtual void Init(int id)
        {
            stepID = id;
            gameObject.SetActive(true);
            UpdatePosition();
        }

        /// <summary>
        /// Load step at calibration module.
        /// </summary>
        /// <param name="stepId">Step number</param>
        protected void LoadStep(int stepId)
        {
            FinchCalibration.LoadStep(stepId);
        }

        /// <summary>
        ///  Load next step at calibration module.
        /// </summary>
        protected void NextStep()
        {
            LoadStep(stepID + 1);
        }

        /// <summary>
        /// Update position based on head.
        /// </summary>
        protected void UpdatePosition()
        {
            Transform hmd = FinchCore.Hmd;

            Vector3 deltaX = Vector3.zero;
            Vector3 deltaY = Vector3.zero;
            Vector3 deltaZ = Vector3.zero;

            if (FreezeYPosition)
            {
                deltaX = GetDirectionWithoutY(hmd.right, DistanceFromHMD.x);
                deltaY = Vector3.up * DistanceFromHMD.y;
                deltaZ = GetDirectionWithoutY(hmd.forward, DistanceFromHMD.z);
            }
            else
            {
                deltaX = hmd.right * DistanceFromHMD.x;
                deltaY = hmd.up * DistanceFromHMD.y;
                deltaZ = hmd.forward * DistanceFromHMD.z;
            }

            transform.position = FinchCore.Hmd.position + deltaX + deltaY + deltaZ;
            transform.LookAt(hmd.position, Vector3.up);
        }

        private Vector3 GetDirectionWithoutY(Vector3 direction, float power)
        {
            direction.y = 0;
            return direction.normalized * power;
        }
    }
}
