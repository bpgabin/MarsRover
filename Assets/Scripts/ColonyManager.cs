using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ColonyManager : MonoBehaviour {
	
	private Dictionary<MarsBase.BaseType, MarsBase> m_bases;
	
	// Resource Tracking Variables
	private int m_money;
	private int m_iron;
	
	private Dictionary<MarsBase.BaseType, int> m_costs;
	
	
	// Accessors
	public int iron { get { return m_iron; } }
	public int money { get { return m_money; } }
	public Dictionary<MarsBase.BaseType, MarsBase> bases { get { return m_bases; } }
	public Dictionary<MarsBase.BaseType, int> costs { get { return m_costs; } }
	
	// Use this for initialization
	void Start () {
		m_bases = new Dictionary<MarsBase.BaseType, MarsBase>();
		m_money = 100;
		m_iron = 0;
		
		m_costs = new Dictionary<MarsBase.BaseType, int>();
		m_costs.Add(MarsBase.BaseType.mining, 50);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void AddBase(MarsBase.BaseType baseType){
		m_bases[baseType] = new MarsBase(baseType);
		m_money -= m_costs[baseType];
	}
}
