using UnityEngine;
using System.Collections;

public class GameStart : MonoBehaviour {

	public GUISystem guiSystem;

	public void StartGame(){
		guiSystem.SwitchToGame();
	}
}
