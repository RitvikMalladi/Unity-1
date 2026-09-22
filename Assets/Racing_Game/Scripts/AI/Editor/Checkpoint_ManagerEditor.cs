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
	[CustomEditor(typeof(Checkpoint_Manager))]
	public class Checkpoint_ManagerEditor : Editor
	{

		Checkpoint_Manager checkpointSystem;

		public override void OnInspectorGUI()
		{

			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.HelpBox("Shift + Mouse Click", MessageType.Info);

			checkpointSystem = (Checkpoint_Manager)target;

			if (GUILayout.Button("Clear"))
			{
				for (int c = 0; c < checkpointSystem.checkpoints.Count; c++)
				{
					DestroyImmediate(checkpointSystem.checkpoints[c].gameObject);
				}

				checkpointSystem.checkpoints.Clear();
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("autoAlign"), new GUIContent("Auto Align", ""), true);

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerSize"), new GUIContent("Trigger Size", ""), true);
			EditorGUILayout.Space();
			EditorGUILayout.Space();


			EditorGUILayout.PropertyField(serializedObject.FindProperty("checkpoints"), new GUIContent("Checkpoints", ""), true);

			serializedObject.ApplyModifiedProperties();

		}

		void OnSceneGUI()
		{
			Event e = Event.current;

			checkpointSystem = (Checkpoint_Manager)target;

			if (e != null)
			{
				if (e.isMouse && e.shift && e.type == EventType.MouseDown)
				{
					Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					RaycastHit hitInfo = new RaycastHit();
					if (Physics.Raycast(ray, out hitInfo, 10000))
					{

						GameObject checkpoint = new GameObject("Checkpoint");
						Undo.RegisterCreatedObjectUndo(checkpoint, "Created checkpoint");

						int LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
						checkpoint.layer = LayerIgnoreRaycast;

						checkpoint.AddComponent<BoxCollider>();
						checkpoint.GetComponent<BoxCollider>().isTrigger = true;
						checkpoint.GetComponent<BoxCollider>().size
							= checkpointSystem.triggerSize;

						checkpoint.AddComponent<Checkpoint_Trigger>();

						checkpoint.transform.position = hitInfo.point;
						checkpoint.transform.parent = checkpointSystem.transform;


					}
				}

				if (checkpointSystem)
					Selection.activeGameObject = checkpointSystem.gameObject;
			}
		}
	}
}