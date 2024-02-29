// AssetPersistentData.cs
// Copyright (c) 2023 CookieBadger. All Rights Reserved.

#if TOOLS
#nullable disable

using Godot;

namespace AssetPlacer;

[Tool]
public partial class AssetPersistentData : Resource
{
	[Export] public string path;
	[Export] public Asset3DData.PreviewPerspective previewPerspective;

	
	public AssetPersistentData()
	{
	}

	public AssetPersistentData(string path, Asset3DData.PreviewPerspective previewPerspective)
	{
		this.path = path;
		this.previewPerspective = previewPerspective;
	}

	public static Asset3DData GetAsset3DData(AssetPersistentData data)
	{
		return new Asset3DData(data.path, data.previewPerspective);
	}
}
#endif
