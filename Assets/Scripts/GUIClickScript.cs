using UnityEngine;
using System.Collections;

public class GUIClickScript : MonoBehaviour {
	
	public GUISkin guiSkin;
	public Object buttonObj;
	public GameObject targetBox;
	Vector3 offset;
	GameObject button;
	
	void OnMouseDown(){
		float xPos = Input.mousePosition.x / Screen.width;
		float yPos = Input.mousePosition.y / Screen.height;
		offset = transform.position - new Vector3(xPos, yPos, transform.position.z);
		
		button = Instantiate(buttonObj, transform.position + new Vector3(0f, 0f, 1f), transform.rotation) as GameObject;
	}
	
	void OnMouseDrag(){
		float xPos = Input.mousePosition.x / Screen.width;
		float yPos = Input.mousePosition.y / Screen.height;
		float zPos = 1.0f;
		
		button.transform.position = new Vector3(xPos, yPos, zPos) + offset;
	}
	
	void OnMouseUp(){
		Vector3 mouseCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		mouseCoords.z = targetBox.transform.position.z;
		if(targetBox.renderer.bounds.Contains(mouseCoords)){
			
		}
		else
			Destroy(button);
	}
	

}
