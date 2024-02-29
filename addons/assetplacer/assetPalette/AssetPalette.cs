// AssetPalette.cs
// Copyright (c) 2023 CookieBadger. All Rights Reserved.

#if TOOLS
#nullable disable
using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot.Collections;
using Array = System.Array;

namespace AssetPlacer;

[Tool]
public partial class AssetPalette : Node
{
	private const string OpenedLibrariesSaveKey = "opened_libraries";
	private const string LastSelectedLibrarySaveKey = "selected_library";
	
	private EditorInterface _editorInterface;
	private AssetPlacerUi _assetPlacerUi;
	public Node3D Hologram { get; private set; }
	public Node3D LastPlacedAsset { get; private set; }
	private Asset3DData _selectedAsset;
	private string _lastSelectedAssetPath;
	public string SelectedAssetName { get; private set; }
	private Godot.Collections.Dictionary<string, AssetLibraryData> _assetPathsByLibrary = new();
	private string _currentLibrary = null;
	private const string NewLibraryName = "Unnamed Library";
	private AssetPreviewGenerator _previewGenerator;
	private AssetPreviewGenerator PreviewGenerator
	{
		get
		{
			if (_previewGenerator == null)
			{
				_previewRenderingViewports = new();
				_previewGenerator = new AssetPreviewGenerator();
				const int previewVpCount = 3;
				for (int i = 0; i < previewVpCount; i++)
				{
					var previewVpScene = ResourceLoader.Load<PackedScene>("res://addons/assetplacer/assetPalette/previewGeneration/PreviewRenderingViewport.tscn");
					var previewRenderingViewport = previewVpScene.Instantiate<PreviewRenderingViewport>();
					_previewGenerator.AddChild(previewRenderingViewport);
					_previewRenderingViewports.Add(previewRenderingViewport);
				}
				_previewGenerator.Init(_previewRenderingViewports);
				AddChild(_previewGenerator);
			}
			return _previewGenerator;
		}
	}

	private Array<PreviewRenderingViewport> _previewRenderingViewports = new();
	private TextureRect _viewportTextureRect;

	public void Init(EditorInterface editorInterface)
	{
		var _ = PreviewGenerator; // Force Initialization

		_editorInterface = editorInterface;
	}

	public void PostInit()
	{
		var openedLibraries = AssetPlacerPersistence.LoadGlobalData(OpenedLibrariesSaveKey,Array.Empty<string>(), Variant.Type.PackedStringArray);
		var lastSelectedLibrary =
			AssetPlacerPersistence.LoadGlobalData(LastSelectedLibrarySaveKey, "", Variant.Type.String);
		InitLibraries(openedLibraries.AsStringArray(), lastSelectedLibrary.AsString());
	}
	
	public void Cleanup()
	{
		foreach (var vp in _previewRenderingViewports)
		{
			vp.QueueFree();
		}
	}

	public const string AssetLibrarySaveFolder = ".assetPlacerLibraries";
	public void SetUi(AssetPlacerUi assetPlacerUi)
	{
		_assetPlacerUi = assetPlacerUi;
		_assetPlacerUi.AssetsAdded += OnAddNewAsset;
		_assetPlacerUi.AssetSelected += OnSelectAsset;
		_assetPlacerUi.AssetsRemoved += OnRemoveAsset;
		_assetPlacerUi.AssetTransformReset += OnResetAssetTransform;
		_assetPlacerUi.AssetsOpened += OnOpenAsset;
		_assetPlacerUi.AssetShownInFileSystem += OnShowAssetInFileSystem;
		_assetPlacerUi.AssetLibrarySelected += OnLibraryLoad;
		_assetPlacerUi.AssetTabSelected += OnAssetLibrarySelect;
		_assetPlacerUi.NewTabPressed += () => OnNewAssetLibrary();
		_assetPlacerUi.SaveButtonPressed += OnSaveCurrentAssetLibrary;
		_assetPlacerUi.AssetLibrarySaved += SaveLibraryAt;
		_assetPlacerUi.AssetLibraryRemoved += OnRemoveAssetLibrary;
		_assetPlacerUi.ReloadLibraryPreviews += OnReloadLibraryPreviews;
		_assetPlacerUi.DefaultLibraryPreviews += OnDefaultLibraryPreviews;
		_assetPlacerUi.ReloadAssetPreview += ReloadAssetPreview;
		_assetPlacerUi.AssetPreviewPerspectiveChanged += OnAssetPreviewPerspectiveChanged;

		_assetPlacerUi.AssetLibraryShownInFileManager += OnShowAssetLibraryInFileManager;
		_assetPlacerUi.LibraryPreviewPerspectiveChanged += OnLibraryPreviewPerspectiveChanged;
		_assetPlacerUi.AssetButtonRightClicked += OnAssetButtonRightClicked;
		_assetPlacerUi.TabRightClicked += OnAssetTabRightClicked;
		_assetPlacerUi.SetAssetLibrarySaveDisabled(true);
		UpdateAssets(null);
	}

