using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Win : MonoBehaviour {

	public LevelManager LM;

	public Image im;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		WinCondition();
	}

	public void WinCondition()
	{
		if(LM.player.transform.position.x > 180)
		{
			print ("Test");
			im.enabled = true;
		}
	}
}
