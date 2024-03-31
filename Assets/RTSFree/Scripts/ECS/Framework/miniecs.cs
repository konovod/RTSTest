#nullable enable

using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;

namespace ECS
{


  public struct Entity
  {
    internal World World;
    internal int Id;

    public Entity Add<T>(in T item) where T : struct
    {
      Pool<T> pool = World.GetStorage<T>();
      pool.Add(Id, item);
      World.IncComponentCount(Id);
      return this;
    }

    public Entity AddDefault(Type type)
    {
      var pool = World.GetStorage(type);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      pool.AddDefault(Id);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      return this;
    }

    public readonly bool Has<T>() where T : struct
    {
      return World.GetStorage<T>().Has(Id);
    }

    public readonly bool Has(Type type)
    {
      if (World.GetStorage(type) is IPool storage)
        return storage.Has(Id);
      else
        return false;
    }

    public Entity Remove<T>() where T : struct
    {
      World.GetStorage<T>().Remove(Id);
      World.DecComponentCount(Id);
      return this;
    }

    public Entity RemoveIfPresent<T>() where T : struct
    {
      if (Has<T>())
      {
        World.GetStorage<T>().Remove(Id);
        World.DecComponentCount(Id);
      }
      return this;
    }

    public Entity Remove(Type type)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      World.GetStorage(type).Remove(Id);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      World.DecComponentCount(Id);
      return this;
    }

    public Entity RemoveIfPresent(Type type)
    {
      if (Has(type))
      {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        World.GetStorage(type).Remove(Id);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        World.DecComponentCount(Id);
      }
      return this;
    }

    public Entity Set<T>(in T item) where T : struct
    {
      if (Has<T>())
      {
        Pool<T> pool = World.GetStorage<T>();
        int index = pool.IndexByEntity[Id];
        pool.Items.Items[index] = item;
      }
      else
        Add(item);
      return this;
    }

    public readonly T Get<T>() where T : struct
    {
      Pool<T> pool = World.GetStorage<T>();
      int index = pool.IndexById(Id);
      return pool.Items.Items[index];
    }
    public ref T GetRef<T>() where T : struct
    {
      Pool<T> pool = World.GetStorage<T>();
      int index = pool.IndexById(Id);
      return ref pool.Items.Items[index];
    }

