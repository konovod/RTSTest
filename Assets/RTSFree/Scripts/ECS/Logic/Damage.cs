#nullable enable

using System;
using ECS;
using RTSToolkitFree;
using UnityECSLink;
using UnityEngine;
using UnityEngine.AI;


namespace ECSGame
{
    [Serializable]
    public struct Health
    {
        public int V;
        public int Max;
    }
    public struct Alive { }
    public struct Dying { }
    public struct Rotting { }

    public struct AttackHit
    {
        public int damage;
        public Entity target;
        public Entity source;
    }


    public class UnitAttackTargets : ECS.System
    {
        public UnitAttackTargets(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<AttackStats>().Inc<ShouldAttack>();
        }
        public override void Process(Entity e)
        {
            var transform = e.Get<LinkedGameObject>().Transform();
            var target = e.Get<ShouldAttack>().target;
            var target_transform = target.Get<LinkedGameObject>().Transform();
            var distance = (transform.position - target_transform.position).magnitude;
            var attack_distance = 2f + transform.localScale.x * target_transform.localScale.x * e.Get<LinkedComponent<NavMeshAgent>>().v.stoppingDistance;

            if (distance > attack_distance)
            {
                e.Remove<ShouldAttack>();
                e.Add(new ShouldApproach(target));
                e.Set(new ChangeColor(Color.yellow));
                return;
            }
            e.Set(new ChangeColor(Color.red));


        }
    }

}