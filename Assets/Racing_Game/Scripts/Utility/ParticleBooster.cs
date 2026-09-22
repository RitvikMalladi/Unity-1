using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class ParticleBooster : MonoBehaviour
    {
        float originalSpeed;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            originalSpeed = GetComponent<ParticleSystem>().main.simulationSpeed;
            var emi = GetComponent<ParticleSystem>().main;
            emi.simulationSpeed = 100000f;
            yield return new WaitForSeconds(0.123f);
            emi.simulationSpeed = originalSpeed;
        }
    }
}
