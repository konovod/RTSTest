// AliyerEdon@mail.com Christmas 2022
// use this component to set up your own customized weapon for every actor (enemy, defender)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Projectile (or bullet) to spawn
    public GameObject projectile;

    // Instantiate the projectile and add force from this point
    public Transform shootPoint;

    // Add force to the projectile(or bullet)
    public float force = 100f;

    // Firing rate
    public float shootingDelay = 1f;



    [HideInInspector] public bool canShoot = false;

    IEnumerator Start()
    {

        while (true)
        {
            // Delay before each fire
            yield return new WaitForSeconds(shootingDelay);

            if (canShoot)
            {
                // Instantiate bullet
                GameObject bullet = Instantiate(projectile, shootPoint.position, shootPoint.rotation) as GameObject;

                // Add force to the Instantiated bullet
                bullet.GetComponent<Rigidbody>().AddForce(shootPoint.forward * force);
            }
        }
    }
}
