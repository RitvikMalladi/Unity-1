//______________________________________________
//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using UnityEngine;
using System.Collections;
using ALIyerEdon;

namespace ALIyerEdon
{
	#region EnumFields
	public enum InputType
	{
		Keyboard,
		Mobile
	}
	public enum DriveType
	{
		Front,
		Rear,
		FrontRear,
		AllWheels
	}
	#endregion

	public class EasyCarController : MonoBehaviour
	{
		#region Variables

		[Space(3)]
		public bool OffroadMode = false;
        public DriveType driveWheels = DriveType.Rear;
		public GameObject body;
		public GameObject carMesh;
		public GameObject interior;

		[Header("Wheels")]
		public WheelCollider[] Wheel_Colliders;

		public Transform[] Wheel_Transforms;

		// Interior view steering wheel game object
		public Transform steeringWheel;

		// public Transform centerOfMass;
		public Transform centerOfMass;

		public float skidmarkWidth = 0.275f;

        [HideInInspector] public float currentSpeed;

		[Header("Engine")]
		public float enginePower = 2000f;
		public float brakePower = 2000f;
		[HideInInspector] public float enginePowerTemp;
		[HideInInspector] public bool nitro_Mode;

		[Header("Inputs")]
		// Speed and Steer limites
		[Range(10,50)]
		public float maxSteer = 43f;
		public float maxSpeed = 74f;
		public float steerSensibility = 10f;

		[Header("Wheels")]
		// Wheel colliders settings
		public float brakeFriction = 4f;
		public float handBrakeFriction = 0.75f;

		// Store default max speed for speed limit triggers
		[HideInInspector] public float originalMaxSpeed = 74f;

        // Input values
        [HideInInspector] public float throttleInput;
		[HideInInspector] public float steerInput;
		bool handBrake;

		[HideInInspector] public bool reversing;

		// Catch rigidbody
		Rigidbody rigid;


		[Header("Gears")]
		// Gear values to control engine sound based on gears    
		public int numberOfGears = 7;
		[HideInInspector] public int currentGear;
		float GearFactor;
		[HideInInspector] public float Revs;
		public float GearShiftDelay = 0.7f;
		public float nextGearSpeed = 150;
		public float[] gearRatio;

		bool changingGearUP;
		bool changingGearDown;
		[HideInInspector] public bool Clutch;

		EasyCarAudio vehicleAudio;

		[Header("Lights")]
		// Vehicle lights
		public Light[] brakeLights;
		public GameObject[] brakeFlares;
		public Light[] reverseLights;
        public GameObject[] reverseFlares;
        public Material brakeMaterial;
		public float minHDR = 2f, maxHDR = 4f;

		// Wheel colliders friction values
		WheelFrictionCurve handBrakeFrictionCurve;
		WheelFrictionCurve brakeFrictionCurve;
		float defaultStiffness_ForwardWheels;
		float defaultStiffness_BackwardWheels;
		float defaultStiffness_Sideways;

		// Detect ground type (road or ground) for shaking and slipping)
		Transform rayPosition;

		[Header("Dynamic Camera")]
		public float defaultFOV = 55f;
		public float nitroFOV = 70f;
		public float dynamicCameraIntensity = 0.5f;
		float originalFOV = 55f;
		Camera mainCamera;
		bool dynamicCamera;

		[Header("Body Shaking")]
		public float collisionShakeIntensity = 1f;
		public float bodyShakeIntensity = 5f;
		public float startDuration = 1.7f;
		public bool exhaustFlame = true;
		[HideInInspector] public bool shaking;

		[HideInInspector] public bool isPlayer = false;

		// Ground check for speed limit
		[HideInInspector] public bool inRoad = true;

		Quaternion bodyStartRotation;
		// Random flame effect for gear up and down mode
		int rnd;

		int difficultyLevel;

		SmoothFollow smoothFollow_1;
		SmoothFollow2 smoothFollow_2;