	private void OnAssetButtonRightClicked(string assetPath, Vector2 pos)
	{
		Debug.Assert(CurrentLibraryData.assetData.Any(a=>a.path == assetPath), $"AssetPath {assetPath} does not exist in current library");
		int prevPerspective = (int) CurrentLibraryData.assetData.First(a => a.path == assetPath).previewPerspective;
		_assetPlacerUi.DisplayAssetRightClickPopup(assetPath, prevPerspective, pos);
	}
	private void OnAssetTabRightClicked(string library, Vector2 pos)
	{
		Debug.Assert(_assetPathsByLibrary.ContainsKey(library), $"Data of library {library} not found");
		int prevPerspective = (int) _assetPathsByLibrary[library].previewPerspective;
		_assetPlacerUi.DisplayTabRightClickPopup(library, prevPerspective, pos);
	}

	private void OnDefaultLibraryPreviews(string library)
	{
		Debug.Assert(_assetPathsByLibrary.ContainsKey(library), $"Data of library {library} not found");
		var lib = _assetPathsByLibrary[library];
		foreach (var asset3DData in lib.assetData)
		{
			asset3DData.previewPerspective = Asset3DData.PreviewPerspective.Default;
			
		}
		OnReloadLibraryPreviews(library);
		lib.dirty = true;
		UpdateSaveDisabled();
	}
	
	private void OnReloadLibraryPreviews(string library)
	{
		Debug.Assert(_assetPathsByLibrary.ContainsKey(library), $"Data of library {library} not found");
		_assetPlacerUi.SelectAssetTab(library);
		GeneratePreviews(_assetPathsByLibrary[library].assetData, true, _assetPathsByLibrary[library].previewPerspective);
	}

	private void ReloadAssetPreview(string path)
	{
		Debug.Assert(CurrentLibraryData.GetAssetPaths().Contains(path), $"Asset {path} is not part of current library");
		var asset = CurrentLibraryData.assetData.Where(a => a.path == path);
		GeneratePreviews(asset, true, CurrentLibraryData.previewPerspective);
	}

	private void OnAssetPreviewPerspectiveChanged(string path, Asset3DData.PreviewPerspective perspective)
	{
		Debug.Assert(CurrentLibraryData.GetAssetPaths().Contains(path), $"Asset {path} is not part of current library");
		var asset = CurrentLibraryData.assetData.Where(a => a.path == path).ToList();
		CurrentLibraryData.dirty = true;
		UpdateSaveDisabled();
		asset.ForEach(a=>a.previewPerspective = perspective); // change perspective
		GeneratePreviews(asset, true, CurrentLibraryData.previewPerspective);
	}
	private void OnLibraryPreviewPerspectiveChanged(string library, Asset3DData.PreviewPerspective perspective)
	{
		Debug.Assert(_assetPathsByLibrary.ContainsKey(library), $"Data of library {library} not found");
		_assetPathsByLibrary[library].previewPerspective = perspective;
		_assetPathsByLibrary[library].dirty = true;
		UpdateSaveDisabled();
		GeneratePreviews(_assetPathsByLibrary[library].assetData, true, perspective);
	}

	private void OnRemoveAssetLibrary(string libraryName)
	{
		if (_assetPathsByLibrary.ContainsKey(libraryName))
		{
			_assetPathsByLibrary.Remove(libraryName);
			if(libraryName == _currentLibrary) _currentLibrary = null;
			AssetPlacerPersistence.StoreGlobalData(OpenedLibrariesSaveKey, 
				_assetPathsByLibrary.Values.Select(l=>l.savePath).ToArray());
			_assetPlacerUi.CallDeferred(nameof(_assetPlacerUi.RemoveAssetLibrary), libraryName); // deferred to avoid out of bounds error
		}
	}

