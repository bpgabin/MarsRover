﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class GUISystem : MonoBehaviour {

	public Camera mainCamera;
    public enum ButtonType { rover, arrowUp, arrowLeft, arrowRight, grab, drop, blank }

    public GUISkin Ourskin;
    private delegate void GUIFunction();
    private GUIFunction currentGUI;
    private ColonyManager colonyManager;

    private MarsBase currentBase = null;

    private bool dragging = false;
    private Vector2 scrollPosition = Vector2.zero;
    private ButtonType draggingButton;
    private Dictionary<int, Texture> drillTextures;
    private Dictionary<int, Texture> refineryTextures;
    private Dictionary<ButtonType, Texture> buttonTextures;
    private Dictionary<ButtonType, MarsBase.Rover.ActionType> buttonToAction;
    private Dictionary<MarsBase.Rover.ActionType, ButtonType> actionToButton;

    void Start() {
        buttonTextures = new Dictionary<ButtonType, Texture>();
        buttonTextures[ButtonType.blank] = Resources.Load("Textures/blankIcon") as Texture;
        buttonTextures[ButtonType.drop] = Resources.Load("Textures/dropIcon") as Texture;
        buttonTextures[ButtonType.grab] = Resources.Load("Textures/grabIcon") as Texture;
        buttonTextures[ButtonType.arrowUp] = Resources.Load("Textures/forwardIcon") as Texture;
        buttonTextures[ButtonType.arrowLeft] = Resources.Load("Textures/rotateLeftIcon") as Texture;
        buttonTextures[ButtonType.arrowRight] = Resources.Load("Textures/rotateRightIcon") as Texture;
        buttonTextures[ButtonType.rover] = Resources.Load("Textures/rover") as Texture;

        buttonToAction = new Dictionary<ButtonType, MarsBase.Rover.ActionType>();
        buttonToAction[ButtonType.arrowUp] = MarsBase.Rover.ActionType.forward;
        buttonToAction[ButtonType.arrowLeft] = MarsBase.Rover.ActionType.turnLeft;
        buttonToAction[ButtonType.arrowRight] = MarsBase.Rover.ActionType.turnRight;
        buttonToAction[ButtonType.grab] = MarsBase.Rover.ActionType.grab;
        buttonToAction[ButtonType.drop] = MarsBase.Rover.ActionType.drop;
        buttonToAction[ButtonType.blank] = MarsBase.Rover.ActionType.none;

        actionToButton = new Dictionary<MarsBase.Rover.ActionType, ButtonType>();
        actionToButton[MarsBase.Rover.ActionType.forward] = ButtonType.arrowUp;
        actionToButton[MarsBase.Rover.ActionType.turnLeft] = ButtonType.arrowLeft;
        actionToButton[MarsBase.Rover.ActionType.turnRight] = ButtonType.arrowRight;
        actionToButton[MarsBase.Rover.ActionType.grab] = ButtonType.grab;
        actionToButton[MarsBase.Rover.ActionType.drop] = ButtonType.drop;
        actionToButton[MarsBase.Rover.ActionType.none] = ButtonType.blank;

        drillTextures = new Dictionary<int, Texture>();
        drillTextures[1] = Resources.Load("Textures/bucketwheelexcavator_small_red") as Texture;
        drillTextures[2] = Resources.Load("Textures/bucketwheelexcavator_small_green") as Texture;
        drillTextures[3] = Resources.Load("Textures/bucketwheelexcavator_small_blue") as Texture;

        refineryTextures = new Dictionary<int, Texture>();
        refineryTextures[1] = Resources.Load("Textures/refinery_small_red") as Texture;
        refineryTextures[2] = Resources.Load("Textures/refinery_small_green") as Texture;
        refineryTextures[3] = Resources.Load("Textures/refinery_small_blue") as Texture;

        currentGUI = MainMenuGUI;
    }

    void OnGUI() {
        currentGUI();
    }

	public void SwitchToGame(){
		currentGUI = ColonyGUI;
	}

	void DrawNothing(){

	}

    void MainMenuGUI() {
        GUI.skin = Ourskin;
		GUI.DrawTexture(new Rect(Screen.width / 2 - 176, 100, 352, 148), Resources.Load("Textures/redrover") as Texture);
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 50 , 100, 200));
        if (GUILayout.Button("Start")) {
            colonyManager = gameObject.AddComponent<ColonyManager>();
			currentGUI = ColonyGUI;
        }
        GUILayout.EndArea();
    }

    void ColonyGUI() {
        GUI.skin = Ourskin;

        // Draw Background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/marsbackground_02") as Texture);


        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Box("Iron: " + colonyManager.iron);
        GUILayout.Box("Money: " + colonyManager.money);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        // Temporary Build Button
        // TODO: Remove temporary build button when no longer needed.
        GUI.enabled = colonyManager.money >= colonyManager.costs[ColonyManager.ShopItems.miningBase];
        if (GUILayout.Button("Buy Mining Base")) {
            colonyManager.AddBase();
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUI.DrawTexture(new Rect(0, -120, 960, 540), Resources.Load("Textures/spaceElevator") as Texture);

        for (int i = 0; i < colonyManager.bases.Count; i++) {
            string text = "Mining";
            if (colonyManager.bases[i].running) {
                text += "\nRunning";
                GUI.color = Color.green;
            }
            else {
                text += "\nStopped";
                GUI.color = Color.red;
            }
            if (GUI.Button(new Rect(100 * (i + 1), 400, 100, 100), text)) {
                currentBase = colonyManager.bases[i];
                currentGUI = BaseGUI;
                break;
            }
            GUI.color = Color.white;
        }
    }

    void BaseGUI() {
        GUI.depth = 3;
        GUI.skin = Ourskin;
        // Track each interactive button's Rect
        Dictionary<ButtonType, Rect> rects = new Dictionary<ButtonType, Rect>();
        Rect dropRect = new Rect(0, 0, 0, 0);

        List<Rect> actionRects = new List<Rect>();

        // Draw Background
        //GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/marsbackground_01") as Texture);
        switch (currentBase.baseNumber) {
            case MarsBase.BaseNumber.baseOne:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level1") as Texture);
                break;
            case MarsBase.BaseNumber.baseTwo:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level2") as Texture);
                break;
            case MarsBase.BaseNumber.baseThree:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level3") as Texture);
                break;
            case MarsBase.BaseNumber.baseFour:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level4") as Texture);
                break;
            case MarsBase.BaseNumber.baseFive:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level5") as Texture);
                break;
        }

        /*
        // Tabs at the top showing various info
        GUI.Box(new Rect(2, 2, 240, 40), GUIContent.none);
        GUILayout.BeginArea(new Rect(20, 14, 220, 40));
        GUILayout.BeginHorizontal();
        GUILayout.Label("IRON: " + colonyManager.iron.ToString());
        GUILayout.Label("MONEY: $" + colonyManager.money.ToString());
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUI.Box(new Rect(255, 0, 120, 30), GUIContent.none);
        GUI.color = Color.Lerp(Color.red, Color.green, (colonyManager.currentSpaceElevatorTime / colonyManager.spaceElevatorTime));
        GUI.Box(new Rect(260, 10, (colonyManager.currentSpaceElevatorTime / colonyManager.spaceElevatorTime) * 100, 20), GUIContent.none, "ProgressBar");
        GUI.color = Color.white;
        GUI.Label(new Rect(260 + 50 - 60, 20, 120, 40), colonyManager.currentSpaceElevatorTime.ToString());

        GUI.color = Color.Lerp(Color.red, Color.green, (colonyManager.currentAuditTime / colonyManager.auditTime));
        GUI.Box(new Rect(370, 10, (colonyManager.currentAuditTime / colonyManager.auditTime) * 100, 20), GUIContent.none, "ProgressBar");
        GUI.color = Color.white;

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 50, 15, 100, 40));
        GUILayout.BeginHorizontal();
        if (!currentBase.running && !currentBase.crashed) {
            GUI.enabled = !currentBase.crashed;
            if (GUILayout.Button("Start")) {
                currentBase.StartSim();
                if (!colonyManager.running) colonyManager.StartSim();
            }
            GUI.enabled = true;
        }
        else if(currentBase.running) {
            if (GUILayout.Button("Stop")) {
                currentBase.StopSim();
                currentBase.ResetBoard();
            }
        }
        else {
            if (GUILayout.Button("Reset")) {
                currentBase.ResetBoard();
            }
        }
        GUI.enabled = currentBase.crashed;

        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        */
         
        // Context Panel
        if (currentBase.selectedRover == null) {
            GUI.Box(new Rect(Screen.width - 260, 10, 250, 520), "Buildings");
            //rects[ButtonType.drill] = new Rect(Screen.width - 260 + 250 / 2 - 132 / 2, 30, 128, 82.286f);
            //rects[ButtonType.refinery] = new Rect(Screen.width - 260 + 250 / 2 - 132 / 2, 30 + 132 + 20, 112, 80);
            rects[ButtonType.rover] = new Rect(Screen.width - 260 + 250 / 2 - 16, 30 + 264 + 40, 32, 32);

            foreach (KeyValuePair<ButtonType, Rect> entry in rects) {
                GUI.DrawTexture(entry.Value, buttonTextures[entry.Key]);
            }
        }
        else {
            GUI.Box(new Rect(Screen.width - 260, 10, 250, 520), "Programming");
            int size = 36, distance = 1, offset = 15, start = 260, yPos = 33;
            rects[ButtonType.arrowUp] = new Rect(Screen.width - start + offset, yPos, size, size);
            rects[ButtonType.arrowLeft] = new Rect(Screen.width - start + offset + size * 1 + distance * 1, yPos, size, size);
            rects[ButtonType.arrowRight] = new Rect(Screen.width - start + offset + size * 2 + distance * 2, yPos, size, size);
            rects[ButtonType.grab] = new Rect(Screen.width - start + offset + size * 3 + distance * 3, yPos, size, size);
            rects[ButtonType.drop] = new Rect(Screen.width - start + offset + size * 4 + distance * 4, yPos, size, size);
            rects[ButtonType.blank] = new Rect(Screen.width - start + offset + size * 5 + distance * 5, yPos, size, size);

            foreach (KeyValuePair<ButtonType, Rect> entry in rects) {
                GUI.DrawTexture(entry.Value, buttonTextures[entry.Key]);
            }

            dropRect = new Rect(Screen.width - 250, 80, 230, 410);
            GUI.skin = null;
            GUI.Box(dropRect, GUIContent.none);
            GUI.skin = Ourskin;

            GUILayout.BeginArea(new Rect(Screen.width - 185, 495, 100, 40));
            GUILayout.BeginHorizontal();
            GUI.enabled = currentBase.selectedRover.actionsSize > 0 && !currentBase.running && !currentBase.crashed;
            if (GUILayout.Button("Clear")) {
                currentBase.selectedRover.ClearActions();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // Draw Selected Rover's Actions
            if (currentBase.selectedRover.actionsSize > 0) {
                ReadOnlyCollection<MarsBase.Rover.ActionType> actions = currentBase.selectedRover.actions;
                GUILayout.BeginArea(new Rect(Screen.width - 245, 85, 230, 400));
                if (actions.Count > 45)
                    scrollPosition = GUI.BeginScrollView(new Rect(0, 0, 235, 410), scrollPosition, new Rect(0, 0, 215, 410 + ((actions.Count - 41) / 5) * 45));
                int colPos = 0;
                int rowPos = 0;
                int actionDistance = 5;
                int actionSize = 40;

                for (int i = 0; i < actions.Count; i++) {
                    Rect newActionRect = new Rect((actionSize + actionDistance) * colPos, (actionSize + actionDistance) * rowPos, actionSize, actionSize);
                    actionRects.Add(new Rect((actionSize + actionDistance) * colPos + (Screen.width - 245), (actionSize + actionDistance) * rowPos + 85, actionSize, actionSize));
                    if (i == currentBase.selectedRover.currentActionIndex) {
                        if (currentBase.selectedRover.crashed) GUI.color = Color.red;
                        else if(currentBase.running) GUI.color = Color.green;
                    }
                    GUI.DrawTexture(newActionRect, buttonTextures[actionToButton[actions[i]]]);
                    GUI.color = Color.white;
                    colPos++;
                    if (colPos == 5) {
                        colPos = 0;
                        rowPos++;
                    }
                }
                if (actions.Count > 45)
                    GUI.EndScrollView();
                GUILayout.EndArea();
            }
        }

        // Buttons to change current scene or UI
        GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 60, 250, 50));
        GUILayout.BeginHorizontal();
        GUI.enabled = false;
        GUILayout.Button("Main Menu");
        GUILayout.Button("Pause");
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Main Base")) {
            currentBase = null;
            currentGUI = ColonyGUI;
            return;
        }
        GUI.enabled = false;
        GUILayout.Button("Options");
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // Draw Board
        DrawBase();

        // Handle Custom Interaction
        if (!currentBase.running) {
            bool crashCheck = true;
            if (currentBase.selectedRover != null) crashCheck = !currentBase.selectedRover.crashed;
            if (crashCheck) {
                if (!dragging && Event.current.type == EventType.mouseDown) {
                    foreach (KeyValuePair<ButtonType, Rect> entry in rects) {
                        if (entry.Value.Contains(Event.current.mousePosition)) {
                            dragging = true;
                            draggingButton = entry.Key;
                            break;
                        }
                    }

                    if (currentBase.selectedRover != null) {
                        for (int i = 0; i < actionRects.Count; i++) {
                            if (actionRects[i].Contains(Event.current.mousePosition)) {
                                currentBase.selectedRover.RemoveAction(i);
                                break;
                            }
                        }
                    }
                }
                else if (dragging && Event.current.type == EventType.mouseUp) {
                    dragging = false;
                    if (currentBase.selectedRover != null) {
                        if (dropRect.Contains(Event.current.mousePosition)) {
                            currentBase.selectedRover.AddAction(buttonToAction[draggingButton]);
                        }
                    }
                    else {
                        Vector2 pos = Event.current.mousePosition;

                        int x = Mathf.FloorToInt(pos.x / 30);
                        int y = Mathf.FloorToInt(pos.y / 30) + 1;

                        if (x >= 0 && x < MarsBase.GRID_WIDTH && y >= 0 && y <= MarsBase.GRID_HEIGHT) {
                            if (currentBase.board[x, MarsBase.GRID_HEIGHT - y].tileType == MarsBase.GridTile.TileType.open)
                                currentBase.BuyPart(draggingButton, x, MarsBase.GRID_HEIGHT - y);
                        }
                    }
                }
            }
        }

        // Draw Dragged Object
        if (dragging) {
            Vector2 mousePosition = Event.current.mousePosition;
            Rect origRect = rects[draggingButton];
            Rect dragRect = new Rect(mousePosition.x - origRect.width / 2, mousePosition.y - origRect.height / 2, origRect.width, origRect.height);
            GUI.DrawTexture(dragRect, buttonTextures[draggingButton]);
        }
    }

    void DrawBase() {
        //GUI.skin = Ourskin;
        Dictionary<Rect, MarsBase.Rover> roverRects = new Dictionary<Rect, MarsBase.Rover>();

        GUILayout.BeginArea(new Rect(-1, -1, 691, 600));
        // Iterate through the grid
        for (int j = MarsBase.GRID_HEIGHT - 1; j >= 0; j--) {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < MarsBase.GRID_WIDTH; i++) {
                switch (currentBase.board[i, j].tileType) {
                    case MarsBase.GridTile.TileType.open:
                        GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
                        GUI.Box(new Rect(30 * i, 30 * (MarsBase.GRID_HEIGHT - j - 1), 31, 31), GUIContent.none, "Grid");
                        GUI.color = Color.white;
                        break;
                    case MarsBase.GridTile.TileType.building:
                        if (currentBase.board[i, j].buildingSprite) {
                            MarsBase.Building building = currentBase.board[i, j].building;
                            if (building.buildingType == MarsBase.Building.BuildingType.mine) {
                                Texture drillTex = drillTextures[building.buildingLevel];
                                GUI.DrawTexture(new Rect((30 * (i - 4)) + (180 - drillTex.width) / 2, (30 * (MarsBase.GRID_HEIGHT - j - 3)) + (120 - drillTex.height) / 2, drillTex.width, drillTex.height), drillTex);
                            }
                            else if (building.buildingType == MarsBase.Building.BuildingType.processingPlant) {
                                Texture refineryTex = refineryTextures[building.buildingLevel];
                                GUI.DrawTexture(new Rect((30 * (i - 3)) + (120 - refineryTex.width) / 2, (30 * (MarsBase.GRID_HEIGHT - j - 2)) + (90 - refineryTex.height) / 2 + 5, refineryTex.width, refineryTex.height), refineryTex);
                            }
                            else if (building.buildingType == MarsBase.Building.BuildingType.tramStation) {
                                Texture tramTexture = Resources.Load("Textures/tram_small") as Texture;
                                GUI.DrawTexture(new Rect((30 * (i - 1)) + 10, (30 * (MarsBase.GRID_HEIGHT - j - 1)), tramTexture.width, tramTexture.height), tramTexture);
                            }
                        }
                        break;    
                    case MarsBase.GridTile.TileType.wall:
                        break;
                    case MarsBase.GridTile.TileType.rover:
                        MarsBase.Rover rover = currentBase.board[i, j].rover;
                        MarsBase.Direction direction = rover.direction;

                        if (currentBase.selectedRover == rover) {
                            GUI.color = new Color(0f, 1f, 0f, 0.5f);
                            GUI.Box(new Rect(30 * i, 30 * (MarsBase.GRID_HEIGHT - j - 1), 31, 31), GUIContent.none, "Grid");
                            GUI.color = Color.white;
                        }
                        else {
                            GUI.color = new Color(1f, 1f, 1f, 0.1f);
                            GUI.Box(new Rect(30 * i, 30 * (MarsBase.GRID_HEIGHT - j - 1), 31, 31), GUIContent.none, "Grid");
                            GUI.color = Color.white;
                        }
                        Rect newRoverRect = new Rect((30 * i) + 1, (30 * (MarsBase.GRID_HEIGHT - j - 1)) + 2, 29, 28);
                        roverRects[newRoverRect] = rover;
                        if (rover.crashed) GUI.color = Color.red;
                        switch (direction) {
                            case MarsBase.Direction.north:
                                GUI.DrawTexture(newRoverRect, buttonTextures[ButtonType.rover]);
                                break;
                            case MarsBase.Direction.east:
                                GUIUtility.RotateAroundPivot(90, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                GUI.DrawTexture(newRoverRect, buttonTextures[ButtonType.rover]);
                                GUIUtility.RotateAroundPivot(-90, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                break;
                            case MarsBase.Direction.south:
                                GUIUtility.RotateAroundPivot(180, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                GUI.DrawTexture(newRoverRect, buttonTextures[ButtonType.rover]);
                                GUIUtility.RotateAroundPivot(-180, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                break;
                            case MarsBase.Direction.west:
                                GUIUtility.RotateAroundPivot(-90, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                GUI.DrawTexture(newRoverRect, buttonTextures[ButtonType.rover]);
                                GUIUtility.RotateAroundPivot(90, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                break;
                        }
                        GUI.color = new Color(1f, 1f, 1f, 1f);

                        // Draw Resource Icons
                        if (rover.resource == MarsBase.ResourceType.rawIron) {
                            Rect iconRect = new Rect(newRoverRect.x, newRoverRect.y, 16, 16);
                            GUI.DrawTexture(iconRect, Resources.Load("Textures/rock_icon") as Texture);
                        }
                        else if (rover.resource == MarsBase.ResourceType.refinedIron) {
                            Rect iconRect = new Rect(newRoverRect.x, newRoverRect.y, 16, 16);
                            GUI.DrawTexture(iconRect, Resources.Load("Textures/iron_icon") as Texture);
                        }
                        break;
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();

        if (Event.current.type == EventType.mouseDown) {
            foreach (KeyValuePair<Rect, MarsBase.Rover> entry in roverRects) {
                if (entry.Key.Contains(Event.current.mousePosition)) {
                    if (currentBase.selectedRover != entry.Value) currentBase.selectedRover = entry.Value;
                    else currentBase.selectedRover = null;
                    break;
                }
            }
        }
    }
}