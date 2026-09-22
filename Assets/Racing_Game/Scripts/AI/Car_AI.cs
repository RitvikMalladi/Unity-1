//______________________________________________
// Car AI is based on the below tutorial:
// https://youtube.com/playlist?list=PLB9LefPJI-5wH5VdLFPkWfnPjeI6OSys1&feature=shared
//______________________________________________

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ALIyerEdon;

namespace ALIyerEdon
{
    public class Car_AI : MonoBehaviour
    {
        [Header("Version 3 - Nov 2025")]

        #region Public Variables

        [Tooltip("A red sphere gizmos means that the car is on a sharp turn in the road and is braking")]
        [Space(5)]
        public bool debugBrake;
        [Range(1f, 3f)]
        [Tooltip("Brake debug mode gizmos radius")]
        public float gizmoRadius = 3f;

        [Range(0.05f, 0.1f)]
        [Tooltip("Steer limit value to detect sharp turn and start braking. Lower value = more brake")]
        public float steerLimit = 0.07f;

        [Tooltip("How many seconds to do reversing if needed")]
        public float reversingDuration = 2f;

        [Space(10)]
        [Tooltip("Respawn the car when he cannot move for a few seconds")]
        public bool canRespawn = true;
        [Tooltip("Ground offset for respawn")]
        public float respawnYOffset = 1f;

        [Space(10)]
        [Header("Waypoint System")]
        [Space(5)]

        [Tooltip("Find the target waypoint by its name")]
        public string pathName = "Path_1";

        [Tooltip("Remaining distance to go to the next waypoint")]
        public float remainingDistance = 22f;

        [Space(10)]
        [Header("Raycast System")]
        [Space(5)]

        [Tooltip("Cast AI rays from here")]
        public Transform rayPositionCenter;
        public Transform rayPositionLeft;
        public Transform rayPositionRight;

        [Space(10)]
        // Corner ray settings
        public float sideSensorLength = 4;
        public float sideSensorAngle = 90;

        [Space(10)]
        // Center ray settings
        public float centerSensorLength = 7;
        public float centerSensorAngle = 15;
        float centerSensorLength_Temp;

        [Space(10)]
        // Front ray settings
        public float frontSensorLength = 7;

        [Space(10)]
        [Tooltip("Hit the AI raycast only to these layers")]
        public LayerMask raycastLayers;

        #endregion

        #region Internal variables
        float steerInput_Temp;
        bool raysHit;
        bool followWaypoint;

        [HideInInspector] public bool raceStarted = false;
        [HideInInspector] public int currentWaypoint = 0;
        [HideInInspector] public int currentLap = 0;
        [HideInInspector] public Waypoint_System path;

        // Temp the overall steer input
        float newSteer = 0;

        // Determine that which ray has been hitted
        bool centerLeft_Hit;
        bool centerRight_Hit;
        bool cornerLeft_Hit;
        bool cornerRight_Hit;
        bool front_Hit_Center;

        // New steer inputs based on hitted rays angles
        float steerInput_cornerRight;
        float steerInput_centerLeft;
        float steerInput_centerRight;
        float steerInput_cornerLeft;

        // Store waypoint points
        List<Transform> waypoints = new List<Transform>();

        EasyCarController vController;

        #endregion

        bool checkReversing = false;
        bool isReversing = false;
        bool isBraking = false;

        //public float respawnDelay = 7f;
        float carSpeed = 0;
        [HideInInspector] public bool canReverseCheck;

        Racer_Nitro racerNitro;

        void Start()
        {

            #region Initialize

            centerSensorLength_Temp = centerSensorLength;

            if (GetComponent<Racer_Nitro>())
                racerNitro = GetComponent<Racer_Nitro>();

            path = GameObject.Find(pathName).GetComponent<Waypoint_System>();

            waypoints = path.waypoints;

            currentLap = 0;

            GetComponent<Car_Position>().currentLap = currentLap;

            GetComponent<Car_Position>().nextCheckpoint = FindObjectOfType<Checkpoint_Manager>()
                .checkpoints[0];
            GetComponent<Car_Position>().currentCheckpoint = 0;

            vController = GetComponent<EasyCarController>();

            #endregion

        }
        void GotoNextPoint()
        {
            Debug.Log("GotoNextPoint");
            // Car positioning system

            if (currentWaypoint < waypoints.Count)
                GetComponent<Car_Position>().nextCheckpoint = waypoints[currentWaypoint];
            else
                GetComponent<Car_Position>().nextCheckpoint = waypoints[0];

            GetComponent<Car_Position>().currentCheckpoint = currentWaypoint;

            GetComponent<Car_Position>().currentLap = currentLap;

        }

