using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using ECS;
using UnityECSLink;
using Unity.VisualScripting;


namespace UnityECSLink
{

    public class CreateECSEntity : MonoBehaviour
    {
        public List<IComponentProvider> providers = new();

        void Start()
        {
            var world = ECSWorldContainer.Active.world;
            ECS.Entity entity;
            LinkedEntity linked = null;
            if (linked = gameObject.GetComponent<LinkedEntity>())
            {
                entity = linked.entity;
            }
            else
                entity = world.NewEntity();
            foreach (var provider in providers)
                provider.ProvideComponent(entity);
            if (!linked)
            {
                linked = gameObject.AddComponent<LinkedEntity>();
                linked.entity = entity;
                entity.Add(new LinkedGameObject(gameObject));
                //TODO - some metaprogramming?
                if (gameObject.TryGetComponent<NavMeshAgent>(out var comp))
                    entity.Add(new LinkedComponent<NavMeshAgent>(comp));
                if (gameObject.TryGetComponent<StatusBar>(out var comp2))
                    entity.Add(new LinkedComponent<StatusBar>(comp2));
                if (gameObject.TryGetComponent<Renderer>(out var comp3))
                    entity.Add(new LinkedComponent<Renderer>(comp3));

            }
        }
    }
}