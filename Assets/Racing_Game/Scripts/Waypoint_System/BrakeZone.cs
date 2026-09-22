using UnityEngine;
using System.Collections;

namespace ALIyerEdon
{
	public class BrakeZone : MonoBehaviour
	{

		// Use this for initialization
		public float targetSpeed = 14f;

		float orginalSpeed;

		void OnTriggerEnter(Collider col)
		{

			if (col.CompareTag("Player"))
			{
				orginalSpeed = col.GetComponent<UnityEngine.AI.NavMeshAgent>().speed;
				col.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = targetSpeed;
			}

		}

		void OnTriggerExit(Collider col)
		{

			if (col.CompareTag("Player"))
				col.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = orginalSpeed;
		}
	}
}
