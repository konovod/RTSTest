using UnityEngine;
using System.Collections.Generic;

// BSystem is core component for simulating RTS battles
// It has 6 phases for attack and gets all different game objects parameters inside.
// Attack phases are: Search, Approach target, Attack, Self-Heal, Die, Rot (Sink to ground).
// All 6 phases are running all the time and checking if object is matching criteria, then performing actions
// Movements between different phases are also described

namespace RTSToolkitFree
{
    public class BattleSystem : MonoBehaviour
    {
        public static BattleSystem active;

        public List<Unit> allUnits = new List<Unit>();

        List<List<Unit>> targets = new List<List<Unit>>();
        List<float> targetRefreshTimes = new List<float>();
        List<KDTree> targetKD = new List<KDTree>();

        public int randomSeed = 0;

        public float searchUpdateFraction = 0.1f;
        public float retargetUpdateFraction = 0.01f;
        public float approachUpdateFraction = 0.1f;
        public float attackUpdateFraction = 0.1f;

        void Awake()
        {
            active = this;
            Random.InitState(randomSeed);
        }

        void Start()
        {
            UnityEngine.AI.NavMesh.pathfindingIterationsPerFrame = 10000;
        }

        void Update()
        {
            UpdateWithoutStatistics();
        }

        void UpdateWithoutStatistics()
        {
            float deltaTime = Time.deltaTime;

            SearchPhase(deltaTime);
            RetargetPhase();
            ApproachPhase();
            AttackPhase();
            DeathPhase();
        }


        int iSearchPhase = 0;
        float fSearchPhase = 0f;

        // The main search method, which starts to search for nearest enemies neighbours and set them for attack
        // NN search works with kdtree.cs NN search class, implemented by A. Stark at 2009.
        // Target candidates are put on kdtree, while attackers used to search for them.
        // NN searches are based on position coordinates in 3D.
        public void SearchPhase(float deltaTime)
        {
            // Refresh targets list
            for (int i = 0; i < targetRefreshTimes.Count; i++)
            {
                targetRefreshTimes[i] -= deltaTime;
                if (targetRefreshTimes[i] < 0f)
                {
                    targetRefreshTimes[i] = 1f;

                    List<Unit> nationTargets = new List<Unit>();
                    List<Vector3> nationTargetPositions = new List<Vector3>();

                    for (int j = 0; j < allUnits.Count; j++)
                    {
                        Unit up = allUnits[j];

                        if (
                            up.nation != i &&
                            up.IsApproachable &&
                            up.attackers.Count < up.maxAttackers &&
                            Diplomacy.active.relations[up.nation][i] == 1
                        )
                        {
                            nationTargets.Add(up);
                            nationTargetPositions.Add(up.transform.position);
                        }
                    }

                    targets[i] = nationTargets;
                    targetKD[i] = KDTree.MakeFromPoints(nationTargetPositions.ToArray());
                }
            }

            fSearchPhase += allUnits.Count * searchUpdateFraction;

            int nToLoop = (int)fSearchPhase;
            fSearchPhase -= nToLoop;

            for (int i = 0; i < nToLoop; i++)
            {
                iSearchPhase++;

                if (iSearchPhase >= allUnits.Count)
                {
                    iSearchPhase = 0;
                }

                Unit up = allUnits[iSearchPhase];
                int nation = up.nation;

                if (up.isReady && targets[nation].Count > 0)
                {
                    int targetId = targetKD[nation].FindNearest(up.transform.position);
                    Unit targetUp = targets[nation][targetId];

                    if (
                        targetUp.Health > 0f &&
                        targetUp.attackers.Count < targetUp.maxAttackers
                    )
                    {
                        targetUp.attackers.Add(up);
                        targetUp.noAttackers = targetUp.attackers.Count;
                        up.target = targetUp;
                        up.isReady = false;
                        up.isApproaching = true;
                    }
                }
            }
        }

