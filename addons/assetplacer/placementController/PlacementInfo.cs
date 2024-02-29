// PlacementInfo.cs
// Copyright (c) 2023 CookieBadger. All Rights Reserved.

#if TOOLS
#nullable disable
namespace AssetPlacer;

public abstract partial class AssetPlacementController
{
    public class PlacementInfo
    {
        public PlacementPositionInfo positionInfo;
        public string placementTooltip;

        public PlacementInfo(PlacementPositionInfo positionInfo, string placementTooltip = null)
        {
            this.positionInfo = positionInfo;
            this.placementTooltip = placementTooltip;
        }
    }
}
#endif