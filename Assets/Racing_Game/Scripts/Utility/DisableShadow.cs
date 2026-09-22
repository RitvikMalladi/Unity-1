using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALIyerEdon
{
    public class DisableShadow : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            GetComponent<Light>().shadows = LightShadows.None;
        }
    }
}