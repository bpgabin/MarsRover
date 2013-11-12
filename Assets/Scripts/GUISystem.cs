using UnityEngine;
using System.Collections;

public class GUISystem : MonoBehaviour {
	
	public Texture background;
	public Texture info_box_border;
	public GUISkin guiSkin;
	
	private delegate void GUIFunction();
	private GUIFunction currentGUI;
	private ColonyManager colonyManager;
	
	private MarsBase currentBase;
	private MarsBase.Rover selectedRover;
	
	void Start(){
		currentGUI = MainMenuGUI;
		currentBase = null;
		selectedRover = null;
	}
	
	void OnGUI(){
		currentGUI();	
	}
	
	void MainMenuGUI(){
		GUI.Box(new Rect(Screen.width / 2 - 100, 20, 200, 20), "Space Elevator");
		GUILayout.BeginArea(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 100, 100, 200));
		if(GUILayout.Button("Start")){
			colonyManager = gameObject.AddComponent<ColonyManager>();
			currentGUI = ColonyGUI;
		}
		GUILayout.EndArea();
	}
	
	void ColonyGUI(){
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Box("$" + colonyManager.money);
		GUILayout.Box("Iron: " + colonyManager.iron);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		
		// Trade Button
		GUI.enabled = false;
		if(GUILayout.Button("Trade", GUILayout.Width(50))){
			
		}
		GUI.enabled = true;
		
		// Temporary Build Button
		// TODO: Remove temporary build button when no longer needed.
		GUI.enabled = colonyManager.money >= colonyManager.costs[MarsBase.BaseType.mining] && !colonyManager.bases.ContainsKey(MarsBase.BaseType.mining);
		if(GUILayout.Button("Buy Mining Base")){
			colonyManager.AddBase(MarsBase.BaseType.mining);
		}
		GUI.enabled = true;
		
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUI.Box(new Rect(Screen.width / 2 - 100, 20, 200, 200), "Elevator");
		
		if(colonyManager.bases.ContainsKey(MarsBase.BaseType.mining)){
			if(GUI.Button(new Rect(100, 350, 100, 100), "Mining")){
				currentBase = colonyManager.bases[MarsBase.BaseType.mining];
				currentGUI = BaseGUI;
			}
		}
	}
	
	void BaseGUI(){
		GUI.skin = guiSkin;
		GUI.depth = 3;
		
		// Draw Background
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background);
		
		//button tabs at the top showing various info
		GUILayout.BeginArea(new Rect(10, Screen.height - 590, 200, 200));
		GUILayout.BeginHorizontal();
		GUILayout.Button("Revenue","tab_buttons");
		GUILayout.Button("Resources","tab_buttons");
		GUILayout.Button("Repairs","tab_buttons");
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		
		// info area of selected object
		GUILayout.BeginArea(new Rect(Screen.width - 245, Screen.height - 585, 220, 470));
		GUILayout.Box(GUIContent.none,"info_box", GUILayout.Width(220), GUILayout.Height(470));
		GUILayout.EndArea();
		GUI.DrawTexture(new Rect(Screen.width - 260, Screen.height - 590, 250, 480), info_box_border);
		
		//buttons to change current scene or UI
		GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 90, 250, 100));
		GUILayout.BeginHorizontal();
        GUILayout.Button("Main Menu");
		GUILayout.Button("Pause");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Main Base")){
			currentBase = null;
			currentGUI = ColonyGUI;
		}
		GUILayout.Button("options");
		GUILayout.EndHorizontal();
		GUILayout.EndArea ();
		
		// Draw Board
		DrawBase();
	}
	
	void DrawBase(){
		System.Diagnostics.Debug.Assert(currentBase != null);
		GUILayout.BeginArea(new Rect(10, 50, 676, 550));
		// Iterate through the grid
		for(int j = MarsBase.GRID_HEIGHT - 1; j >= 0; j--){
			GUILayout.BeginHorizontal();
			for(int i = 0; i < MarsBase.GRID_WIDTH; i++){
				switch(currentBase.board[i, j].tileType){
				case MarsBase.GridTile.TileType.open:
					GUILayout.Box(GUIContent.none, GUILayout.Width(64), GUILayout.Height(64));
					break;
				case MarsBase.GridTile.TileType.building:
					GUILayout.FlexibleSpace();
					GUI.Box(new Rect (68 * i, 68 * (MarsBase.GRID_HEIGHT - j - 1), 132, 132), "Mining");
					break;
				case MarsBase.GridTile.TileType.wall:
					GUILayout.FlexibleSpace();
					break;
				case MarsBase.GridTile.TileType.rover:
					GUILayout.FlexibleSpace();
					MarsBase.Rover rover = currentBase.board[i, j].rover;
					MarsBase.Direction direction = rover.direction;
					if(selectedRover == rover) GUI.color = new Color(0.5f, 1.0f, 0.5f, 1.0f);
					switch(direction){
					case MarsBase.Direction.north:
						GUI.
						GUI.DrawTexture(new Rect(68 * i, 68 * (MarsBase.GRID_HEIGHT - j - 1), 64, 64), Resources.Load("Textures/rover_back_64") as Texture);
						break;
					case MarsBase.Direction.east:
						GUI.DrawTexture(new Rect(68 * i, 68 * (MarsBase.GRID_HEIGHT - j - 1), 64, 64), Resources.Load("Textures/rover_right_64") as Texture);
						break;
					case MarsBase.Direction.south:
						GUI.DrawTexture(new Rect(68 * i, 68 * (MarsBase.GRID_HEIGHT - j - 1), 64, 64), Resources.Load("Textures/rover_front_64") as Texture);
						break;
					case MarsBase.Direction.west:
						GUI.DrawTexture(new Rect(68 * i, 568 * (MarsBase.GRID_HEIGHT - j - 1), 64, 64), Resources.Load("Textures/rover_left_64") as Texture);
						break;
					}
					GUI.color = new Color(1f, 1f, 1f, 1f);
					break;
				}
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();
	}
}