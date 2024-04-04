#nullable enable

using UnityECSLink;
using UnityEngine;

namespace ECSGame
{
  public struct Controllable { }

  public struct IsSelected
  {
  }

  public struct JustSelected
  {
  }

  public struct ManualTarget
  {
    public Vector3 v;
    public ManualTarget(Vector3 target) { v = target; }
  }
  public struct UnitCommand
  {
    public Vector3 v;
    public UnitCommand(Vector3 target) { v = target; }
  }

  public class SelectUnits : ECS.System
  {
    public SelectUnits(ECS.World aworld) : base(aworld) { }

    public override ECS.Filter? Filter(ECS.World world)
    {
      return world.Inc<JustSelected>().Inc<Alive>();
    }

    public override void PreProcess()
    {
      if (world.CountComponents<JustSelected>() > 0)
        foreach (var e in world.Each<IsSelected>())
        {
          e.Remove<IsSelected>();
          e.Get<LinkedComponent<StatusBar>>().v.SelectView.SetActive(false);
        }
    }

    public override void Process(ECS.Entity e)
    {
      e.Add(new IsSelected());
      e.Get<LinkedComponent<StatusBar>>().v.SelectView.SetActive(true);
    }

  }



  public class TargetSelectedUnits : ECS.System
  {
    public TargetSelectedUnits(ECS.World aworld) : base(aworld) { }

    public override ECS.Filter? Filter(ECS.World world)
    {
      return world.Inc<ManualTarget>();
    }


    public override void Process(ECS.Entity e)
    {
      var target = e.Get<ManualTarget>().v;
      foreach (var unit in world.Each<IsSelected>())
        unit.Set(new UnitCommand(target));
    }
  }


}