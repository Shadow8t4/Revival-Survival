using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

	//  Player game object
	public GameObject player;
	//  Player HP
	const int maxHP = 8;
	public int hp;

	//  Where the player is going to respawn
	public Transform spawnPosition;

	// Use this for initialization
	void Start () {
		hp = maxHP;
	}
	
	// Update is called once per frame
	void Update () {
		DeathCheck ();
	}

	public void DeathCheck() {

		//  Combat death
		if (hp <= 0) {
			Respawn ();
		}

		//  Bottomless pit death
		if (player.transform.position.y < -20) {
			Respawn ();
		}
	}

	public void Respawn() {
		player.transform.position = spawnPosition.position;
		hp = maxHP;
	}
}
