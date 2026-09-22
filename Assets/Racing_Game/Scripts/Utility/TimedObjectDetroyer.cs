//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using ALIyerEdon;

namespace ALIyerEdon
{
	public class TimedObjectDetroyer : MonoBehaviour
	{

		public float destroyTime = 3f;
		public ParticleSystem smoke;
		IEnumerator Start()
		{
			yield return new WaitForSeconds(destroyTime);
			Destroy(gameObject);
		}
	}
}