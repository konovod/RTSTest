using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;

namespace RTSToolkitFree
{
	public class Unit : MonoBehaviour, IHealth
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

		//public int noAttackers = 0;
		public int maxAttackers = 3;

		[HideInInspector] public float prevTargetD;
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

				if (health <= 0) { health = 0; isDying = true; }
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

		[HideInInspector] public NavMeshAgent agent;
		[HideInInspector] public Renderer my_renderer;
		[HideInInspector] public StatusBar StatusBar;

		void Start()
		{
			agent = GetComponent<NavMeshAgent>();
			my_renderer = GetComponent<Renderer>();
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

			if (agent.enabled)
			{
				agent.SetDestination(transform.position);
			}

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

				if (agent.enabled)
				{
					agent.SetDestination(transform.position);
				}

				if (changeMaterial)
				{
					GetComponent<Renderer>().material.color = Color.grey;
				}
			}
		}


		public void ApplyDamage(float argDamage)
		{
			Health -= argDamage;
		}


		public void Search()
		{
			if (isReady)
			{
				Unit tmpTarget = BattleSystem.active.FindNearest(nation, transform.position);

				if (tmpTarget != null &&
					tmpTarget.IsDead == false &&
					tmpTarget.attackers.Count < tmpTarget.maxAttackers
				)
				{
					tmpTarget.attackers.Add(this);
					target = tmpTarget;
					isReady = false;
					isApproaching = true;
				}
			}
		}

		public void Retarget()
		{
			if (isApproaching && target != null)
			{
				Unit tmpTarget = BattleSystem.active.FindNearest(nation, transform.position);

				if (tmpTarget != null &&
					tmpTarget.IsDead == false &&
					tmpTarget.attackers.Count < tmpTarget.maxAttackers
				)
				{
					float oldTargetDistanceSq = (target.transform.position - transform.position).sqrMagnitude;
					float newTargetDistanceSq = (tmpTarget.transform.position - transform.position).sqrMagnitude;

					if (newTargetDistanceSq < oldTargetDistanceSq)
					{
						target.attackers.Remove(this);

						tmpTarget.attackers.Add(this);
						target = tmpTarget;
						isReady = false;
						isApproaching = true;
					}
				}
			}
		}

		public void Approach()
		{
			if (isApproaching && target != null)
			{
				if (target.IsApproachable == true)
				{
					// дистанция между юнитом и целью
					float newTargetD = (transform.position - target.transform.position).magnitude;

					// Если атакующий не может подойти к своей цели, увеличивается счетчик failedR
					// и если счетчик становится больше critFailedR, то цель скидывается и запрашивается новая цель
					if (prevTargetD <= newTargetD)
					{
						failedR = failedR + 1;
						if (failedR > critFailedR)
						{
							isApproaching = false;
							isReady = true;
							failedR = 0;

							if (target != null)
							{
								target.attackers.Remove(this);
								target = null;
							}
							ChangeMaterial(Color.yellow);
						}
					}
					else
					{
						agent.stoppingDistance = agent.radius / (transform.localScale.x) + target.agent.radius / (target.transform.localScale.x);
						float stoppDistance = (2f + transform.localScale.x * target.transform.localScale.x * agent.stoppingDistance);

						// если приближающийся уже близок к своей цели
						if (newTargetD < stoppDistance)
						{
							agent.SetDestination(transform.position);

							// pre-setting for attacking
							isApproaching = false;
							isAttacking = true;

							ChangeMaterial(Color.red);
						}
						else
						{
							ChangeMaterial(Color.green);

							// начинаем двигаться
							if (isMovable)
							{
								if ((agent.destination - target.transform.position).sqrMagnitude > 1f)
								{
									agent.SetDestination(target.transform.position);
									agent.speed = 3.5f;
								}
							}
						}
					}

					// saving previous R
					prevTargetD = newTargetD;
				}
				// condition for non approachable targets	
				else
				{
					target = null;
					agent.SetDestination(transform.position);

					isApproaching = false;
					isReady = true;

					ChangeMaterial(Color.yellow);
				}
			}

		}

		public void Attack()
		{
			if (isAttacking && target != null)
			{
				agent.stoppingDistance = agent.radius / (transform.localScale.x) + target.agent.radius / (target.transform.localScale.x);

				// distance between attacker and target

				float rTarget = (transform.position - target.transform.position).magnitude;
				float stoppDistance = (2.5f + transform.localScale.x * target.transform.localScale.x * agent.stoppingDistance);

				// if target moves away, resetting back to approach target phase

				if (rTarget > stoppDistance)
				{
					isApproaching = true;
					isAttacking = false;
				}
				// attacker starts attacking their target	
				else
				{
					ChangeMaterial(Color.red);

					// if attack passes target through target defence, cause damage to target
					if (UnityEngine.Random.value > (strength / (strength + defence)))
					{
						target.Health = target.Health - 2.0f * strength * UnityEngine.Random.value;
					}
				}
			}
		}


		public void ChangeMaterial(Color argColor)
		{
			if (changeMaterial)
			{
				my_renderer.material.color = argColor;
			}
		}

	}

	public interface IHealth
	{
		void ApplyDamage(float argDamage);
	}
}
