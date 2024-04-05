using RTSToolkitFree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender_AI : MonoBehaviour
{
    // Gun head for rotation (seek animation and look at the target mode)
    public Transform gunHead;

    // Use damping to have smoother look at the target rotation
    public float dampingSpeed = 10f;

    // Start shooting from this distance from the target enemy 
    public float shootingDistance = 30f;

    public float seekSpeed = 50f;
    public float rotateAngle = 70f;

    // Internal variables
    Vector3 originalRotation;
    bool isActive;
    Transform target;

    private Weapon weapon;

    void Start()
    {
        weapon = GetComponent<Weapon>();
        StartCoroutine(Control());
    }



    IEnumerator Control()
    {
        // Save the original rotation of the gun head
        originalRotation = gunHead.localRotation.eulerAngles;

        while (true)
        {
            // Find the closest enemy
            target = FindClosestEnemy();

            if (weapon != null && target != null)
            {
                // Check that the distance from the enemy is in the shooting distance range
                if (Vector3.Distance(transform.position, target.position) <= shootingDistance)
                {
                    // Start attach
                    weapon.canShoot = true;
                    isActive = true;
                }
                else
                {
                    // Stop attach
                    weapon.canShoot = false;
                    isActive = false;
                }
            }
            else
            {
                // The enemy is out of the shooting range
                weapon.canShoot = false;
                isActive = false;
            }
            // Use delay to have better performance (instead of update function)
            yield return new WaitForSeconds(0.3f);
        }
    }

    void Update()
    {
        if (isActive)
        {
            if (target)
            {
                // Look at the target code
                Vector3 lookPos = target.position - gunHead.position;
                lookPos.y = 0;
                Quaternion rotation = Quaternion.LookRotation(lookPos);
                gunHead.rotation = Quaternion.Slerp(gunHead.rotation, rotation, Time.deltaTime * dampingSpeed);
            }
        }
        else
        {
            // Weapon head's seek animation (when the enemy is not available or it's out of the shooting distance)        if (!isActive)
            gunHead.localRotation = Quaternion.Euler(originalRotation.x, Mathf.PingPong(Time.time * seekSpeed, rotateAngle * 2) - rotateAngle, 1f);
        }
    }

    Transform FindClosestEnemy()
    {
        Transform closest = null;
        // Unit unit = BattleSystem.active.FindNearest(1, transform.position);
        // if (unit != null)
        // { 
        //     closest = unit.transform;
        // }
        return closest;
    }


}
