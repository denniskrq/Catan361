﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeButton : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
	}

	public string getPlayerName() {
		return transform.GetComponentInParent<PlayerResourcePanel> ().PlayerName;	
	}

	public void ClickSendTradeRequest() {
		TradeManager.Instance.SendTradeRequest (GameManager.LocalPlayer.GetComponent<GamePlayer> ().myName, getPlayerName());
	}

	// Update is called once per frame
	void Update () {
		/*if (GameManager.LocalPlayer.GetComponent<GamePlayer>().myName == getPlayerName()) {
			GetComponent<Button> ().enabled = false;
		}*/
	}
}
 