    public void Destroy()
    {
      foreach (var type in World.pools.Keys)
        if (Has(type))
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          World.GetStorage(type).Remove(Id);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      World.component_counts.Remove(Id);
    }

  }

  public class World
  {
    internal int MaxEntityID;
    public Entity NewEntity()
    {
      Entity entity;
      entity.World = this;
      entity.Id = MaxEntityID++;
      component_counts.Add(entity.Id, 0);
      return entity;
    }

    internal Dictionary<Type, IPool> pools = new();
    internal Dictionary<int, int> component_counts = new();
    internal void IncComponentCount(int id)
    {
      component_counts[id] += 1;
    }
    internal void DecComponentCount(int id)
    {
      component_counts[id] -= 1;
      if (component_counts[id] == 0)
        component_counts.Remove(id);
    }

    internal Pool<T> GetStorage<T>() where T : struct
    {
      if (!pools.TryGetValue(typeof(T), out IPool? pool))
      {
        pool = new Pool<T>();
        pools.Add(typeof(T), pool);
      }
#pragma warning disable CS8603 // Possible null reference return.
      return pool as Pool<T>;
#pragma warning restore CS8603 // Possible null reference return.
    }

    public ComponentEnumerable Each<T>() where T : struct
    {
      return new ComponentEnumerable(this, typeof(T));
    }
    public WorldEnumerable EachEntity()
    {
      return new WorldEnumerable(this);
    }

    public int EntitiesCount()
    {
      return component_counts.Count;
    }

    public void DeleteAll()
    {
      component_counts.Clear();
      foreach (var pool in pools.Values)
        pool.Clear();
    }

    internal IPool? GetStorage(Type type)
    {
      if (pools.TryGetValue(type, out IPool? result))
        return result;
      else
        return null;
    }
    public int CountComponents<T>() where T : struct
    {
      if (pools.TryGetValue(typeof(T), out IPool? result))
        return result.Count();
      else
        return 0;
    }

    public T FirstComponent<T>() where T : struct
    {
      var pool = GetStorage<T>();
      if (pool.Count() == 0)
        throw new Exception("No components in pool");
      return pool.Items.Items[0];
    }
    public ref T RefFirstComponent<T>() where T : struct
    {
      var pool = GetStorage<T>();
      if (pool.Count() == 0)
        throw new Exception("No components in pool");
      return ref pool.Items.Items[0];
    }

    public int CountComponents(Type type)
    {
      if (pools.TryGetValue(type, out IPool? result))
        return result.Count();
      else
        return 0;
    }

    public Filter Inc<T>()
    {
      return new Filter(this).Inc<T>();
    }

  }
  internal interface IPool
  {
    int Count();
    bool Has(int id);
    void Remove(int id);
    int IdByIndex(int id);
    void Clear();
    void AddDefault(int id);
  }

  // Created because older c# do not support getting ref on a list element
  internal class MyList<T>
  {
    public T[] Items = new T[128];
    public int Count { get; private set; } = 0;

    public MyList() { }
    public void Add(T item)
    {
      if (Count >= Items.Length)
      {
        Array.Resize(ref Items, Items.Length * 2);
      }
      Items[Count] = item;
      Count++;
    }
    public void RemoveLast()
    {
      Count--;
    }
    public void Clear()
    {
      Count = 0;
    }

  }

  internal class Pool<T> : IPool where T : struct
  {
    public int Count() { return Items.Count; }
    public MyList<T> Items = new();
    public List<int> Entities = new(128);
    public Dictionary<int, int> IndexByEntity = new();

    public bool Has(int id)
    {
      return IndexByEntity.ContainsKey(id);
    }
    public void Remove(int id)
    {
      int index = IndexByEntity[id];
      int last = Items.Count - 1;
      if (index != last)
      {
        IndexByEntity[Entities[last]] = index;
        Items.Items[index] = Items.Items[last];
        Entities[index] = Entities[last];
      }
      Items.RemoveLast();
      Entities.RemoveAt(last);
      IndexByEntity.Remove(id);
    }
    public void Add(int id, T item)
    {
      var index = AddIndex(id);
      Items.Items[index] = item;
    }

    public void AddDefault(int id)
    {
      var index = AddIndex(id);
      Items.Items[index] = default;
    }

    public int AddIndex(int id)
    {
      Items.Add(default);
      Entities.Add(id);
      int index = Items.Count - 1;
      IndexByEntity.Add(id, index);
      return index;
    }

    public int IndexById(int id)
    {
      return IndexByEntity[id];
    }
    public int IdByIndex(int id)
    {
      return Entities[id];
    }

    public void Clear()
    {
      Entities.Clear();
      Items.Clear();
      IndexByEntity.Clear();
    }

  }

  public readonly struct ComponentEnumerable
  {
    public ComponentEnumerable(World aworld, Type? atype)
    {
      world = aworld;
      type = atype;
    }

    readonly World world;
    readonly Type? type;

    public ComponentEnumerator GetEnumerator()
    {
      return new ComponentEnumerator(world, type);
    }

  }
  public readonly struct WorldEnumerable
  {

    public WorldEnumerable(World aworld) { world = aworld; }
    readonly World world;

    public WorldEnumerator GetEnumerator()
    {
      return new WorldEnumerator(world);
    }
  }


  public struct ComponentEnumerator
  {

    public ComponentEnumerator(World aworld, Type? atype)
    {
      world = aworld;
      if (atype is Type type)
        pool = aworld.GetStorage(type);
      else
        pool = null;
      index = 0;
      cached = -1;
    }

    internal readonly World world;
    internal readonly IPool? pool;
    int index;
    int cached;

    public bool MoveNext()
    {
      if (pool is null)
        return false;
      if (index >= pool.Count())
        return false;
      int entity = pool.IdByIndex(index);
      if (entity == cached)
      {
        index++;
        if (index >= pool.Count())
          return false;
        cached = pool.IdByIndex(index);
      }
      else
        cached = entity;
      return true;
    }

    public Entity Current
    {
      get
      {
        Entity entity;
        entity.World = world;
        entity.Id = cached;
        return entity;
      }
    }
  }
  public struct WorldEnumerator
  {
    public WorldEnumerator(World aworld)
    {
      world = aworld;
      under = aworld.component_counts.Keys.GetEnumerator();
    }
    internal readonly World world;
    internal IEnumerator<int> under;

    public bool MoveNext()
    {
      return under.MoveNext();
    }

    public Entity Current
    {
      get
      {
        Entity entity;
        entity.World = world;
        entity.Id = under.Current;
        return entity;
      }
    }
  }

  public struct FilterEnumerator
  {

    public FilterEnumerator(Filter afilter, ComponentEnumerator aunder)
    {
      filter = afilter;
      under = aunder;
    }
    internal readonly Filter filter;
    internal ComponentEnumerator under;

    public bool MoveNext()
    {
      while (true)
      {
        if (!under.MoveNext())
          return false;
        if (filter.Satisfy(under.Current))
          return true;
      }
    }

    public Entity Current
    {
      get => under.Current;
    }
  }

  public class Filter
  {

    public Filter(World aworld)
    {
      world = aworld;
    }
    World world;
    List<Type> Included = new();
    List<Type> Excluded = new();

    public Filter Inc<T>()
    {
      Included.Add(typeof(T));
      return this;
    }

    public Filter Exc<T>()
    {
      Excluded.Add(typeof(T));
      return this;
    }

    public bool Satisfy(Entity entity)
    {
      foreach (var typ in Included)
        if (!entity.Has(typ))
          return false;
      foreach (var typ in Excluded)
        if (entity.Has(typ))
          return false;
      return true;
    }

    public FilterEnumerator GetEnumerator()
    {
      var bestcount = int.MaxValue;
      Type? best = null;
      foreach (var v in Included)
      {
        var count = world.CountComponents(v);
        if (count < bestcount)
        {
          bestcount = count;
          best = v;
          if (bestcount == 0)
            return new FilterEnumerator(this, new ComponentEnumerator(world, null));
        }
      }
      return new FilterEnumerator(this, new ComponentEnumerator(world, best));
    }

  }

  public class System
  {
    public System(World aworld)
    {
      world = aworld;
    }

    public World world;
    public virtual void Init()
    {

    }
    public virtual void PreProcess()
    {

    }
    public virtual void Execute()
    {

    }
    public virtual void Teardown()
    {

    }
    public virtual Filter? Filter(World world)
    {
      return null;
    }

    public virtual void Process(Entity e)
    {
    }
  }


  public class Systems : System
  {
    public Systems(World aworld) : base(aworld) { }
    List<System> children = new();
    List<Filter?> filters = new();
    List<Stopwatch> Timers = new();
    Stopwatch FullTimer = new();
    public Dictionary<String, double> Statistics = new();

    public override void Init()
    {
      foreach (var child in children)
        child.Init();
      foreach (var child in children)
        filters.Add(child.Filter(world));
      foreach (var child in children)
      {
        Timers.Add(new Stopwatch());
        Statistics.Add(child.GetType().ToString(), 0);
      }
      Statistics.Add("Total", 0);
    }

    public override void Execute()
    {
      FullTimer.Reset();
      FullTimer.Start();
      foreach (var timer in Timers)
        timer.Reset();

      int i = 0;
      foreach (var child in children)
      {
        Timers[i].Start();
        child.PreProcess();
        Timers[i].Stop();
        i++;
      }
      i = 0;
      foreach (var child in children)
      {
        var fltx = filters[i];
        if (fltx is Filter flt)
        {
          Timers[i].Start();
          foreach (var entity in flt)
            child.Process(entity);
          Timers[i].Stop();
        }
        i += 1;
      }
      i = 0;
      foreach (var child in children)
      {
        Timers[i].Start();
        child.Execute();
        Timers[i].Stop();
        i += 1;
      }
      i = 0;
      foreach (var child in children)
      {
        Statistics[child.GetType().ToString()] = Timers[i].ElapsedTicks * 1000.0 / Stopwatch.Frequency;
        i += 1;
      }
      FullTimer.Stop();
      Statistics["Total"] = FullTimer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
    }
    public override void Teardown()
    {
      foreach (var child in children)
        child.Teardown();
    }

    public void Add(System sys)
    {
      children.Add(sys);
    }

    public void DelHere<T>() where T : struct
    {
      children.Add(new RemoveComponents<T>(world));
    }

  }

  internal class RemoveComponents<T> : System where T : struct
  {

    internal RemoveComponents(World aworld) : base(aworld) { }

    public override void Execute()
    {
      var pool = world.GetStorage<T>();
      foreach (var id in pool.Entities)
        world.DecComponentCount(id);
      pool.Clear();

    }
  }
}