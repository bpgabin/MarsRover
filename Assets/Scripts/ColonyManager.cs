using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColonyManager : MonoBehaviour {
	
	private List<MarsBase> bases;
	private Hashtable resourceTable;
	
	// Use this for initialization
	void Start () {
		bases = new List<MarsBase>();
		resourceTable = new Hashtable();
		resourceTable.Add("Iron", 0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void AddBase(MarsBase.BaseType baseType){
		MarsBase newBase = new MarsBase(baseType);
		bases.Add(newBase);
	}
	
	public void DrawColony(){
		GUI.Box(new Rect(Screen.width / 2 - 100, 50, 200, 200), "Elevator");
		foreach(MarsBase mBase in base){	
		}
	}
}
