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
        public static bool USE_IT = false;
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
        public override void PreProcess()
        {
            foreach (var tree in world.Each<DistanceTree>())
                tree.GetRef<DistanceTree>().frame_requests = 0;
        }


        public override void Process(Entity e)
        {
            if (!e.Has<UnitNation>())
                return;
            ref var tree = ref e.Get<UnitNation>().e.GetRef<DistanceTree>();
            var transform = e.Get<LinkedGameObject>().Transform();
            tree.frame_requests++;
            if (!tree.FindNearest(transform.position, out var target))
            {
                e.Set(new ChangeColor(Color.yellow));
                e.RemoveIfPresent<ShouldAttack>();
                return;
            }
            e.RemoveIfPresent<ShouldAttack>();
            e.Set(new HasTarget(target));
            if (MaxAttackers.USE_IT)
                target.Get<Attackers>().v.Add(e);
            e.Set(new ChangeColor(Color.green));
            var agent = e.Get<LinkedComponent<NavMeshAgent>>().v;
            var target_transform = target.Get<LinkedGameObject>().Transform();
            if (!agent.enabled)
                agent.enabled = true;
            agent.stoppingDistance = agent.radius / transform.localScale.x + target.Get<LinkedComponent<NavMeshAgent>>().v.radius / target_transform.localScale.x;
            e.GetRef<AttackStats>().TotalDistance = e.Get<AttackStats>().WeaponDistance + 2f + transform.localScale.x * target_transform.localScale.x * agent.stoppingDistance;
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
            float stoppDistance = e.Get<AttackStats>().TotalDistance;
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
                // LogicActive.WaitFor(e, UnityEngine.Random.Range(0.4f, 0.7f));
            }
        }
    }

    public class RetargetUnits : ECS.System
    {
        public RetargetUnits(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<Movable>().Inc<Alive>().Inc<LogicActive>().Inc<HasTarget>().Exc<ShouldAttack>().Exc<UnitCommand>();
        }
        public override void Process(Entity e)
        {
            var position = e.Get<Position>().v;
            ref var tree = ref e.Get<UnitNation>().e.GetRef<DistanceTree>();
            tree.frame_requests++;
            if (!tree.FindNearest(position, out var another))
                return;
            var target = e.Get<HasTarget>().v;
            var target_position = target.Get<Position>().v;
            bool good = (!MaxAttackers.USE_IT) || (another.Get<Attackers>().v.Count < another.Get<MaxAttackers>().v);
            var another_position = another.Get<Position>().v;
            if (good && (another_position - position).sqrMagnitude < (target_position - position).sqrMagnitude)
            {
                e.Set(new HasTarget(another));
                if (MaxAttackers.USE_IT)
                {
                    target.Get<Attackers>().v.Remove(e);
                    another.Get<Attackers>().v.Add(e);
                }
            }
            LogicActive.WaitFor(e, UnityEngine.Random.Range(0.4f, 0.7f));

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
            if (MaxAttackers.USE_IT)
                target.Get<Attackers>().v.Remove(e);
            e.Remove<HasTarget>();
            e.RemoveIfPresent<ShouldAttack>();
        }
    }

}