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
            head.v.localRotation = Quaternion.Euler(head.originalRotation.x, Mathf.PingPong(time * anim.seekSpeed, anim.rotateAngle * 2) - anim.rotateAngle, 1f);
        }
    }


}