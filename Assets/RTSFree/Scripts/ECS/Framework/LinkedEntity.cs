using UnityEngine;

namespace UnityECSLink
{
    // Компонент указывающий на связанную с игровым объектом сущность из мира ецс. 
    // Добавляется автоматически при создании объекта из мира ецс
    public class LinkedEntity : MonoBehaviour
    {
        public ECS.Entity entity;
    }
}