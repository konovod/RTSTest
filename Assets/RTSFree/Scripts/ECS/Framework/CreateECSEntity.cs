using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ECS;
using UnityECSLink;


namespace UnityECSLink
{

    public class CreateECSEntity : MonoBehaviour
    {
        public List<IComponentProvider> providers = new();

        void Start()
        {
            var world = ECSWorldContainer.Active.world;
            var entity = world.NewEntity();
            foreach (var provider in providers)
                provider.ProvideComponent(entity);
            var link = gameObject.AddComponent<LinkedEntity>();
            link.entity = entity;
            entity.Add(new LinkedGameObject(gameObject));
        }
    }
}