using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ALIyerEdon
{
    public class Car_Health : MonoBehaviour
    {
        public float totalHealth = 100;
        public float damageAmount = 10f;
        public float collisionVelocity = 30f;
        public ParticleSystem damageSmoke;

        EasyCarController carController;
        bool can_EmitSmoke;
        GameOver gameOver;

        public void Refill_Health()
        {
            totalHealth = 100f;
            gameOver.healthBar.value = totalHealth;
            gameOver.fillArea.color = Color.green;            
            var emi = damageSmoke.emission;
            emi.enabled = false;
        }

        IEnumerator Start()
        {

            gameOver = FindFirstObjectByType<Race_Manager>().gameoverMenu;
           
            gameOver.healthBar.value = totalHealth;

            carController = GetComponent<EasyCarController>();

            if (damageSmoke)
            {
                var emi = damageSmoke.emission;
                emi.enabled = false;
            }

            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                if (carController.currentSpeed > 50f)
                    can_EmitSmoke = false;
                else
                    can_EmitSmoke = true;

                if (!can_EmitSmoke)
                {
                    var emi = damageSmoke.emission;

                        emi.enabled = false;
                }
                else
                {
                    if (totalHealth <= 30)
                    {
                        var emi = damageSmoke.emission;

                            emi.enabled = true;
                    }
                }
            }
        }
        void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > collisionVelocity)
            {
                totalHealth = totalHealth - damageAmount;

                gameOver.healthBar.value = totalHealth;

                if (damageSmoke)
                {
                    if (totalHealth <= 30)
                    {
                        var emi = damageSmoke.emission;
                        emi.enabled = true;

                        gameOver.fillArea.color = Color.red;
                    }
                    else
                    {
                        var emi = damageSmoke.emission;
                        emi.enabled = false;
                        gameOver.fillArea.color = Color.green;
                    }
                }

                if (totalHealth <= 0)
                    gameOver.Game_Over_Now();
            }
        }
    }
}