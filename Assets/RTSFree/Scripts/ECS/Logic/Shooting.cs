#nullable enable

using System;
using System.Dynamic;
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

    public struct Melee
    {
    }

    [Serializable]
    public struct RangedWeapon
    {
        public GameObject projectile;
        public Transform shootPoint;
        public float force;
        public float shootingDelay;
    }

    [Serializable]
    public struct Bullet
    {
        public Entity source;
        public float damage;
    }
    public struct BulletInitialForce
    {
        public BulletInitialForce(Vector3 force) { v = force; }
        public Vector3 v;
    }

    [Serializable]
    public struct BulletHit
    {
        public Entity target;
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


    public class RangedAttacks : ECS.System
    {
        public RangedAttacks(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<PerformAttack>().Inc<RangedWeapon>();
        }

        public override void Process(Entity e)
        {
            var gun = e.Get<RangedWeapon>();
            var created = world.NewEntity();
            UnityECSLink.InstantiateGameObject request;
            request.pos = gun.shootPoint.position;
            request.rot = gun.shootPoint.rotation;
            request.Template = gun.projectile;
            created.Add(request);
            created.Add(new BulletInitialForce(gun.shootPoint.forward * gun.force));
            Bullet bullet;
            bullet.damage = e.Get<AttackStats>().Strength;
            bullet.source = e;
            created.Add(bullet);
            LogicActive.WaitFor(e, gun.shootingDelay);

            UnityECSLink.AddRequest add_request;
            add_request.Component = typeof(DestroyGameObject);
            add_request.entity = created;
            add_request.time = e.World.FirstComponent<UnityECSLink.GlobalTime>().Time + 5f;
            world.NewEntity().Add(add_request);


        }
    }

    public class BulletsApplyInitialForce : ECS.System
    {
        public BulletsApplyInitialForce(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<BulletInitialForce>().Inc<LinkedGameObject>();
        }
        public override void Process(Entity e)
        {
            e.Get<LinkedGameObject>().Obj.GetComponent<Rigidbody>().AddForce(e.Get<BulletInitialForce>().v);
            e.Remove<BulletInitialForce>();
        }
    }

    public class ProcessBulletHits : ECS.System
    {
        public ProcessBulletHits(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<BulletHit>();
        }
        public override void Process(Entity e)
        {
            AttackHit hit;
            var bullet = e.Get<Bullet>();
            hit.damage = 2.0f * bullet.damage * UnityEngine.Random.value;
            hit.source = bullet.source;
            hit.target = e.Get<BulletHit>().target;
            world.NewEntity().Add(hit);
            e.Add(new DestroyGameObject());
        }
    }


}