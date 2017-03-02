﻿using System;

// Written by Alex B
using System.Collections.Generic;


namespace AssemblyCSharp
{
	public class Player
	{
		// private username and password
		string username;
		string password;
		// status, vps, gold, and longestRoad are simple c# attributes
		PlayerStatus status { get; set; }
		int defenderOfCatanVPs { get; set; }
		int gold { get; set; }
		bool longestRoad { get; set; }
		// using dictionary for holding resources and commodities
		Dictionary<StealableType, int> ownedResources= new Dictionary<StealableType, int>();
		// using list for holding owned progress cards
		List<ProgressCardType> ownedProgressCards = new List<ProgressCardType>();
		List<OwnableUnit> ownedUnits = new List<OwnableUnit>();

		// NOTE: we can use the built-in methods for the list and dictionary to edit the values

		// constructor
		public Player (string u, string p)
		{
			this.username = u;
			this.password = p;
			this.status = PlayerStatus.Available;
			this.defenderOfCatanVPs = 0;
			this.defenderOfCatanVPs = 0;
			this.gold = 0;
			this.longestRoad = false;
		}

	}
}

