//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

using ALIyerEdon;

namespace ALIyerEdon
{
	public class SmoothFollow : MonoBehaviour
	{

		public Transform target;
		// The distance in the x-z plane to the target
		public float distance = 10.0f;
		// the height we want the camera to be above the target
		public float height = 5.0f;
		// How much we
		public float heightDamping = 2.0f;
		public float rotationDamping = 3.0f;
		[HideInInspector] public float daynamicCameraIntensity = 0.5f;

		public Vector3 offset = Vector3.zero;

		// Rigidbody for smooth rotation   
		Rigidbody carRigidBody;

		Vector3 originalPos;

		EasyCarController carController;
		bool isReversing;

		IEnumerator Start()
		{			

			yield return new WaitForEndOfFrame();

			carController = target.GetComponent<EasyCarController>();

			carRigidBody = target.GetComponent<Rigidbody>();

			//Face the camera to the car forward at start
			transform.position = new Vector3(target.position.x,
				target.position.y + 300f, target.position.z);

		}
		void Update()
		{
			// Early out if we don't have a target
			if (!target)
				return;

			if (!carRigidBody)
				return;

			isReversing = carController.reversing;

			//		Vector3 localVilocity = target.InverseTransformDirection (target.GetComponent<Rigidbody> ().velocity);

			// Calculate the current rotation angles
			float wantedRotationAngle;

			if (isReversing)
				wantedRotationAngle = -target.eulerAngles.y;
			else
				wantedRotationAngle = target.eulerAngles.y;


			Vector3 pos = target.position + Quaternion.AngleAxis(wantedRotationAngle, Vector3.up) * offset;
			float wantedHeight = height + pos.y;


			float currentRotationAngle = transform.eulerAngles.y;
			float currentHeight = transform.position.y;

			// Smooth rotation by rigidboy  
			rotationDamping =
				Mathf.Lerp(0f, 3f, (carRigidBody.linearVelocity.magnitude * 3f) / 40f);

			// Damp the rotation around the y-axis
			currentRotationAngle =
				Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

			// Damp the height
			currentHeight =
				Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

			// Convert the angle into a rotation
			Quaternion currentRotation =
				Quaternion.Euler(0, currentRotationAngle, 0);

			// Set the position of the camera on the x-z plane to:
			// distance meters behind the target
			transform.position = pos;

			transform.position -=
				currentRotation * Vector3.forward * (distance + daynamicCameraIntensity);
			 
			// Set the height of the camera
			transform.position =
				new Vector3(transform.position.x, currentHeight, transform.position.z);

			// Always look at the target
			transform.LookAt(pos);
		}
	}
}