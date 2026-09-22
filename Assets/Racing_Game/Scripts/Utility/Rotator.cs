//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using ALIyerEdon;

namespace ALIyerEdon
{
	public class Rotator : MonoBehaviour
	{


		Transform target;
		public Vector3 dir;
		public float speed = 100f;

		void Start()
		{
			target = GetComponent<Transform>();
		}


		void Update()
		{
			target.Rotate(dir * speed * Time.deltaTime);
		}
	}
}