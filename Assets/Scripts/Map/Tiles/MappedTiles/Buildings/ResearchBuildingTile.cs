﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchBuildingTile : PoweredBuildingTile
{
	public ResearchBuildingInfo researchInfo;

	public ResearchBuildingTile(HexCoords coords, float height, ResearchBuildingInfo tInfo) : base(coords, height, tInfo)
	{
		researchInfo = tInfo;
	}

	public override void OnPlaced()
	{
		base.OnPlaced();
		ResearchSystem.UnlockCategory(researchInfo.researchCategory);
	}
}
