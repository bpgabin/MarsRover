using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class MarsBase {
    public enum Direction { north, east, west, south }

    private GridTile[,] baseGrid;
    private bool m_running = false;
    private bool m_crashed = false;

    public const int GRID_HEIGHT = 8;
    public const int GRID_WIDTH = 10;
    public Rover selectedRover;

    public GridTile[,] board { get { return baseGrid; } }
    public bool running { get { return m_running; } }
    public bool crashed { get { return m_crashed; } }

    // Classes for board simulation
    public class GridTile {
        // Public Variables
        public enum TileType { rover, building, wall, open }

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

        // Protected Variables
        protected BuildingType bType;

        // Public Accessors
        public BuildingType buildingType {
            get { return bType; }
            protected set { bType = value; }
        }

        // Virtual Functions
        public virtual bool PickUp() {
            return false;
        }
    }

    private class MiningBuilding : Building {
        // New Private Variables
        private float lastPickup;

        // Constructor
        public MiningBuilding() {
            lastPickup = Time.time;
            bType = BuildingType.mine;
        }

        // Overridden Virtual Function
        public override bool PickUp() {
            if (Time.time - lastPickup > 3.0f) {
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
        public enum ActionType { none, forward, turnRight, turnLeft, grab, drop }

        // Private Variables
        private int m_currentActionIndex;
        private List<ActionType> actionList;
        private Direction m_direction;
        private bool m_crashed = false;
        private Vector2 m_startPos;

        // Public Accessors
        public int actionsSize { get { return actionList.Count; } }

        public bool crashed {
            get { return m_crashed; }
            set { m_crashed = value; }
        }

        public Vector2 startPos { get { return m_startPos; } }

        public ReadOnlyCollection<ActionType> actions { get { return actionList.AsReadOnly(); } }

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

        public void Reset() {
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
        }

        public void AddAction(ActionType newAction) {
            actionList.Add(newAction);
        }

        public void RemoveAction(int index) {
            actionList.RemoveAt(index);
        }
    }

    public MarsBase() {
        // Initialize Grid Structure
        baseGrid = new GridTile[GRID_WIDTH, GRID_HEIGHT];
        for (int i = 0; i < GRID_WIDTH; i++) {
            for (int j = 0; j < GRID_HEIGHT; j++) {
                baseGrid[i, j] = new GridTile(GridTile.TileType.open);
            }
        }
    }

    void PlaceBuilding(Building building, int x, int y) {
        baseGrid[x, y].building = building;
        baseGrid[x + 1, y].tileType = GridTile.TileType.wall;
        baseGrid[x, y - 1].tileType = GridTile.TileType.wall;
        baseGrid[x + 1, y - 1].tileType = GridTile.TileType.wall;
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

        // TODO: Reset Buildings
        // Reset All Rovers
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

                            break;
                        case Rover.ActionType.drop:

                            break;
                    }
                    if (m_running) rover.AdvanceAction();
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
            case GUISystem.ButtonType.drill:
                PlaceBuilding(new MiningBuilding(), x, y);
                break;
            case GUISystem.ButtonType.refinery:

                break;
        }
    }
}
