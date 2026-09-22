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
    public class Checkpoint_Trigger : MonoBehaviour
    {
        public string PlayerTag = "Player";
        public string RacerTag = "Racer";

        public int currentCheckpoint = 0;

        void OnTriggerEnter(Collider col)
        {
            if (col.transform.tag == PlayerTag || col.transform.tag == RacerTag)
            {
                col.GetComponent<Car_Position>().currentCheckpoint = currentCheckpoint;

                // Pass to the next checkpoint
                if (currentCheckpoint < GetComponentInParent<Checkpoint_Manager>().checkpoints.Count - 1)
                {
                    col.GetComponent<Car_Position>().nextCheckpoint =
                        GetComponentInParent<Checkpoint_Manager>().checkpoints[col.GetComponent<Car_Position>().currentCheckpoint + 1];
                }

                // Siwtch to the first checkpoint at the final checkpoint
                if (currentCheckpoint == GetComponentInParent<Checkpoint_Manager>().checkpoints.Count - 1)
                {
                    col.GetComponent<Car_Position>().nextCheckpoint =
                        GetComponentInParent<Checkpoint_Manager>().checkpoints[0];
                }

                // Only player can pass to the next lap on the center of the race track
                if (currentCheckpoint == GetComponentInParent<Checkpoint_Manager>().checkpoints.Count /2)
                {
                    col.GetComponent<Car_Position>().canPassLap = true;
                }

                // Pass to the next Lap
                if (currentCheckpoint == 0)
                {
                    if (col.GetComponent<Car_Position>().canPassLap == true)
                        col.GetComponent<Car_Position>().currentLap++;

                    if (col.transform.tag == PlayerTag)
                    {
                        if (col.GetComponent<Car_Position>().currentLap >
                            FindObjectOfType<Race_Manager>().totalLaps)
                        {
                            FindObjectOfType<Race_Manager>().Finish_Race();
                        }
                    }

                    // Can not pass the lap for multiple times at a same time
                    col.GetComponent<Car_Position>().canPassLap = false;
               
                }
            }
        }
    }
}