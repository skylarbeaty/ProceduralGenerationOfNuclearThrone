using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restarter : MonoBehaviour {
	void Update(){
		if (Input.GetKeyDown("space")){
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		if (Input.GetKeyDown("escape")){
			Application.Quit();
		}
	}
}
