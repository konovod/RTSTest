using System.Collections;

using UnityEngine;
using UnityEngine.AI;


namespace RTSToolkitFree
{
    public class ManualControl : MonoBehaviour
    {
		private bool isSelected = false;
		public bool IsSelected
        { 
            get { return isSelected; }
            set 
            { 
                isSelected = value;
                if (StatusBar != null)
                {
                    StatusBar.Select(isSelected);
                }
			}
        }

        public bool isMoving = false;

        [HideInInspector] public float prevDist = 0.0f;
        [HideInInspector] public int failedDist = 0;
        public int critFailedDist = 10;

        public Vector3 manualDestination;

		public Coroutine moveCoroutine;

		private NavMeshAgent agent;
		private StatusBar StatusBar;
		private Unit unit;


		void Start()
        {
			agent = GetComponent<NavMeshAgent>();
            StatusBar = GetComponentInChildren<StatusBar>();
			unit = GetComponent<Unit>();
		}


        public IEnumerator Move()
        {
			if (unit.IsDead == false)
			{
				unit.UnSetSearching();
				agent.SetDestination(manualDestination);
				isMoving = true;

				while (isMoving)
				{
					float r = (transform.position - manualDestination).magnitude;
					if (r >= prevDist)
					{
						failedDist++;
						if (failedDist > critFailedDist)
						{
							failedDist = 0;
							isMoving = false;
							unit.ResetSearching();
							yield return new WaitForSeconds(1.0f);
						}
					}

					prevDist = r;
					yield return new WaitForSeconds(0.1f);
				}
			}

		}


	}
}