	public override void _Process(double delta)
	{
		if (!Engine.IsEditorHint()) return;
		PreviewGenerator.Process();
	}

	private void OnShowAssetLibraryInFileManager(string libraryName)
	{
		if (!string.IsNullOrEmpty(_assetPathsByLibrary[libraryName].savePath))
		{
			OS.ShellOpen($"{ProjectSettings.GlobalizePath(GetFolderPathFromFilePath(_assetPathsByLibrary[libraryName].savePath))}");
		}
		else GD.PrintErr($"{nameof(AssetPlacerPlugin)}: Can't open library in file manager. Library is not saved.");
	}

	public static string GetAssetLibraryDirPath(FileDialog.AccessEnum access)
	{
		return $"{(access == FileDialog.AccessEnum.Userdata ? "user://" : "res://")}{AssetLibrarySaveFolder}";
	}

	private void OnSaveCurrentAssetLibrary()
	{
		if(_currentLibrary != null && CurrentLibraryData.savePath != null) {
			SaveLibraryAt(_currentLibrary, CurrentLibraryData.savePath, false);
		}
		else
		{
			_assetPlacerUi.ShowSaveDialog(_currentLibrary, true);
		}
	}

	private AssetLibraryData CurrentLibraryData => _assetPathsByLibrary[_currentLibrary];
	
	private string OnNewAssetLibrary(string name = NewLibraryName)
	{
		var libraryName = GetAvailableLibraryName(name);
		_assetPathsByLibrary.Add(libraryName, new AssetLibraryData());
		_assetPlacerUi.AddAndSelectAssetTab(libraryName);
		return libraryName;
	}

	private void InitLibraries(string[] openedLibraries, string selectLibraryPath)
	{
		foreach (var libraryPath in openedLibraries)
		{
			OnLibraryLoad(libraryPath);
		}

		var library = GetLibraryNameFromPath(selectLibraryPath);
		if(library != null) _assetPlacerUi.SelectAssetTab(library);
	}

	// Returns the name with which the library can be selected in the UI (tab title).
	// Not to be confused with the library file name.
	private string GetLibraryNameFromPath(string selectLibraryPath)
	{
		return _assetPathsByLibrary.Keys.FirstOrDefault(a => _assetPathsByLibrary[a].savePath == selectLibraryPath);
	}

	private void OnAssetLibrarySelect(string tabTitle, int scrollPosition)
	{
		if (tabTitle == _currentLibrary) return;
		_currentLibrary = tabTitle;
		if (_currentLibrary != null)
		{
			var perspective = _assetPathsByLibrary.ContainsKey(_currentLibrary)
				? _assetPathsByLibrary[_currentLibrary].previewPerspective
				: Asset3DData.PreviewPerspective.Default;
			UpdateAssets(null, perspective, scrollPosition);
		}
		UpdateSaveDisabled();
	}

	private void OnLibraryLoad(string path)
	{
		var assetLibraryResource = ResourceLoader.Load<Resource>(path, null, ResourceLoader.CacheMode.Ignore);
		if (assetLibraryResource is AssetLibrary assetLibrary) // path must be new
		{
			var existingLibrary = GetLibraryNameFromPath(path);
			if (existingLibrary == null)
			{
				OnNewAssetLibrary(GetFileNameFromFilePath(path));
				var asset3DData = assetLibrary.assetData.Select(a => AssetPersistentData.GetAsset3DData(a));
				OnAddAssetData(asset3DData);
				CurrentLibraryData.dirty = false;
				CurrentLibraryData.savePath = path;
				UpdateSaveDisabled();
				AssetPlacerPersistence.StoreGlobalData(OpenedLibrariesSaveKey, 
					_assetPathsByLibrary.Values.Select(l=>l.savePath).ToArray());
			}
			else
			{
				GD.Print($"{nameof(AssetPlacerPlugin)}: {path} is already loaded");
				_assetPlacerUi.SelectAssetTab(existingLibrary);
			}
		}
		else
		{
			GD.PrintErr($"{nameof(AssetPlacerPlugin)}: Resource found at {path} is not a scene library");
		}
		
	}

