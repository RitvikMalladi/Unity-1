//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ALIyerEdon;
namespace ALIyerEdon
{
	[CustomEditor(typeof(Waypoint_System))]
	public class Waypoint_SystemEditor : Editor
	{

		Waypoint_System waypointSystem;

		public override void OnInspectorGUI()
		{

			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.HelpBox("Shift + Mouse Click", MessageType.Info);
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			waypointSystem = (Waypoint_System)target;

			if (GUILayout.Button("Clear"))
			{
				for (int w = 0; w < waypointSystem.waypoints.Count; w++)
				{
					DestroyImmediate(waypointSystem.waypoints[w].gameObject);
				}

				waypointSystem.waypoints.Clear();
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("waypoints"), new GUIContent("Waypoints", ""), true);

			serializedObject.ApplyModifiedProperties();

		}

		void OnSceneGUI()
		{
			Event e = Event.current;

			waypointSystem = (Waypoint_System)target;

			if (e != null)
			{
				if (e.isMouse && e.shift && e.type == EventType.MouseDown)
				{
					Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					RaycastHit hitInfo = new RaycastHit();
					if (Physics.Raycast(ray, out hitInfo, 10000))
					{
						GameObject waypoint = new GameObject("Way");
						Undo.RegisterCreatedObjectUndo(waypoint, "Created waypoint");
						waypoint.transform.position = hitInfo.point;
						waypoint.transform.parent = waypointSystem.transform;
					}
				}

				if (waypointSystem)
					Selection.activeGameObject = waypointSystem.gameObject;
			}
		}
	}
}