using UnityEngine;
using UnityEngine.UI;

namespace RTSToolkitFree
{
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager active;
        public Texture marqueeGraphics;
        private Vector2 marqueeOrigin;
        private Vector2 marqueeSize;
        public Rect marqueeRect;
        private Rect backupRect;
        public Color rectColor = new Color(1f, 1f, 1f, 0.3f);
        bool selectedByClickRunning = false;

        void Awake()
        {
            active = this;
        }

        private void OnGUI()
        {
            marqueeRect = new Rect(marqueeOrigin.x, marqueeOrigin.y, marqueeSize.x, marqueeSize.y);
            GUI.color = rectColor;
            GUI.DrawTexture(marqueeRect, marqueeGraphics);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                float _invertedY = Screen.height - Input.mousePosition.y;
                marqueeOrigin = new Vector2(Input.mousePosition.x, _invertedY);
            }

            if (Input.GetMouseButton(0))
            {
                float _invertedY = Screen.height - Input.mousePosition.y;
                marqueeSize = new Vector2(Input.mousePosition.x - marqueeOrigin.x, (marqueeOrigin.y - _invertedY) * -1);

                //FIX FOR RECT.CONTAINS NOT ACCEPTING NEGATIVE VALUES
                if (marqueeRect.width < 0)
                {
                    backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y, Mathf.Abs(marqueeRect.width), marqueeRect.height);
                }
                else if (marqueeRect.height < 0)
                {
                    backupRect = new Rect(marqueeRect.x, marqueeRect.y - Mathf.Abs(marqueeRect.height), marqueeRect.width, Mathf.Abs(marqueeRect.height));
                }
                if (marqueeRect.width < 0 && marqueeRect.height < 0)
                {
                    backupRect = new Rect(marqueeRect.x - Mathf.Abs(marqueeRect.width), marqueeRect.y - Mathf.Abs(marqueeRect.height), Mathf.Abs(marqueeRect.width), Mathf.Abs(marqueeRect.height));
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!selectedByClickRunning)
                {
                    foreach (var unit in ECSWorldContainer.Active.world.Each<ECSGame.Controllable>())
                    {

                        //Convert the world position of the unit to a screen position and then to a GUI point
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.Get<UnityECSLink.LinkedGameObject>().Transform().position);
                        Vector2 screenPoint = new Vector2(screenPos.x, Screen.height - screenPos.y);

                        if (marqueeRect.Contains(screenPoint) || backupRect.Contains(screenPoint))
                        {
                            unit.Set(new ECSGame.JustSelected());
                        }
                    }

                }

                selectedByClickRunning = false;
            }

            if (Input.GetMouseButtonUp(0))
            {
                //Reset the marquee so it no longer appears on the screen.
                marqueeRect.width = 0;
                marqueeRect.height = 0;
                backupRect.width = 0;
                backupRect.height = 0;
                marqueeSize = Vector2.zero;
            }

            if (Input.GetMouseButtonUp(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                    ECSWorldContainer.Active.world.NewEntity().Add(new ECSGame.ManualTarget(hit.point));
            }
        }
    }
}
