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
    public struct InitialUnitState { }
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

    public struct Position
    {
        public Vector3 v;
        public Position(Vector3 pos) { v = pos; }
    }

    public class CacheUnitPosition : ECS.System
    {
        public CacheUnitPosition(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<LinkedGameObject>();
        }
        public override void Process(Entity e)
        {
            e.Set(new Position(e.Get<LinkedGameObject>().Transform().position));
        }
    }

    public class ProcessInitialUnitStates : ECS.System
    {
        public ProcessInitialUnitStates(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<InitialUnitState>();
        }
        public override void Process(Entity e)
        {
            e.Add(new Alive());
            Attackers list;
            list.v = new();
            e.Add(list);
            e.Add(new LogicActive());
        }
    }

}