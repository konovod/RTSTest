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

    public struct ChangeColor
    {
        public ChangeColor(Color color) { v = color; }
        public Color v;
    }

    public struct ShouldAttack
    {
        public ShouldAttack(Entity whom) { target = whom; }
        public Entity target;

    }
    public struct ShouldApproach
    {
        public ShouldApproach(Entity whom) { target = whom; }
        public Entity target;

    }
    public struct ShouldFindTarget { }

    public class FindAttackTarget : ECS.System
    {
        public FindAttackTarget(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<Alive>().Inc<AttackStats>().Inc<ShouldFindTarget>();
        }
        public override void Process(Entity e)
        {
            if (!e.Has<UnitNation>())
                return;
            var tree = e.Get<UnitNation>().e.Get<DistanceTree>();
            var targetId = tree.targetKD.FindNearest(e.Get<LinkedGameObject>().Transform().position);
            if (targetId < 0)
            {
                e.RemoveIfPresent<ShouldApproach>();
                e.RemoveIfPresent<ShouldAttack>();
                e.Set(new ChangeColor(Color.yellow));
                return;
            }
            var target = tree.targets[targetId];
            e.Set(new ChangeColor(Color.yellow));
            e.Remove<ShouldFindTarget>();
            e.RemoveIfPresent<ShouldAttack>();
            e.Set(new ShouldApproach(target));
        }
    }

    public class RecolorUnit : ECS.System
    {
        public RecolorUnit(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<ChangeColor>().Inc<LinkedComponent<Renderer>>();
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
            return world.Inc<ShouldApproach>().Inc<Movable>();
        }
        public override void Process(Entity e)
        {
            var agent = e.Get<LinkedComponent<NavMeshAgent>>().v;
            var transform = e.Get<LinkedGameObject>().Transform();
            var target = e.Get<ShouldApproach>().target;
            var target_transform = target.Get<LinkedGameObject>().Transform();

            agent.stoppingDistance = agent.radius / transform.localScale.x + target.Get<LinkedComponent<NavMeshAgent>>().v.radius / target_transform.localScale.x;
            float stoppDistance = 2f + transform.localScale.x * target_transform.localScale.x * agent.stoppingDistance;
            var distance = (transform.position - target_transform.position).magnitude;

            // если приближающийся уже близок к своей цели
            if (distance < stoppDistance)
            {
                agent.SetDestination(transform.position);
                e.Remove<ShouldApproach>();
                e.Add(new ShouldAttack(target));
                e.Set(new ChangeColor(Color.yellow));
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