        void OnDrawGizmos()
        {
            if (debugBrake)
            {
                if (isBraking)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(transform.position, gizmoRadius);
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(transform.position, gizmoRadius);
                }
            }
        }

        void Update()
        {
            if (!raceStarted)
                return;

            if (racerNitro)
                racerNitro.frontHit = front_Hit_Center;

            #region Steering

            Vector3 steerVector = transform.InverseTransformPoint(new Vector3(waypoints[currentWaypoint].position.x,
                transform.position.y, waypoints[currentWaypoint].position.z));


            if (followWaypoint)
                newSteer = (steerVector.x / steerVector.magnitude);
            else
                newSteer = 0;

            // Brake on hard corners of the road (high steers)
            if (Mathf.Abs(newSteer) > steerLimit && carSpeed > 30f)
                isBraking = true;
            else
                isBraking = false;


            #endregion

            #region Waypoint

            // Go to the next waypoint based on the remaining distance
            if (steerVector.magnitude <= remainingDistance)
            {

                currentWaypoint++;
                //GotoNextPoint();
            }


            // Reset to the first one if needed
            if (currentWaypoint >= waypoints.Count)
            {
                currentWaypoint = 0;

                // go to the next lap
                //currentLap++;
            }

            #endregion

            #region Inputs
            // Steering input controlled by AI
            if (!isReversing)
                vController.steerInput =
                                Mathf.Clamp(newSteer + steerInput_Temp, -1f, 1f);
            else
                vController.steerInput = 0;

            // Throttle input controlled by AI
            if (isBraking || isReversing || front_Hit_Center)
                vController.maxSpeed = 30f;
            else
                vController.maxSpeed = vController.originalMaxSpeed;

            if (!isReversing)
                vController.throttleInput = 1f;
            else
                vController.throttleInput = -1f;

            #endregion

        }

        void FixedUpdate()
        {
            AI_Sensors();
        }

