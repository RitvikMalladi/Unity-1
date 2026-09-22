//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class Racer_Nitro : MonoBehaviour
    {
        public float randomizerDelay = 5f;
        public NitroBoostPower nitroBoost = NitroBoostPower.X2;

        // Nitro sound effect
        public AudioSource nitroSource;

        // Nitro particle effects
        public GameObject[] nitroParticles;

        Rigidbody carRigidbody;
        EasyCarController carController;

        bool nitroState;
        int randomNitro;
        float mass = 0;

        [HideInInspector] public bool raceIsStarted;
        [HideInInspector] public bool inSpeedLimiter;
        [HideInInspector] public bool frontHit;

        IEnumerator Start()
        {
            carRigidbody = GetComponent<Rigidbody>();
            carController = GetComponent<EasyCarController>();
            mass = carRigidbody.mass;

            for (int a = 0; a < nitroParticles.Length; a++)
            {
                var emi = nitroParticles[a].GetComponent<ParticleSystem>().emission;
                emi.enabled = false;
            }

            yield return new WaitForSeconds(10f);


            while (true)
            {
                if (raceIsStarted)
                {
                    randomNitro = Random.Range(0, 2);

                    if (randomNitro == 0)
                        nitroState = false;
                    else
                        nitroState = true;


                    if (nitroState)
                    {
                        if (!inSpeedLimiter && !frontHit)
                        {
                            if (nitroBoost == NitroBoostPower.X0)
                                carRigidbody.mass = mass;
                            if (nitroBoost == NitroBoostPower.X1)
                                carRigidbody.mass = mass / 2;
                            if (nitroBoost == NitroBoostPower.X2)
                                carRigidbody.mass = mass / 3;
                            if (nitroBoost == NitroBoostPower.X3)
                                carRigidbody.mass = mass / 4;

                            for (int a = 0; a < nitroParticles.Length; a++)
                            {
                                var emi = nitroParticles[a].GetComponent<ParticleSystem>().emission;
                                emi.enabled = true;
                            }

                            if (!nitroSource.isPlaying)
                                nitroSource.Play();
                        }
                        else
                        {
                            carRigidbody.mass = mass;

                            for (int a = 0; a < nitroParticles.Length; a++)
                            {
                                var emi = nitroParticles[a].GetComponent<ParticleSystem>().emission;
                                emi.enabled = false;
                            }

                            if (nitroSource.isPlaying)
                                nitroSource.Stop();
                        }
                    }// Nitro state = true
                    else
                    {
                        carRigidbody.mass = mass;

                        for (int a = 0; a < nitroParticles.Length; a++)
                        {
                            var emi = nitroParticles[a].GetComponent<ParticleSystem>().emission;
                            emi.enabled = false;
                        }

                        if (nitroSource.isPlaying)
                            nitroSource.Stop();
                    }// Nitro state = false
                }// Race is started

                yield return new WaitForSeconds(randomizerDelay);

            }
        }
    }
}