	private void SaveLibraryAt(string libraryKey, string path, bool changeName)
	{
		if (!_assetPathsByLibrary.ContainsKey(libraryKey))
		{
			GD.PrintErr($"{nameof(AssetPlacerPlugin)}: Error saving asset library '{libraryKey}': Library not found in loaded libraries.");
			return;
		}

		if (_assetPathsByLibrary.Keys.Any(lib=>lib != libraryKey && _assetPathsByLibrary[lib].savePath == path)) // different library with same file location loaded
		{
			GD.PrintErr($"{nameof(AssetPlacerPlugin)}: Error saving asset library at {path}: A library saved to {path} is currently open.");
			return;
		}

		// create a SceneLibrary resource and copy the assetPaths into it
		var assetLibraryData = _assetPathsByLibrary[libraryKey];
		var assetLibrary = AssetLibrary.BuildAssetLibary(assetLibraryData);
		var folder = GetFolderPathFromFilePath(path);
		if (!string.IsNullOrEmpty(folder) && !DirAccess.DirExistsAbsolute(folder))
		{
			DirAccess.MakeDirRecursiveAbsolute(folder);
		}
		
		var error = ResourceSaver.Save(assetLibrary, path);
		if (error == Error.Ok)
		{
			GD.PrintRich($"[b]Asset selection saved to: {path}[/b]");
			_assetPathsByLibrary[libraryKey].dirty = false;
			_assetPathsByLibrary[libraryKey].savePath = path;
			UpdateSaveDisabled();
			if(changeName) ChangeLibraryName(libraryKey, GetFileNameFromFilePath(path));
			AssetPlacerPersistence.StoreGlobalData(OpenedLibrariesSaveKey, 
				_assetPathsByLibrary.Values.Select(l=>l.savePath).ToArray());
		}
		else
		{
			GD.PrintErr($"{nameof(AssetPlacerPlugin)}: Error saving asset library: {error}");
		}
	}

	private void UpdateSaveDisabled()
	{
		if (!_assetPathsByLibrary.ContainsKey(_currentLibrary))
		{
			_assetPlacerUi.SetAssetLibrarySaveDisabled(true);
		}
		else
		{
			var hasChanges = CurrentLibraryData.savePath == null || CurrentLibraryData.dirty;
			_assetPlacerUi.SetAssetLibrarySaveDisabled(!hasChanges);
		}
	}
	
	private void ChangeLibraryName(string oldName, string newName)
	{
		if (oldName == newName) return;
		var data = _assetPathsByLibrary[oldName];
		_assetPathsByLibrary.Remove(oldName);
		var availableNewName = GetAvailableLibraryName(newName);
		_assetPlacerUi.ChangeTabTitle(oldName, availableNewName);
		_assetPathsByLibrary.Add(availableNewName, data);
		
		if(oldName == _currentLibrary) _currentLibrary = availableNewName;
	}

	private void OnShowAssetInFileSystem(string assetPath)
	{
		_editorInterface.GetFileSystemDock().NavigateToPath(assetPath);
	}

	private void OnOpenAsset(string assetPath)
	{
		_editorInterface.OpenSceneFromPath(assetPath);
	}

	private void OnSelectAsset(string path, string name)
	{
		if (path == _selectedAsset?.path) return;
		var pathNull = string.IsNullOrEmpty(path);
		_selectedAsset = pathNull ? null : CurrentLibraryData.GetAsset(path);
		SelectedAssetName = name;
		ClearHologram();
		
		if (!pathNull)
		{
			Hologram = CreateHologram();
			if (Hologram == null)
			{
				DeselectAsset();
			}
			else
			{
				_lastSelectedAssetPath = _selectedAsset.path;
			}
		}
	}

	private void OnResetAssetTransform(string path)
	{
		var data = _assetPathsByLibrary[_currentLibrary].GetAsset(path);
		data.lastTransform = data.defaultTransform;
		UpdateResetTransformButton(data);
	}
	
	public const string resFileEnding = ".res";
	public const string tresFileEnding = ".tres";

