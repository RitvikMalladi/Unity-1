//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ALIyerEdon;

namespace ALIyerEdon
{
	[ExecuteInEditMode]
	public class Checkpoint_Manager : MonoBehaviour
	{
		public List<Transform> checkpoints = new List<Transform>();

		public int index = 0;
		public int totalCheckpoints = 0;
		public bool autoAlign = true;

		public Vector3 triggerSize = new Vector3(50, 20, 2);

		void Update()
		{
#if UNITY_EDITOR
			Transform[] tem = GetComponentsInChildren<Transform>();

			if (tem.Length > 0)
			{
				checkpoints.Clear();

				index = 0;

				foreach (Transform t in tem)
				{
					if (t != transform)
					{
						t.name = "Checkpoint_" + index.ToString();

						t.GetComponent<Checkpoint_Trigger>().currentCheckpoint = index;
						checkpoints.Add(t);

						index++;
					}
				}

				if (autoAlign)
				{
					for (int tt = 1; tt < tem.Length; tt++)
					{
						if (tt == tem.Length - 1)
							tem[tt].LookAt(tem[1]);
						else
							tem[tt].LookAt(tem[tt + 1]);
					}
				}
			}

			totalCheckpoints = index;

#endif
		}

		void OnDrawGizmos()
		{
			if (checkpoints.Count > 0)
			{
				Gizmos.color = Color.white;

				foreach (Transform t in checkpoints)
					Gizmos.DrawSphere(t.position, 1f);

				Gizmos.color = Color.blue;

				for (int a = 0; a < checkpoints.Count - 1; a++)
					Gizmos.DrawLine(checkpoints[a].position, checkpoints[a + 1].position);
			}
		}
	}
}