        int iRetargetPhase = 0;
        float fRetargetPhase = 0f;

        // Similar as SearchPhase but is used to retarget approachers to closer targets.
        public void RetargetPhase()
        {
            fRetargetPhase += allUnits.Count * retargetUpdateFraction;

            int nToLoop = (int)fRetargetPhase;
            fRetargetPhase -= nToLoop;

            for (int i = 0; i < nToLoop; i++)
            {
                iRetargetPhase++;

                if (iRetargetPhase >= allUnits.Count)
                {
                    iRetargetPhase = 0;
                }

                Unit up = allUnits[iRetargetPhase];
                int nation = up.nation;

                if (up.isApproaching && up.target != null && targets[nation].Count > 0)
                {
                    int targetId = targetKD[nation].FindNearest(up.transform.position);
                    Unit targetUp = targets[nation][targetId];

                    if (
                        targetUp.Health > 0f &&
                        targetUp.attackers.Count < targetUp.maxAttackers
                    )
                    {
                        float oldTargetDistanceSq = (up.target.transform.position - up.transform.position).sqrMagnitude;
                        float newTargetDistanceSq = (targetUp.transform.position - up.transform.position).sqrMagnitude;

                        if (newTargetDistanceSq < oldTargetDistanceSq)
                        {
                            up.target.attackers.Remove(up);
                            up.target.noAttackers = up.target.attackers.Count;

                            targetUp.attackers.Add(up);
                            targetUp.noAttackers = targetUp.attackers.Count;
                            up.target = targetUp;
                            up.isReady = false;
                            up.isApproaching = true;
                        }
                    }
                }
            }
        }

        int iApproachPhase = 0;
        float fApproachPhase = 0f;

        // this phase starting attackers to move towards their targets
        public void ApproachPhase()
        {
            fApproachPhase += allUnits.Count * approachUpdateFraction;

            int nToLoop = (int)fApproachPhase;
            fApproachPhase -= nToLoop;

            // checking through allUnits list which units are set to approach (isApproaching)
            for (int i = 0; i < nToLoop; i++)
            {
                iApproachPhase++;

                if (iApproachPhase >= allUnits.Count)
                {
                    iApproachPhase = 0;
                }

                Unit apprPars = allUnits[iApproachPhase];

                if (apprPars.isApproaching && apprPars.target != null)
                {

                    Unit targ = apprPars.target;

                    UnityEngine.AI.NavMeshAgent apprNav = apprPars.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    UnityEngine.AI.NavMeshAgent targNav = targ.GetComponent<UnityEngine.AI.NavMeshAgent>();

                    if (targ.IsApproachable == true)
                    {
                        // stopping condition for NavMesh

                        apprNav.stoppingDistance = apprNav.radius / (apprPars.transform.localScale.x) + targNav.radius / (targ.transform.localScale.x);

                        // distance between approacher and target

                        float rTarget = (apprPars.transform.position - targ.transform.position).magnitude;
                        float stoppDistance = (2f + apprPars.transform.localScale.x * targ.transform.localScale.x * apprNav.stoppingDistance);

                        // counting increased distances (failure to approach) between attacker and target;
                        // if counter failedR becomes bigger than critFailedR, preparing for new target search.

                        if (apprPars.prevR <= rTarget)
                        {
                            apprPars.failedR = apprPars.failedR + 1;
                            if (apprPars.failedR > apprPars.critFailedR)
                            {
                                apprPars.isApproaching = false;
                                apprPars.isReady = true;
                                apprPars.failedR = 0;

                                if (apprPars.target != null)
                                {
                                    apprPars.target.attackers.Remove(apprPars);
                                    apprPars.target.noAttackers = apprPars.target.attackers.Count;
                                    apprPars.target = null;
                                }

                                if (apprPars.changeMaterial)
                                {
                                    apprPars.GetComponent<Renderer>().material.color = Color.yellow;
                                }
                            }
                        }
                        else
                        {
                            // if approachers already close to their targets
                            if (rTarget < stoppDistance)
                            {
                                apprNav.SetDestination(apprPars.transform.position);

                                // pre-setting for attacking
                                apprPars.isApproaching = false;
                                apprPars.isAttacking = true;

                                if (apprPars.changeMaterial)
                                {
                                    apprPars.GetComponent<Renderer>().material.color = Color.red;
                                }
                            }
                            else
                            {
                                if (apprPars.changeMaterial)
                                {
                                    apprPars.GetComponent<Renderer>().material.color = Color.green;
                                }

                                // starting to move
                                if (apprPars.isMovable)
                                {
                                    Vector3 destination = apprNav.destination;
                                    if ((destination - targ.transform.position).sqrMagnitude > 1f)
                                    {
                                        apprNav.SetDestination(targ.transform.position);
                                        apprNav.speed = 3.5f;
                                    }
                                }
                            }
                        }

                        // saving previous R
                        apprPars.prevR = rTarget;
                    }
                    // condition for non approachable targets	
                    else
                    {
                        apprPars.target = null;
                        apprNav.SetDestination(apprPars.transform.position);

                        apprPars.isApproaching = false;
                        apprPars.isReady = true;

                        if (apprPars.changeMaterial)
                        {
                            apprPars.GetComponent<Renderer>().material.color = Color.yellow;
                        }
                    }
                }
            }
        }

