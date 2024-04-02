#nullable enable

using System;
using ECS;
using RTSToolkitFree;
using UnityECSLink;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;


namespace ECSGame
{
    [Serializable]
    public struct AttackStats
    {
        public float strength;
    }
    [Serializable]
    public struct DefenseStats
    {
        public float defense;
    }
    [Serializable]
    public struct MaxHealth
    {
        public float v;
    }
    public struct Health
    {
        public Health(float value) { v = value; }
        public float v;
    }
    public struct Alive { }
    public struct Dying { }
    public struct Rotting { }

    public struct AttackHit
    {
        public float damage;
        public Entity target;
        public Entity source;
    }


    public class UnitAttackTargets : ECS.System
    {
        public UnitAttackTargets(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<AttackStats>().Inc<ShouldAttack>().Inc<Alive>();
        }
        public override void Process(Entity e)
        {
            var transform = e.Get<LinkedGameObject>().Transform();
            var target = e.Get<ShouldAttack>().target;
            var target_transform = target.Get<LinkedGameObject>().Transform();
            var distance = (transform.position - target_transform.position).magnitude;
            var attack_distance = 2f + transform.localScale.x * target_transform.localScale.x * e.Get<LinkedComponent<NavMeshAgent>>().v.stoppingDistance;
            if (!target.Has<Alive>() || distance > attack_distance)
            {
                e.Remove<ShouldAttack>();
                e.Add(new ShouldFindTarget());
                e.Set(new ChangeColor(Color.yellow));
                // Debug.Log($"#{distance} > {attack_distance}");
                return;
            }
            var strength = e.Get<AttackStats>().strength;
            var defense = target.Get<DefenseStats>().defense;
            if (UnityEngine.Random.value > (strength / (strength + defense)))
            {
                AttackHit hit;
                hit.damage = 2.0f * strength * UnityEngine.Random.value;
                hit.source = e;
                hit.target = target;
                world.NewEntity().Add(hit);
            }
        }
    }

    public class ApplyDamage : ECS.System
    {
        public ApplyDamage(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<AttackHit>();
        }
        public override void Process(Entity e)
        {
            var hit = e.Get<AttackHit>();
            var target = hit.target;
            if (!target.Has<Alive>())
                return;
            var health = 0f;
            if (target.Has<Health>())
                health = target.Get<Health>().v;
            else
                health = target.Get<MaxHealth>().v;
            health -= hit.damage;
            // Debug.Log(health);
            if (health <= 0)
            {
                target.Remove<Alive>();
                target.Add(new Dying());
                target.Set(new ChangeColor(Color.blue));

                // UnityECSLink.AddRequest add_request;
                // add_request.Component = typeof(DestroyGameObject);
                // add_request.Entity = target;
                // add_request.time = world.FirstComponent<UnityECSLink.GlobalTime>().Time + 5;
                // world.NewEntity().Add(add_request);

            }
            target.Set(new Health(health));
        }
    }

}