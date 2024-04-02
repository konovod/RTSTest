#nullable enable

using System;
using RTSToolkitFree;
using UnityEngine;


namespace ECSGame
{

    public struct LogicActive
    {

    }


    [Serializable]
    public struct SpawnPoint
    {
        public GameObject objectToSpawn;
        public int numberOfObjects;
        public float timestep;
        public float size;
        public bool randomizeRotation;
        public Vector3 posOffset;
    }

    public struct Terrain
    {
        public UnityEngine.Terrain ter;
    }

    public class SpawnSystem : ECS.System
    {
        public SpawnSystem(ECS.World aworld) : base(aworld) { }

        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<SpawnPoint>().Inc<LogicActive>();
        }

        public override void Init()
        {
            Terrain our_ter;
            our_ter.ter = GameObject.FindObjectOfType<UnityEngine.Terrain>();
            world.NewEntity().Add(our_ter);
        }

        public override void Process(ECS.Entity e)
        {
            ref var spawner = ref e.GetRef<SpawnPoint>();
            Transform transform = e.Get<UnityECSLink.LinkedGameObject>().Transform();

            Quaternion rot = transform.rotation;
            if (spawner.randomizeRotation)
            {
                rot = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
            }

            Vector2 randPos = UnityEngine.Random.insideUnitCircle * spawner.size;

            Vector3 pos = transform.position + new Vector3(randPos.x, 0f, randPos.y) + transform.rotation * spawner.posOffset;
            var ter = world.FirstComponent<Terrain>().ter;
            pos = TerrainVector(pos, ter);

            var created = world.NewEntity();
            UnityECSLink.InstantiateGameObject request;
            request.pos = pos;
            request.rot = rot;
            request.Template = spawner.objectToSpawn;
            created.Add(request);

            spawner.numberOfObjects--;
            if (spawner.numberOfObjects <= 0)
            {
                e.Remove<SpawnPoint>();
                return;
            }
            e.Remove<LogicActive>();
            UnityECSLink.AddRequest add_request;
            add_request.Component = typeof(LogicActive);
            add_request.Entity = e;
            add_request.time = world.FirstComponent<UnityECSLink.GlobalTime>().Time + spawner.timestep;
            world.NewEntity().Add(add_request);
        }

        Vector3 TerrainVector(Vector3 origin, UnityEngine.Terrain ter1)
        {
            if (ter1 == null)
            {
                return origin;
            }

            Vector3 planeVect = new Vector3(origin.x, 0f, origin.z);
            float y = ter1.SampleHeight(planeVect);

            y = y + ter1.transform.position.y;

            Vector3 tv = new Vector3(origin.x, y, origin.z);
            return tv;
        }

    }



}