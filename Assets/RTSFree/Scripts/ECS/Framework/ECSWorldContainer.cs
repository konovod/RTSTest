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

    [SerializeField]
    public List<string> DebugStatistics = new();


    void Awake()
    {
        Active = this;
        OnUpdate = new ECS.Systems(world);
        OnUpdate.Add(new ProcessGameObjects(world));
        OnFixedUpdate = new ECS.Systems(world);
        ECSGame.Config.InitSystems(world, OnUpdate, OnFixedUpdate);
        OnFixedUpdate.Add(new ProcessComponentRequests(world));
        OnFixedUpdate.DelHere<RemoveRequest>();
        OnFixedUpdate.DelHere<AddRequest>();
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
        DebugStatistics.Clear();
        AddLabel($"Entities: n={world.EntitiesCount()}");
        AddLabel("OnUpdate:");
        foreach (var pair in OnUpdate.Statistics)
            if (pair.Value > StatisticsThreshold)
                AddLabel($"  {pair.Key}: {pair.Value:G4}ms");
        AddLabel("OnFixedUpdate:");
        foreach (var pair in OnFixedUpdate.Statistics)
            if (pair.Value > StatisticsThreshold)
                AddLabel($"  {pair.Key}: {pair.Value:G4}ms");
    }

    void AddLabel(string text, float height = 0.025f)
    {
        GuiPos += height;
        GUI.Label(new Rect(Screen.width * 0.05f, Screen.height * GuiPos, 500f, 20f), text);
        DebugStatistics.Add(text);
    }

}
