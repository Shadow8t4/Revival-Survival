using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthCounterTest : MonoBehaviour {

	public LevelManager LM;

	public Text HealthDisplay;

	public int health;

	// Use this for initialization
	void Start () {
		health = LM.hp;
	}
	
	// Update is called once per frame
	void Update () {
		health = LM.hp;
		HealthDisplay.text = "Health: " + health;
	}
}