	private bool IsValid3DFile(string filePath)
	{
		const string sceneFileEnding = ".tscn";
		const string objFileEnding = ".obj";
		const string gltfFileEnding = ".gltf";
		const string glbFileEnding = ".glb";
		const string fbxFileEnding = ".fbx";
		const string colladaFileEnding = ".dae";
		const string blendFileEnding = ".blend";

		string[] validEndings =
		{
			sceneFileEnding, objFileEnding, gltfFileEnding, glbFileEnding, fbxFileEnding, colladaFileEnding, blendFileEnding, resFileEnding, tresFileEnding
		};

		// Check File Ending
		if (validEndings.Any(filePath.EndsWith))
		{
			var res = ResourceLoader.Load(filePath);
			if (res is PackedScene or Mesh) return true;
		}

		return false;
	}

	private void OnAddNewAsset(string[] assetPaths)
	{
		List<Asset3DData> validAssets = new();
		foreach (var assetPath in assetPaths)
		{
			validAssets.Add(new Asset3DData(assetPath, Asset3DData.PreviewPerspective.Default));
		}
		OnAddAssetData(validAssets);
	}
	
	private void OnAddAssetData(IEnumerable<Asset3DData> assets)
	{
		List<Asset3DData> validAssets = new();
		foreach (var asset in assets)
		{
			if (_currentLibrary == null || !_assetPathsByLibrary.ContainsKey(_currentLibrary)) OnNewAssetLibrary();
			Debug.Assert(_currentLibrary != null, nameof(_currentLibrary) + " != null");
			if (!CurrentLibraryData.ContainsAsset(asset.path))
			{
				if (IsValid3DFile(asset.path))
				{
					CurrentLibraryData.assetData.Add(asset);
					CurrentLibraryData.dirty = true;
					validAssets.Add(asset);
					UpdateSaveDisabled();
				}
				else
				{
					GD.PrintErr($"{nameof(AssetPlacerPlugin)}: {asset.path} is either not a valid 3D asset, or might have been moved or deleted");
				}
			}
		}
		UpdateAssets(validAssets, CurrentLibraryData.previewPerspective);
	}

	private void OnRemoveAsset(string[] paths)
	{
		foreach (var path in paths)
		{
			if (CurrentLibraryData.ContainsAsset(path))
			{
				CurrentLibraryData.RemoveAsset(path);
				CurrentLibraryData.dirty = true;
				UpdateSaveDisabled();
			}
		}
		_assetPlacerUi.RemoveAssets(paths);
	}

	private void UpdateAssets(IEnumerable<Asset3DData> newAssets, Asset3DData.PreviewPerspective perspective = Asset3DData.PreviewPerspective.Default, int scrollPosition = 0)
	{
		if (_currentLibrary == null) return;
		if (!_assetPathsByLibrary.ContainsKey(_currentLibrary)) //e.g. "[Empty]" 
		{
			DeselectAsset();
			_assetPlacerUi.UpdateAllAssets(new List<Asset3DData>());
			return;
		}
		if (newAssets == null)
		{
			DeselectAsset();
			_assetPlacerUi.UpdateAllAssets(CurrentLibraryData.assetData, scrollPosition);
			newAssets = CurrentLibraryData.assetData;
		}
		else
		{
			_assetPlacerUi.AddAssets(newAssets, scrollPosition);
		}
		AssetPlacerPersistence.StoreGlobalData(LastSelectedLibrarySaveKey, CurrentLibraryData.savePath);
		GeneratePreviews(newAssets, false, perspective);
	}

	private void GeneratePreviews(IEnumerable<Asset3DData> libraryData, bool forceReload, Asset3DData.PreviewPerspective libraryPerspective = Asset3DData.PreviewPerspective.Default)
	{
		EditorResourcePreview resourcePreviewer = _editorInterface.GetResourcePreviewer();
		var thumbnailSize = Vector2I.One * _editorInterface.GetEditorSettings()
			.GetSetting("filesystem/file_dialog/thumbnail_size").AsInt32();
		foreach (Asset3DData asset in libraryData)
		{
			if (PreviewGenerator.HandlesFile(asset.path))
			{
				var perspective = asset.previewPerspective != Asset3DData.PreviewPerspective.Default ? asset.previewPerspective : libraryPerspective;
				PreviewGenerator._GenerateFromPath(asset.path, thumbnailSize, new Callable(this, MethodName.OnPreviewLoaded), forceReload, _assetPlacerUi.GetBrokenTexture(), perspective);
			}
			else
			{
				resourcePreviewer.QueueResourcePreview(asset.path, _assetPlacerUi,
					nameof(_assetPlacerUi.UpdateAssetPreview), new Variant());
			}
		}
	}

