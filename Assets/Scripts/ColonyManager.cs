using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ColonyManager : MonoBehaviour {

    // Public Variables
    public enum ShopItems { miningBase }

    private List<MarsBase> m_bases;

    // Resource Tracking Variables
    private int m_money;
    private int m_iron;
    private bool m_running = false;
    private Dictionary<ShopItems, int> m_costs;


    // Accessors
    public int iron { get { return m_iron; } }
    public int money { get { return m_money; } }
    public bool running { get { return m_running; } }
    public List<MarsBase> bases { get { return m_bases; } }
    public Dictionary<ShopItems, int> costs { get { return m_costs; } }

    // Use this for initialization
    void Start() {
        m_bases = new List<MarsBase>();
        m_money = 100;
        m_iron = 0;
        m_costs = new Dictionary<ShopItems, int>();
        m_costs.Add(ShopItems.miningBase, 50);
    }

    public void AddBase() {
        m_bases.Add(new MarsBase());
        m_money -= m_costs[ShopItems.miningBase];
    }

    public void StartSim() {
        m_running = true;
        StartCoroutine("GridClock");
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