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


}