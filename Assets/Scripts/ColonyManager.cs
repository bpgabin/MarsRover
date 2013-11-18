using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ColonyManager : MonoBehaviour {

    // Public Variables
    public enum ShopItems { miningBase, rover, firstUpgrade, secondUpgrade }
    public enum SellItems { rover }

    private List<MarsBase> m_bases;

    // Resource Tracking Variables
    private int m_iron;
    private int m_money;
    private bool m_running = false;
    private int m_ironSinceAudit;
    private int m_lastAuditAmount;
    private int m_auditGoal;
    private int m_timesAudited;
    private int m_totalIronSold;
    private int m_strikes;
    private int m_lastAuitGoal;
    private Dictionary<ShopItems, int> m_costs;
    private Dictionary<SellItems, int> m_sell;

    // Timer Events Variables
    public float currentSpaceElevatorTime;
    public float spaceElevatorTime = 60f;
    public float currentAuditTime;
    public float auditTime = 240f;
    public bool auditing = false;

    // Accessors
    public int iron { get { return m_iron; } }
    public int money { get { return m_money; } }
    public int timesAudited { get { return m_timesAudited; } }
    public int ironSoldSinceAudit { get { return m_ironSinceAudit; } }
    public int totalIronSold { get { return m_totalIronSold; } }
    public int strikes { get { return m_strikes; } }
    public int lastAuditGoal { get { return m_lastAuitGoal; } }
    public int auditGoal { get { return m_auditGoal; } }
    public int lastAuditAmount { get { return m_lastAuditAmount; } }
    public bool running { get { return m_running; } }
    public List<MarsBase> bases { get { return m_bases; } }
    public Dictionary<ShopItems, int> costs { get { return m_costs; } }
    public Dictionary<SellItems, int> sellPrice { get { return m_sell; } }

    // Use this for initialization
    void Start() {
        currentSpaceElevatorTime = spaceElevatorTime;
        currentAuditTime = auditTime;
        m_bases = new List<MarsBase>();
        m_iron = 0;
        m_money = 250;
        m_ironSinceAudit = 0;
        m_lastAuditAmount = 0;
        m_auditGoal = 0;
        m_timesAudited = 0;
        m_lastAuitGoal = 0;
        m_totalIronSold = 0;
        m_strikes = 0;
        m_costs = new Dictionary<ShopItems, int>();
        m_costs[ShopItems.miningBase] = 150;
        m_costs[ShopItems.rover] = 50;
        m_costs[ShopItems.firstUpgrade] = 100;
        m_costs[ShopItems.secondUpgrade] = 200;

        m_sell = new Dictionary<SellItems, int>();
        m_sell[SellItems.rover] = 25;
    }

    void Update() {
        if (running) {
            currentSpaceElevatorTime -= Time.deltaTime;
            if (currentSpaceElevatorTime <= 0) {
                SpaceElevatorArrived();
            }

            currentAuditTime -= Time.deltaTime;
            if (currentAuditTime <= 0) {
                AuditInventory();
            }
        }
    }

    public void SellItem(SellItems item) {
        m_money += m_sell[item];
    }

    public void BuyItem(ShopItems item) {
        m_money -= m_costs[item];
    }

    private void SpaceElevatorArrived() {
        m_money += m_iron * 50;
        m_totalIronSold += m_iron;
        m_ironSinceAudit += m_iron;
        m_iron = 0;
        currentSpaceElevatorTime = spaceElevatorTime;
    }

    private void AuditInventory() {
        currentAuditTime = auditTime;
        auditing = true;

        if (m_auditGoal != 0) {
            if (m_ironSinceAudit < m_auditGoal)
                m_strikes++;
        }

        m_lastAuitGoal = m_auditGoal;
        m_auditGoal = Mathf.RoundToInt(m_lastAuditAmount * 1.5f) + 1;
        m_lastAuditAmount = m_ironSinceAudit;
        m_ironSinceAudit = 0;
        m_timesAudited++;
    }

    public void AddBase() {
        switch (m_bases.Count) {
            case 0:
                m_bases.Add(new MarsBase(MarsBase.BaseNumber.baseOne, TramLaunched));
                break;
            case 1:
                m_bases.Add(new MarsBase(MarsBase.BaseNumber.baseTwo, TramLaunched));
                break;
            case 2:
                m_bases.Add(new MarsBase(MarsBase.BaseNumber.baseThree, TramLaunched));
                break;
            case 3:
                m_bases.Add(new MarsBase(MarsBase.BaseNumber.baseFour, TramLaunched));
                break;
            case 4:
                m_bases.Add(new MarsBase(MarsBase.BaseNumber.baseFive, TramLaunched));
                break;
            default:
                Debug.LogError("Sixth MarsBase is attempting to be made.");
                break;
        }
        m_money -= m_costs[ShopItems.miningBase];
    }

    public void StartSim() {
        m_running = true;
        StartCoroutine("GridClock");
    }

    public void TramLaunched(List<MarsBase.ResourceType> resources) {
        foreach (MarsBase.ResourceType resource in resources) {
            if (resource == MarsBase.ResourceType.refinedIron) {
                m_iron++;
            }
            else if (resource == MarsBase.ResourceType.doubleRefinedIron) {
                m_iron += 2;
            }
            else if (resource == MarsBase.ResourceType.tripleRefinedIron) {
                m_iron += 3;
            }
        }
        resources.Clear();
    }

    public void StopSim() {
        m_running = false;
    }

    IEnumerator GridClock() {
        yield return new WaitForSeconds(1f);
        while (m_running) {
            foreach (MarsBase mBase in m_bases) {
                if (mBase.running)
                    mBase.CalculateMoves();
            }
            yield return new WaitForSeconds(1f);
        }
    }
}