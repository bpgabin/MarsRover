using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class GUISystem : MonoBehaviour {

    public enum ButtonType { rover, drill, refinery, arrowUp, arrowLeft, arrowRight, grab, drop }

	public GUISkin Ourskin;
    private delegate void GUIFunction();
    private GUIFunction currentGUI;
    private ColonyManager colonyManager;

    private MarsBase currentBase = null;
    private MarsBase.Rover selectedRover = null;

    private bool dragging = false;
    private Vector2 scrollPosition = Vector2.zero;
    private ButtonType draggingButton;
    private Dictionary<ButtonType, string> buttonStrings;
    private Dictionary<ButtonType, Texture> buttonTextures;
    private Dictionary<ButtonType, MarsBase.Rover.ActionType> buttonToAction;
    private Dictionary<MarsBase.Rover.ActionType, ButtonType> actionToButton;

    void Start() {
        buttonStrings = new Dictionary<ButtonType, string>();
        buttonStrings[ButtonType.drill] = "Drill";
        buttonStrings[ButtonType.refinery] = "Refinery";

        buttonTextures = new Dictionary<ButtonType, Texture>();
        buttonTextures[ButtonType.drop] = Resources.Load("Textures/open_claw") as Texture;
        buttonTextures[ButtonType.grab] = Resources.Load("Textures/close_claw") as Texture;
        buttonTextures[ButtonType.arrowUp] = Resources.Load("Textures/arrow_icons_up") as Texture;
        buttonTextures[ButtonType.arrowLeft] = Resources.Load("Textures/arrow_icons_left") as Texture;
        buttonTextures[ButtonType.arrowRight] = Resources.Load("Textures/arrow_icons_right") as Texture;
        buttonTextures[ButtonType.rover] = Resources.Load("Textures/rover_back_64") as Texture;

        buttonToAction = new Dictionary<ButtonType, MarsBase.Rover.ActionType>();
        buttonToAction[ButtonType.arrowUp] = MarsBase.Rover.ActionType.forward;
        buttonToAction[ButtonType.arrowLeft] = MarsBase.Rover.ActionType.turnLeft;
        buttonToAction[ButtonType.arrowRight] = MarsBase.Rover.ActionType.turnRight;
        buttonToAction[ButtonType.grab] = MarsBase.Rover.ActionType.grab;
        buttonToAction[ButtonType.drop] = MarsBase.Rover.ActionType.drop;

        actionToButton = new Dictionary<MarsBase.Rover.ActionType, ButtonType>();
        actionToButton[MarsBase.Rover.ActionType.forward] = ButtonType.arrowUp;
        actionToButton[MarsBase.Rover.ActionType.turnLeft] = ButtonType.arrowLeft;
        actionToButton[MarsBase.Rover.ActionType.turnRight] = ButtonType.arrowRight;
        actionToButton[MarsBase.Rover.ActionType.grab] = ButtonType.grab;
        actionToButton[MarsBase.Rover.ActionType.drop] = ButtonType.drop;

        currentGUI = MainMenuGUI;
    }

    void OnGUI() {

        currentGUI();
	
    }

    void MainMenuGUI() {
		GUI.skin = Ourskin;
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
        GUI.enabled = colonyManager.money >= colonyManager.costs[MarsBase.BaseType.mining] && !colonyManager.bases.ContainsKey(MarsBase.BaseType.mining);
        if (GUILayout.Button("Buy Mining Base")) {
            colonyManager.AddBase(MarsBase.BaseType.mining);
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUI.Box(new Rect(Screen.width / 2 - 100, 20, 200, 200), "Elevator");

        if (colonyManager.bases.ContainsKey(MarsBase.BaseType.mining)) {
            if (GUI.Button(new Rect(100, 350, 100, 100), "Mining")) {
                currentBase = colonyManager.bases[MarsBase.BaseType.mining];
                currentGUI = BaseGUI;
            }
        }
    }

    void BaseGUI() {
        GUI.depth = 3;
		GUI.skin = Ourskin;
        // Track each interactive button's Rect
        Dictionary<ButtonType, Rect> rects = new Dictionary<ButtonType, Rect>();
        Rect dropRect = new Rect(0, 0, 0, 0);

        // Draw Background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/marsbackground_01") as Texture);

        //button tabs at the top showing various info
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

        // Context Panel
        if (selectedRover == null) {
			GUI.Box(new Rect(Screen.width - 260, 10, 250, 520), "Buildings");
            rects[ButtonType.drill] = new Rect(Screen.width - 260 + 250 / 2 - 132 / 2, 30, 132, 132);
            rects[ButtonType.refinery] = new Rect(Screen.width - 260 + 250 / 2 - 132 / 2, 30 + 132 + 20, 132, 132);

            foreach (KeyValuePair<ButtonType, Rect> entry in rects) {
                GUI.Box(entry.Value, buttonStrings[entry.Key]);
            }

            rects[ButtonType.rover] = new Rect(Screen.width - 260 + 250 / 2 - 32, 30 + 264 + 40, 64, 64);
            GUI.DrawTexture(rects[ButtonType.rover], buttonTextures[ButtonType.rover]);
        }
        else {
			GUI.Box(new Rect(Screen.width - 260, 10, 250, 520), "Programming");
            int size = 40, distance = 5, offset = 35, start = 280, yPos = 33;
            rects[ButtonType.arrowUp] = new Rect(Screen.width - start + offset, yPos, size, size);
            rects[ButtonType.arrowLeft] = new Rect(Screen.width - start + offset + size * 1 + distance * 1, yPos, size, size);
            rects[ButtonType.arrowRight] = new Rect(Screen.width - start + offset + size * 2 + distance * 2, yPos, size, size);
            rects[ButtonType.grab] = new Rect(Screen.width - start + offset + size * 3 + distance * 3, yPos, size, size);
            rects[ButtonType.drop] = new Rect(Screen.width - start + offset + size * 4 + distance * 4, yPos, size, size);

            foreach (KeyValuePair<ButtonType, Rect> entry in rects) {
                GUI.DrawTexture(entry.Value, buttonTextures[entry.Key]);
            }

            dropRect = new Rect(Screen.width - 250, 80, 230, 410);
			GUI.skin = null;
			GUI.Box(dropRect, GUIContent.none);
			GUI.skin = Ourskin;

            GUILayout.BeginArea(new Rect(Screen.width - 235, 495, 200, 40));
            GUILayout.BeginHorizontal();
            GUI.enabled = selectedRover.actionsSize > 0;
            if (GUILayout.Button("Test")) {
                colonyManager.StartSim();
            }
            if (GUILayout.Button("Clear")) {
                selectedRover.ClearActions();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (selectedRover.actionsSize > 0) {
                ReadOnlyCollection<MarsBase.Rover.ActionType> actions = selectedRover.actions;
                GUILayout.BeginArea(new Rect(Screen.width - 245, 85, 230, 400));
                if (actions.Count > 45)
                    scrollPosition = GUI.BeginScrollView(new Rect(0, 0, 235, 410), scrollPosition, new Rect(0, 0, 215, 410 + ((actions.Count - 41) / 5) * 45));
                int colPos = 0;
                int rowPos = 0;
                int actionDistance = 5;
                int actionSize = 40;
                foreach (MarsBase.Rover.ActionType action in actions) {
                    GUI.DrawTexture(new Rect((actionSize + actionDistance) * colPos, (actionSize + actionDistance) * rowPos, actionSize, actionSize), buttonTextures[actionToButton[action]]);
                    colPos++;
                    if(colPos == 5){
                        colPos = 0;
                        rowPos++;
                    }
                }
                if (actions.Count > 45)
                    GUI.EndScrollView();
                GUILayout.EndArea();
            }
        }

        //buttons to change current scene or UI
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
        if (!dragging && Event.current.type == EventType.mouseDown) {
            foreach (KeyValuePair<ButtonType, Rect> entry in rects) {
                if (entry.Value.Contains(Event.current.mousePosition)) {
                    dragging = true;
                    draggingButton = entry.Key;
                }
            }
        }
        else if (dragging && Event.current.type == EventType.mouseUp) {
            dragging = false;
            if (dropRect != new Rect(0, 0, 0, 0)) {
                if (dropRect.Contains(Event.current.mousePosition)) {
                    selectedRover.AddAction(buttonToAction[draggingButton]);
                }
            }
            else {
                Vector2 pos = Event.current.mousePosition;
                int x = Mathf.RoundToInt((pos.x - 10) / 68);
                int y = Mathf.RoundToInt((pos.y - 50) / 68);
                currentBase.BuyPart(draggingButton, x, MarsBase.GRID_HEIGHT - y);
            }
        }

        // Draw Dragged Object
        if (dragging) {
            Vector2 mousePosition = Event.current.mousePosition;
            Rect origRect = rects[draggingButton];
            Rect dragRect = new Rect(mousePosition.x - origRect.width / 2, mousePosition.y - origRect.height / 2, origRect.width, origRect.height);
            if (draggingButton == ButtonType.drill || draggingButton == ButtonType.refinery)
                GUI.Box(dragRect, buttonStrings[draggingButton]);
            else
                GUI.DrawTexture(dragRect, buttonTextures[draggingButton]);
        }
    }

    void DrawBase() {
		//GUI.skin = Ourskin;
        Dictionary<Rect, MarsBase.Rover> roverRects = new Dictionary<Rect,MarsBase.Rover>();

        GUILayout.BeginArea(new Rect(10, 50, 676, 550));
        // Iterate through the grid
        for (int j = MarsBase.GRID_HEIGHT - 1; j >= 0; j--) {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < MarsBase.GRID_WIDTH; i++) {
                switch (currentBase.board[i, j].tileType) {
                    case MarsBase.GridTile.TileType.open:
                        GUILayout.Box(GUIContent.none, GUILayout.Width(64), GUILayout.Height(64));
                        break;
                    case MarsBase.GridTile.TileType.building:
                        GUILayout.FlexibleSpace();
                        GUI.Box(new Rect(68 * i, 68 * (MarsBase.GRID_HEIGHT - j - 1), 132, 132), "Drill");
                        break;
                    case MarsBase.GridTile.TileType.wall:
                        GUILayout.FlexibleSpace();
                        break;
                    case MarsBase.GridTile.TileType.rover:
                        GUILayout.FlexibleSpace();
                        MarsBase.Rover rover = currentBase.board[i, j].rover;
                        MarsBase.Direction direction = rover.direction;
                        Rect newRoverRect = new Rect(68 * i, 68 * (MarsBase.GRID_HEIGHT - j - 1), 64, 64);
                        roverRects[newRoverRect] = rover;
                        if (selectedRover == rover) GUI.color = new Color(0.5f, 1.0f, 0.5f, 1.0f);
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

                        if (Event.current.type == EventType.mouseDown) {
                            foreach (KeyValuePair<Rect, MarsBase.Rover> entry in roverRects) {
                                if (entry.Key.Contains(Event.current.mousePosition)) {
                                    if (selectedRover != entry.Value) selectedRover = entry.Value;
                                    else selectedRover = null;
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }
}