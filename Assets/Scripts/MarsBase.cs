using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MarsBase {
    public enum Direction { north, east, west, south }
    public enum ResourceType { rawIron, refinedIron, doubleRawIron, doubleRefinedIron, none }
    public enum BaseNumber { baseOne, baseTwo, baseThree, baseFour, baseFive }

    public delegate void TramLaunchedDelegate(List<ResourceType> resources);
    public TramLaunchedDelegate tramLaunchedFunction;

    private GridTile[,] baseGrid;
    private bool m_running = false;
    private bool m_crashed = false;
    // TODO: Track Different Levels / Base numbers and do stuff with that data.
    private BaseNumber m_baseNumber;

    public const int GRID_HEIGHT = 20;
    public const int GRID_WIDTH = 23;
    public Rover selectedRover;

    public GridTile[,] board { get { return baseGrid; } }
    public bool running { get { return m_running; } }
    public bool crashed { get { return m_crashed; } }
    public BaseNumber baseNumber { get { return m_baseNumber; } }

    // Classes for board simulation
    public class GridTile {
        // Public Variables
        public enum TileType { rover, building, wall, open }
        public bool buildingSprite = false;

        // Private variables
        private TileType m_tileType;
        private Rover m_rover;
        private Building m_building;
        

        // Public Accessors
        public TileType tileType {
            get { return m_tileType; }
            set {
                if (m_tileType == TileType.rover && value != TileType.rover)
                    m_rover = null;
                else if (m_tileType == TileType.building && value != TileType.building)
                    m_building = null;
                m_tileType = value;
            }
        }

        public Rover rover {
            get { return m_rover; }
            set {
                if (m_tileType == TileType.building)
                    building = null;
                if (m_tileType != TileType.rover && value != null)
                    m_tileType = TileType.rover;
                m_rover = value;
            }
        }

        public Building building {
            get { return m_building; }
            set {
                if (m_tileType == TileType.rover)
                    rover = null;
                if (m_tileType != TileType.building && value != null)
                    m_tileType = TileType.building;
                m_building = value;
            }
        }

        // Constructor
        public GridTile(TileType type) {
            m_tileType = type;
            rover = null;
            building = null;
        }
    }

    public abstract class Building {
        // Public Variables
        public enum BuildingType { mine, processingPlant, tramStation }
        public int buildingLevel = 1;

        // Protected Variables
        protected BuildingType bType;

        // Public Accessors
        public BuildingType buildingType {
            get { return bType; }
            protected set { bType = value; }
        }

        // Abstract Functions
        public abstract ResourceType PickUp(Direction direction);
        public virtual bool DropOff(ResourceType rType, Direction direction) { return false; }
        public abstract void Reset();
        public virtual void Update() { return; }
    }

    private class TramBuilding : Building {
        public List<ResourceType> storedResources;
        public bool docked = true;
        private TramLaunchedDelegate tramLaunchedFunction;

        public TramBuilding(TramLaunchedDelegate tramFunction) {
            storedResources = new List<ResourceType>();
            bType = BuildingType.tramStation;
            tramLaunchedFunction = tramFunction;
        }

        public override ResourceType PickUp(Direction direction) {
            return ResourceType.none;
        }

        public override bool DropOff(ResourceType rType, Direction direction) {
            if (docked) {
                if (direction == Direction.south) {
                    if (rType == ResourceType.refinedIron) {
                        storedResources.Add(rType);
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Reset() {
            storedResources.Clear();
        }

        public override void Update() {
            if (storedResources.Count >= 5) {
                //docked = false;
                tramLaunchedFunction(storedResources);
            }
        }
    }

    private class MiningBuilding : Building {
        // Constructor
        public MiningBuilding() {
            bType = BuildingType.mine;
        }

        // Overridden Virtual Function
        public override ResourceType PickUp(Direction direction) {
            return ResourceType.rawIron;
        }

        public override void Reset() {
            return;
        }
    }

    private class RefineryBuilding : Building {
        private List<int> processTimes;
        private int processedIron = 0;
        private int processTime = 3;

        public RefineryBuilding() {
            processTimes = new List<int>();
            bType = BuildingType.processingPlant;
        }

        public override ResourceType PickUp(Direction direction) {
            if (processedIron > 0) {
                if (direction == Direction.west) {
                    processedIron--;
                    return ResourceType.refinedIron;
                }
            }
            return ResourceType.none;
        }

        public override bool DropOff(ResourceType rType, Direction direction) {
            if (direction == Direction.east) {
                if (rType == ResourceType.rawIron) {
                    processTimes.Add(processTime);
                    return true;
                }
            }
            return false;
        }

        public override void Update() {
            for (int i = 0; i < processTimes.Count; i++) {
                processTimes[i]--;
                if (processTimes[i] == 0) {
                    processTimes.RemoveAt(i);
                    processedIron++;
                }
            }
        }

        public override void Reset() {
            processTimes.Clear();
            processedIron = 0;
        }
    }

    // Rover class that tracks rover board piece information.
    // Includes the action list associated with that rover.
    public class Rover {
        // Public Variables
        public enum ActionType { none, forward, turnRight, turnLeft, grab, drop }

        // Private Variables
        private int m_currentActionIndex;
        private List<ActionType> actionList;
        private Direction m_direction;
        private bool m_crashed = false;
        private Vector2 m_startPos;
        private ResourceType m_resource;

        // Public Accessors
        public int actionsSize { get { return actionList.Count; } }

        public bool crashed {
            get { return m_crashed; }
            set { m_crashed = value; }
        }

        public Vector2 startPos { get { return m_startPos; } }

        public ReadOnlyCollection<ActionType> actions { get { return actionList.AsReadOnly(); } }

        public ResourceType resource {
            get { return m_resource; }
            set { m_resource = value; }
        }

        public ActionType currentAction {
            get {
                if (actionList.Count > 0)
                    return actionList[m_currentActionIndex];
                else return ActionType.none;
            }
        }

        public Direction direction { get { return m_direction; } }

        public int currentActionIndex { get { return m_currentActionIndex; } }

        public ActionType nextAction {
            get {
                if (actionList.Count > 0) {
                    int nextAction = m_currentActionIndex + 1;
                    if (nextAction > actionList.Count)
                        nextAction = 0;
                    return actionList[nextAction];
                }
                else return ActionType.none;
            }
        }

        // Constructor
        public Rover(int x, int y) {
            m_startPos = new Vector2(x, y);
            m_direction = Direction.north;
            actionList = new List<ActionType>();
            m_currentActionIndex = 0;
            m_resource = ResourceType.none;
        }

        public void TurnRight() {
            switch (m_direction) {
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

        public void TurnLeft() {
            switch (m_direction) {
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

        public void AdvanceAction() {
            m_currentActionIndex++;
            if (m_currentActionIndex >= actionList.Count)
                m_currentActionIndex = 0;
        }

        public void ClearActions() {
            actionList.Clear();
            m_currentActionIndex = 0;
        }

        public void ResetRover() {
            m_currentActionIndex = 0;
            m_crashed = false;
            m_direction = Direction.north;
            m_resource = ResourceType.none;
        }

        public void AddAction(ActionType newAction) {
            actionList.Add(newAction);
        }

        public void RemoveAction(int index) {
            actionList.RemoveAt(index);
        }
    }

    public MarsBase(MarsBase.BaseNumber bNumber, TramLaunchedDelegate tramFunction) {
        // Handoff constructor inputs
        m_baseNumber = bNumber;
        tramLaunchedFunction = tramFunction;

        // Initialize Grid Structure
        baseGrid = new GridTile[GRID_WIDTH, GRID_HEIGHT];
        for (int i = 0; i < GRID_WIDTH; i++) {
            for (int j = 0; j < GRID_HEIGHT; j++) {
                baseGrid[i, j] = new GridTile(GridTile.TileType.open);
            }
        }

        // Load the level
        LoadLevel();
    }

    void LoadLevel() {
        switch (m_baseNumber) {
            case BaseNumber.baseOne:
                LevelOne();
                break;
            case BaseNumber.baseTwo:
                LevelTwo();
                break;
            case BaseNumber.baseThree:
                LevelThree();
                break;
            case BaseNumber.baseFour:
                LevelFour();
                break;
            case BaseNumber.baseFive:
                LevelFive();
                break;
        }
    }

    void LevelOne() {
        // Add Tram Station
        PlaceTram(GRID_WIDTH - 3, 1);

        // Add Drill
        PlaceDrill(1, 9);

        // Add Refinery
        PlaceRefinery(10, 13);

        // Add Walls
        baseGrid[0, 0].tileType = GridTile.TileType.wall;
        baseGrid[1, 0].tileType = GridTile.TileType.wall;
        baseGrid[2, 0].tileType = GridTile.TileType.wall;
        baseGrid[3, 0].tileType = GridTile.TileType.wall;
        baseGrid[4, 0].tileType = GridTile.TileType.wall;
        baseGrid[5, 0].tileType = GridTile.TileType.wall;
        baseGrid[0, 1].tileType = GridTile.TileType.wall;
        baseGrid[1, 1].tileType = GridTile.TileType.wall;
        baseGrid[2, 1].tileType = GridTile.TileType.wall;
        baseGrid[3, 1].tileType = GridTile.TileType.wall;
        baseGrid[4, 1].tileType = GridTile.TileType.wall;
        baseGrid[0, 2].tileType = GridTile.TileType.wall;
        baseGrid[1, 2].tileType = GridTile.TileType.wall;
        baseGrid[2, 2].tileType = GridTile.TileType.wall;
        baseGrid[3, 2].tileType = GridTile.TileType.wall;
        baseGrid[4, 2].tileType = GridTile.TileType.wall;
        baseGrid[0, 3].tileType = GridTile.TileType.wall;
        baseGrid[1, 3].tileType = GridTile.TileType.wall;
        baseGrid[2, 3].tileType = GridTile.TileType.wall;
        baseGrid[3, 3].tileType = GridTile.TileType.wall;
        baseGrid[1, 4].tileType = GridTile.TileType.wall;
        baseGrid[0, 12].tileType = GridTile.TileType.wall;
        baseGrid[0, 13].tileType = GridTile.TileType.wall;
        baseGrid[0, 14].tileType = GridTile.TileType.wall;
        baseGrid[1, 14].tileType = GridTile.TileType.wall;
        baseGrid[2, 14].tileType = GridTile.TileType.wall;
        baseGrid[2, 15].tileType = GridTile.TileType.wall;
        baseGrid[3, 15].tileType = GridTile.TileType.wall;
        baseGrid[4, 15].tileType = GridTile.TileType.wall;
        baseGrid[5, 15].tileType = GridTile.TileType.wall;
        baseGrid[6, 15].tileType = GridTile.TileType.wall;
        baseGrid[7, 15].tileType = GridTile.TileType.wall;
        baseGrid[17, 18].tileType = GridTile.TileType.wall;
        baseGrid[18, 18].tileType = GridTile.TileType.wall;
        baseGrid[19, 18].tileType = GridTile.TileType.wall;
        baseGrid[20, 18].tileType = GridTile.TileType.wall;
        baseGrid[21, 18].tileType = GridTile.TileType.wall;
        baseGrid[22, 18].tileType = GridTile.TileType.wall;
        baseGrid[16, 19].tileType = GridTile.TileType.wall;
        baseGrid[17, 19].tileType = GridTile.TileType.wall;
        baseGrid[18, 19].tileType = GridTile.TileType.wall;
        baseGrid[19, 19].tileType = GridTile.TileType.wall;
        baseGrid[20, 19].tileType = GridTile.TileType.wall;
        baseGrid[21, 19].tileType = GridTile.TileType.wall;
        baseGrid[22, 19].tileType = GridTile.TileType.wall;
    }

    void LevelTwo() {

    }

    void LevelThree() {

    }

    void LevelFour() {

    }

    void LevelFive() {

    }

    void PlaceTram(int x, int y) {
        baseGrid[x, y].tileType = GridTile.TileType.wall;
        baseGrid[x + 1, y].building = new TramBuilding(tramLaunchedFunction);
        baseGrid[x + 1, y].buildingSprite = true;
        baseGrid[x + 2, y].tileType = GridTile.TileType.wall;
        baseGrid[x, y - 1].tileType = GridTile.TileType.wall;
        baseGrid[x + 1, y - 1].tileType = GridTile.TileType.wall;
        baseGrid[x + 2, y - 1].tileType = GridTile.TileType.wall;
    }

    void PlaceDrill(int x, int y) {
        //baseGrid[x, y].tileType = GridTile.TileType.wall;
        //baseGrid[x + 1, y].tileType = GridTile.TileType.wall;
        //baseGrid[x + 2, y].tileType = GridTile.TileType.wall;
        //baseGrid[x + 3, y].tileType = GridTile.TileType.wall;
        //baseGrid[x + 4, y].tileType = GridTile.TileType.wall;
        //baseGrid[x + 5, y].tileType = GridTile.TileType.wall;

        //baseGrid[x, y - 1].tileType = GridTile.TileType.wall;
        //baseGrid[x + 1, y - 1].tileType = GridTile.TileType.wall;
        baseGrid[x + 2, y - 1].tileType = GridTile.TileType.wall;
        baseGrid[x + 3, y - 1].tileType = GridTile.TileType.wall;
        baseGrid[x + 4, y - 1].tileType = GridTile.TileType.wall;
        //baseGrid[x + 5, y - 1].tileType = GridTile.TileType.wall;

        //baseGrid[x, y - 2].tileType = GridTile.TileType.wall;
        //baseGrid[x + 1, y - 2].tileType = GridTile.TileType.wall;
        baseGrid[x + 2, y - 2].tileType = GridTile.TileType.wall;
        baseGrid[x + 3, y - 2].tileType = GridTile.TileType.wall;
        baseGrid[x + 4, y - 2].building = new MiningBuilding();
        baseGrid[x + 4, y - 2].buildingSprite = true;
        //baseGrid[x + 5, y - 2].tileType = GridTile.TileType.wall;

        //baseGrid[x, y - 3].tileType = GridTile.TileType.wall;
        //baseGrid[x + 1, y - 3].tileType = GridTile.TileType.wall;
        baseGrid[x + 2, y - 3].tileType = GridTile.TileType.wall;
        baseGrid[x + 3, y - 3].tileType = GridTile.TileType.wall;
        baseGrid[x + 4, y - 3].tileType = GridTile.TileType.wall;
        //baseGrid[x + 5, y - 3].building = new MiningBuilding();
        //baseGrid[x + 5, y - 3].buildingSprite = true;
    }

    void PlaceRefinery(int x, int y) {
        baseGrid[x, y].tileType = GridTile.TileType.wall;
        baseGrid[x + 1, y].tileType = GridTile.TileType.wall;
        baseGrid[x + 2, y].tileType = GridTile.TileType.wall;
        baseGrid[x + 3, y].tileType = GridTile.TileType.wall;

        baseGrid[x, y - 1].building = new RefineryBuilding();
        baseGrid[x + 1, y- 1].tileType = GridTile.TileType.wall;
        baseGrid[x + 2, y - 1].tileType = GridTile.TileType.wall;
        baseGrid[x + 3, y - 1].building = new RefineryBuilding();
        baseGrid[x + 3, y - 1].buildingSprite = true;

        baseGrid[x, y - 2].tileType = GridTile.TileType.wall;
        baseGrid[x + 1, y - 2].tileType = GridTile.TileType.wall;
        baseGrid[x + 2, y - 2].tileType = GridTile.TileType.wall;
        baseGrid[x + 3, y - 2].tileType = GridTile.TileType.wall;
    }

    public void StartSim() {
        m_running = true;
    }

    public void StopSim() {
        m_running = false;
    }

    public void ResetBoard() {
        m_crashed = false;

        // Make a Duplicate
        GridTile[,] newBaseGrid = new GridTile[GRID_WIDTH, GRID_HEIGHT];
        for (int i = 0; i < GRID_WIDTH; i++) {
            for (int j = 0; j < GRID_HEIGHT; j++) {
                newBaseGrid[i, j] = baseGrid[i, j];
            }
        }

        // Reset All Tiles
        for (int i = 0; i < GRID_WIDTH; i++) {
            for (int j = 0; j < GRID_HEIGHT; j++) {
                if (baseGrid[i, j].tileType == GridTile.TileType.rover) {
                    Rover rover = baseGrid[i, j].rover;
                    rover.ResetRover();
                    Vector2 startPos = rover.startPos;
                    int x = (int)startPos.x;
                    int y = (int)startPos.y;
                    newBaseGrid[x, y] = baseGrid[i, j];
                    if(x != i || y != j)
                        newBaseGrid[i, j] = new GridTile(GridTile.TileType.open);
                }
                else if (baseGrid[i, j].tileType == GridTile.TileType.building) {
                    baseGrid[i, j].building.Reset();
                }
            }
        }

        baseGrid = newBaseGrid;
    }

    void MoveTile(GridTile[,] tileGrid, int i, int j, Direction direction) {
        bool valid = false;
        switch (direction) {
            case Direction.north:
                if (j + 1 < GRID_HEIGHT) {
                    if (tileGrid[i, j + 1].tileType == GridTile.TileType.open) {
                        tileGrid[i, j + 1] = tileGrid[i, j];
                        valid = true;
                    }
                }
                break;
            case Direction.east:
                if (i + 1 < GRID_WIDTH) {
                    if (tileGrid[i + 1, j].tileType == GridTile.TileType.open) {
                        tileGrid[i + 1, j] = tileGrid[i, j];
                        valid = true;
                    }
                }
                break;
            case Direction.west:
                if (i - 1 >= 0) {
                    if (tileGrid[i - 1, j].tileType == GridTile.TileType.open) {
                        tileGrid[i - 1, j] = tileGrid[i, j];
                        valid = true;
                    }
                }
                break;
            case Direction.south:
                if (j - 1 >= 0) {
                    if (tileGrid[i, j - 1].tileType == GridTile.TileType.open) {
                        tileGrid[i, j - 1] = tileGrid[i, j];
                        valid = true;
                    }
                }
                break;
        }
        if (!valid) {
            tileGrid[i, j].rover.crashed = true;
            m_crashed = true;
            StopSim();
        }
        else tileGrid[i, j] = new GridTile(GridTile.TileType.open);
    }

    void GrabTile(int i, int j) {
        Rover rover = baseGrid[i, j].rover;
        Direction direction = rover.direction;
        switch (direction) {
            case Direction.north:
                break;
            case Direction.east:
                if (baseGrid[i + 1, j].tileType == GridTile.TileType.building) {
                    ResourceType resource = baseGrid[i + 1, j].building.PickUp(direction);
                    rover.resource = resource;
                }
                break;
            case Direction.west:
                if (baseGrid[i - 1, j].tileType == GridTile.TileType.building) {
                    ResourceType resource = baseGrid[i - 1, j].building.PickUp(direction);
                    rover.resource = resource;
                }
                break;
            case Direction.south:
                break;
        }
    }

    void DropTile(int i, int j) {
        Rover rover = baseGrid[i, j].rover;
        Direction direction = rover.direction;
        switch (direction) {
            case Direction.north:
                if (baseGrid[i, j + 1].tileType == GridTile.TileType.building) {
                    if (baseGrid[i, j + 1].building.DropOff(rover.resource, direction)) {
                        rover.resource = ResourceType.none;
                    }
                }
                else if (baseGrid[i, j + 1].tileType == GridTile.TileType.rover) {
                    Rover otherRover = baseGrid[i, j + 1].rover;
                    if (otherRover.resource == ResourceType.none) {
                        otherRover.resource = rover.resource;
                        rover.resource = ResourceType.none;
                    }
                }
                break;
            case Direction.east:
                if (baseGrid[i + 1, j].tileType == GridTile.TileType.building) {
                    if (baseGrid[i + 1, j].building.DropOff(rover.resource, direction)) {
                        rover.resource = ResourceType.none;
                    }
                }
                else if (baseGrid[i + 1, j].tileType == GridTile.TileType.rover) {
                    Rover otherRover = baseGrid[i + 1, j].rover;
                    if (otherRover.resource == ResourceType.none) {
                        otherRover.resource = rover.resource;
                        rover.resource = ResourceType.none;
                    }
                }
                break;
            case Direction.west:
                if (baseGrid[i - 1, j].tileType == GridTile.TileType.building) {
                    if (baseGrid[i - i, j].building.DropOff(rover.resource, direction)) {
                        rover.resource = ResourceType.none;
                    }
                }
                else if (baseGrid[i - 1, j].tileType == GridTile.TileType.rover) {
                    Rover otherRover = baseGrid[i - 1, j].rover;
                    if (otherRover.resource == ResourceType.none) {
                        otherRover.resource = rover.resource;
                        rover.resource = ResourceType.none;
                    }
                }
                break;
            case Direction.south:
                if (baseGrid[i, j - 1].tileType == GridTile.TileType.building) {
                    if (baseGrid[i, j - 1].building.DropOff(rover.resource, direction)) {
                        rover.resource = ResourceType.none;
                    }
                }
                else if (baseGrid[i, j - 1].tileType == GridTile.TileType.rover) {
                    Rover otherRover = baseGrid[i, j - 1].rover;
                    if (otherRover.resource == ResourceType.none) {
                        otherRover.resource = rover.resource;
                        rover.resource = ResourceType.none;
                    }
                }
                break;
        }
    }

    public void CalculateMoves() {
        // Generate a Copy of baseGrid
        GridTile[,] newBaseGrid = new GridTile[GRID_WIDTH, GRID_HEIGHT];
        for (int i = 0; i < GRID_WIDTH; i++) {
            for (int j = 0; j < GRID_HEIGHT; j++) {
                newBaseGrid[i, j] = baseGrid[i, j];
            }
        }

        // Iterate through all the tiles and update the board.
        for (int i = 0; i < GRID_WIDTH; i++) {
            for (int j = 0; j < GRID_HEIGHT; j++) {
                if (baseGrid[i, j].tileType == GridTile.TileType.rover) {
                    Rover rover = baseGrid[i, j].rover;
                    Rover.ActionType action = rover.currentAction;
                    switch (action) {
                        case Rover.ActionType.forward:
                            MoveTile(newBaseGrid, i, j, rover.direction);
                            break;
                        case Rover.ActionType.turnRight:
                            rover.TurnRight();
                            break;
                        case Rover.ActionType.turnLeft:
                            rover.TurnLeft();
                            break;
                        case Rover.ActionType.grab:
                            if (rover.resource == ResourceType.none) GrabTile(i, j);
                            break;
                        case Rover.ActionType.drop:
                            if (rover.resource != ResourceType.none) DropTile(i, j);
                            break;
                    }
                    if (m_running) rover.AdvanceAction();
                }
                else if (baseGrid[i, j].tileType == GridTile.TileType.building) {
                    baseGrid[i, j].building.Update();
                }
            }
        }

        baseGrid = newBaseGrid;
    }

    public void BuyPart(GUISystem.ButtonType buttonType, int x, int y) {
        switch (buttonType) {
            case GUISystem.ButtonType.rover:
                baseGrid[x, y].rover = new Rover(x, y);
                break;
        }
    }
}
