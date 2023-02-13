using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class localGame : MonoBehaviour
{
	public GameObject controller;
   public Button mybtn;
   public GameObject canvas;
	void Start () {
		// Button btn = mybtn.GetComponent<Button>();
		controller = GameObject.FindGameObjectWithTag("GameController");
		mybtn.onClick.AddListener(onClick);
	}

	void onClick(){
		canvas.SetActive(false);
		controller.GetComponent<Grid>().startGame();
	}
}
