// AliyerEdon@mail.com Christmas 2022
// Use this component to manage the projectiles or bullets items

using System.Collections;
using System.Collections.Generic;
using ECSGame;
using UnityECSLink;
using UnityEngine;

namespace RTSToolkitFree
{

    public class Projectile : MonoBehaviour
    {
        void OnCollisionEnter(Collision collision)
        {
            var linked = GetComponent<LinkedEntity>().entity;
            if (linked.Has<BulletHit>())
                return;
            if (collision.gameObject.TryGetComponent<LinkedEntity>(out var target))
            {
                BulletHit hit;
                hit.target = target.entity;
                if (!linked.Has<BulletHit>())
                    linked.Add(hit);
            }
        }
    }
}