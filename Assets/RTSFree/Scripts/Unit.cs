using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;

namespace RTSToolkitFree
{
    public class Unit : MonoBehaviour
    {
        public bool isMovable = true;

        public bool isReady = false;
        public bool isApproaching = false;
        public bool isAttacking = false;

        /// <summary>
        /// Начал ли юнит процесс умирания
        /// </summary>
        private bool isDying = false;

        public Unit target = null;
        public List<Unit> attackers = new List<Unit>();

        public int noAttackers = 0;
        public int maxAttackers = 3;

        [HideInInspector] public float prevR;
        [HideInInspector] public int failedR = 0;
        public int critFailedR = 100;


		public float health = 100.0f;
		/// <summary>
        /// Здоровье от 0 до 100 (0 - мертв)
        /// </summary>
        public float Health
        { 
            get { return health; }
            set 
            { 
                health = value;

                if (health <= 0) {  health = 0; isDying = true; }
                if (health > 100) { health = 100; }
                if (StatusBar != null)
                {
					StatusBar.SetHealth(health);
				}
                if (isDying == true)
                {
                    StartCoroutine(DelayDeath(5));
                }
            }
        }

        /// <summary>
        /// Мертв ли
        /// </summary>
        public bool IsDead
        {
            get { return Health == 0; }
        }

        /// <summary>
        /// Можно ли давать задание
        /// </summary>
		public bool IsApproachable
        {
            get { return IsDead == false; }
        }


		public float maxHealth = 100.0f;
        public float selfHealFactor = 10.0f;

        public float strength = 10.0f;
        public float defence = 10.0f;

        [HideInInspector] public bool changeMaterial = true;

        public int nation = 1;

		private NavMeshAgent agent;
		private StatusBar StatusBar;

		void Start()
        {
			agent = GetComponent<NavMeshAgent>();
			StatusBar = GetComponentInChildren<StatusBar>();
           
            if (agent != null)
            {
				agent.enabled = true;
            }
        }


        public IEnumerator DelayDeath(float argTime)
        {
			isMovable = false;
			isReady = false;
			isApproaching = false;
			isAttacking = false;
			target = null;

			// unselecting deads	
			ManualControl manualControl = GetComponent<ManualControl>();
			if (manualControl != null)
			{
				manualControl.IsSelected = false;
			}

			transform.gameObject.tag = "Untagged";

			GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

			if (changeMaterial)
			{
				GetComponent<Renderer>().material.color = Color.blue;
			}
			yield return new WaitForSeconds(argTime);
            StartCoroutine(DelaySink());
		}

		public float sinkUpdateFraction = 1f;

		public IEnumerator DelaySink()
        {
			if (changeMaterial)
			{
				GetComponent<Renderer>().material.color = new Color((148.0f / 255.0f), (0.0f / 255.0f), (211.0f / 255.0f), 1.0f);
			}

			// moving sinking object down into the ground	
			while (transform.position.y > -1.0f)
			{
				float sinkSpeed = -0.2f;
				transform.position += new Vector3(0f, sinkSpeed * Time.deltaTime / sinkUpdateFraction, 0f);
				yield return new WaitForSeconds(0.1f);
			}

			Destroy(gameObject);
		}


		public void ResetSearching()
		{
			isApproaching = false;
			isAttacking = false;
			target = null;

			agent.SetDestination(transform.position);

			if (changeMaterial)
			{
				GetComponent<Renderer>().material.color = Color.yellow;
			}

			isReady = true;
		}

		public void UnSetSearching()
		{
			if (isMovable)
			{
				isReady = false;
				isApproaching = false;
				isAttacking = false;
				target = null;
				
				agent.SetDestination(transform.position);

				if (changeMaterial)
				{
					GetComponent<Renderer>().material.color = Color.grey;
				}
			}
		}


	}
}
