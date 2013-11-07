using UnityEngine;
using System.Collections;

public class GUISystem : MonoBehaviour {
	
	public GUISkin guiSkin;

	void OnGUI(){
		GUI.skin = guiSkin;
		
		//button tabs at the top showing various info
		GUILayout.BeginArea(new Rect(10, Screen.height - 590, 350, 200));
		GUILayout.BeginHorizontal();
		GUILayout.Button("Revenue");
		GUILayout.Button("Resources");
		GUILayout.Button("Repairs");
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		
		//objects to be grabbed and dropped
		GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 590, 250, 100));
		GUILayout.Box(GUIContent.none, GUILayout.Width(250), GUILayout.Height(100));
		GUILayout.EndArea();
		
		// info area of selected object
		GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 480, 250, 380));
		GUILayout.Box(GUIContent.none, GUILayout.Width(250), GUILayout.Height(380));
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
	}
}
        