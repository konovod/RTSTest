#nullable enable

using System;
using ECS;
using RTSToolkitFree;
using UnityECSLink;
using UnityEngine;


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

    public struct AttackHit
    {
        public int damage;
        public Entity target;
        public Entity source;
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
            var targetId = tree.targetKD.FindNearest(e.Get<LinkedGameObject>().Obj.transform.position);
            if (targetId < 0)
            {
                e.RemoveIfPresent<AttackTarget>();
                return;
            }
            var target = tree.targets[targetId];
            AttackTarget comp;
            comp.v = target;
            e.Set(comp);
        }
    }
}