		#endregion

		void Start()
		{
			if(body)
				bodyStartRotation = body.transform.localRotation;

			BrakeMaterial(minHDR);

			mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			originalFOV = mainCamera.fieldOfView;
			enginePowerTemp = enginePower;

            difficultyLevel = PlayerPrefs.GetInt("Difficulty Level");

			if (FindObjectOfType<SmoothFollow>())
				smoothFollow_1 = FindObjectOfType<SmoothFollow>();

			if (FindObjectOfType<SmoothFollow2>())
				smoothFollow_2 = FindObjectOfType<SmoothFollow2>();

			for (int w = 0; w < Wheel_Colliders.Length; w++)
			{
				if (Wheel_Colliders[w].GetComponent<WheelSkidmarks>())
					Wheel_Colliders[w].GetComponent<WheelSkidmarks>().wheelID = w;
			}

			if (gameObject.tag == "Player")
				isPlayer = true;

			if (isPlayer)
			{
				if (PlayerPrefs.GetInt("Dynamic_Camera") == 1)
					dynamicCamera = true;
				else
					dynamicCamera = false;
			}

			// Update skidmarks width based on the car type (racing car, truck...)
			if (isPlayer)
			{
				if (FindFirstObjectByType<Skidmarks_Manager>())
					FindFirstObjectByType<Skidmarks_Manager>().markWidth = skidmarkWidth;
			}

            // Detect ground type (road or ground) for shaking and slipping)
            rayPosition = GetComponent<Car_AI>().rayPositionCenter;

			// Store original max speed value for speed limit trigger
			originalMaxSpeed = maxSpeed;

			StartCoroutine(GearChanging());

			rigid = GetComponent<Rigidbody>();

			// used to smoothing smooth follow camera movement behind vehicle
			rigid.interpolation = RigidbodyInterpolation.Interpolate;

			// Set center of mass to center of mass transform localposition
			rigid.centerOfMass = centerOfMass.localPosition;

			// Get vehicle audio component
			vehicleAudio = GetComponent<EasyCarAudio>();

			// Find back wheels friction to applay hand brake
			handBrakeFrictionCurve = Wheel_Colliders[2].sidewaysFriction;

			// Find front wheels friction to applay brake
			brakeFrictionCurve = Wheel_Colliders[2].forwardFriction;

			defaultStiffness_ForwardWheels = Wheel_Colliders[0].forwardFriction.stiffness;
			defaultStiffness_BackwardWheels = Wheel_Colliders[2].forwardFriction.stiffness;
			defaultStiffness_Sideways = Wheel_Colliders[2].sidewaysFriction.stiffness;
		}

		//Since you'll be working with physics (rigidbody's velocity), you'll be using fixedupdate
		void FixedUpdate()
		{

			#region Reversing Detection

			// Detect the reversing mode
			float dotP = Vector3.Dot(transform.forward.normalized, rigid.linearVelocity.normalized);

			if (dotP > 0.5f)
			{
				reversing = false;

				if (isPlayer)
				{
					if (smoothFollow_2)
						smoothFollow_2.YOffset = 0;
				}
			}
			else if (dotP < -0.5f)
			{
				reversing = true;

				if (isPlayer)
				{
					if (currentSpeed > 1f)
					{
						if (!smoothFollow_2.dashboardCameraMode)
							smoothFollow_2.YOffset = 180f;
					}
				}
			}
			else
			{
				// Sliding sideways
			}
			#endregion


			// Ground check for camera shake and car slipping
			Ground_Check();

		}

