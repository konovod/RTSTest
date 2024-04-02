#nullable enable

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using ECS;
using Unity.VisualScripting;
using UnityECSLink;
using UnityEngine;

namespace ECSGame
{

    [Serializable]
    public struct DistanceTree
    {
        public List<Entity> targets;
        public RTSToolkitFree.KDTree targetKD;
    }

    [Serializable]
    public struct NationID
    {
        public int v;
    }

    [Serializable]
    public struct AllNations
    {
        public Dictionary<int, Entity> items;
    }

    public class CreateNations : ECS.System
    {
        public CreateNations(ECS.World aworld) : base(aworld) { }

        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<UnitNeedNation>();
        }

        public override void Init()
        {
            var e = world.NewEntity();
            AllNations nations;
            nations.items = new();
            e.Add(nations);
        }

        public override void Process(Entity e)
        {
            var needed_id = e.Get<UnitNeedNation>().id;
            var nations = world.FirstComponent<AllNations>().items;
            if (!nations.TryGetValue(needed_id, out var the_nation))
            {
                the_nation = world.NewEntity();
                NationID id;
                id.v = needed_id;
                the_nation.Add(id);
                DistanceTree tree;
                tree.targets = new();
                tree.targetKD = new();
                the_nation.Add(tree);
                nations.Add(needed_id, the_nation);
            }
            e.Remove<UnitNeedNation>();
            if (e.Alive())
            {
                UnitNation link;
                link.e = the_nation;
                e.Add(link);
            }
        }
    }

    public class UpdateSearchTree : ECS.System
    {
        Filter all_units;
        public UpdateSearchTree(ECS.World aworld) : base(aworld)
        {
            all_units = world.Inc<Alive>().Inc<DefenseStats>();
        }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<DistanceTree>();
        }
        public override void Process(Entity e)
        {
            ref var tree = ref e.GetRef<DistanceTree>();
            tree.targets.Clear();
            foreach (var unit in all_units)
            {
                if (unit.Get<UnitNation>().e.Id != e.Id)
                    tree.targets.Add(unit);
            }
            tree.targetKD = RTSToolkitFree.KDTree.MakeFromPoints(tree.targets.Select((v) => v.Get<LinkedGameObject>().Transform().position).ToArray());
        }
    }


}