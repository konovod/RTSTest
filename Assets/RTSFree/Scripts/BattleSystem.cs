using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

// Управление сражениям проходит через 4 фазы: Search (поиск противника), Retarget (реакция на смену ближайшего противника)
// Approach (движение к противнику), Attack (атака противника) 
// А так же после смерти юнита происходят следующие процесы Die (смерть и труп остается какое-то время неизменным)
// и Sink to ground (медленное спускание под землю).

namespace RTSToolkitFree
{
    public class BattleSystem : MonoBehaviour
    {
        public static BattleSystem active;

		public int numberNations;
		public int playerNation = 0;

		public List<Unit> allUnits = new List<Unit>();

		List<List<Unit>> targets = new List<List<Unit>>();
		List<KDTree> targetKD = new List<KDTree>();

		public int randomSeed = 0;

        void Awake()
        {
            active = this;
			UnityEngine.Random.InitState(randomSeed);
        }

        void Start()
        {
            UnityEngine.AI.NavMesh.pathfindingIterationsPerFrame = 10000;
            StartCoroutine(RefreshDistanceTree());
        }

        void Update()
        {
            UpdateWithoutStatistics();
        }

        void UpdateWithoutStatistics()
        {
			UpdateRate(SearchPhase, "Search", 0.1f);
			UpdateRate(RetargetPhase, "Retarget", 0.01f);
			UpdateRate(ApproachPhase, "Approach", 0.1f);
			UpdateRate(AttackPhase, "Attack", 0.1f);

            DeathPhase();
        }


        public IEnumerator RefreshDistanceTree()
        {
            while (true)
            {
                // Разделение по нациям
                for (int i = 0; i < targetKD.Count; i++)
                {
                    List<Unit> nationTargets = new List<Unit>();
                    List<Vector3> nationTargetPositions = new List<Vector3>();

                    for (int j = 0; j < allUnits.Count; j++)
                    {
                        Unit up = allUnits[j];
                        if (
                            up.nation != i &&
                            up.IsApproachable &&
                            up.attackers.Count < up.maxAttackers
                        )
                        {
                            nationTargets.Add(up);
                            nationTargetPositions.Add(up.transform.position);
                        }
                    }
                    targets[i] = nationTargets;
                    targetKD[i] = KDTree.MakeFromPoints(nationTargetPositions.ToArray());
                }

                yield return new WaitForSeconds(1.0f);
            }
		}


		public void AddNation()
		{
			numberNations++;
			targets.Add(new List<Unit>());
			targetKD.Add(null);
		}

        public delegate void Run(int unitIndex);
        public void UpdateRate(Run run, string phaseName, float rate)
        {
			DateTime begin = DateTime.Now;

			if (rIndex.ContainsKey(phaseName) == false)
            { 
                rIndex.Add(phaseName, 0);
            }

			int nToLoop = (int)(allUnits.Count * rate);
			for (int i = 0; i < nToLoop; i++)
			{
				rIndex[phaseName]++;
				if (rIndex[phaseName] >= allUnits.Count)
				{
					rIndex[phaseName] = 0;
				}
				run(rIndex[phaseName]);
			}

			double t = (DateTime.Now - begin).Milliseconds;
			if (t > 5)
			{
				Debug.Log(phaseName + ": " + t.ToString() + " ms");
			}
		}

		private Dictionary<string, int> rIndex = new Dictionary<string, int>();

		private void SearchPhase(int unitIndex)
		{
			allUnits[unitIndex].Search();
		}
		private void RetargetPhase(int unitIndex)
        {
            allUnits[unitIndex].Retarget();
        }
		private void ApproachPhase(int unitIndex)
		{
			allUnits[unitIndex].Approach();
		}
		private void AttackPhase(int unitIndex)
		{
			allUnits[unitIndex].Attack();
		}
		private void DeathPhase()
		{
			for (int i = 0; i < allUnits.Count; i++)
			{
				if (allUnits[i].IsDead)
				{
					allUnits.RemoveAt(i);
				}
			}
		}


		public Unit FindNearest(int nation, Vector3 argPosition)
        {
            if (nation < targets.Count && targets[nation].Count > 0)
            {
                int targetId = targetKD[nation].FindNearest(argPosition);
                return targets[nation][targetId];
            }
            return null;
		}

    }
}