		void Update()
		{
			// Body shaking in cutoff
			if (isPlayer)
			{
				if (shaking)
				{
					float angle;
					if (Clutch)
						angle = Mathf.Sin(Time.time * 30) * (bodyShakeIntensity / 10); 
					else
						angle = Mathf.Sin(Time.time * 30) * (bodyShakeIntensity / 17);

					body.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
				}
			}

			#region Camera
			if (isPlayer)
			{
				if (nitro_Mode)
				{
					mainCamera.fieldOfView =
						Mathf.Lerp(mainCamera.fieldOfView, nitroFOV, Time.deltaTime);
				}
				else
				{
					mainCamera.fieldOfView =
						Mathf.Lerp(mainCamera.fieldOfView, defaultFOV, Time.deltaTime * 0.77f);
					if (dynamicCamera)
					{
						if (changingGearUP)
						{
							if (smoothFollow_1)
							{
								smoothFollow_1.daynamicCameraIntensity =
									Mathf.Lerp(smoothFollow_1.daynamicCameraIntensity,
									smoothFollow_1.daynamicCameraIntensity - dynamicCameraIntensity,
									Time.deltaTime * 1);
							}
							if (smoothFollow_2)
							{
								smoothFollow_2.daynamicCameraIntensity =
								Mathf.Lerp(smoothFollow_2.daynamicCameraIntensity,
								smoothFollow_2.daynamicCameraIntensity - dynamicCameraIntensity,
								Time.deltaTime * 1);
							}
						}
						else
						{
							if (smoothFollow_1)
							{
								smoothFollow_1.daynamicCameraIntensity =
									Mathf.Lerp(smoothFollow_1.daynamicCameraIntensity, 0,
									Time.deltaTime * 3);
							}
							if (smoothFollow_2)
							{
								smoothFollow_2.daynamicCameraIntensity =
									Mathf.Lerp(smoothFollow_2.daynamicCameraIntensity, 0,
									Time.deltaTime * 3);
							}
						}
					}
				}

				
			}
			#endregion

			// Apply engine inputs
			VehicleEngine();

			// Update current speed and multiply
			currentSpeed = rigid.linearVelocity.magnitude * 2.23693629f;

			#region Wheel Align
			// Align wheel mesh across wheel collider rotation and position
			for (int i = 0; i < Wheel_Colliders.Length; i++)
			{
				Quaternion quat;
				Vector3 position;
				Wheel_Colliders[i].GetWorldPose(out position, out quat);
				Wheel_Transforms[i].transform.position = position;
				Wheel_Transforms[i].transform.rotation = quat;
			}

			// Rotate the steering wheel game object (interior steering wheel)
			if (steeringWheel)
				steeringWheel.rotation =
					Quaternion.Lerp(steeringWheel.rotation, transform.rotation * Quaternion.Euler(34f, 0, (Wheel_Colliders[0].steerAngle) * -1.5f),Time.deltaTime * 3f);
			#endregion

		}
		void Ground_Check()
		{

			// Ground check (road or ground)
			if (transform.tag == "Player")
			{
				RaycastHit hit;

				if (Physics.Raycast(rayPosition.position, -rayPosition.up, out hit, 3))
				{
					Debug.DrawRay(rayPosition.position, -rayPosition.up * 3, Color.yellow);

					if (hit.transform.tag == "Ground")
					{
						inRoad = false;
						if (currentSpeed > 14f)
						{
							if (!vehicleAudio.grassSkidSource.isPlaying)
								vehicleAudio.grassSkidSource.Play();
						}
						else
                        {
							if (vehicleAudio.grassSkidSource.isPlaying)
								vehicleAudio.grassSkidSource.Stop();
						}
					}
					else
					{
						inRoad = true;
						if (vehicleAudio.grassSkidSource.isPlaying)
							vehicleAudio.grassSkidSource.Stop();
					}
				}
			}
			if (OffroadMode)
			{
				if (currentSpeed > 2.13f)
				{
					var emi = vehicleAudio.offroadSmoke.GetComponent<ParticleSystem>().emission;
					emi.enabled = true;
				}
				else
				{
					var emi = vehicleAudio.offroadSmoke.GetComponent<ParticleSystem>().emission;
					emi.enabled = false;
				}
			}
			else
			{
                if (inRoad)
                {
                    var emi = vehicleAudio.offroadSmoke.GetComponent<ParticleSystem>().emission;
                    emi.enabled = false;
                }
                else
                {
                    if (currentSpeed > 2.13f)
                    {
                        var emi = vehicleAudio.offroadSmoke.GetComponent<ParticleSystem>().emission;
                        emi.enabled = true;
                    }
                    else
                    {
                        var emi = vehicleAudio.offroadSmoke.GetComponent<ParticleSystem>().emission;
                        emi.enabled = false;
                    }
                }
            }
        }

