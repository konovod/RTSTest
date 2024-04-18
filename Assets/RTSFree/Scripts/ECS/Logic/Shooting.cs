#nullable enable

using System;
using ECS;
using UnityECSLink;
using UnityEngine;


namespace ECSGame
{

    [Serializable]
    public struct GunHead
    {
        public Transform v;
        public Vector3 originalRotation;
    }

    [Serializable]
    public struct AimingAnimation
    {
        public float seekSpeed;
        public float rotateAngle;
        public float dampingSpeed;
    }


    public class AimTurretToTarget : ECS.System
    {
        public AimTurretToTarget(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<GunHead>().Inc<AimingAnimation>();
        }
        public override void Process(Entity e)
        {
            var head = e.Get<GunHead>();
            var anim = e.Get<AimingAnimation>();
            var time = e.World.FirstComponent<UnityECSLink.GlobalTime>().Time;

            if (e.Has<HasTarget>())
            {
                var target_position = e.Get<HasTarget>().v.Get<Position>().v;
                Vector3 lookPos = target_position - head.v.position;
                lookPos.y = 0;
                Quaternion rotation = Quaternion.LookRotation(lookPos);
                head.v.rotation = Quaternion.Slerp(head.v.rotation, rotation, Time.deltaTime * anim.dampingSpeed);
            }
            // else
            //     head.v.localRotation = Quaternion.Euler(head.originalRotation.x, Mathf.PingPong(time * anim.seekSpeed, anim.rotateAngle * 2) - anim.rotateAngle, 1f);
        }
    }


}