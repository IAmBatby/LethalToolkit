using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LethalToolkit.General
{
    [CustomEditor(typeof(LethalToolkitSettings))]
    public class LethalToolkitSettingsEditor : Editor
    {
        private void OnEnable()
        {
            SceneView.duringSceneGui += DrawGizmos;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DrawGizmos;
        }

        private void DrawGizmos(SceneView sceneView)
        {
            // draw some Gizmos-like gizmo
            Vector3 parentOffset = new Vector3(-17.4f, 7.6f, -16.4f);
            Vector3 offset = parentOffset + LethalToolkitManager.Settings.manualShipLandingOffset;
            Vector3 startingPosition = new Vector3(116.7f, 63.1f, 8.9f);
            Vector3 endingPosition = new Vector3(18.7f, -7.3f, 8.97f);
            Handles.DrawWireCube(Vector3.zero, Vector3.one * 2f);
            Handles.color = Color.yellow;
            Handles.DrawDottedLine(startingPosition + offset, endingPosition + offset, LethalToolkitManager.Settings.gizmosDashSize);
            Handles.DrawWireCube(startingPosition + offset, new Vector3(0.25f, 0.25f, 0.25f));
            Handles.DrawWireCube(endingPosition + offset, new Vector3(0.25f, 0.25f, 0.25f));

            Vector3 currentShipOffset = Vector3.Lerp(startingPosition + offset, endingPosition + offset, LethalToolkitManager.Settings.shipGizmosProgress);

            if (LethalToolkitManager.Settings.shipHullMesh != null)
            {
                List<Vector3> verts = new List<Vector3>(LethalToolkitManager.Settings.shipHullMesh.vertices);

                foreach (Vector3 v in new List<Vector3>(verts))
                    verts[verts.IndexOf(v)] = v + currentShipOffset;

                Handles.DrawAAConvexPolygon(verts.ToArray());
            }


        }
    }
}
