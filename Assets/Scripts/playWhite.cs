using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class playWhite : MonoBehaviour
{
   public Button mybtn;
   public GameObject canvas;
	void Start () {
		// Button btn = mybtn.GetComponent<Button>();
		mybtn.onClick.AddListener(onClick);
	}

	void onClick(){
		canvas.SetActive(false);
	}
}

