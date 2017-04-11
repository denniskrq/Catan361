﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitMoveButton : MonoBehaviour {

	bool inUse = false;

    string localPlayerName;

    public void ClickBuild()
    {
        // get local player and current intersection selection
        GamePlayer localPlayer = getLocalPlayer();
        UIIntersection selectedUIIntersection = localPlayer.selectedUIIntersection;
        Vec3[] selectedPos = new Vec3[] { selectedUIIntersection.HexPos1, selectedUIIntersection.HexPos2, selectedUIIntersection.HexPos3 };
        Intersection selectedIntersection = GameManager.Instance.GetCurrentGameState().CurrentIntersections.getIntersection(new List<Vec3>(selectedPos));
        
        // check if player has knights he must reposition
        if (localPlayer.knightsToMove.Count != 0)
        {
            // game rules checks 
            if (selectedUIIntersection == null)
            {
                StartCoroutine(GameManager.GUI.ShowMessage("Please select an intersection to move to."));
                return;
            }
            else if (selectedIntersection.Owner != localPlayer.myName && selectedIntersection.Owner != "")
            {
                StartCoroutine(GameManager.GUI.ShowMessage("You do not own that intersection."));
                return;
            }
            else if (selectedIntersection.unit != null)
            {
                StartCoroutine(GameManager.GUI.ShowMessage("This intersection already contains a unit."));
                return;
            }

            // check for valid path
            KeyValuePair<Vec3[], Knight> pair = localPlayer.knightsToMove.Peek();
            Vec3[] oldPos = pair.Key;
            if (!Intersection.checkForPath(oldPos, selectedPos))
            {
                StartCoroutine(GameManager.GUI.ShowMessage("Selected intersection must on the same path."));
                return;
            }

            // call command to replace knight and reset the button
            localPlayer.CmdReplaceKnight(SerializationUtils.ObjectToByteArray(selectedPos));
            StartCoroutine(GameManager.GUI.ShowMessage("You have repositioned your knight."));
            inUse = false;
        }
        // if the local player hasn't selected a unit to move
        else if (!inUse)
        {
            // invalid selection checks 
            if (selectedIntersection.unit == null)
            {
                StartCoroutine(GameManager.GUI.ShowMessage("Knight must be selected for movement."));
                return;
            }
            else if (selectedIntersection.unit.GetType() != typeof(Knight))
            {
                StartCoroutine(GameManager.GUI.ShowMessage("Knight must be selected for movement."));
                return;
            }
            else
            {
                Knight k = (Knight)selectedIntersection.unit;

                if (!k.active)
                {
                    StartCoroutine(GameManager.GUI.ShowMessage("Only active knights may move."));
                    return;
                }
                // set the local player's move selection and set the button to in use
                localPlayer.SetMoveSelection();
                StartCoroutine(GameManager.GUI.ShowMessage("Set the unit to move."));
                inUse = true;
            }

           
        }
        // if the local player has selected a unit 
        else
        {
            UIIntersection oldUIIntersection = localPlayer.uiIntersectionToMove;
            Vec3[] oldPos = new Vec3[] { oldUIIntersection.HexPos1, oldUIIntersection.HexPos2, oldUIIntersection.HexPos3 };
            Intersection oldIntersection = GameManager.Instance.GetCurrentGameState().CurrentIntersections.getIntersection(new List<Vec3>(oldPos));

            // check for a valid path
            if (!Intersection.checkForPath(oldPos, selectedPos))
            {
                StartCoroutine(GameManager.GUI.ShowMessage("Selected intersection must on the same path."));
                inUse = false;
                return;
            }
            // check for owned intersection
            else if (selectedIntersection.Owner != "")
            {
                // throw error if local player already owns selected intersection
                if (selectedIntersection.Owner == localPlayer.myName)
                {
                    StartCoroutine(GameManager.GUI.ShowMessage("You already own a unit at the selected location."));
                    inUse = false;
                    return;
                }
                // check for a knight in the selected intersection
                else if (selectedIntersection.unit != null)
                {
                    if (selectedIntersection.unit.GetType() == typeof(Knight))
                    {
                        // get the knights and the other owner's knight
                        Knight localPlayerKnight = (Knight)oldIntersection.unit;
                        Knight replacedKnight = (Knight)selectedIntersection.unit;
                        String oldOwnerName = GameManager.ConnectedPlayersByName[selectedIntersection.Owner].GetComponent<GamePlayer>().myName;

                        // compare the knight's levels 
                        if (localPlayerKnight.level <= replacedKnight.level)
                        {
                            StartCoroutine(GameManager.GUI.ShowMessage("You may only displace a lower level knight."));
                            inUse = false;
                            return;
                        }

                        // moves knight and calls rpc function to add replaced knight to queue for the previous owner
                        localPlayer.CmdMoveUnitWithReplacement(
                            SerializationUtils.ObjectToByteArray(oldPos),
                            SerializationUtils.ObjectToByteArray(selectedPos),
                            SerializationUtils.ObjectToByteArray(oldOwnerName),
                            SerializationUtils.ObjectToByteArray(replacedKnight));

                        StartCoroutine(GameManager.GUI.ShowMessage("Successfully moved the unit."));
                        inUse = false;
                        return;
                    }
                    else
                    {
                        StartCoroutine(GameManager.GUI.ShowMessage("Intersection contains a non-knight unit."));
                        inUse = false;
                        return;
                    }
                }
            }
            // if selection is empty intersection
            else
            {
                // move the knight and reset the button
                localPlayer.CmdMoveUnit(SerializationUtils.ObjectToByteArray(oldPos), SerializationUtils.ObjectToByteArray(selectedPos));
                StartCoroutine(GameManager.GUI.ShowMessage("Successfully moved the unit."));
                inUse = false;
                return;
            }
        }
    }

    private GamePlayer getLocalPlayer()
    {
        return GameManager.LocalPlayer.GetComponent<GamePlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        GamePlayer localPlayer = getLocalPlayer();
        if (localPlayer.knightsToMove.Count != 0)
        {
            GetComponent<Button>().enabled = true;
            GetComponentInChildren<Text>().text = "REPOSITION KNIGHT";
            return;
            // player must move his knight first 
        }
        else if (inUse)
        {
            GetComponent<Button>().enabled = true;
            GetComponentInChildren<Text>().text = "CHOOSE DESTINATION";
            return;
        }
        else
        {
            if (!GameManager.Instance.GameStateReadyAtStage(GameState.GameStatus.GRID_CREATED))
            {
                GetComponent<Button>().enabled = false;
                GetComponentInChildren<Text>().text = "SETTING UP GAME";
                return;
            }

            if (!GameManager.Instance.GetCurrentGameState().CurrentTurn.IsLocalPlayerTurn())
            {
                GetComponent<Button>().enabled = false;
                GetComponentInChildren<Text>().text = "NOT YOUR TURN";
                return;
            }
            if (localPlayer.selectedUIIntersection != null)
            {
                UIIntersection selectedUIIntersection = localPlayer.selectedUIIntersection;
                Intersection intersection = GameManager.Instance.GetCurrentGameState().CurrentIntersections.getIntersection(new List<Vec3>
                    (new Vec3[] { selectedUIIntersection.HexPos1, selectedUIIntersection.HexPos2, selectedUIIntersection.HexPos3 }));

                if (intersection.unit == null)
                {
                    GetComponent<Button>().enabled = false;
                    GetComponentInChildren<Text>().text = "EMPTY INTERSECTION";
                    return;
                }
                else if (intersection.Owner != localPlayer.myName)
                {
                    GetComponent<Button>().enabled = false;
                    GetComponentInChildren<Text>().text = "INTERSECTION NOT OWNED";
                    return;
                }
                else if (intersection.unit.GetType() == typeof(Knight))
                {
                    GetComponent<Button>().enabled = true;
                    GetComponentInChildren<Text>().text = "MOVE KNIGHT";
                    return;
                }
            }
            else
            {
                GetComponent<Button>().enabled = false;
                GetComponentInChildren<Text>().text = "NO BUILD SELECTION";
                return;
            }
        }
    }
        
}
