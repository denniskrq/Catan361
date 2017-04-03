﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEdge : MonoBehaviour {

	public Vec3 HexPos1 { get; set; }
	public Vec3 HexPos2 { get; set; }
	public bool IsSelected = false;

	void OnMouseEnter() {
		GetComponent<SpriteRenderer> ().color = Color.red;
	}

	void OnMouseDown() {
		GamePlayer localPlayer = GameManager.LocalPlayer.GetComponent<GamePlayer>();
		localPlayer.SetBuildSelection (this);
		return;
	}

	public void ConstructRoad() {
		if (!GameManager.Instance.GameStateReadyAtStage (GameState.GameStatus.GRID_CREATED)) {
			Debug.Log ("Grid not created");
			return;
		}

		if (!GameManager.Instance.GetCurrentGameState ().CurrentTurn.IsLocalPlayerTurn ()) {
			Debug.Log ("Is not local player turn");
			return;
		}

		Edge currentEdge = GameManager.Instance.GetCurrentGameState ().CurrentEdges.getEdge (HexPos1, HexPos2);
		if (currentEdge.IsOwned) {
			Debug.Log ("Edge is already owned");
			return;
		}

		GamePlayer localPlayer = GameManager.LocalPlayer.GetComponent<GamePlayer> ();
		if (GameManager.Instance.GetCurrentGameState ().CurrentTurn.IsInSetupPhase () && localPlayer.placedRoad) {
			StartCoroutine (GameManager.GUI.ShowMessage ("You already placed a road during this round."));
			return;
		}

		if (!isConnectedToOwnedUnit ()) {
			StartCoroutine (GameManager.GUI.ShowMessage ("You must place a road adjacent to any intersection!"));
			return;
		}

		if (!GameManager.Instance.GetCurrentGameState ().CurrentTurn.IsInSetupPhase ()) {
			Dictionary<StealableType, int> requiredRes = new Dictionary<StealableType, int> () {
				{ StealableType.Resource_Brick, 1 },
				{ StealableType.Resource_Lumber, 1 }
			};

			if (!localPlayer.HasEnoughResources (requiredRes)) {
				StartCoroutine (GameManager.GUI.ShowMessage ("You don't have enough resources."));
				return;
			}

			localPlayer.CmdConsumeResources (requiredRes);
		} else {
			if (!localPlayer.placedSettlement) {
				StartCoroutine(GameManager.GUI.ShowMessage ("You must place a settlement first."));
				return;
			}
		}

		localPlayer.placedRoad = true;
		localPlayer.CmdBuildRoad(SerializationUtils.ObjectToByteArray(new Vec3[] { HexPos1, HexPos2 }));
	}

	private bool isConnectedToOwnedUnit() {
		GamePlayer localPlayer = GameManager.LocalPlayer.GetComponent<GamePlayer> ();
		List<List<Vec3>> adjIntersectionsPos1 = UIHex.getIntersectionsAdjacentPos (this.HexPos1);
		List<List<Vec3>> adjIntersectionsPos2 = UIHex.getIntersectionsAdjacentPos (this.HexPos2);
		List<Intersection> adjIntersections = new List<Intersection> ();
		List<Intersection> intersectionsIntersection = new List<Intersection> ();

		// get the intersection of both list
		foreach (List<Vec3> intersection1 in adjIntersectionsPos1) {
			Intersection i = GameManager.Instance.GetCurrentGameState ().CurrentIntersections.getIntersection (intersection1);
			adjIntersections.Add (i);
		}

		foreach (List<Vec3> intersection2 in adjIntersectionsPos2) {
			Intersection i = GameManager.Instance.GetCurrentGameState ().CurrentIntersections.getIntersection (intersection2);
			if (adjIntersections.Contains (i)) {
				intersectionsIntersection.Add (i);
			}
		}

		// check if any of the intersections intersection is owned by the local player
		foreach(Intersection i in intersectionsIntersection) {
			if (i.Owner == localPlayer.myName) {
				return true;
			}

			Edge roadTest1 = GameManager.Instance.GetCurrentGameState ().CurrentEdges.getEdge (i.adjTile1, i.adjTile2);
			Edge roadTest2 = GameManager.Instance.GetCurrentGameState ().CurrentEdges.getEdge (i.adjTile1, i.adjTile3);
			Edge roadTest3 = GameManager.Instance.GetCurrentGameState ().CurrentEdges.getEdge (i.adjTile2, i.adjTile3);

			if (roadTest1.Owner == localPlayer.myName || roadTest2.Owner == localPlayer.myName || roadTest3.Owner == localPlayer.myName) {
				return true;
			}
		}

		return false;
	}

	void OnMouseExit() {
		GetComponent<SpriteRenderer> ().color = new Color (0.0f, 0.0f, 0.0f, 0.3f);
	}

	// Use this for initialization
	void Start () {
		GetComponent<SpriteRenderer>().sortingLayerName = "edge";
	}
	
	// Update is called once per frame
	void Update () {
		if (!GameManager.Instance.GameStateReadyAtStage (GameState.GameStatus.GRID_CREATED)) {			
			return;
		}

		Edge e = GameManager.Instance.GetCurrentGameState ().CurrentEdges.getEdge (HexPos1, HexPos2);
		if (e == null) { return; }

		if (e.IsOwned) {
			GetComponent<SpriteRenderer> ().color = GameManager.ConnectedPlayersByName [e.Owner].GetComponent<GamePlayer> ().GetPlayerColor ();
			return;
		}

		if (e.isHarbour == true) {
			GetComponent<SpriteRenderer> ().color = Color.yellow;
			return;
		}

		if (this.IsSelected) {
			GetComponent<SpriteRenderer> ().color = Color.green;
			return;
		} else {
			GetComponent<SpriteRenderer> ().color = new Color(0, 0, 0, 55);
			return;
		}

	}
}
