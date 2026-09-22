using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ALIyerEdon
{
	public class CollisionEffects : MonoBehaviour
	{
		public bool sparks = true;
		ALIyerEdon.EasyCarAudio carAudio;

		void Start()
		{
			carAudio = transform.root
					.GetComponent<ALIyerEdon.EasyCarAudio>();
		}


		void OnCollisionEnter(Collision collision)
		{
			if (sparks)
			{
				if (collision.relativeVelocity.magnitude > carAudio.collisionVelocity)
				{
					carAudio.collisionSource.gameObject.transform.position =
					collision.GetContact(0).point;

					if (carAudio.collisionSpark && collision.relativeVelocity.magnitude > carAudio.collisionVelocity + 3)
					{
						carAudio.collisionSpark.transform.position =
							collision.GetContact(0).point;

						var emi = carAudio.collisionSpark.GetComponent<ParticleSystem>().emission;
						emi.enabled = true;
					}

					if (!carAudio.collisionSource.isPlaying)
					{
						carAudio.collisionSource.PlayOneShot(carAudio.collisionClip);
					}
				}
			}
		}
		void OnCollisionExit(Collision collision)
		{
			if (carAudio.collisionSpark)
			{
				var emi = carAudio.collisionSpark.GetComponent<ParticleSystem>().emission;
				emi.enabled = false;
			}
		}
	}
}