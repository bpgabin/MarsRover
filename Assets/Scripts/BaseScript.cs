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
	
	void DrawGame(){
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
	
	void OnGUI(){
		GUI.depth = 2;
		
		Rect forwardRect;
		Rect rightRect;
		Rect leftRect;
		
		GUILayout.BeginArea(new Rect(10, 45, 676, 550));
			DrawGame();
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect(Screen.width - 210, Screen.height - 580, 150, 80));
		GUILayout.Box("Forward");
		forwardRect = GUILayoutUtility.GetLastRect();
		forwardRect = new Rect(Screen.width - 210 + forwardRect.x, Screen.height - 580 + forwardRect.y, forwardRect.width, forwardRect.height);
		GUILayout.Box("Turn Right");
		rightRect = GUILayoutUtility.GetLastRect();
		rightRect = new Rect(Screen.width - 210 + rightRect.x, Screen.height - 580 + rightRect.y, rightRect.width, rightRect.height);
		GUILayout.Box("Turn Left");
		leftRect = GUILayoutUtility.GetLastRect();
		leftRect = new Rect(Screen.width - 210 + leftRect.x, Screen.height - 580 + leftRect.y, leftRect.width, leftRect.height);
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 480, 250, 380));
		foreach(actionType action in selectedRover.GetActions()){
			switch(action){
			case actionType.forward:
				GUILayout.Box("Forward");
				break;
			case actionType.turnLeft:
				GUILayout.Box("Turn Left");
				break;
			case actionType.turnRight:
				GUILayout.Box("Turn Right");
				break;
			}
		}
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 30, 250, 380));
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if(running){
					if(GUILayout.Button("Stop", GUILayout.Width(50))){
						running = false;
						ResetGame();
					}
				}
				else{
					if(GUILayout.Button("Start", GUILayout.Width(50))){
						if(selectedRover.GetActions().Count != 0){
							running = true;
							StartCoroutine("GridClock");
						}
					}
				}
				if(GUILayout.Button("Clear", GUILayout.Width(50))){
					selectedRover.ClearActions();
				}
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		GUILayout.EndArea();
		
		if(Event.current.button == 0){
			if(Event.current.type == EventType.MouseDown){
				if(forwardRect.Contains(Event.current.mousePosition)){
					dragging = true;
					dragAction = actionType.forward;
				}
				else if(rightRect.Contains(Event.current.mousePosition)){
					dragging = true;
					dragAction = actionType.turnRight;
				}
				else if(leftRect.Contains(Event.current.mousePosition)){
					dragging = true;
					dragAction = actionType.turnLeft;
				}
			}
			else if(Event.current.type == EventType.MouseUp){
				if(dragging){
					dragging = false;
					Rect dragArea = new Rect(Screen.width - 260, Screen.height - 480, 250, 380);
					//float xMin = Screen.width - 260;
					//float yMin = Screen.height - 90;
					//float xMax = Screen.width - 260 + 250;
					//float yMax = Screen.height - 90 + 100;
					//if((xMin < Event.current.mousePosition.x) && (Event.current.mousePosition.x < xMax) && (yMin < Event.current.mousePosition.y) && (Event.current.mousePosition.y < yMax)){
					if(dragArea.Contains(Event.current.mousePosition)){
						selectedRover.AddAction(dragAction);	
					}
				}
			}
		}
		
		if(dragging){
			switch(dragAction){
			case actionType.forward:
				GUI.Button(new Rect(Event.current.mousePosition.x - forwardRect.width / 2, Event.current.mousePosition.y - forwardRect.height / 2, forwardRect.width, forwardRect.height), "Forward");
				break;
			case actionType.turnLeft:
				GUI.Button(new Rect(Event.current.mousePosition.x - forwardRect.width / 2, Event.current.mousePosition.y - forwardRect.height / 2, forwardRect.width, forwardRect.height), "Turn Left");
				break;
			case actionType.turnRight:
				GUI.Button(new Rect(Event.current.mousePosition.x - forwardRect.width / 2, Event.current.mousePosition.y - forwardRect.height / 2, forwardRect.width, forwardRect.height), "Turn Right");
				break;
			}
		}
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
