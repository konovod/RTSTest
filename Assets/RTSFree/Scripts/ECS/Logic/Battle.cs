#nullable enable

using System;
using ECS;
using RTSToolkitFree;
using UnityEngine;


namespace ECSGame
{

    [Serializable]
    public struct Health
    {
        public int V;
        public int Max;
    }
    [Serializable]
    public struct Movable { }
    public struct Alive { }
    public struct Dying { }
    public struct Rotting { }

    public struct IsApproachable { }

    public struct AttackTarget
    {
        public Entity v;
    }

    [Serializable]
    public struct BattleStats
    {
        public float strength;
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
    public struct AttackHit
    {
        public int damage;
    }

}