        void AI_Sensors()
        {

            #region Init
            Vector3 fwd = transform.TransformDirection(new Vector3(0, 0, 1));
            Vector3 pivotPos = new Vector3(rayPositionCenter.localPosition.x, rayPositionCenter.localPosition.y, rayPositionCenter.localPosition.z);

            RaycastHit cornerRight_RayHit;
            RaycastHit cornerLeft_RayHit;

            RaycastHit center_Right_RayHit;
            RaycastHit center_Left_RayHit;

            RaycastHit front_Center_RayHit;

            carSpeed = GetComponent<Rigidbody>().linearVelocity.magnitude * 3.6f;
            #endregion

            #region Debug
            // Debug rays
            Debug.DrawRay(rayPositionCenter.position, Quaternion.AngleAxis(sideSensorAngle, transform.up) * fwd * sideSensorLength, Color.green);
            Debug.DrawRay(rayPositionCenter.position, Quaternion.AngleAxis(-sideSensorAngle, transform.up) * fwd * sideSensorLength, Color.green);
            Debug.DrawRay(new Vector3(rayPositionRight.position.x, rayPositionRight.position.y, rayPositionRight.position.z), Quaternion.AngleAxis(sideSensorAngle, transform.up) * fwd * sideSensorLength, Color.green);
            Debug.DrawRay(new Vector3(rayPositionLeft.position.x, rayPositionLeft.position.y, rayPositionLeft.position.z), Quaternion.AngleAxis(-sideSensorAngle, transform.up) * fwd * sideSensorLength, Color.green);

            Debug.DrawRay(rayPositionCenter.position, Quaternion.AngleAxis(centerSensorAngle, transform.up) * fwd * centerSensorLength, Color.green);
            Debug.DrawRay(rayPositionCenter.position, Quaternion.AngleAxis(-centerSensorAngle, transform.up) * fwd * centerSensorLength, Color.green);

            // Front center
            if (!front_Hit_Center)
            {
                Debug.DrawRay(new Vector3(rayPositionCenter.position.x, rayPositionCenter.position.y, rayPositionCenter.position.z), Quaternion.AngleAxis(0, transform.up) * fwd * frontSensorLength, Color.yellow);
                Debug.DrawRay(new Vector3(rayPositionRight.position.x, rayPositionRight.position.y, rayPositionRight.position.z), Quaternion.AngleAxis(0, transform.up) * fwd * frontSensorLength, Color.yellow);
                Debug.DrawRay(new Vector3(rayPositionLeft.position.x, rayPositionLeft.position.y, rayPositionLeft.position.z), Quaternion.AngleAxis(0, transform.up) * fwd * frontSensorLength, Color.yellow);
            }
            else
            {
                Debug.DrawRay(new Vector3(rayPositionCenter.position.x, rayPositionCenter.position.y, rayPositionCenter.position.z), Quaternion.AngleAxis(0, transform.up) * fwd * frontSensorLength, Color.red);
                Debug.DrawRay(new Vector3(rayPositionRight.position.x, rayPositionRight.position.y, rayPositionRight.position.z), Quaternion.AngleAxis(0, transform.up) * fwd * frontSensorLength, Color.red);
                Debug.DrawRay(new Vector3(rayPositionLeft.position.x, rayPositionLeft.position.y, rayPositionLeft.position.z), Quaternion.AngleAxis(0, transform.up) * fwd * frontSensorLength, Color.red);
            }

            #endregion

            #region Corner Rays
            // Corner rays
            if (Physics.Raycast(rayPositionCenter.position, Quaternion.AngleAxis(sideSensorAngle, transform.up) * fwd, out cornerRight_RayHit, sideSensorLength, raycastLayers)
                || Physics.Raycast(new Vector3(rayPositionRight.position.x, rayPositionRight.position.y, rayPositionRight.position.z - 3f), Quaternion.AngleAxis(sideSensorAngle, transform.up) * fwd, out cornerRight_RayHit, sideSensorLength, raycastLayers)
                )
            {
                if (!cornerRight_RayHit.collider.isTrigger && cornerRight_RayHit.transform.root != transform)
                {
                    Debug.DrawRay(rayPositionCenter.position, Quaternion.AngleAxis(sideSensorAngle, transform.up) * fwd * sideSensorLength, Color.red);
                    Debug.DrawRay(new Vector3(rayPositionRight.position.x, rayPositionRight.position.y, rayPositionRight.position.z - 3f), Quaternion.AngleAxis(sideSensorAngle, transform.up) * fwd * sideSensorLength, Color.red);
                    steerInput_cornerRight = Mathf.Lerp(-.5f, 0, (cornerRight_RayHit.distance / sideSensorLength));
                    cornerRight_Hit = true;
                }
            }
            else
            {
                steerInput_cornerRight = 0;
                cornerRight_Hit = false;
            }

            if (Physics.Raycast(rayPositionCenter.position, Quaternion.AngleAxis(-sideSensorAngle, transform.up) * fwd, out cornerLeft_RayHit, sideSensorLength, raycastLayers)
                || Physics.Raycast(new Vector3(rayPositionLeft.position.x, rayPositionLeft.position.y, rayPositionLeft.position.z - 3f), Quaternion.AngleAxis(-sideSensorAngle, transform.up) * fwd, out cornerLeft_RayHit, sideSensorLength, raycastLayers)
                )
            {
                if (!cornerLeft_RayHit.collider.isTrigger && cornerLeft_RayHit.transform.root != transform)
                {
                    Debug.DrawRay(rayPositionCenter.position, Quaternion.AngleAxis(-sideSensorAngle, transform.up) * fwd * sideSensorLength, Color.red);
                    Debug.DrawRay(new Vector3(rayPositionLeft.position.x, rayPositionLeft.position.y, rayPositionLeft.position.z - 3f), Quaternion.AngleAxis(-sideSensorAngle, transform.up) * fwd * sideSensorLength, Color.red);
                    steerInput_cornerLeft = Mathf.Lerp(.5f, 0, (cornerLeft_RayHit.distance / sideSensorLength));
                    cornerLeft_Hit = true;
                }
            }

            else
            {
                steerInput_cornerLeft = 0;
                cornerLeft_Hit = false;
            }
            #endregion

            #region Center Rays
            // Center rays
            if (Physics.Raycast(rayPositionCenter.position, Quaternion.AngleAxis(centerSensorAngle, transform.up) * fwd, out center_Right_RayHit, centerSensorLength, raycastLayers) && !center_Right_RayHit.collider.isTrigger && center_Right_RayHit.transform.root != transform)
            {
                Debug.DrawRay(rayPositionCenter.position, Quaternion.AngleAxis(centerSensorAngle, transform.up) * fwd * centerSensorLength, Color.red);
                steerInput_centerRight = Mathf.Lerp(-1, 0, (center_Right_RayHit.distance / centerSensorLength));
                centerRight_Hit = true;
            }

            else
            {
                steerInput_centerRight = 0;
                centerRight_Hit = false;
            }

            if (Physics.Raycast(rayPositionCenter.position, Quaternion.AngleAxis(-centerSensorAngle, transform.up) * fwd, out center_Left_RayHit, centerSensorLength, raycastLayers) && !center_Left_RayHit.collider.isTrigger && center_Left_RayHit.transform.root != transform)
            {
                Debug.DrawRay(rayPositionCenter.position, Quaternion.AngleAxis(-centerSensorAngle, transform.up) * fwd * centerSensorLength, Color.red);
                steerInput_centerLeft = Mathf.Lerp(1, 0, (center_Left_RayHit.distance / centerSensorLength));
                centerLeft_Hit = true;
            }

            else
            {
                steerInput_centerLeft = 0;
                centerLeft_Hit = false;
            }
            #endregion

            #region Front Rays

            // Front center ray
            if (Physics.Raycast(rayPositionCenter.position, Quaternion.AngleAxis(0, transform.up) * fwd, out front_Center_RayHit, frontSensorLength, raycastLayers)
                || (Physics.Raycast(new Vector3(rayPositionCenter.position.x + 1, rayPositionCenter.position.y, rayPositionCenter.position.z), Quaternion.AngleAxis(0, transform.up) * fwd, out front_Center_RayHit, frontSensorLength, raycastLayers)
                || (Physics.Raycast(new Vector3(rayPositionCenter.position.x - 1, rayPositionCenter.position.y, rayPositionCenter.position.z), Quaternion.AngleAxis(0, transform.up) * fwd, out front_Center_RayHit, frontSensorLength, raycastLayers))))
            {
                if (!front_Center_RayHit.collider.isTrigger && front_Center_RayHit.transform.root != transform)
                {
                    {
                        if (front_Center_RayHit.transform.tag == "Racer" ||
                        front_Center_RayHit.transform.tag == "Player")
                            front_Hit_Center = true;
                        else
                            front_Hit_Center = false;
                    }
                }
            }
            else
            {
                front_Hit_Center = false;
            }

            #endregion

            #region Steering
            if (centerLeft_Hit && centerRight_Hit)
                steerInput_centerRight = 1f;

            if (centerLeft_Hit || centerRight_Hit || cornerRight_Hit || cornerLeft_Hit)
                raysHit = true;
            else
                raysHit = false;

            if (raysHit)
                steerInput_Temp = (steerInput_cornerRight + steerInput_centerLeft + steerInput_centerRight + steerInput_cornerLeft);
            else
                steerInput_Temp = 0;

            if (raysHit && Mathf.Abs(steerInput_Temp) > 0.5f)
                followWaypoint = false;
            else
                followWaypoint = true;
            #endregion

            // Reverse check
            if (canReverseCheck)
            {

                if (carSpeed <= 10f)
                {
                    if (centerLeft_Hit || centerRight_Hit ||
                        cornerLeft_Hit || cornerRight_Hit)
                    {

                        if (!checkReversing)
                            StartCoroutine(Check_Reversing());
                    }
                }
            }


            // Respawn check
            if (canRespawn)
                Respawn_Check();


        }