        int iAttackPhase = 0;
        float fAttackPhase = 0f;

        // Attacking phase set attackers to attack their targets and cause damage when they already approached their targets
        public void AttackPhase()
        {
            fAttackPhase += allUnits.Count * attackUpdateFraction;

            int nToLoop = (int)fAttackPhase;
            fAttackPhase -= nToLoop;

            // checking through allUnits list which units are set to approach (isAttacking)
            for (int i = 0; i < nToLoop; i++)
            {
                iAttackPhase++;

                if (iAttackPhase >= allUnits.Count)
                {
                    iAttackPhase = 0;
                }

                Unit attPars = allUnits[iAttackPhase];

                if (attPars.isAttacking && attPars.target != null)
                {
                    Unit targPars = attPars.target;

                    UnityEngine.AI.NavMeshAgent attNav = attPars.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    UnityEngine.AI.NavMeshAgent targNav = targPars.GetComponent<UnityEngine.AI.NavMeshAgent>();

                    attNav.stoppingDistance = attNav.radius / (attPars.transform.localScale.x) + targNav.radius / (targPars.transform.localScale.x);

                    // distance between attacker and target

                    float rTarget = (attPars.transform.position - targPars.transform.position).magnitude;
                    float stoppDistance = (2.5f + attPars.transform.localScale.x * targPars.transform.localScale.x * attNav.stoppingDistance);

                    // if target moves away, resetting back to approach target phase

                    if (rTarget > stoppDistance)
                    {
                        attPars.isApproaching = true;
                        attPars.isAttacking = false;
                    }
                    // attacker starts attacking their target	
                    else
                    {
                        if (attPars.changeMaterial)
                        {
                            attPars.GetComponent<Renderer>().material.color = Color.red;
                        }

                        float strength = attPars.strength;
                        float defence = attPars.defence;

                        // if attack passes target through target defence, cause damage to target
                        if (Random.value > (strength / (strength + defence)))
                        {
                            targPars.Health = targPars.Health - 2.0f * strength * Random.value;
                        }
                    }
                }
            }
        }

        // Death phase unset all unit activity and prepare to die
        public void DeathPhase()
        {
            for (int i = 0; i < allUnits.Count; i++)
            {
                if (allUnits[i].IsDead)
                {
                    allUnits.RemoveAt(i);
                }
            }
        }

       

        public void AddNation()
        {
            targets.Add(new List<Unit>());
            targetRefreshTimes.Add(-1f);
            targetKD.Add(null);
        }
    }
}
