#nullable enable

using System;
using ECS;
using UnityECSLink;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace ECSGame
{
    public struct Attackers
    {
        public HashSet<Entity> v;
    }
    [Serializable]
    public struct MaxAttackers
    {
        public int v;
    }
    public struct HasTarget
    {
        public HasTarget(Entity whom) { v = whom; }
        public Entity v;
    }
    public struct RemoveTarget { }
    public struct ShouldAttack { }

    public class FindAttackTarget : ECS.System
    {
        public FindAttackTarget(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<Alive>().Inc<AttackStats>().Exc<HasTarget>().Inc<LogicActive>().Exc<UnitCommand>();
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
                e.RemoveIfPresent<ShouldAttack>();
                return;
            }
            var target = tree.targets[targetId];
            e.RemoveIfPresent<ShouldAttack>();
            e.Set(new HasTarget(target));
            target.Get<Attackers>().v.Add(e);
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

    public class ApproachTarget : ECS.System
    {
        public ApproachTarget(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<Movable>().Inc<Alive>().Inc<LogicActive>().Inc<HasTarget>().Exc<ShouldAttack>().Exc<UnitCommand>();
        }
        public override void Process(Entity e)
        {
            var agent = e.Get<LinkedComponent<NavMeshAgent>>().v;
            var position = e.Get<Position>().v;
            var target = e.Get<HasTarget>().v;
            var target_position = target.Get<Position>().v;
            float stoppDistance = e.Get<AttackStats>().distance;
            var distance = (position - target_position).magnitude;

            // если приближающийся уже близок к своей цели
            if (distance < stoppDistance)
            {
                agent.SetDestination(position);
                e.Add(new ShouldAttack());
                e.Set(new ChangeColor(Color.red));
            }
            else if ((agent.destination - target_position).sqrMagnitude > MathF.Max(1f, distance * distance * 0.01f))
            {
                agent.SetDestination(target_position);
                LogicActive.WaitFor(e, UnityEngine.Random.Range(0.4f, 0.7f));
                if (UnityEngine.Random.Range(0, 4) == 0)
                {
                    e.Set(new RemoveTarget());
                }
            }
        }
    }
    public class RemoveUnitTargets : ECS.System
    {
        public RemoveUnitTargets(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<RemoveTarget>();
        }
        public override void Process(Entity e)
        {
            var target = e.Get<HasTarget>().v;
            target.Get<Attackers>().v.Remove(e);
            e.Remove<HasTarget>();
            e.RemoveIfPresent<ShouldAttack>();
        }
    }

}