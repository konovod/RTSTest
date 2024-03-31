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

    [SerializeField]
    public bool ShowStatistics;
    public float StatisticsThreshold;

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
        OnFixedUpdate.Add(new ProcessComponentRequests(world));
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

    // Display performance UI
    static float GuiPos = 0.0f;
    public void OnGUI()
    {
        if (!ShowStatistics)
            return;
        GuiPos = 0;
        GUI.Label(GUIRect(), $"Entities: n={world.EntitiesCount()}");
        GUI.Label(GUIRect(), "OnUpdate:");
        foreach (var pair in OnUpdate.Statistics)
            if (pair.Value > StatisticsThreshold)
                GUI.Label(GUIRect(), $"  {pair.Key}: {pair.Value}ms");
        GUI.Label(GUIRect(), "OnFixedUpdate:");
        foreach (var pair in OnFixedUpdate.Statistics)
            if (pair.Value > StatisticsThreshold)
                GUI.Label(GUIRect(), $"  {pair.Key}: {pair.Value}ms");
    }

    static Rect GUIRect(float height = 0.025f)
    {
        GuiPos += height;
        return new Rect(Screen.width * 0.05f, Screen.height * GuiPos, 500f, 20f);
    }

}
