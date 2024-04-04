#nullable enable

using System;
using ECS;
using UnityECSLink;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;


namespace ECSGame
{


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
            return world.Inc<Alive>().Inc<AttackStats>().Inc<ShouldFindTarget>().Inc<LogicActive>();
        }
        public override void Process(Entity e)
        {
            if (!e.Has<UnitNation>())
                return;
            var tree = e.Get<UnitNation>().e.Get<DistanceTree>();
            var transform = e.Get<LinkedGameObject>().Transform();
            var targetId = tree.targetKD.FindNearest(transform.position);
            if (targetId < 0)
            {
                e.Set(new ChangeColor(Color.yellow));
                e.RemoveIfPresent<ShouldApproach>();
                e.RemoveIfPresent<ShouldAttack>();
                return;
            }
            var target = tree.targets[targetId];
            e.Remove<ShouldFindTarget>();
            e.RemoveIfPresent<ShouldAttack>();
            e.Set(new ShouldApproach(target));
            e.Set(new ChangeColor(Color.green));
            var agent = e.Get<LinkedComponent<NavMeshAgent>>().v;
            var target_transform = target.Get<LinkedGameObject>().Transform();
            if (!agent.enabled)
                agent.enabled = true;
            agent.stoppingDistance = agent.radius / transform.localScale.x + target.Get<LinkedComponent<NavMeshAgent>>().v.radius / target_transform.localScale.x;
            e.GetRef<AttackStats>().distance = 2f + transform.localScale.x * target_transform.localScale.x * agent.stoppingDistance;
            LogicActive.WaitFor(e, UnityEngine.Random.Range(0.5f, 1f));
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
            return world.Inc<ShouldApproach>().Inc<Movable>().Inc<Alive>().Inc<LogicActive>();
        }
        public override void Process(Entity e)
        {
            var agent = e.Get<LinkedComponent<NavMeshAgent>>().v;
            var transform = e.Get<LinkedGameObject>().Transform();
            var target = e.Get<ShouldApproach>().target;
            var target_transform = target.Get<LinkedGameObject>().Transform();
            float stoppDistance = e.Get<AttackStats>().distance;
            var distance = (transform.position - target_transform.position).magnitude;

            // если приближающийся уже близок к своей цели
            if (distance < stoppDistance)
            {
                agent.SetDestination(transform.position);
                e.Remove<ShouldApproach>();
                e.Add(new ShouldAttack(target));
                e.Set(new ChangeColor(Color.red));
            }
            else if ((agent.destination - target_transform.position).sqrMagnitude > MathF.Max(1f, distance * distance * 0.01f))
            {
                agent.SetDestination(target_transform.position);
            }
        }
    }

}