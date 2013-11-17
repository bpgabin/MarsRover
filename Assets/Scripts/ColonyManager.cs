using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ColonyManager : MonoBehaviour {

    // Public Variables
    public enum ShopItems { miningBase }

    private List<MarsBase> m_bases;

    // Resource Tracking Variables
    private int m_iron;
    private int m_money;
    private bool m_running = false;
    private Dictionary<ShopItems, int> m_costs;

    // Timer Events Variables
    public float currentSpaceElevatorTime;
    public float spaceElevatorTime = 60f;
    public float currentAuditTime;
    public float auditTime = 240f;

    // Accessors
    public int iron { get { return m_iron; } }
    public int money { get { return m_money; } }
    public bool running { get { return m_running; } }
    public List<MarsBase> bases { get { return m_bases; } }
    public Dictionary<ShopItems, int> costs { get { return m_costs; } }

    // Use this for initialization
    void Start() {
        currentSpaceElevatorTime = spaceElevatorTime;
        currentAuditTime = auditTime;
        m_bases = new List<MarsBase>();
        m_iron = 0;
        m_money = 100;
        m_costs = new Dictionary<ShopItems, int>();
        m_costs.Add(ShopItems.miningBase, 50);
    }

    void Update() {
        currentSpaceElevatorTime -= Time.deltaTime;
        if (currentSpaceElevatorTime <= 0) {
            SpaceElevatorArrived();
        }

        currentAuditTime -= Time.deltaTime;
        if (currentAuditTime <= 0) {
            AuditInventory();
        }
    }

    private void SpaceElevatorArrived() {
        m_money += m_iron * 50;
        m_iron = 0;
        currentSpaceElevatorTime = spaceElevatorTime;
    }

    private void AuditInventory() {
        currentAuditTime = auditTime;
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