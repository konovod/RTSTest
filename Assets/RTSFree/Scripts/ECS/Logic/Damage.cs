#nullable enable

using System;
using ECS;
using UnityECSLink;
using UnityEngine;
using UnityEngine.AI;


namespace ECSGame
{
    [Serializable]
    public struct AttackStats
    {
        public float strength;
        public float distance;
    }
    [Serializable]
    public struct DefenseStats
    {
        public float defense;
    }
    [Serializable]
    public struct MaxHealth
    {
        public float v;
    }
    public struct Health
    {
        public Health(float value) { v = value; }
        public float v;
    }
    public struct Alive { }

    public struct StartDying { }
    public struct Dying { }
    public struct Rotting { }

    public struct AttackHit
    {
        public float damage;
        public Entity target;
        public Entity source;
    }


    public class UnitAttackTargets : ECS.System
    {
        public UnitAttackTargets(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<AttackStats>().Inc<ShouldAttack>().Inc<Alive>().Inc<LogicActive>().Exc<UnitCommand>();
        }
        public override void Process(Entity e)
        {
            var position = e.Get<Position>().v;
            var target = e.Get<HasTarget>().v;
            var target_position = target.Get<Position>().v;
            var distance = (position - target_position).magnitude;
            var attack_distance = e.Get<AttackStats>().distance;
            if (distance > attack_distance)
            {
                e.Remove<ShouldAttack>();
                e.Set(new ChangeColor(Color.green));
                return;
            }
            var strength = e.Get<AttackStats>().strength;
            var defense = target.Get<DefenseStats>().defense;
            if (UnityEngine.Random.value > (strength / (strength + defense)))
            {
                AttackHit hit;
                hit.damage = 2.0f * strength * UnityEngine.Random.value;
                hit.source = e;
                hit.target = target;
                world.NewEntity().Add(hit);
                LogicActive.WaitFor(e, 0.5f);
            }
        }
    }

    public class ApplyDamage : ECS.System
    {
        public ApplyDamage(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<AttackHit>();
        }
        public override void Process(Entity e)
        {
            var hit = e.Get<AttackHit>();
            var target = hit.target;
            if (!target.Has<Alive>())
                return;
            float health;
            if (target.Has<Health>())
                health = target.Get<Health>().v;
            else
                health = target.Get<MaxHealth>().v;
            health -= hit.damage;
            if (health <= 0)
                target.Set(new StartDying());
            target.Set(new Health(health));
        }
    }

    public class ProcessDeath : ECS.System
    {
        public ProcessDeath(ECS.World aworld) : base(aworld) { aworld.GetStorage<Rotting>(); }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<StartDying>();
        }
        public override void Process(Entity e)
        {
            e.Remove<Alive>();
            e.Add(new Dying());
            e.Set(new ChangeColor(Color.blue));
            e.Get<LinkedComponent<NavMeshAgent>>().v.enabled = false;
            foreach (var attacker in world.Each<HasTarget>())
                if (attacker.Get<HasTarget>().v.Id == e.Id)
                {
                    attacker.Remove<HasTarget>();
                }
            AddRequest add_request;
            add_request.Component = typeof(Rotting);
            add_request.Entity = e;
            add_request.time = world.FirstComponent<UnityECSLink.GlobalTime>().Time + 5;
            world.NewEntity().Add(add_request);

        }
    }

    public class ProcessRotting : ECS.System
    {
        public ProcessRotting(ECS.World aworld) : base(aworld) { }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<Rotting>().Inc<LogicActive>();
        }
        public override void Process(Entity e)
        {
            e.Set(new ChangeColor(new Color((148.0f / 255.0f), (0.0f / 255.0f), (211.0f / 255.0f), 1.0f)));
            var transform = e.Get<LinkedGameObject>().Obj.transform;
            if (transform.position.y > -1.0f)
            {
                float sinkSpeed = -0.2f;
                transform.position += new Vector3(0f, sinkSpeed, 0f);
                LogicActive.WaitFor(e, 0.1f);
            }
            else
                e.Add(new DestroyGameObject());
        }
    }
}