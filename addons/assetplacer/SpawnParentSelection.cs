// SpawnParentSelection.cs
// Copyright (c) 2023 CookieBadger. All Rights Reserved.

#if TOOLS
#nullable disable
using System;
using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace AssetPlacer;

[Tool]
public partial class SpawnParentSelection : Node
{
    private const string SpawnParentPathSaveKey = "spawn_parent_path";
    private const string NullPath = "@#@null";
    
    private EditorInterface _editorInterface;
    private SpawnParentSelectionUi _spawnParentSelectionUi;
    private Node _spawnParent;
    private NodePath _spawnParentPath;
    private Node _sceneRoot;
    private bool _initialized = false;
    public Node SpawnParent
    {
        get { return _spawnParent;}
        private set => _spawnParent = value;
    }

    public void Init(EditorInterface editorInterface)
    {
        _editorInterface = editorInterface;
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        
        try
        {
            if (SpawnParent != null)
            {
                if (SpawnParent.IsInsideTree())
                {
                    var path = SpawnParent.GetTree().EditedSceneRoot.GetPathTo(SpawnParent);
                    if (!_spawnParentPath.Equals(path))
                    {
                        SetSpawnParent(SpawnParent);
                    }

                    if (path == "." && SpawnParent.Name != _spawnParentSelectionUi.spawnParentSelectionButton.Text)
                    {
                        SetSpawnParent(SpawnParent);
                    }
                } else if (_initialized)
                {
                    SetSpawnParent(null);
                }
            }
        }
        catch (ObjectDisposedException)
        {
            SpawnParent = null;
        }
    }

    public void SetUi(SpawnParentSelectionUi spawnParentSelectionUi)
    {
        _spawnParentSelectionUi = spawnParentSelectionUi;
        _spawnParentSelectionUi.spawnParentSelectionButton.NodeDropped += SetSpawnParent;
        _spawnParentSelectionUi.spawnParentSelectionButton.Pressed += SelectSpawnParent;
        _spawnParentSelectionUi._selectedAsSpawnNodeButton.Pressed += SetSelectedAsSpawnNode;
    }

    
    private void SetSelectedAsSpawnNode()
    {
        var selectedNodes = _editorInterface.GetSelection().GetSelectedNodes();
        if (selectedNodes.Count == 1 && selectedNodes[0] != SpawnParent)
        {
            SetSpawnParent(selectedNodes[0]);
            _spawnParentSelectionUi?.SetSelectedAsSpawnNodeButtonDisabled(true);
        }
    }
    
    private void SelectSpawnParent()
    {
        if (SpawnParent == null || !SpawnParent.IsInsideTree()) return;
        _editorInterface.GetSelection().Clear();
        _editorInterface.GetSelection().AddNode(SpawnParent);
    }
    
    
    public void SetSpawnParent(Node node)
    {
        SpawnParent = node;
        Texture2D spawnNodeIcon = null;
        if (node != null)
        {
            Debug.Assert(_sceneRoot != null && _sceneRoot.IsInsideTree());
            Debug.Assert(node.IsInsideTree());
            _spawnParentPath = _sceneRoot.GetPathTo(node);

            var nodeName = SpawnParent.GetClass().Split('.')[^1];
            spawnNodeIcon = _editorInterface.GetBaseControl().GetThemeIcon(nodeName, "EditorIcons");
        }
        else
        {
            _spawnParentPath = NullPath;
        }
        _spawnParentSelectionUi.SetSpawnNode(SpawnParent, spawnNodeIcon);
        AssetPlacerPersistence.StoreSceneData(SpawnParentPathSaveKey, _spawnParentPath);
    }

    // The scene has changed i.e., user has closed the scene, or opened a new one
    public void OnSceneChanged(Node newRoot)
    {
        // load the new spawn parent
        string path = null;
        if (newRoot != null)
        {
            path = AssetPlacerPersistence.LoadSceneData(SpawnParentPathSaveKey, new NodePath("."), Variant.Type.NodePath).AsNodePath();
        }
        _sceneRoot = newRoot;
        if(!_initialized) _initialized = _sceneRoot != null;
        if(_initialized) UpdateSpawnParentPath(path);
    }

    public void OnSelectionChanged(Array<Node> selectedNodes)
    {
        _spawnParentSelectionUi?.SetSelectedAsSpawnNodeButtonDisabled(selectedNodes.Count != 1 || selectedNodes[0] == SpawnParent);
    }
    
    // The scene root changed, i.e. a different node has been made the root, or the scene was changed
    public void SetSceneRoot(Node root)
    {
        _sceneRoot = root;
        if(!_initialized) _initialized = _sceneRoot != null;
        if(_initialized) UpdateSpawnParentPath(_spawnParentPath);
    }

    private void UpdateSpawnParentPath(string path)
    {
        if (_sceneRoot == null)
        {
            // no root in the scene -> no nodes
            SetSpawnParent(null);
            _spawnParentPath = null; // this is to prevent the path==NullPath, thus when a root will be added, this will be the new spawn node.
            return;
        }
        
        if (path == NullPath)
        {
            // path == NullPath: the old node could not be found anymore
            SetSpawnParent(null);
            return;
        }
        
        // Check if spawnNode is still in the scene
        try
        {
            if (SpawnParent != null && SpawnParent.IsInsideTree())
            {
                SetSpawnParent(SpawnParent); // Updates the path
                return;
            }
        }
        catch (ObjectDisposedException)
        {
            SpawnParent = null;
        }

        // Update spawn parent
        // Check if the path is valid
        var spawnNode = _sceneRoot.GetNodeOrNull(path);
        if (spawnNode != null)
        {
            SetSpawnParent(spawnNode);
            return;
        }
        
        // Root exists, a path was loaded, but it doesn't lead to a valid node and the current SpawnParent is invalid
        // e.g. when the spawn parent was the root and it was deleted (and the scene change is not triggered first)
        // e.g. when you have an empty scene and you add a root for the first time.
        SetSpawnParent(_sceneRoot);
    }
}

#endif