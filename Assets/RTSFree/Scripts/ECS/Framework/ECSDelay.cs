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
    public interface ComponentRequest
    {
        public void Execute();
        public float Time();
    }


    // component should be added to entity, system will removes component T from entity Entity when GlobalTime.Time will reach Time
    // Note that the added RemoveRequest will be removed from entity once it is taken into queue
    public struct RemoveRequest : ComponentRequest
    {
        public Type Component;
        public Entity Entity;
        public float time;
        public float Time() { return time; }
        public void Execute() { Entity.Remove(Component); }
    };

    public struct AddRequest : ComponentRequest
    {
        public Type Component;
        public Entity Entity;
        public float time;
        public float Time() { return time; }
        public void Execute() { Entity.AddDefault(Component); }
    };

    public struct GlobalTime
    {
        public float Time;
    };
    public struct GlobalTimeQueue
    {
        public PriorityQueue<ComponentRequest, float> queue;
    };


    public class ProcessComponentRequests : ECS.System
    {
        public ProcessComponentRequests(ECS.World aworld) : base(aworld) { }

        Filter? FilterAddRequests;
        Filter? FilterRemoveRequests;
        public override void Init()
        {
            var e = world.NewEntity();
            GlobalTime time;
            time.Time = 0;
            e.Add(time);
            GlobalTimeQueue queue;
            queue.queue = new();
            e.Add(queue);
            world.GetStorage<AddRequest>();
            world.GetStorage<RemoveRequest>();
            FilterAddRequests = world.Inc<AddRequest>();
            FilterRemoveRequests = world.Inc<RemoveRequest>();
        }

        public override void Execute()
        {
            //add new requests
            var queue = world.FirstComponent<GlobalTimeQueue>().queue;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (var e in FilterAddRequests)
            {
                var request = e.Get<AddRequest>();
                queue.Enqueue(request, request.Time());
            }
            foreach (var e in FilterRemoveRequests)
            {
                var request = e.Get<RemoveRequest>();
                queue.Enqueue(request, request.Time());
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            // process requests that should trigger
            world.RefFirstComponent<GlobalTime>().Time += Time.fixedTime;
            var CurTime = world.FirstComponent<GlobalTime>().Time;
            while ((queue.Count > 0) && (queue.Peek().Time() >= CurTime))
            {
                var request = queue.Dequeue();
                request.Execute();
            }
        }
    }

}