		public void VehicleEngine()
		{
			// For engine sound system
			CalculateRevs();

			if (!Clutch)
			{

				#region Speed Limiter
				// Speed limiter
				if (currentSpeed >= maxSpeed)
					rigid.linearDamping = 0.3f;
				else
					rigid.linearDamping = 0.05f;
                #endregion

                #region Offroad
                // Camera shaking control on the out of the road (ground tag)
                if (isPlayer)
				{
					vehicleAudio.shakeUtility.currentSpeed = currentSpeed;

					if (inRoad)
						vehicleAudio.shakeUtility.offRoadShaking = false;
					else
						vehicleAudio.shakeUtility.offRoadShaking = true;
				}
                #endregion

                if (currentSpeed <= 7f)
				{
					brakeFrictionCurve.stiffness = brakeFriction;

					Wheel_Colliders[0].forwardFriction = brakeFrictionCurve;
					Wheel_Colliders[1].forwardFriction = brakeFrictionCurve;
					Wheel_Colliders[2].forwardFriction = brakeFrictionCurve;
					Wheel_Colliders[3].forwardFriction = brakeFrictionCurve;
				}
				else
				{
					brakeFrictionCurve.stiffness = defaultStiffness_ForwardWheels;

					Wheel_Colliders[0].forwardFriction = brakeFrictionCurve;
					Wheel_Colliders[1].forwardFriction = brakeFrictionCurve;

					brakeFrictionCurve.stiffness = defaultStiffness_BackwardWheels;

					Wheel_Colliders[2].forwardFriction = brakeFrictionCurve;
					Wheel_Colliders[3].forwardFriction = brakeFrictionCurve;
				}

				#region Drive_Mode
				if (driveWheels == DriveType.Rear)
				{
					if (!reversing)
					{
						Wheel_Colliders[2].motorTorque = enginePower * gearRatio[currentGear + 1] * throttleInput;
						Wheel_Colliders[3].motorTorque = enginePower * gearRatio[currentGear + 1] * throttleInput;

						Wheel_Colliders[2].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower / 2, enginePower);
						Wheel_Colliders[3].motorTorque = Mathf.Clamp(Wheel_Colliders[3].motorTorque, -enginePower / 2, enginePower);
					}
					else
                    {
						Wheel_Colliders[2].motorTorque = enginePower * gearRatio[currentGear + 1] * throttleInput * 2;
						Wheel_Colliders[3].motorTorque = enginePower * gearRatio[currentGear + 1] * throttleInput * 2;

						Wheel_Colliders[2].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower , enginePower);
						Wheel_Colliders[3].motorTorque = Mathf.Clamp(Wheel_Colliders[3].motorTorque, -enginePower , enginePower);

					}
				}
				//__________________________________
				if (driveWheels == DriveType.Front)
				{
					if (!reversing)
					{
						Wheel_Colliders[0].motorTorque = enginePower * throttleInput;
						Wheel_Colliders[1].motorTorque = enginePower * throttleInput;

						Wheel_Colliders[0].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower / 2, enginePower);
						Wheel_Colliders[1].motorTorque = Mathf.Clamp(Wheel_Colliders[3].motorTorque, -enginePower / 2, enginePower);
					}
					else
                    {
						Wheel_Colliders[0].motorTorque = enginePower * throttleInput * 2;
						Wheel_Colliders[1].motorTorque = enginePower * throttleInput * 2;

						Wheel_Colliders[0].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower , enginePower);
						Wheel_Colliders[1].motorTorque = Mathf.Clamp(Wheel_Colliders[3].motorTorque, -enginePower , enginePower);

					}
				}
				//__________________________________
				if (driveWheels == DriveType.FrontRear)
				{
					if (!reversing)
					{
						Wheel_Colliders[0].motorTorque = enginePower * throttleInput;
						Wheel_Colliders[1].motorTorque = enginePower * throttleInput;

						Wheel_Colliders[0].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower / 2, enginePower);
						Wheel_Colliders[1].motorTorque = Mathf.Clamp(Wheel_Colliders[3].motorTorque, -enginePower / 2, enginePower);

						Wheel_Colliders[2].motorTorque = enginePower * throttleInput;
						Wheel_Colliders[3].motorTorque = enginePower * throttleInput;

						Wheel_Colliders[2].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower / 2, enginePower);
						Wheel_Colliders[3].motorTorque = Mathf.Clamp(Wheel_Colliders[3].motorTorque, -enginePower / 2, enginePower);
					}
					else
                    {
						Wheel_Colliders[0].motorTorque = enginePower * throttleInput * 2;
						Wheel_Colliders[1].motorTorque = enginePower * throttleInput * 2;

						Wheel_Colliders[0].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower , enginePower);
						Wheel_Colliders[1].motorTorque = Mathf.Clamp(Wheel_Colliders[3].motorTorque, -enginePower , enginePower);

						Wheel_Colliders[2].motorTorque = enginePower * throttleInput * 2;
						Wheel_Colliders[3].motorTorque = enginePower * throttleInput * 2;

						Wheel_Colliders[2].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower , enginePower);
						Wheel_Colliders[3].motorTorque = Mathf.Clamp(Wheel_Colliders[3].motorTorque, -enginePower , enginePower);

					}
				}
				//__________________________________
				if (driveWheels == DriveType.AllWheels)
				{
					if (!reversing)
					{
						for (int w = 0; w < Wheel_Colliders.Length; w++)
						{
							Wheel_Colliders[w].motorTorque = enginePower * throttleInput;

							Wheel_Colliders[w].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower / 2, enginePower);
						}
					}
					else
                    {
						for (int w = 0; w < Wheel_Colliders.Length; w++)
						{
							Wheel_Colliders[w].motorTorque = enginePower * throttleInput * 2;

							Wheel_Colliders[w].motorTorque = Mathf.Clamp(Wheel_Colliders[2].motorTorque, -enginePower , enginePower);
						}
					}
				}
				#endregion

				#region Steer
				Wheel_Colliders[0].steerAngle = Mathf.Lerp(Wheel_Colliders[1].steerAngle,
					maxSteer * steerInput, Time.deltaTime * steerSensibility);

				Wheel_Colliders[1].steerAngle = Mathf.Lerp(Wheel_Colliders[1].steerAngle,
					maxSteer * steerInput, Time.deltaTime * steerSensibility);

				Wheel_Colliders[1].steerAngle = Mathf.Clamp(Wheel_Colliders[1].steerAngle, -(maxSteer / (currentSpeed / 10)), (maxSteer / (currentSpeed / 10)));
				Wheel_Colliders[0].steerAngle = Mathf.Clamp(Wheel_Colliders[0].steerAngle, -(maxSteer / (currentSpeed / 10)), (maxSteer / (currentSpeed / 10)));
				#endregion

				#region Brake
				// Hand brake state
				if (handBrake)
				{
					// Update friction for hand brake (slip)
					handBrakeFrictionCurve.stiffness = handBrakeFriction;

					Wheel_Colliders[2].sidewaysFriction = handBrakeFrictionCurve;
					Wheel_Colliders[3].sidewaysFriction = handBrakeFrictionCurve;

					Wheel_Colliders[2].brakeTorque = brakePower;
					Wheel_Colliders[3].brakeTorque = brakePower;
				}
				else
				{
					if (isPlayer)
					{
						if (difficultyLevel == 0) // Simulation (slip on the ground tag, out of the road tag)
						{
							if (inRoad)
								handBrakeFrictionCurve.stiffness = defaultStiffness_Sideways;
							else
								handBrakeFrictionCurve.stiffness = handBrakeFriction;
						}
						if (difficultyLevel == 1) // Arcade (normal driving)
							handBrakeFrictionCurve.stiffness = defaultStiffness_Sideways;
					}
					else
						handBrakeFrictionCurve.stiffness = defaultStiffness_Sideways;

					Wheel_Colliders[2].sidewaysFriction = handBrakeFrictionCurve;
					Wheel_Colliders[3].sidewaysFriction = handBrakeFrictionCurve;

					// Brake in forward moving
					if (throttleInput < 0 && !reversing && currentSpeed > 3f)
					{
						brakeFrictionCurve.stiffness = brakeFriction;

						Wheel_Colliders[0].forwardFriction = brakeFrictionCurve;
						Wheel_Colliders[1].forwardFriction = brakeFrictionCurve;

						Wheel_Colliders[0].brakeTorque = brakePower * Mathf.Abs(throttleInput);
						Wheel_Colliders[1].brakeTorque = brakePower * Mathf.Abs(throttleInput);
						Wheel_Colliders[2].brakeTorque = brakePower * Mathf.Abs(throttleInput / 2);
						Wheel_Colliders[3].brakeTorque = brakePower * Mathf.Abs(throttleInput / 2);
						BrakeLights(1f);
						BrakeMaterial(maxHDR);
						ReverseLights(0);
					}

					// Brake in backward moving
					else if (throttleInput > 0 && reversing && currentSpeed > 3f)
					{
						brakeFrictionCurve.stiffness = brakeFriction;

						Wheel_Colliders[0].forwardFriction = brakeFrictionCurve;
						Wheel_Colliders[1].forwardFriction = brakeFrictionCurve;

						Wheel_Colliders[0].brakeTorque = brakePower * Mathf.Abs(throttleInput);
						Wheel_Colliders[1].brakeTorque = brakePower * Mathf.Abs(throttleInput);
						Wheel_Colliders[2].brakeTorque = brakePower * Mathf.Abs(throttleInput / 2);
						Wheel_Colliders[3].brakeTorque = brakePower * Mathf.Abs(throttleInput / 2);
						BrakeLights(1f);
						BrakeMaterial(maxHDR);
						ReverseLights(0);
					}
					// Release brake
					else
					{
						Wheel_Colliders[2].brakeTorque = 0;
						Wheel_Colliders[3].brakeTorque = 0;
						Wheel_Colliders[0].brakeTorque = 0;
						Wheel_Colliders[1].brakeTorque = 0;

						brakeFrictionCurve.stiffness = defaultStiffness_ForwardWheels;

						Wheel_Colliders[0].forwardFriction = brakeFrictionCurve;
						Wheel_Colliders[1].forwardFriction = brakeFrictionCurve;

						BrakeLights(0);
						BrakeMaterial(minHDR);
						ReverseLights(0);
					}
				}

				if (reversing && throttleInput < 0)
				{

					BrakeLights(0);
					BrakeMaterial(minHDR);
					ReverseLights(1f);
				}
				#endregion

			}
		}

		// Apply input system values to the vehicle
		public void Move(float motor, float steer, bool hand)
		{
			if (nitro_Mode)
				throttleInput = 1f;
			else
				throttleInput = motor;

			steerInput = steer;
			handBrake = hand;

            if (throttleInput != 0)
                vehicleAudio.releaseThrottle = false;
            else
                vehicleAudio.releaseThrottle = true;
        }

		#region Lights
		// Update lights intensity value
		void BrakeLights(float value)
		{
			for (int a = 0; a < brakeLights.Length; a++)
			{
				brakeLights[a].intensity = value;
                brakeLights[a].GetComponent<UnityEngine.Rendering.LensFlareComponentSRP>()
                    .intensity = value;
            }
			if (value > 0)
			{
				for (int a = 0; a < brakeFlares.Length; a++)
					brakeFlares[a].SetActive(true);
			}
			else
			{
                for (int a = 0; a < brakeFlares.Length; a++)
                    brakeFlares[a].SetActive(false);
            }
        }

		void ReverseLights(float value)
		{
			for (int a = 0; a < reverseLights.Length; a++)
			{
				reverseLights[a].intensity = value;
                reverseLights[a].GetComponent<UnityEngine.Rendering.LensFlareComponentSRP>()
                                        .intensity = value;
            }
            if (value > 0)
            {
                for (int a = 0; a < reverseFlares.Length; a++)
                    reverseFlares[a].SetActive(true);
            }
            else
            {
                for (int a = 0; a < reverseFlares.Length; a++)
                    reverseFlares[a].SetActive(false);
            }
        }
		void BrakeMaterial(float value)
		{
			if (brakeMaterial)
			{
				brakeMaterial.SetColor("_EmissionColor",
					new Vector4(1, 0, 0, 1) * value);
			}
		}
        #endregion

        #region Sound
        // Engine sound system calculation
        // Gear changing only used for sound system
        IEnumerator GearChanging()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.01f);
                if (!reversing)
                {
                    float f = Mathf.Abs(currentSpeed / nextGearSpeed);
                    float upgearlimit = (1 / (float)numberOfGears) * (currentGear + 1);
                    float downgearlimit = (1 / (float)numberOfGears) * currentGear;

                    // Changinbg gear down
                    if (currentGear > 0 && f < downgearlimit)
                    {
                        changingGearDown = true;

                        currentGear--;

                        if (exhaustFlame)
                        {
                            rnd = Random.Range(0, 2);
                            if (rnd == 1)
                                vehicleAudio.Play_Flame_Sound();
                        }

                    }

                    // Changing gear Up
                    if (f > upgearlimit && (currentGear < (numberOfGears - 1)))
                    {

                        changingGearUP = true;
                        changingGearDown = false;

                        if (isPlayer)
                            GetComponent<EasyCarAudio>().Play_ChangeGear_Sound();

                        // Delay before changing gear up
                        yield return new WaitForSeconds(GearShiftDelay);

                        changingGearUP = false;

                        currentGear++;
                    }
                }
                else
                {

                    if (reversing)
                        currentGear = 0;
                }
            }
        }

        // simple function to add a curved bias towards 1 for a value in the 0-1 range
        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }

        // unclamped version of Lerp, to allow value to exceed the from-to range
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }
        // Used for engine sound system    
        private void CalculateGearFactor()
        {
            float f = (1 / (float)numberOfGears);
            // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
            // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
            var targetGearFactor = Mathf.InverseLerp(f * currentGear, f * (currentGear + 1), Mathf.Abs(currentSpeed / nextGearSpeed));
            GearFactor = Mathf.Lerp(GearFactor, targetGearFactor, Time.deltaTime * 5f);
        }

        // Used for engine sound system
        private void CalculateRevs()
        {
            // calculate engine revs (for display / sound)
            // (this is done in retrospect - revs are not used in force/power calculations)
            CalculateGearFactor();
            var gearNumFactor = currentGear / (float)numberOfGears;
            var revsRangeMin = ULerp(0f, 1f, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(1f, 1f, gearNumFactor);


            #region Gear Changing UP
            if (!Clutch)
            {
                if (!nitro_Mode)
                {
                    if (changingGearUP)
                    {
                        /*if (!vehicleAudio.autoGearPitch)
						{
							vehicleAudio.engineSource.clip =
							vehicleAudio.engineSoundOFF;

							if (!vehicleAudio.engineSource.isPlaying)
								vehicleAudio.engineSource.Play();
						}*/

                        Revs = Mathf.Lerp(Revs, 0.3f, Time.deltaTime * 5);

                        enginePower = enginePowerTemp / 1.5f;

                        vehicleAudio.engineSource.pitch =
                            Mathf.Lerp(vehicleAudio.engineSource.pitch, vehicleAudio.PitchGearChanging,
                            Time.deltaTime * 2);

                        vehicleAudio.engineSource.volume =
                            Mathf.Lerp(vehicleAudio.engineSource.volume, 0.77f,
                            Time.deltaTime * 2);
                    }
                    else // Normal mode
                    {
                        /*if (!vehicleAudio.autoGearPitch)
						{
							vehicleAudio.engineSource.clip =
							vehicleAudio.engineSound;

							if (!vehicleAudio.engineSource.isPlaying)
								vehicleAudio.engineSource.Play();
						}*/

                        if (changingGearDown)
                        {
                            if (currentSpeed < 1f)
                            {
                                if (throttleInput != 0)
                                {
                                    Revs = Mathf.Lerp(0.6f, 1f, Mathf.PingPong(Time.time / 0.07f, 1));

                                }
                                else
                                {
                                    if (Revs > 0)
                                        Revs = Revs - Time.deltaTime * 1f;
                                }
                            }
                            else
                            {
                                Revs = Mathf.Lerp(Revs,
                                    ULerp(revsRangeMin, revsRangeMax, GearFactor) * gearRatio[currentGear + 1]
                                    , Time.deltaTime * 100);
                            }
                        }
                        else
                        {
                            if (currentSpeed < 1f)
                            {
                                if (throttleInput != 0)
                                {
                                    Revs = Mathf.Lerp(0.6f, 1f, Mathf.PingPong(Time.time / 0.07f, 1));
                                }
                                else
                                {
                                    if (Revs > 0)
                                        Revs = Revs - Time.deltaTime * 1f;
                                }
                            }
                            else
                            {
                                Revs = Mathf.Lerp(Revs,
                                    ULerp(revsRangeMin, revsRangeMax, GearFactor) * gearRatio[currentGear + 1]
                                    , Time.deltaTime * 1);
                            }
                        }
                        enginePower = enginePowerTemp;

                        vehicleAudio.engineSource.volume =
                            Mathf.Lerp(vehicleAudio.engineSource.volume, vehicleAudio.engineVolume,
                            Time.deltaTime * 2);
                    }
                }
                else // Nitro
                {
                    Revs = Mathf.Lerp(Revs, 1f, Time.deltaTime * 5);
                    vehicleAudio.engineSource.pitch = Mathf.Lerp
                        (vehicleAudio.engineSource.pitch,
                        vehicleAudio.PitchMax * 0.95f,
                        Time.deltaTime * 1);
                }
            }
            else // Clutch
            {
                if (Revs < 0.6f)
                    Revs = Mathf.Lerp(Revs, Mathf.Abs(throttleInput), Time.deltaTime * 10);
                else
                {
                    if (throttleInput == 1f)
                    {
                        Revs = Mathf.Lerp(0.6f, 1f, Mathf.PingPong(Time.time / 0.07f, 1));

                        shaking = true;

                        vehicleAudio.stopRandom = false;

                        if (exhaustFlame)
                            vehicleAudio.Play_RandomFlame_Sound();
                    }
                    else
                    {
                        Revs = Mathf.Lerp(Revs, Mathf.Abs(throttleInput), Time.deltaTime * 10);

                        shaking = false;

                        vehicleAudio.stopRandom = true;
                    }
                }
            }
            #endregion

        }
        #endregion
    }




}