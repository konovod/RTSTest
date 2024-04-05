#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using ECS;
using UnityECSLink;
using UnityEngine;

namespace ECSGame
{

    [Serializable]
    public struct DistanceTree
    {
        public Entity[] targets;
        public Vector3[] positions;
        public int[] indices;
        public int count;
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
                tree.targets = new Entity[0];
                tree.count = 0;
                tree.positions = new Vector3[1];
                tree.positions[0] = new Vector3(-999999999999.99f, -999999999999.99f, -999999999999.99f);
                tree.indices = new int[1];
                tree.indices[0] = 0;

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
            var count = world.CountComponents<UnitNation>();
            if (count > tree.targets.Count())
            {
                Array.Resize(ref tree.targets, count);
                Array.Resize(ref tree.positions, count + 1);
                Array.Resize(ref tree.indices, count + 1);
            }
            var i = 0;
            tree.positions[0] = new Vector3(-999999999999.99f, -999999999999.99f, -999999999999.99f);
            tree.indices[0] = 0;
            foreach (var unit in all_units)
            {
                if (unit.Get<Attackers>().v.Count >= unit.Get<MaxAttackers>().v)
                    continue;
                if (unit.Get<UnitNation>().e.Id != e.Id)
                {
                    tree.targets[i] = unit;
                    tree.indices[i + 1] = i + 1;
                    tree.positions[i + 1] = unit.Get<Position>().v;
                    i++;
                }
            }
            tree.count = i;
            tree.targetKD = RTSToolkitFree.KDTree.MakeFromPointsInner(0, 0, tree.count, tree.positions, tree.indices);
        }
    }


}