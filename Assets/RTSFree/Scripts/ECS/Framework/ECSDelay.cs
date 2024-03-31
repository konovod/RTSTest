#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PriorityQueue;
using System;
using ECS;
using UnityEditor.PackageManager.Requests;

namespace UnityECSLink
{
    // component should be added to entity, system will removes component T from entity Entity when GlobalTime.Time will reach Time
    // Note that the added RemoveRequest will be removed from entity once it is taken into queue
    public struct RemoveRequest
    {
        public Type Component;
        public Entity Entity;
        public float Time;

    };

    public struct GlobalTime
    {
        public float Time;
    };
    public struct GlobalTimeQueue
    {
        public PriorityQueue<RemoveRequest, float> queue;
    };


    public class ProcessRemoveAtTime : ECS.System
    {
        public ProcessRemoveAtTime(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<RemoveRequest>();
        }

        public override void Process(ECS.Entity e)
        {
            var request = e.Get<RemoveRequest>();
            var queue = world.FirstComponent<GlobalTimeQueue>().queue;
            queue.Enqueue(request, request.Time);
        }
        public override void Init()
        {
            var e = world.NewEntity();
            GlobalTime time;
            time.Time = 0;
            e.Add(time);
            GlobalTimeQueue queue;
            queue.queue = new();
            e.Add(queue);
        }

        public override void Execute()
        {
            world.RefFirstComponent<GlobalTime>().Time += Time.fixedTime;

            var CurTime = world.FirstComponent<GlobalTime>().Time;
            var queue = world.FirstComponent<GlobalTimeQueue>().queue;
            while ((queue.Count > 0) && (queue.Peek().Time >= CurTime))
            {
                var request = queue.Dequeue();
                request.Entity.RemoveIfPresent(request.Component);
            }
        }
    }

}