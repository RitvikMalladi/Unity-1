//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public enum NitroBoostPower
    {
        X0, X1, X2, X3
    }

    public class Nitro_Feature : MonoBehaviour
    {
        [Space(10)]
        // Enabled or disable nitro feature for this player car
        public bool enableNitro = true;

        [Space(5)]
        // Add force amount to the player rigidbody at nitro
        public NitroBoostPower nitroBoost = NitroBoostPower.X2;

        // Reduce nitro rate when is using
        public float reduceRate = 50f;
        public float increaseRate = 10f;

        // Nitro sound effect
        public AudioSource nitroSource;

        // Nitro particle effects
        public GameObject[] nitroParticles;

        [HideInInspector] public float Amount = 100f;
        [HideInInspector] public bool raceIsStarted;

        void Start()
        {
            for (int a = 0; a <  nitroParticles.Length; a++)
            {
                var emi = nitroParticles[a].GetComponent<ParticleSystem>().emission;
                emi.enabled = false;
            }
        }
    }
}