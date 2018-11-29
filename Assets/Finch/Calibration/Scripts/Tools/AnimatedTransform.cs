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

using System.Collections.Generic;
using UnityEngine;
using Finch;

public class AnimatedTransform : MonoBehaviour
{
    [System.Serializable]
    public class Point
    {
        [Range(0, 1)]
        public float X;

        [Range(0, 1)]
        public float Y;

        public static implicit operator Vector2(Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static implicit operator Point(Vector2 v)
        {
            return new Point() { X = v.x, Y = v.y };
        }
    }

    [Header("Sprites")]
    public SpriteRenderer[] Sprites = new SpriteRenderer[0];

    [Header("Graphic")]
    public List<Point> Points = new List<Point>();

    [Header("Positions")]
    public float MinY;
    public float MaxY;

    [Header("Timers")]
    public float TimeToMove = 2.0f;

    private float startTime;

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        startTime = Time.time;
        transform.localPosition = new Vector3(transform.localPosition.x, MinY, transform.localPosition.z);

        foreach (var i in Sprites)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        }
    }

    private void Update()
    {
        float y = Mathf.Lerp(MinY, MaxY, GetPoint(Points, (Time.time - startTime) / TimeToMove).y);
        transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);

        bool bothPressed = FinchCore.NodesState.GetControllersCount() == 2 && FinchController.GetPressDown(FinchChirality.Both, FinchControllerElement.HomeButton);
        bool anyPressed = FinchCore.NodesState.GetControllersCount() != 2 && FinchController.GetPressDown(FinchChirality.Any, FinchControllerElement.HomeButton);

        if (bothPressed || anyPressed)
        {
            startTime = Time.time;
        }
    }

    private Vector2 GetPoint(List<Point> points, float time)
    {
        if (points.Count > 1)
        {
            List<Point> newPoints = new List<Point>();

            for (int i = 0; i < points.Count - 1; i++)
            {
                newPoints.Add(Vector2.Lerp(points[i], points[i + 1], time));
            }

            return GetPoint(newPoints, time);
        }
        else
        {
            return points[0];
        }
    }
}
