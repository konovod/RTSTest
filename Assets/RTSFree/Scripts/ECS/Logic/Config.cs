using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityECSLink;

namespace ECSGame
{
  public static class Config
  {
    public static void InitSystems(ECS.World world, ECS.Systems OnUpdate, ECS.Systems OnFixedUpdate)
    {
      ////////////////// add here systems that is called on Update
      OnUpdate.Add(new SpawnSystem(world));
      OnUpdate.Add(new CreateNations(world));
      OnUpdate.Add(new UpdateSearchTree(world));
      OnUpdate.Add(new FindAttackTarget(world));
      OnUpdate.Add(new ApproachTarget(world));
      OnUpdate.Add(new UnitAttackTargets(world));


      OnUpdate.Add(new RecolorUnit(world));
      OnUpdate.DelHere<ChangeColor>();
      ////////////////// add here systems that is called on FixedUpdate
      ///

      ///
      OnFixedUpdate.Add(new ProcessComponentRequests(world));
      OnFixedUpdate.DelHere<RemoveRequest>();
      OnFixedUpdate.DelHere<AddRequest>();

    }
    private static void Link<T>(GameObject gameObject, ECS.Entity entity) where T : Component
    {
      if (gameObject.TryGetComponent<T>(out T comp))
        entity.Add(new LinkedComponent<T>(comp));
    }

    public static void LinkComponents(GameObject gameObject, ECS.Entity entity)
    {
      //TODO - some metaprogramming?
      Link<NavMeshAgent>(gameObject, entity);
      Link<StatusBar>(gameObject, entity);
      Link<Renderer>(gameObject, entity);
    }
  }
}