#nullable enable

using System;
using UnityEngine;

namespace ECSGame
{

    [Serializable]
    public struct ExampleComponent
    {
        public int value;
    };

    public class ExampleSystem : ECS.System
    {
        public ExampleSystem(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<ExampleComponent>();
        }

        public override void Process(ECS.Entity e)
        {
            Debug.Log(e.Get<ExampleComponent>().value);
            e.Remove<ExampleComponent>();
        }
    }


}