	public void OnPreviewLoaded(string assetPath, Texture2D preview)
	{
		_assetPlacerUi.UpdateAssetPreview(assetPath, preview, preview, new Variant());
	}

	private Node3D CreateHologram()
	{
		var asset = ResourceLoader.Load<Resource>(_selectedAsset.path);

		if (asset is PackedScene scene)
		{
			return scene.Instantiate<Node3D>();
		}
		if (asset is Mesh mesh)
		{
			MeshInstance3D hologram = new MeshInstance3D();
			hologram.Mesh = mesh;
			return hologram;
		}
		
		GD.PrintErr($"{nameof(AssetPlacerPlugin)}: Can not instantiate asset at {_selectedAsset}: File might have been deleted or removed or resource is not a Packed Scene");
		_assetPlacerUi.MarkAssetAsBroken(_selectedAsset.path);
		return null;
	}
	
	public void ClearHologram()
	{
		Hologram?.QueueFree();
		Hologram = null;
	}

	public void SetAssetTransformDataFromHologram() // when added to tree
	{
		if (_selectedAsset == null || Hologram == null || !Hologram.IsInsideTree()) return;
		if (!_selectedAsset.hologramInstantiated)
		{
			_selectedAsset.hologramInstantiated = true;
			_selectedAsset.defaultTransform = Hologram.GlobalTransform;
			SaveTransform();
		}
		else
		{
			var holoTransform = Hologram.GlobalTransform;
			holoTransform.Basis = _selectedAsset.lastTransform.Basis;
			Hologram.GlobalTransform = holoTransform;
		}
	}
	
	public void DeselectAsset()
	{
		_selectedAsset = null;
		SelectedAssetName = null;
		ClearHologram();
		_assetPlacerUi.MarkButtonAsDeselected();
	}

	public bool IsAssetSelected()
	{
		return _selectedAsset != null;
	}

	public void TrySelectPreviousAsset()
	{
		_assetPlacerUi.SelectAsset(_lastSelectedAssetPath);
	}

	public void SaveTransform()
	{
		_selectedAsset.lastTransform = Hologram.GlobalTransform;
		UpdateResetTransformButton(_selectedAsset);
	}

	public void UpdateResetTransformButton(Asset3DData data)
	{
		_assetPlacerUi.SetResetTransformButtonVisible(data.path, data.lastTransform != data.defaultTransform);
	}

	public Node3D CreateInstance()
	{
		Debug.Assert(Hologram != null, "Trying to create an instance without a hologram");
		LastPlacedAsset = Hologram.Duplicate() as Node3D;
		Hologram.GetParent().AddChild(LastPlacedAsset);
		return LastPlacedAsset;
	}

	private string GetAvailableLibraryName(string desiredName)
	{
		if (desiredName == AssetPlacerUi.EmptyTabTitle) desiredName = "Empty";
		var name = desiredName;
		var i = 1;
		while (_assetPathsByLibrary.Keys.Any(x => x == name))
		{
			name = $"{desiredName} ({i})";
			i++;
		}
		return name;
	}

	private string GetFolderPathFromFilePath(string path)
	{
		var idx = path.LastIndexOf('/');

		var folder = idx >=0 ? path.Substring(0, idx) : "";
		if (path.Substring(0, idx+1).EndsWith("//")) return path.Substring(0, idx+1); //root folder
		return folder;
	}
	
	private string GetFileNameFromFilePath(string path)
	{
		var fileNameFull = path.GetFile();
		return fileNameFull.Substring(0, fileNameFull.Length - 5);
	}

	public void ResetHologramTransform()
	{
		if (_selectedAsset != null && Hologram != null)
		{
			var holoTransform = Hologram.GlobalTransform;
			holoTransform.Basis = _selectedAsset.defaultTransform.Basis;
			Hologram.GlobalTransform = holoTransform;
			SaveTransform();
		}
	}
}

#endif
