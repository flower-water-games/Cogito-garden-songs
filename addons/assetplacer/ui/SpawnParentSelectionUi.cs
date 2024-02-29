// SpawnParentSelectionUi.cs
// Copyright (c) 2023 CookieBadger. All Rights Reserved.

#if TOOLS
#nullable disable
using Godot;

namespace AssetPlacer;

[Tool]
public partial class SpawnParentSelectionUi : Node
{
	[Export] public NodePath spawnNodeSelector;
	[Export] public NodePath selectedAsSpawnNodeButton;

	public SpawnParentSelectionButton spawnParentSelectionButton;
	public Button _selectedAsSpawnNodeButton;

	public void Init()
	{
		spawnParentSelectionButton = GetNode<SpawnParentSelectionButton>(spawnNodeSelector);
		_selectedAsSpawnNodeButton = GetNode<Button>(selectedAsSpawnNodeButton);
		_selectedAsSpawnNodeButton.Disabled = true;
		_selectedAsSpawnNodeButton.Text = "";
	}

	public void ApplyTheme(Control baseControl)
	{
		// apply special themes for hard edge buttons with no coloring
		var selectIcon = baseControl.GetThemeIcon("ListSelect", "EditorIcons");
		_selectedAsSpawnNodeButton.Icon = selectIcon;
		var themeStyleboxNormal = spawnParentSelectionButton.GetThemeStylebox("normal") as StyleBoxFlat;
		themeStyleboxNormal.BgColor = ((StyleBoxFlat) baseControl.GetThemeStylebox("normal", "Button")).BgColor;
		spawnParentSelectionButton.AddThemeStyleboxOverride("normal", themeStyleboxNormal);
		_selectedAsSpawnNodeButton.AddThemeStyleboxOverride("normal", themeStyleboxNormal);
		Color font = baseControl.GetThemeColor("font_color", "Label");
		Color highlightedFont = (font.R + font.G + font.B) / 3f > 0.5f ? Colors.White : new Color(0.05f, 0.05f, 0.05f); 
		spawnParentSelectionButton.AddThemeColorOverride("font_hover_color", highlightedFont);
	}

	public void SetSelectedAsSpawnNodeButtonDisabled(bool value)
	{
		_selectedAsSpawnNodeButton.Disabled = value;
	}
	
	public void SetSpawnNode(Node node, Texture2D icon)
	{
		spawnParentSelectionButton.SetSpawnNode(node, icon);
	}
}
#endif
