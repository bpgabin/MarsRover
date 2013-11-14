using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class GUISystem : MonoBehaviour {

    public enum ButtonType { rover, drill, refinery, arrowUp, arrowLeft, arrowRight, grab, drop, blank }

    public GUISkin Ourskin;
    private delegate void GUIFunction();
    private GUIFunction currentGUI;
    private ColonyManager colonyManager;

    private MarsBase currentBase = null;

    private bool dragging = false;
    private Vector2 scrollPosition = Vector2.zero;
    private ButtonType draggingButton;
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
        buttonTextures[ButtonType.rover] = Resources.Load("Textures/rover_back_64") as Texture;
        buttonTextures[ButtonType.drill] = Resources.Load("Textures/bucketwheelexcavator_small") as Texture;
        buttonTextures[ButtonType.refinery] = Resources.Load("Textures/refinery_small") as Texture;

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

        currentGUI = MainMenuGUI;
    }

    void OnGUI() {

        currentGUI();

    }

    void MainMenuGUI() {
        GUI.skin = Ourskin;

        // Draw Background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/marsbackground_01") as Texture);

        GUI.Box(new Rect(Screen.width / 2 - 100, 20, 200, 20), "Space Elevator");
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 100, 100, 200));
        if (GUILayout.Button("Start")) {
            colonyManager = gameObject.AddComponent<ColonyManager>();
            currentGUI = ColonyGUI;
        }
        GUILayout.EndArea();
    }

    void ColonyGUI() {
        GUI.skin = Ourskin;

        // Draw Background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/marsbackground_01") as Texture);


        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Box("$" + colonyManager.money);
        GUILayout.Box("Iron: " + colonyManager.iron);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();

        // Trade Button
        GUI.enabled = false;
        if (GUILayout.Button("Trade", GUILayout.Width(50))) {

        }
        GUI.enabled = true;

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
            if (GUI.Button(new Rect(220 * (i + 1), 400, 100, 100), text)) {
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
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/marsbackground_01") as Texture);

        // Button tabs at the top showing various info
        GUI.Box(new Rect(0, 0, 250, 50), GUIContent.none);
        GUILayout.BeginArea(new Rect(10, 20, 200, 250));
        GUILayout.BeginHorizontal();
        GUI.enabled = false;
        GUILayout.Button("Revenue");
        GUILayout.Button("Resources");
        GUILayout.Button("Repairs");
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

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

        // Context Panel
        if (currentBase.selectedRover == null) {
            GUI.Box(new Rect(Screen.width - 260, 10, 250, 520), "Buildings");
            rects[ButtonType.drill] = new Rect(Screen.width - 260 + 250 / 2 - 132 / 2, 30, 128, 82.286f);
            rects[ButtonType.refinery] = new Rect(Screen.width - 260 + 250 / 2 - 132 / 2, 30 + 132 + 20, 112, 80);
            rects[ButtonType.rover] = new Rect(Screen.width - 260 + 250 / 2 - 32, 30 + 264 + 40, 64, 64);

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
                        int x = Mathf.RoundToInt((pos.x - 10) / 68);
                        int y = Mathf.RoundToInt((pos.y - 50) / 68);
                        if (x >= 0 && x < MarsBase.GRID_WIDTH && y >= 0 && y < MarsBase.GRID_HEIGHT)
                            currentBase.BuyPart(draggingButton, x, MarsBase.GRID_HEIGHT - y);
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

        GUILayout.BeginArea(new Rect(10, 50, 676, 550));
        // Iterate through the grid
        for (int j = MarsBase.GRID_HEIGHT - 1; j >= 0; j--) {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < MarsBase.GRID_WIDTH; i++) {
                switch (currentBase.board[i, j].tileType) {
                    case MarsBase.GridTile.TileType.open:
                        //if (selectedRover == null) GUILayout.FlexibleSpace();
                        GUI.skin = null;
                        GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                        GUILayout.Box(GUIContent.none, GUILayout.Width(64), GUILayout.Height(64));
                        GUI.color = Color.white;
                        GUI.skin = Ourskin;
                        break;
                    case MarsBase.GridTile.TileType.building:
                        GUILayout.FlexibleSpace();
                        GUI.skin = null;
                        GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                        GUI.Box(new Rect(68 * i, 68 * (MarsBase.GRID_HEIGHT - j - 1), 132, 132), GUIContent.none);
                        GUI.color = Color.white;
                        GUI.skin = Ourskin;
                        GUI.DrawTexture(new Rect(68 * i, 68 * (MarsBase.GRID_HEIGHT - j - 1) + 25, 132, 84.857f), buttonTextures[ButtonType.drill]);
                        break;
                    case MarsBase.GridTile.TileType.wall:
                        GUILayout.FlexibleSpace();
                        break;
                    case MarsBase.GridTile.TileType.rover:
                        MarsBase.Rover rover = currentBase.board[i, j].rover;
                        MarsBase.Direction direction = rover.direction;

                        GUI.skin = null;
                        if (currentBase.selectedRover != rover) GUI.color = new Color(0f, 1f, 0f, 0.5f);
                        GUILayout.Box(GUIContent.none, GUILayout.Width(64), GUILayout.Height(64));
                        GUI.color = Color.white;
                        GUI.skin = Ourskin;

                        Rect newRoverRect = new Rect(68 * i, 68 * (MarsBase.GRID_HEIGHT - j - 1), 64, 64);
                        Rect newScreenRect = new Rect(68 * i + 10, 68 * (MarsBase.GRID_HEIGHT - j - 1) + 50, 64, 64);
                        roverRects[newScreenRect] = rover;
                        if (rover.crashed) GUI.color = Color.red;
                        switch (direction) {
                            case MarsBase.Direction.north:
                                GUI.DrawTexture(newRoverRect, Resources.Load("Textures/rover_back_64") as Texture);
                                break;
                            case MarsBase.Direction.east:
                                GUI.DrawTexture(newRoverRect, Resources.Load("Textures/rover_right_64") as Texture);
                                break;
                            case MarsBase.Direction.south:
                                GUI.DrawTexture(newRoverRect, Resources.Load("Textures/rover_front_64") as Texture);
                                break;
                            case MarsBase.Direction.west:
                                GUI.DrawTexture(newRoverRect, Resources.Load("Textures/rover_left_64") as Texture);
                                break;
                        }
                        GUI.color = new Color(1f, 1f, 1f, 1f);
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