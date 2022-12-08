﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SplineAI
{
    // when selected in editor
    [CustomEditor(typeof(UISpline))]
    public class UISplineEditor : Editor
    {
        private Vector3 offsetPos(Vector3 p, float aa)
        {
            float x = p.x;
            float y = p.y;
            float z = p.z;

            // radius
            float r = Mathf.Sqrt((x * x) + (z * z));

            // angle
            float a = Mathf.Atan2(z, x) - (aa * (Mathf.PI / 180));

            x = r * Mathf.Cos(a);
            z = r * Mathf.Sin(a);

            return new Vector3(x, y, z);
        }

        void OnSceneGUI()
        {
            Debug.Log("fasdsf");
            UISpline nn = target as UISpline;
            float a = nn.gameObject.transform.eulerAngles.y;

            // loop to draw line from point to point in list
            Handles.color = Color.white;
            for (int i = 1; i < nn.points.Length; i++)
                Handles.DrawLine(offsetPos(nn.points[i - 1], a) + nn.transform.parent.transform.position, offsetPos(nn.points[i], a) + nn.transform.parent.transform.position);

            // loop to draw numbers from order in list
            GUIStyle style = new GUIStyle();
            style.fontSize = 30;
            style.normal.textColor = Color.white;
            for (int i = 0; i < nn.points.Length; i++)
            {
                Handles.Label(offsetPos(nn.points[i], a) + nn.transform.parent.transform.position, "" + i, style);
                nn.points[i] = Handles.PositionHandle(nn.points[i], Quaternion.identity);
            }
        }
    }
}