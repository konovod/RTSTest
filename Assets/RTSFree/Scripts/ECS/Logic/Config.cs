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
      OnUpdate.Add(new CacheUnitPosition(world));
      OnUpdate.Add(new SpawnSystem(world));

      OnUpdate.Add(new ProcessInitialUnitStates(world));
      OnUpdate.DelHere<InitialUnitState>();

      OnUpdate.Add(new CreateNations(world));
      OnUpdate.Add(new UpdateSearchTree(world));
      OnUpdate.Add(new FindAttackTarget(world));
      OnUpdate.Add(new ApproachTarget(world));
      OnUpdate.Add(new UnitAttackTargets(world));

      OnUpdate.Add(new ApplyDamage(world));
      OnUpdate.DelHere<AttackHit>();

      OnUpdate.Add(new DeselectOnDeath(world));
      OnUpdate.Add(new ProcessDeath(world));
      OnUpdate.DelHere<StartDying>();

      OnUpdate.Add(new ProcessRotting(world));
      OnUpdate.Add(new SelectUnits(world));
      OnUpdate.DelHere<JustSelected>();

      OnUpdate.Add(new TargetSelectedUnits(world));
      OnUpdate.Add(new CheckUnitCommandStatus(world));
      OnUpdate.DelHere<ManualTarget>();

      OnUpdate.Add(new RemoveUnitTargets(world));
      OnUpdate.DelHere<RemoveTarget>();

      OnUpdate.Add(new RecolorUnit(world));
      OnUpdate.DelHere<ChangeColor>();
      ////////////////// add here systems that is called on FixedUpdate
      // OnFixedUpdate.Add(new ExampleFixedSystem(world));

      ///

      UnityEngine.Random.InitState(0);
      UnityEngine.AI.NavMesh.pathfindingIterationsPerFrame = 10000;
    }
    private static void Link<T>(GameObject gameObject, ECS.Entity entity) where T : Component
    {
      if (gameObject.TryGetComponent<T>(out T comp))
        entity.Add(new LinkedComponent<T>(comp));
    }
    private static void LinkChild<T>(GameObject gameObject, ECS.Entity entity) where T : Component
    {
      var comp = gameObject.GetComponentInChildren<T>();
      if (comp != null)
        entity.Add(new LinkedComponent<T>(comp));
    }
    public static void LinkComponents(GameObject gameObject, ECS.Entity entity)
    {
      //TODO - some metaprogramming?
      Link<NavMeshAgent>(gameObject, entity);
      Link<Renderer>(gameObject, entity);
      LinkChild<StatusBar>(gameObject, entity);
    }
  }
}