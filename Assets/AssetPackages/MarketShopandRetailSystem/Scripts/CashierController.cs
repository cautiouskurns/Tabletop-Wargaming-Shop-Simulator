using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace MarketShopandRetailSystem
{
    public class CashierController : MonoBehaviour
    {
        private NavMeshAgent agent;
        public string Name;
        private Animator animator;
        private Transform target;
        private float lastCheckTime = 0;
        public bool isworking = false;
        private bool gotMySalaryToday = false;
        private CashRegister currentCashRegister;


        private void OnEnable()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            isworking = false;
            target = null;
        }

        IEnumerator ScanAndGetPayment()
        {
            isworking = true;
            animator.SetTrigger("Work");
            for (int i = 0; i < currentCashRegister.productsofCustomer.Count; i++)
            {
                currentCashRegister.Scan(currentCashRegister.productsofCustomer[i]);
                yield return new WaitForSeconds(0.6f);
            }
            if(currentCashRegister != null)
            {
                currentCashRegister.Click_Button_Process_ByCashier();
            }
            yield return new WaitForSeconds(1);
            target = null;
            currentCashRegister = null;
            isworking = false;
        }

        private void Start()
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }


        // Update is called once per frame
        void Update()
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
            if (isworking)
            {
                if(target != null)
                {
                    return;
                }
                else
                {
                    isworking = false;
                    target = null;
                    return;
                }
            }
            if (DayNightManager.Instance != null)
            {
                if (!DayNightManager.Instance.isDark && !gotMySalaryToday)
                {
                    gotMySalaryToday = true;
                    AdvancedGameManager.Instance.Spend(AdvancedGameManager.Instance.cashierDailySalary);
                }
                else if (DayNightManager.Instance.isDark)
                {
                    gotMySalaryToday = false;
                }
            }
            if (Time.time > lastCheckTime + 0.5f)
            {
                lastCheckTime = Time.time;
                if (agent.hasPath && target != null)
                {
                    float distance = 0;
                    if (target != null)
                    {
                        distance = Vector3.Distance(transform.position, target.position);
                    }
                    if (distance <= agent.stoppingDistance)
                    {
                        if(currentCashRegister.CurrentCashier != null && currentCashRegister.CurrentCashier != this)
                        {
                            agent.isStopped = true;
                            target = null;
                            currentCashRegister = null;
                        }
                        else
                        {
                            agent.isStopped = true;
                            transform.LookAt(currentCashRegister.transform);
                            currentCashRegister.CurrentCashier = this;
                            if (currentCashRegister != null && currentCashRegister.isBusy)
                            {
                                StartCoroutine(ScanAndGetPayment());
                            }
                        }
                    }
                    else if (agent.hasPath && (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid))
                    {
                        agent.isStopped = true;
                        target = null;
                        currentCashRegister = null;
                    }
                    else if (target != null && agent.pathStatus == NavMeshPathStatus.PathComplete)
                    {
                        agent.SetDestination(target.position);
                    }
                }
                else
                {
                    InventoryManager.Instance.CheckCurrentEquipmentList();
                    CashRegister cashRegister = InventoryManager.Instance.CurrentEquipmentList.Where(x => x.CompareTag("Item") && x.GetComponent<CashRegister>() != null && !x.GetComponent<CashRegister>().isBusy && (x.GetComponent<CashRegister>().CurrentCashier == null || x.GetComponent<CashRegister>().CurrentCashier == this)).OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).Select(x => x.GetComponent<CashRegister>()).FirstOrDefault();
                    if (cashRegister != null && !cashRegister.isBusy)
                    {
                        target = cashRegister.pointPlayerStand.transform;
                        NavMeshPath path = new NavMeshPath();
                        bool pathFound = NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
                        if (pathFound && path.status == NavMeshPathStatus.PathComplete)
                        {
                            currentCashRegister = cashRegister;
                            agent.isStopped = false;
                            agent.SetDestination(target.position);
                        }
                    }
                }
            }
        }
    }
}
