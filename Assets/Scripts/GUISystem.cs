using UnityEngine;
using System.Collections;

public class GUISystem : MonoBehaviour {
	
	public Texture background;
	public Texture up_arrow;
    public Texture left_arrow;
	public Texture right_arrow;
	public Texture pick_up;
    public Texture drop_off;
	//public GUISkin guiSkin;
	public GUIStyle tab_buttons;
	public GUIStyle info_box;
	
	void OnGUI(){
	
		//GUI.skin = guiSkin;
		GUI.depth = 3;
	
		
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background);
		
		//button tabs at the top showing various info
		GUILayout.BeginArea(new Rect(10, Screen.height - 590, 400, 200));
		GUILayout.BeginHorizontal();
		GUILayout.Button("Revenue", tab_buttons);
		GUILayout.Button("Resources", tab_buttons);
		GUILayout.Button("Repairs", tab_buttons);
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		
		//objects to be grabbed and dropped
		GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 590, 250, 100));
		GUILayout.Box(GUIContent.none, GUILayout.Width(250), GUILayout.Height(100));
		GUILayout.EndArea();
		//draws icons to control rover
		GUI.DrawTexture(new Rect(Screen.width - 260, Screen.height - 590, 48, 48), up_arrow);
		GUI.DrawTexture(new Rect(Screen.width - 212, Screen.height - 590, 48, 48), left_arrow);
		GUI.DrawTexture(new Rect(Screen.width - 164, Screen.height - 590, 48, 48), right_arrow);
		GUI.DrawTexture(new Rect(Screen.width - 116, Screen.height - 590, 48, 48), pick_up);
		GUI.DrawTexture(new Rect(Screen.width - 68,  Screen.height - 590, 48, 48), drop_off);
		
		// info area of selected object
		GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 480, 250, 380));
		GUILayout.Box(GUIContent.none, info_box, GUILayout.Width(250), GUILayout.Height(380));
		GUILayout.EndArea();
		
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
        