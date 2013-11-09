using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseScript : MonoBehaviour {
	
	public Texture front;
	public Texture back;
	public Texture left;
	public Texture right;
	
	public enum direction { north, east, west, south }
	private enum actionType { forward, turnRight, turnLeft }
	private enum GridTileType { rover, wall, drill, processIn, processOut, tram, open }
	private GridTile[,] baseGrid;
	private int GRID_HEIGHT = 8;
	private int GRID_WIDTH = 10;
	private bool running = false;
	private Rover selectedRover;
	private actionType dragAction;
	private bool dragging = false;
	
	private class GridTile {
		private GridTileType tileType;
		private Rover rover;
		
		public GridTile	(GridTileType tileType)	{
			this.tileType = tileType;
		}
		
		public void SetTileType (GridTileType tileType) {
			this.tileType = tileType;	
		}
		
		public GridTileType GetTileType () {
			return tileType;
		}
		
		public Rover GetRover () {
			return rover;	
		}
		
		public void SetRover (Rover rover) {
			this.rover = rover;	
		}
	}
	
	private class Rover {
		
		private List<actionType> actions;
		private int currentAction;
		private direction forward;
		
		public Rover(){
			forward = direction.north;
			actions = new List<actionType>();
			currentAction = 0;
		}
		
		public List<actionType> GetActions(){
			return actions;	
		}
		
		public direction GetDirection(){
			return forward;
		}
		
		public void TurnRight(){
			switch(forward){
			case direction.north:
				forward = direction.east;
				break;
			case direction.east:
				forward = direction.south;
				break;
			case direction.west:
				forward = direction.north;
				break;
			case direction.south:
				forward = direction.west;
				break;
			}
		}
		
		public void TurnLeft(){
			switch(forward){
			case direction.north:
				forward = direction.west;
				break;
			case direction.east:
				forward = direction.north;
				break;
			case direction.west:
				forward = direction.south;
				break;
			case direction.south:
				forward = direction.east;
				break;
			}
		}
		
		public actionType GetCurrentAction(){
			return actions[currentAction];
		}
		
		public actionType GetNextAction(){
			int nextAction = currentAction + 1;
			if(nextAction > actions.Count)
				nextAction = 0;
			return actions[nextAction];
		}
		
		public void AdvanceAction(){
			currentAction++;
			if(currentAction >= actions.Count)
				currentAction = 0;
		}
		
		public void Reset(){
			currentAction = 0;
		}
		
		public void ClearActions(){
			actions.Clear();
			currentAction = 0;
		}
		
		public void AddAction(actionType newAction){
			actions.Add(newAction);
		}
	}
	
	// Use this for initialization
	void Start () {
		// Initialize Grid Structure
		baseGrid = new GridTile[GRID_WIDTH, GRID_HEIGHT];
		for(int i = 0; i < GRID_WIDTH; i++){
			for(int j = 0; j < GRID_HEIGHT; j++){
				baseGrid[i, j] = new GridTile(GridTileType.open);
			}	
		}
		
		// Create Starter Rover
		Rover newRover = new Rover();
		baseGrid[4, 2].SetTileType(GridTileType.rover);
		baseGrid[4, 2].SetRover(newRover);
		selectedRover = newRover;
		//StartCoroutine("GridClock");
	}
	
	public void DrawGame(){
		// Iterate through the grid
		for(int j = GRID_HEIGHT - 1; j >= 0; j--){
			GUILayout.BeginHorizontal();
			for(int i = 0; i < GRID_WIDTH; i++){
				switch(baseGrid[i, j].GetTileType()){	
				case GridTileType.open:
					GUILayout.Box(GUIContent.none, GUILayout.Width(64), GUILayout.Height(64));
					break;
				case GridTileType.rover:
					GUILayout.FlexibleSpace();
					Rover rover = baseGrid[i, j].GetRover();
					direction facing = rover.GetDirection();
					switch(facing){
					case direction.north:
						GUI.DrawTexture(new Rect(68 * i, 550 - (70 * (j + 1)), 64, 64), back);
						break;
					case direction.east:
						GUI.DrawTexture(new Rect(68 * i, 550 - (70 * (j + 1)), 64, 64), right);
						break;
					case direction.south:
						GUI.DrawTexture(new Rect(68 * i, 550 - (70 * (j + 1)), 64, 64), front);
						break;
					case direction.west:
						GUI.DrawTexture(new Rect(68 * i, 550 - (70 * (j + 1)), 64, 64), left);
						break;
					}
					break;
				}
			}
			GUILayout.EndHorizontal();
		}
	}
	
	void MoveTile(GridTile[,] tileGrid, int i, int j, direction forwards){
		switch(forwards){
		case direction.north:
			tileGrid[i, j + 1] = tileGrid[i, j];
			break;
		case direction.east:
			tileGrid[i + 1, j] = tileGrid[i, j];
			break;
		case direction.west:
			tileGrid[i - 1, j] = tileGrid[i, j];
			break;
		case direction.south:
			tileGrid[i, j - 1] = tileGrid[i, j];
			break;
		}
		tileGrid[i, j] = new GridTile(GridTileType.open);
	}
	
	void CalculateMoves () {
		GridTile[,] newBaseGrid = new GridTile[GRID_WIDTH, GRID_HEIGHT];
		for(int i = 0; i < GRID_WIDTH; i++){
			for(int j = 0; j < GRID_HEIGHT; j++){
				newBaseGrid[i, j] = baseGrid[i, j];
			}	
		}
		
		for(int i = 0; i < GRID_WIDTH; i++){
			for(int j = 0; j < GRID_HEIGHT; j++){
				if(baseGrid[i, j].GetTileType() == GridTileType.rover){
					Rover rover = baseGrid[i, j].GetRover();
					actionType action = rover.GetCurrentAction();
					rover.AdvanceAction();
					direction facing = rover.GetDirection();
					switch(action){
					case actionType.forward:
						MoveTile(newBaseGrid, i, j, facing);
						break;
					case actionType.turnRight:
						rover.TurnRight();
						break;
					case actionType.turnLeft:
						rover.TurnLeft();
						break;
					}
				}
			}
		}
		
		baseGrid = newBaseGrid;
	}
	
	void ResetGame(){
		List<actionType> actions = selectedRover.GetActions();
		
		for(int i = 0; i < GRID_WIDTH; i++){
			for(int j = 0; j < GRID_HEIGHT; j++){
				baseGrid[i, j] = new GridTile(GridTileType.open);
			}	
		}
		
		Rover newRover = new Rover();
		baseGrid[4, 2].SetTileType(GridTileType.rover);
		baseGrid[4, 2].SetRover(newRover);
		foreach(actionType action in actions)
			newRover.AddAction(action);
		selectedRover = newRover;
	}
	
	IEnumerator GridClock(){
		yield return new WaitForSeconds(1f);
		while(running){
			CalculateMoves();
			yield return new WaitForSeconds(1f);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
