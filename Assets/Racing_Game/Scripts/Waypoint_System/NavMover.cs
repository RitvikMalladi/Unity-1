using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ALIyerEdon
{
	public class NavMover : MonoBehaviour
	{

		public WaypointSystem path;
		public float remainingDistance = 0.1f;

		public float speed = 5;

		List<Transform> points = new List<Transform>();
		private int destPoint = 0;
		[HideInInspector] public bool isActive;


		void GotoNextPoint()
		{
			// Returns if no points have been set up
			if (points.Count == 0)
				return;


			// Choose the next point in the array as the destination,
			// cycling to the start if necessary.
			destPoint = (destPoint + 1) % points.Count;



			if (destPoint == 0)
				Destroy(gameObject);
		}


		void Update()
		{
			if (!isActive)
				return;

			// Choose the next destination point when the agent gets
			// close to the current one.
			if (Vector3.Distance(transform.position, points[destPoint].position) < remainingDistance)
				GotoNextPoint();

			transform.position = Vector3.MoveTowards(transform.position, points[destPoint].position, speed * Time.deltaTime);
			transform.LookAt(new Vector3(points[destPoint].position.x, transform.position.y, points[destPoint].position.z));
		}

		public void ActivateMover()
		{
			isActive = true;

			points = path.waypoints;

			GetComponent<Animator>().SetBool("Run", true);

			GotoNextPoint();
		}
	}
}