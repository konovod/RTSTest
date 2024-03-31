using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECSLink;

//Контейнер для "мира" и систем ецс. Ставится на сцену в единственном экземлпяре 
// В этот класс добавляются все системы игры
public class ECSWorldContainer : MonoBehaviour
{
    public static ECSWorldContainer Active;
    public ECS.World world = new();
    public ECS.Systems OnUpdate;
    public ECS.Systems OnFixedUpdate;

    void Awake()
    {
        Active = this;
        OnUpdate = new ECS.Systems(world);
        ////////////////// add here systems that is called on Update
        OnUpdate.Add(new ProcessGameObjects(world));
        OnUpdate.Add(new ECSGame.ExampleSystem(world));

        ///
        OnFixedUpdate = new ECS.Systems(world);
        ////////////////// add here systems that is called on FixedUpdate
        ///

        ///
        OnFixedUpdate.Add(new ProcessRemoveAtTime(world));
        OnFixedUpdate.DelHere<RemoveRequest>();

    }

    void Start()
    {
        OnUpdate.Init();
        OnFixedUpdate.Init();
    }

    void Update()
    {
        OnUpdate.Execute();
    }

    void FixedUpdate()
    {
        OnFixedUpdate.Execute();
    }

}
