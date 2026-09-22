using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ALIyerEdon
{
    public class RPM : MonoBehaviour
    {
        [Header("RPM Meter___________________________________________")]
        public Image rpmSlider;
        public Text speed;
        public Text gear;
        public float speedMultiplier = 1.7f;

        [Header("Display___________________________________________")]
        public UnityEngine.UI.Image radialImage; // Assign your Image component here
        public RectTransform targetObject;      // Assign the 2D object to rotate
        public float minAngle = 30;             // Minimum angle (in degrees)
        public float maxAngle = 300;           // Maximum angle (in degrees)


        ALIyerEdon.EasyCarController carController;
        float rpm;

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            carController = GameObject.FindGameObjectWithTag("Player").GetComponent
                <ALIyerEdon.EasyCarController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (carController)
            {
                // Ensure the image is a filled radial type
                if (radialImage.type != UnityEngine.UI.Image.Type.Filled || radialImage.fillMethod != UnityEngine.UI.Image.FillMethod.Radial360)
                {
                    Debug.LogWarning("Image must be of type 'Filled' with 'Radial 360' fill method.");
                    return;
                }

                // Calculate the angle based on the fill amount
                float angle = radialImage.fillAmount * 360f;

                // Clamp the angle within the specified min and max range
                angle = Mathf.Clamp(angle, minAngle, maxAngle);

                // Apply the rotation to the target object's RectTransform
                targetObject.localRotation = Quaternion.Euler(0f, 0f, -angle); // Negative to match Unity's clockwise rotation

                //////////////////////////////////////////////////

                rpm = Mathf.Lerp(rpm, carController.Revs, Time.deltaTime * 5f);

                rpmSlider.fillAmount = rpm - 0.1f;
                rpmSlider.fillAmount = Mathf.Clamp(rpmSlider.fillAmount, 0.07f, 0.7f);

                speed.text = Mathf.RoundToInt(
                    (Mathf.Lerp(carController.currentSpeed, carController.currentSpeed, Time.deltaTime * 2f) * speedMultiplier)
                    ).ToString() + " km";

                gear.text = Mathf.Lerp((1 + carController.currentGear), (1 + carController.currentGear),
                Time.deltaTime * 5f).ToString();
            }


        }
    }
}