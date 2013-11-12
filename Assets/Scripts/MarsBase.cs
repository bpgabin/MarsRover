using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MarsBase {
	
	public enum BaseType { mining }
	public enum Direction { north, east, west, south }
	
	private BaseType bType;
	private GridTile[,] baseGrid;
	
	public const int GRID_HEIGHT = 8;
	public const int GRID_WIDTH = 10;
	
	private bool running = false;
	private Rover selectedRover;
	
	public GridTile[,] board { get { return baseGrid; } }
	
	// Classes for board simulation
	public class GridTile {
		// Public Variables
		public enum TileType { rover, building, wall, open }
		
		// Private variables
		private TileType m_tileType;
		private Rover m_rover;
		private Building m_building;
		
		// Public Accessors
		public TileType tileType{
			get { return m_tileType; }
			set {
				if(m_tileType == TileType.rover && value != TileType.rover)
					m_rover = null;
				else if(m_tileType == TileType.building && value != TileType.building)
					m_building = null;
				m_tileType = value;
			}
		}
		
		public Rover rover{
			get { return m_rover; }
			set {
				if(m_tileType == TileType.building)
					building = null;
				if(m_tileType != TileType.rover && value != null)
					m_tileType = TileType.rover;
				m_rover = value;
			}
		}
		
		public Building building{
			get { return m_building; }
			set {
				if(m_tileType == TileType.rover)
					rover = null;
				if(m_tileType != TileType.building && value != null)
					m_tileType = TileType.building;
				m_building = value;
			}
		}
		
		// Constructor
		public GridTile(TileType type){
			m_tileType = type;
			rover = null;
			building = null;
		}
	}
	
	public abstract class Building {
		// Public Variables
		public enum BuildingType { mine, processingPlant, tramStation }
		
		// Protected Variables
		protected BuildingType bType;
		
		// Public Accessors
		public BuildingType buildingType{
			get { return bType; }
			protected set { bType = value; }
		}
		
		// Virtual Functions
		public virtual bool PickUp(){
			return false;
		}
	}
	
	private class MiningBuilding : Building {
		// New Private Variables
		private float lastPickup;
		
		// Constructor
		public MiningBuilding(){
			lastPickup = Time.time;
			bType = BuildingType.mine;
		}
		
		// Overridden Virtual Function
		public override bool PickUp(){
			if(Time.time - lastPickup > 3.0f){
				lastPickup = Time.time;
				return true;
			}
			else return false;
		}
	}
	
	// Rover class that tracks rover board piece information.
	// Includes the action list associated with that rover.
	public class Rover {
		// Public Variables
		public enum ActionType { none, forward, turnRight, turnLeft }
		
		// Private Variables
		private int currentActionIndex;
		private List<ActionType> actionList;
		private Direction m_direction;
		
		// Public Accessors
		public int actionsSize{
			get { return actionList.Count; }
		}
		
		public ReadOnlyCollection<ActionType> actions{
			get { return actionList.AsReadOnly(); }
		}
		
		public ActionType currentAction{
			get { 
				if(actionList.Count > 0)
					return actionList[currentActionIndex];
				else return ActionType.none;
			}
		}
		
		public Direction direction{
			get { return m_direction; }
		}
		
		public ActionType nextAction{
			get {
				if(actionList.Count > 0){
					int nextAction = currentActionIndex + 1;
					if(nextAction > actionList.Count)
						nextAction = 0;
					return actionList[nextAction];
				}
				else return ActionType.none;
			}
		}
		
		// Constructor
		public Rover(){
			m_direction = Direction.north;
			actionList = new List<ActionType>();
			currentActionIndex = 0;
		}
		
		public void TurnRight(){
			switch(m_direction){
			case Direction.north:
				m_direction = Direction.east;
				break;
			case Direction.east:
				m_direction = Direction.south;
				break;
			case Direction.west:
				m_direction = Direction.north;
				break;
			case Direction.south:
				m_direction = Direction.west;
				break;
			}
		}
		
		public void TurnLeft(){
			switch(m_direction){
			case Direction.north:
				m_direction = Direction.west;
				break;
			case Direction.east:
				m_direction = Direction.north;
				break;
			case Direction.west:
				m_direction = Direction.south;
				break;
			case Direction.south:
				m_direction = Direction.east;
				break;
			}
		}
		
		public void AdvanceAction(){
			currentActionIndex++;
			if(currentActionIndex >= actionList.Count)
				currentActionIndex = 0;
		}
		
		public void Reset(){
			currentActionIndex = 0;
		}
		
		public void ClearActions(){
			actionList.Clear();
			currentActionIndex = 0;
		}
		
		public void AddAction(ActionType newAction){
			actionList.Add(newAction);
		}
	}
	
	public MarsBase(BaseType baseType){
		// Set Base Type
		bType = baseType;
	
		// Initialize Grid Structure
		baseGrid = new GridTile[GRID_WIDTH, GRID_HEIGHT];
		for(int i = 0; i < GRID_WIDTH; i++){
			for(int j = 0; j < GRID_HEIGHT; j++){
				baseGrid[i, j] = new GridTile(GridTile.TileType.open);
			}	
		}
		
		// TODO: Remove Starter Assets and Add UI Adding
		// Create Starter Rover
		Rover newRover = new Rover();
		baseGrid[4, 2].rover = newRover;
		selectedRover = newRover;
		
		// Create Starter Building
		Building newBuilding = new MiningBuilding();
		PlaceBuilding(newBuilding, 5, 5);
	}
	
	void PlaceBuilding(Building building, int x, int y){
		baseGrid[x, y].building = building;
		baseGrid[x + 1, y].tileType = GridTile.TileType.wall;
		baseGrid[x, y - 1].tileType = GridTile.TileType.wall;
		baseGrid[x + 1, y - 1].tileType = GridTile.TileType.wall;
	}
	
	void MoveTile(GridTile[,] tileGrid, int i, int j, Direction direction){
		switch(direction){
		case Direction.north:
			tileGrid[i, j + 1] = tileGrid[i, j];
			break;
		case Direction.east:
			tileGrid[i + 1, j] = tileGrid[i, j];
			break;
		case Direction.west:
			tileGrid[i - 1, j] = tileGrid[i, j];
			break;
		case Direction.south:
			tileGrid[i, j - 1] = tileGrid[i, j];
			break;
		}
		tileGrid[i, j] = new GridTile(GridTile.TileType.open);
	}
	
	void CalculateMoves () {
		// Generate a Copy of baseGrid
		GridTile[,] newBaseGrid = new GridTile[GRID_WIDTH, GRID_HEIGHT];
		for(int i = 0; i < GRID_WIDTH; i++){
			for(int j = 0; j < GRID_HEIGHT; j++){
				newBaseGrid[i, j] = baseGrid[i, j];
			}	
		}
		
		// Iterate through all the tiles and update the board.
		for(int i = 0; i < GRID_WIDTH; i++){
			for(int j = 0; j < GRID_HEIGHT; j++){
				if(baseGrid[i, j].tileType == GridTile.TileType.rover){
					Rover rover = baseGrid[i, j].rover;
					Rover.ActionType action = rover.currentAction;
					rover.AdvanceAction();
					switch(action){
					case Rover.ActionType.forward:
						MoveTile(newBaseGrid, i, j, rover.direction);
						break;
					case Rover.ActionType.turnRight:
						rover.TurnRight();
						break;
					case Rover.ActionType.turnLeft:
						rover.TurnLeft();
						break;
					}
				}
			}
		}
		
		baseGrid = newBaseGrid;
	}
	
	void ResetGame(){
		ReadOnlyCollection<Rover.ActionType> actions = selectedRover.actions;
		
		for(int i = 0; i < GRID_WIDTH; i++){
			for(int j = 0; j < GRID_HEIGHT; j++){
				baseGrid[i, j] = new GridTile(GridTile.TileType.open);
			}	
		}
		
		Rover newRover = new Rover();
		baseGrid[4, 2].tileType = GridTile.TileType.rover;
		baseGrid[4, 2].rover = newRover;
		foreach(Rover.ActionType action in actions)
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
}