        IEnumerator Check_Reversing()
        {
            checkReversing = true;

            yield return new WaitForSeconds(1f);

            if (carSpeed <= 10f)
            {
                isReversing = true;
            }

            yield return new WaitForSeconds(reversingDuration);

            isReversing = false;
            centerSensorLength = centerSensorLength_Temp * 2f;

            yield return new WaitForSeconds(2);

            checkReversing = false;

            yield return new WaitForSeconds(reversingDuration);

            centerSensorLength = centerSensorLength_Temp;

        }
        //_________________________________________________________________________
        bool respawnCheck_1 = false;
        bool respawnCheck_2 = false;
        bool respawnCheck_3 = false;
        bool respawnCheck_4 = false;
        bool respawnCheking = false;

        public void Respawn_Check()
        {
            if (canReverseCheck)
            {
                if (carSpeed <= 7f)
                {
                    if (!respawnCheking)
                        StartCoroutine(Check_Respawn());
                }
            }
        }

        IEnumerator Check_Respawn()
        {
            respawnCheking = true;

            //_________________________________111
            yield return new WaitForSeconds(Time.timeScale);

            if (carSpeed <= 7f)
                respawnCheck_1 = true;
            else
            {
                respawnCheck_1 = false;
                respawnCheck_2 = false;
                respawnCheck_3 = false;
                respawnCheck_4 = false;

                respawnCheking = false;

                StopCoroutine(Check_Respawn());
            }
            //_________________________________222
            yield return new WaitForSeconds(Time.timeScale);

            if (carSpeed <= 7f)
                respawnCheck_2 = true;
            else
            {
                respawnCheck_1 = false;
                respawnCheck_2 = false;
                respawnCheck_3 = false;
                respawnCheck_4 = false;

                respawnCheking = false;

                StopCoroutine(Check_Respawn());
            }
            //_________________________________333
            yield return new WaitForSeconds(Time.timeScale);

            if (carSpeed <= 7f)
                respawnCheck_3 = true;
            else
            {
                respawnCheck_1 = false;
                respawnCheck_2 = false;
                respawnCheck_3 = false;
                respawnCheck_4 = false;

                respawnCheking = false;

                StopCoroutine(Check_Respawn());
            }
            //_________________________________444
            yield return new WaitForSeconds(Time.timeScale);

            if (carSpeed <= 7f)
                respawnCheck_4 = true;
            else
            {
                respawnCheck_1 = false;
                respawnCheck_2 = false;
                respawnCheck_3 = false;
                respawnCheck_4 = false;

                respawnCheking = false;

                StopCoroutine(Check_Respawn());
            }

            if (respawnCheck_1 && respawnCheck_2
                && respawnCheck_3 & respawnCheck_4)
            {
                //Respawn now
                if (waypoints.Count != 0 && raceStarted)
                {
                    transform.position = new Vector3(
                                            waypoints[currentWaypoint - 1].position.x,
                                            waypoints[currentWaypoint - 1].position.y + respawnYOffset,
                                            waypoints[currentWaypoint - 1].position.z);
                    GetComponent<Rigidbody>().position = new Vector3(
                                            waypoints[currentWaypoint - 1].position.x,
                                            waypoints[currentWaypoint - 1].position.y + respawnYOffset,
                                            waypoints[currentWaypoint - 1].position.z);

                    var targetPosition = waypoints[currentWaypoint].position;
                    targetPosition.y = transform.position.y;

                    transform.rotation = Quaternion.identity;
                    GetComponent<Rigidbody>().rotation = Quaternion.identity;
                    transform.LookAt(targetPosition);
                }

                isReversing = false;
                checkReversing = false;

            }

            respawnCheking = false;
            respawnCheck_1 = false;
            respawnCheck_2 = false;
            respawnCheck_3 = false;
            respawnCheck_4 = false;
        }
    }
}