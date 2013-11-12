using UnityEngine;
using System.Collections;

public class GUISystem : MonoBehaviour {
	
	public Texture background;
	public Texture info_box_border;
	public GUISkin guiSkin;
	
	
	void OnGUI(){
	
		GUI.skin = guiSkin;
		GUI.depth = 3;
	
		
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background);
		
		//button tabs at the top showing various info
		GUILayout.BeginArea(new Rect(10, Screen.height - 590, 200, 200));
		GUILayout.BeginHorizontal();
		GUILayout.Button("Revenue","tab_buttons");
		GUILayout.Button("Resources","tab_buttons");
		GUILayout.Button("Repairs","tab_buttons");
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		/*
		//objects to be grabbed and dropped
		GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 590, 250, 100));
		GUILayout.Box(GUIContent.none, GUILayout.Width(250), GUILayout.Height(100));
		GUILayout.EndArea();
		//draws icons to control rover
	*/
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
		GUILayout.Button("Main Base");
		GUILayout.Button("options");
		GUILayout.EndHorizontal();
		
	
		GUILayout.EndArea ();
		
		GUILayout.BeginArea(new Rect(10, 50, 676, 550));
		GetComponent<BaseScript>().DrawGame();
		GUILayout.EndArea();
	}
}
        