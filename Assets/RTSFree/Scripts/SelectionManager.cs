using UnityEngine;

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
                //Poppulate the selectableUnits array with all the selectable units that exist

                float _invertedY = Screen.height - Input.mousePosition.y;
                marqueeOrigin = new Vector2(Input.mousePosition.x, _invertedY);

                //Check if the player just wants to select a single unit opposed to drawing a marquee and selecting a range of units
                Vector3 camPos = Camera.main.transform.position;

                for (int i = 0; i < BattleSystem.active.allUnits.Count; i++)
                {
                    Unit up = BattleSystem.active.allUnits[i];
                    ManualControl manualControl = up.GetComponent<ManualControl>();

                    if (manualControl != null)
                    {
                        manualControl.IsSelected = false;
                    }
                }
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
                    for (int i = 0; i < BattleSystem.active.allUnits.Count; i++)
                    {
                        Unit up = BattleSystem.active.allUnits[i];

                        //Convert the world position of the unit to a screen position and then to a GUI point
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(up.transform.position);
                        Vector2 screenPoint = new Vector2(screenPos.x, Screen.height - screenPos.y);

                        if (marqueeRect.Contains(screenPoint) || backupRect.Contains(screenPoint))
                        {
                            Unit unit = up.GetComponent<Unit>();

                            if (
                                unit.nation == Diplomacy.active.playerNation &&
                                unit.isMovable &&
                                unit.Health > 0f
                            )
                            {
                                ManualControl manualControl = up.GetComponent<ManualControl>();

                                if (manualControl != null)
                                {
                                    manualControl.IsSelected = true;
                                }
                            }
                        }
                    }

                }

                selectedByClickRunning = false;
            }

			CheckForUnitCommands();

			if (Input.GetMouseButtonUp(0))
            {
                //Reset the marquee so it no longer appears on the screen.
                marqueeRect.width = 0;
                marqueeRect.height = 0;
                backupRect.width = 0;
                backupRect.height = 0;
                marqueeSize = Vector2.zero;
            }
        }

        


		void CheckForUnitCommands()
        {
            if (Input.GetMouseButtonUp(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    for (int i = 0; i < BattleSystem.active.allUnits.Count; i++)
                    {
                        Unit up = BattleSystem.active.allUnits[i];
                        ManualControl manualControl = up.GetComponent<ManualControl>();

                        if (manualControl != null && manualControl.IsSelected && up.nation == Diplomacy.active.playerNation)
                        {
                            manualControl.manualDestination = hit.point;

                            if (manualControl.moveCoroutine != null)
                            { 
                                StopCoroutine(manualControl.moveCoroutine);
                            }
                            manualControl.moveCoroutine = StartCoroutine(manualControl.Move());

                            if (up.target != null)
                            {
                                Unit currentTarget = up.target.GetComponent<Unit>();
                                currentTarget.attackers.Remove(up);
                                currentTarget.noAttackers = currentTarget.attackers.Count;
                                up.target = null;
                            }

                        }
                    }
                }
            }
        }
        
    }
}
