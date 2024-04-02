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
                ECSGame.Config.LinkComponents(gameObject, entity);
            }
        }
    }
}