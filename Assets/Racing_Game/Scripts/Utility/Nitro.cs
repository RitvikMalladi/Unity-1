//______________________________________________
// ALIyerEdon
// https://assetstore.unity.com/publishers/23606
//______________________________________________

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ALIyerEdon
{
    public class Nitro : MonoBehaviour
    {
        public Image nitroSliderMobile;
        public Image nitroSliderPC;

        public GameObject mobileUI;
        public GameObject PcUI;


        Rigidbody carRigidbody;
        EasyCarController carController;
        Nitro_Feature nitroController;
        InputSystem inputSystem;
            
        bool nitroState = false;
        bool PC_Mode;
        float mass = 0;
        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            // Disable nitro UI if the player car has no nitro feature component
            if (!FindObjectOfType<Nitro_Feature>().enableNitro)
            {
                mobileUI.SetActive(false);
                PcUI.SetActive(false);
            }
            else
            {
                StartCoroutine(Init_Nitro());
            }
        }

        IEnumerator Init_Nitro()
        {
            yield return new WaitForEndOfFrame();
                        
            carController = GameObject.FindGameObjectWithTag("Player")
                .GetComponent<EasyCarController>();

            carRigidbody = carController.GetComponent<Rigidbody>();
            mass = carRigidbody.mass;

            nitroController = carController.GetComponent<Nitro_Feature>();

            inputSystem = FindObjectOfType<InputSystem>();

            if (inputSystem.controlType == InputType.Keyboard)
            {
                PC_Mode = true;
                mobileUI.SetActive(false);
                PcUI.SetActive(true);
            }
            if (inputSystem.controlType == InputType.Mobile)
            {
                PC_Mode = false;
                mobileUI.SetActive(true);
                PcUI.SetActive(false);
            }
        }

        void Update()
        {
            if (!carController || !nitroController.raceIsStarted)
                return;

            if(PC_Mode)
            {
                if (Gamepad.current != null)
                {
                    if (Gamepad.current.buttonSouth.ReadValue() > 0)
                    {
                        if (!carController || !nitroController.raceIsStarted)
                            return;

                        if (nitroController.Amount > 0)
                            nitroState = true;
                        else
                            nitroState = false;
                    }
                    else
                    {
                        if (!carController || !nitroController.raceIsStarted)
                            return;

                        nitroState = false;
                    }
                }
                else
                {
                    if (Keyboard.current != null)
                    {
                        if (Keyboard.current.leftShiftKey.ReadValue() > 0)
                        {
                            if (!carController || !nitroController.raceIsStarted)
                                return;

                            if (nitroController.Amount > 0)
                                nitroState = true;
                            else
                                nitroState = false;

                        }
                        if (Keyboard.current.leftShiftKey.ReadValue() == 0)
                        {
                            if (!carController || !nitroController.raceIsStarted)
                                return;

                            nitroState = false;
                        }
                    }
                }
            }

            if (!nitroState && nitroController.Amount < 100)
            {
                nitroController.Amount += (nitroController.increaseRate * Time.deltaTime);

                if (nitroController.nitroSource.isPlaying)
                    nitroController.nitroSource.Stop();

                carController.nitro_Mode = false;

                carRigidbody.mass = mass;

                for (int a = 0; a < nitroController.nitroParticles.Length; a++)
                {
                    var emi = nitroController.nitroParticles[a].GetComponent<ParticleSystem>().emission;
                    emi.enabled = false;
                }
            }
            if (nitroState && nitroController.Amount > 0)
            {
                nitroController.Amount -= (nitroController.reduceRate * Time.deltaTime);

                // Reduce mass of the car at nitro mode to move faster !!!
                if(nitroController.nitroBoost == NitroBoostPower.X1)
                    carRigidbody.mass = mass / 2;
                if (nitroController.nitroBoost == NitroBoostPower.X2)
                    carRigidbody.mass = mass / 3;
                if (nitroController.nitroBoost == NitroBoostPower.X3)
                    carRigidbody.mass = mass / 4;

                if (!nitroController.nitroSource.isPlaying)
                    nitroController.nitroSource.Play();

                carController.nitro_Mode = true;

                for (int a = 0; a < nitroController.nitroParticles.Length; a++)
                {
                    var emi = nitroController.nitroParticles[a].GetComponent<ParticleSystem>().emission;
                    emi.enabled = true;
                }
            }
            if (nitroState && nitroController.Amount < 0)
            {
                if (nitroController.nitroSource.isPlaying)
                    nitroController.nitroSource.Stop();

                carController.nitro_Mode = false;

                carRigidbody.mass = mass;

                for (int a = 0; a < nitroController.nitroParticles.Length; a++)
                {
                    var emi = nitroController.nitroParticles[a].GetComponent<ParticleSystem>().emission;
                    emi.enabled = false;
                }
            }

            nitroSliderMobile.fillAmount = nitroController.Amount / 100;
            nitroSliderPC.fillAmount = nitroController.Amount / 100;
        }

        public void Apply_Nitro()
        {
            if (!carController || !nitroController.raceIsStarted)
                return;

            if (nitroController.Amount > 0)
                nitroState = true;
            else
                nitroState = false;
        }
        public void Release_Nitro()
        {
            if (!carController || !nitroController.raceIsStarted)
                return;

            nitroState = false;
        }
    }
}