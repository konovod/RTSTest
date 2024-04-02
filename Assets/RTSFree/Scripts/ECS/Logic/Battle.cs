#nullable enable

using System;
using ECS;
using RTSToolkitFree;
using UnityECSLink;
using UnityEngine;
using UnityEngine.AI;


namespace ECSGame
{

    public struct AttackTarget
    {
        public Entity v;
    }

    [Serializable]
    public struct AttackStats
    {
        public float strength;
    }
    [Serializable]
    public struct DefenseStats
    {
        public float defence;
    }

    [Serializable]
    public struct UnitNeedNation
    {
        public int id;
    }

    [Serializable]
    public struct UnitNation
    {
        public Entity e;
    }
    [Serializable]
    public struct Movable { }

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

    public struct ChangeColor
    {
        public ChangeColor(Color color) { v = color; }
        public Color v;
    }


    public class FindAttackTarget : ECS.System
    {
        public FindAttackTarget(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<Alive>().Inc<AttackStats>();
        }
        public override void Process(Entity e)
        {
            if (!e.Has<UnitNation>())
                return;
            var tree = e.Get<UnitNation>().e.Get<DistanceTree>();
            var targetId = tree.targetKD.FindNearest(e.Get<LinkedGameObject>().Transform().position);
            if (targetId < 0)
            {
                e.RemoveIfPresent<AttackTarget>();
                return;
            }
            var target = tree.targets[targetId];
            AttackTarget comp;
            comp.v = target;
            e.Set(comp);
            e.Set(new ChangeColor(Color.yellow));
        }
    }

    public class RecolorUnit : ECS.System
    {
        public RecolorUnit(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<ChangeColor>();//.Inc<LinkedComponent<Renderer>>();
        }
        public override void Process(Entity e)
        {
            e.Get<LinkedComponent<Renderer>>().v.material.color = e.Get<ChangeColor>().v;

        }
    }

    public class ApproachTarget : ECS.System
    {
        public ApproachTarget(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<AttackStats>().Inc<AttackTarget>().Inc<LinkedComponent<NavMeshAgent>>();
        }
        public override void Process(Entity e)
        {
            var agent = e.Get<LinkedComponent<NavMeshAgent>>().v;
            var transform = e.Get<LinkedGameObject>().Transform();
            var target = e.Get<AttackTarget>().v;
            var target_transform = target.Get<LinkedGameObject>().Transform();

            agent.stoppingDistance = agent.radius / (transform.localScale.x) + target.Get<LinkedComponent<NavMeshAgent>>().v.radius / (target_transform.localScale.x);
            float stoppDistance = (2f + transform.localScale.x * target_transform.localScale.x * agent.stoppingDistance);
            var distance = (transform.position - target_transform.position).magnitude;

            // если приближающийся уже близок к своей цели
            if (distance < stoppDistance)
            {
                agent.SetDestination(transform.position);
                e.Set(new ChangeColor(Color.red));
            }
            else
            {
                e.Set(new ChangeColor(Color.green));
                // начинаем двигаться
                if (e.Has<Movable>())
                {
                    if ((agent.destination - target_transform.position).sqrMagnitude > 1f)
                    {
                        agent.SetDestination(target_transform.position);
                        agent.speed = 3.5f;
                    }
                }
            }
        }
    }

}