using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECSLink;
using ECSGame;

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
        OnUpdate.Add(new ProcessGameObjects(world));
        OnFixedUpdate = new ECS.Systems(world);
        ECSGame.Config.InitSystems(world, OnUpdate, OnFixedUpdate);
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
                GUI.Label(GUIRect(), $"  {pair.Key}: {pair.Value:G4}ms");
        GUI.Label(GUIRect(), "OnFixedUpdate:");
        foreach (var pair in OnFixedUpdate.Statistics)
            if (pair.Value > StatisticsThreshold)
                GUI.Label(GUIRect(), $"  {pair.Key}: {pair.Value:G4}ms");
    }

    static Rect GUIRect(float height = 0.025f)
    {
        GuiPos += height;
        return new Rect(Screen.width * 0.05f, Screen.height * GuiPos, 500f, 20f);
    }

}
