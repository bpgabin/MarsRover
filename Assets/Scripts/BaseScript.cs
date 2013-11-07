using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseScript : MonoBehaviour {
	
	private enum GridTileType { rover, wall, drill, processIn, processOut, tram, open }
	private GridTile[,] baseGrid;
	private int GRID_HEIGHT = 10;
	private int GRID_WIDTH = 10;
	
	private class GridTile {
		private GridTileType tileType;
		private Rover rover;
		
		public GridTile	(GridTileType tileType)	{
			this.tileType = tileType;
		}
		
		public void SetTileType (GridTileType tileType) {
			this.tileType = tileType;	
		}
		
		public void GetTileType () {
			return tileType;
		}
		
		public Rover GetRover () {
			return Rover;	
		}
		
		public void SetRover (Rover rover) {
			this.rover = rover;	
		}
	}
	
	private class Rover {
		private List<Action> actions;
		private int currentAction;
		
		public Rover(){
			currentAction = 0;	
		}
		
		public Action GetCurrentAction(){
			return actions[currentAction];
		}
		
		public Action GetNextAction(){
			int nextAction = currentAction + 1;
			if(nextAction > actions.Count)
				nextAction = 0;
			return actions[nextAction];
		}
		
		public void AdvanceAction(){
			currentAction++;	
		}
		
		public void Reset(){
			currentAction = 0;
		}
		
		public void ClearActions(){
			actions.Clear();
			currentAction = 0;
		}
		
		public void AddAction(Action newAction){
			actions.Add(newAction);
		}
	}
	
	private class Action {
		
	}
	
	// Use this for initialization
	void Start () {
		// Initialize Grid Structure
		baseGrid = new GridTile[GRID_WIDTH, GRID_HEIGHT];
		for(int i = 0; i < GRID_WIDTH; i++){
			for(int j = 0; j < GRID_WIDTH; j++){
				baseGrid[i, j] = new GridTile(GridTileType.open);
			}	
		}
		
		// Create Starter Rover
		Rover newRover = new Rover();
	}
	
	void CalculateMoves () {
		gameObject.GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
