using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HexasphereGrid {
    [CustomEditor(typeof(Hexasphere)), CanEditMultipleObjects]
    public class HexasphereEditor : Editor {

        GUIStyle titleLabelStyle, blackBack;
        Color titleColor;

        int tileHighlightedIndex = -1, tileTextureIndex;
        List<int> tileSelectedIndices;
        Color colorSelection, tileColor;
        int textureMode, tileTagInt;
        string tileTag;
        static GUIStyle toggleButtonStyleNormal = null;
        static GUIStyle toggleButtonStyleToggled = null;
        Hexasphere hexa;
        int divisions;
        StringBuilder sb;

        SerializedProperty style, numDivisions, smartEdges, transparent, transparencyTiles, transparencyZWrite, transparencyDoubleSided, transparencyCull, invertedMode, extruded, extrudeMultiplier, bevel, gradientIntensity, raycast3D, wireframeColor, wireframeColorFromTile, wireframeIntensity;
        SerializedProperty defaultShadedColor, tileTintColor, castShadows, receiveShadows, tileTextureSize, rotationShift, vrEnabled;
        SerializedProperty lighting, ambientColor, minimumLight, specularTint, smoothness;
        SerializedProperty highlightEnabled, highlightColor, highlightSpeed, pathFindingFormula, pathFindingSearchLimit, pathFindingUseExtrusion, pathFindingExtrusionWeight;
        SerializedProperty rotationEnabled, rotationSpeed, rotationAxisAllowed, rotationAxisVerticalThreshold, zoomEnabled, zoomSpeed, zoomDamping, zoomMinDistance, zoomMaxDistance;
        SerializedProperty rightClickRotates, rightClickRotatingClockwise, rightButtonDrag, dragThreshold, clickDuration;
        SerializedProperty cameraMain, enableGridEditor, respectOtherUI;

        Texture2D _headerTexture;

        void OnEnable() {
            titleColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.12f, 0.16f, 0.4f);
            style = serializedObject.FindProperty("_style");
            numDivisions = serializedObject.FindProperty("_numDivisions");
            smartEdges = serializedObject.FindProperty("_smartEdges");
            transparent = serializedObject.FindProperty("_transparent");
            transparencyTiles = serializedObject.FindProperty("_transparencyTiles");
            transparencyCull = serializedObject.FindProperty("_transparencyCull");
            transparencyZWrite = serializedObject.FindProperty("_transparencyZWrite");
            transparencyDoubleSided = serializedObject.FindProperty("_transparencyDoubleSided");
            invertedMode = serializedObject.FindProperty("_invertedMode");
            extruded = serializedObject.FindProperty("_extruded");
            extrudeMultiplier = serializedObject.FindProperty("_extrudeMultiplier");
            bevel = serializedObject.FindProperty("_bevel");
            gradientIntensity = serializedObject.FindProperty("_gradientIntensity");
            raycast3D = serializedObject.FindProperty("_raycast3D");
            tileTextureSize = serializedObject.FindProperty("_tileTextureSize");
            rotationShift = serializedObject.FindProperty("_rotationShift");
            vrEnabled = serializedObject.FindProperty("_VREnabled");
            enableGridEditor = serializedObject.FindProperty("_enableGridEditor");

            wireframeColor = serializedObject.FindProperty("_wireframeColor");
            wireframeColorFromTile = serializedObject.FindProperty("_wireframeColorFromTile");
            wireframeIntensity = serializedObject.FindProperty("_wireframeIntensity");
            defaultShadedColor = serializedObject.FindProperty("_defaultShadedColor");
            tileTintColor = serializedObject.FindProperty("_tileTintColor");
            lighting = serializedObject.FindProperty("_lighting");
            ambientColor = serializedObject.FindProperty("_ambientColor");
            minimumLight = serializedObject.FindProperty("_minimumLight");
            specularTint = serializedObject.FindProperty("_specularTint");
            smoothness = serializedObject.FindProperty("_smoothness");
            castShadows = serializedObject.FindProperty("_castShadows");
            receiveShadows = serializedObject.FindProperty("_receiveShadows");

            cameraMain = serializedObject.FindProperty("_cameraMain");
            respectOtherUI = serializedObject.FindProperty("_respectOtherUI");
            highlightEnabled = serializedObject.FindProperty("_highlightEnabled");
            highlightColor = serializedObject.FindProperty("_highlightColor");
            highlightSpeed = serializedObject.FindProperty("_highlightSpeed");
            rotationEnabled = serializedObject.FindProperty("_rotationEnabled");
            rotationSpeed = serializedObject.FindProperty("_rotationSpeed");
            rotationAxisAllowed = serializedObject.FindProperty("_rotationAxisAllowed");
            rotationAxisVerticalThreshold = serializedObject.FindProperty("_rotationAxisVerticalThreshold");
            rightClickRotates = serializedObject.FindProperty("_rightClickRotates");
            rightClickRotatingClockwise = serializedObject.FindProperty("_rightClickRotatingClockwise");
            rightButtonDrag = serializedObject.FindProperty("_rightButtonDrag");
            dragThreshold = serializedObject.FindProperty("_dragThreshold");
            clickDuration = serializedObject.FindProperty("_clickDuration");
            zoomEnabled = serializedObject.FindProperty("_zoomEnabled");
            zoomSpeed = serializedObject.FindProperty("_zoomSpeed");
            zoomDamping = serializedObject.FindProperty("_zoomDamping");
            zoomMinDistance = serializedObject.FindProperty("_zoomMinDistance");
            zoomMaxDistance = serializedObject.FindProperty("_zoomMaxDistance");
            pathFindingFormula = serializedObject.FindProperty("_pathFindingHeuristicFormula");
            pathFindingSearchLimit = serializedObject.FindProperty("_pathFindingSearchLimit");
            pathFindingUseExtrusion = serializedObject.FindProperty("_pathFindingUseExtrusion");
            pathFindingExtrusionWeight = serializedObject.FindProperty("_pathFindingExtrusionWeight");

            _headerTexture = Resources.Load<Texture2D>("HexasphereEditorHeader");
            blackBack = new GUIStyle();
            blackBack.normal.background = MakeTex(4, 4, Color.black);

            sb = new StringBuilder();
            hexa = (Hexasphere)target;
            if (hexa.tiles == null) {
                hexa.Init();
            }
            divisions = hexa.numDivisions;
            colorSelection = new Color(1, 1, 0.5f, 0.85f);
            tileColor = Color.white;
            tileSelectedIndices = new List<int>();

            HideEditorMesh();
        }

        public override void OnInspectorGUI() {
#if UNITY_5_6_OR_NEWER
            serializedObject.UpdateIfRequiredOrScript();
#else
			serializedObject.UpdateIfDirtyOrScript ();
#endif

            if (titleLabelStyle == null) {
                titleLabelStyle = new GUIStyle(EditorStyles.label);
            }
            titleLabelStyle.normal.textColor = titleColor;
            titleLabelStyle.fontStyle = FontStyle.Bold;


            EditorGUILayout.Separator();

            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.BeginHorizontal(blackBack);
            GUILayout.Label(_headerTexture, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hexasphere Settings", titleLabelStyle);
            if (GUILayout.Button("Help", GUILayout.Width(40))) {
                if (!EditorUtility.DisplayDialog("Hexasphere Grid System", "To learn more about a property in this inspector move the mouse over the label for a quick description (tooltip).\n\nPlease check README file in the root of the asset for details and contact support.\n\nIf you like Hexasphere Grid System, please rate it on the Asset Store. For feedback and suggestions visit our support forum on kronnect.com.", "Close", "Visit Support Forum")) {
                    Application.OpenURL("http://kronnect.com/taptapgo");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(style, new GUIContent("Style", "Style for the hexasphere."));

            EditorGUILayout.BeginHorizontal();
            divisions = EditorGUILayout.IntSlider(new GUIContent("Divisions", "Number of divisions during the generation of the hexasphere."), divisions, 1, 200);
            if (GUILayout.Button("Set")) {
                numDivisions.intValue = divisions;
            }
            EditorGUILayout.EndHorizontal();


            if (hexa != null) {
                EditorGUILayout.LabelField("Tile Count", hexa.tiles.Length.ToString());
            }

            GUI.enabled = style.intValue == (int)STYLE.ShadedWireframe || style.intValue == (int)STYLE.Wireframe;
            EditorGUILayout.PropertyField(smartEdges, new GUIContent("Smart Edges", "Only renders edges between two tiles with different materials."));
            GUI.enabled = true;
            EditorGUILayout.PropertyField(transparent, new GUIContent("Transparent", "Enable transparency support."));
            if (transparent.boolValue) {
                EditorGUILayout.PropertyField(transparencyTiles, new GUIContent("   Tiles Alpha", "Global transparency for tiles."));
                EditorGUILayout.PropertyField(transparencyZWrite, new GUIContent("   ZWrite", "Enable writing to z-buffer even in transparent mode."));
                EditorGUILayout.PropertyField(transparencyDoubleSided, new GUIContent("   Double Sided", "Disabled back face culling."));
            }
            EditorGUILayout.PropertyField(invertedMode, new GUIContent("Inverted Mode", "Renders the hexasphere inwards, making the camera stay at center of the sphere."));
            if (invertedMode.boolValue)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(extruded, new GUIContent("Extruded", "Enable to allow extrusion of tiles."));
            if (extruded.boolValue) {
                EditorGUILayout.PropertyField(extrudeMultiplier, new GUIContent("   Multiplier", "Global extrusion multiplier."));
                EditorGUILayout.PropertyField(bevel, new GUIContent("   Bevel", "Apply a bevel effect."));
                EditorGUILayout.PropertyField(transparencyCull, new GUIContent("   Cull Back Tiles", "Prevents rendering of back side tiles."));
                EditorGUILayout.PropertyField(gradientIntensity, new GUIContent("   Gradient Intensity", "Intensity of the color gradient effect."));
                EditorGUILayout.PropertyField(raycast3D, new GUIContent("   Raycast 3D", "Improves precision of tile selection when extrusion is enabled."));
            }
            GUI.enabled = true;
            EditorGUILayout.PropertyField(wireframeColor, new GUIContent("Wireframe Color", "Color for the wireframe."));
            if (extruded.boolValue) {
                EditorGUILayout.PropertyField(wireframeColorFromTile, new GUIContent("   Color From Tile", "Use tile color as a base color for the wireframe."));
                EditorGUILayout.PropertyField(wireframeIntensity, new GUIContent("   Intensity", "Darkens or lightens the wireframe."));
            }

            EditorGUILayout.PropertyField(defaultShadedColor, new GUIContent("Default Tile Color", "Default color for the tiles that are not colored or textured by user."));
            EditorGUILayout.PropertyField(tileTintColor, new GUIContent("Tile Tint Color", "Tint color applied to all tiles, either colored or non-colored tiles."));
            EditorGUILayout.PropertyField(lighting, new GUIContent("Use Lighting", "If the hexasphere geometry can cast shadows over itself or other geometry, and also be influenced by the directional light."));
            EditorGUILayout.PropertyField(ambientColor, new GUIContent("Ambient Color", "Ambient color is added to the final tile color."));
            EditorGUILayout.PropertyField(minimumLight, new GUIContent("Minimum Light", "Minimum lighting applied to all tiles."));
            if (!extruded.boolValue) {
                EditorGUILayout.PropertyField(specularTint, new GUIContent("Specular Tint", "Color for the specular lighting."));
                EditorGUILayout.PropertyField(smoothness, new GUIContent("Smoothness", "Surface smoothness whith is applied to the specular lighting."));
            }
            EditorGUILayout.PropertyField(castShadows, new GUIContent("Cast Shadows", "If hexasphere can cast shadows."));
            EditorGUILayout.PropertyField(receiveShadows, new GUIContent("Receive Shadows", "If hexasphere can receive shadows."));
            EditorGUILayout.PropertyField(tileTextureSize, new GUIContent("Tile Texture Size", "Textures assigned to tiles will be rescaled to this size if different. Note that textures should be marked as readable."));
            EditorGUILayout.PropertyField(rotationShift, new GUIContent("Rotation Shift", "Applies an internal rotation to the generated vertices. Let's you control where the pentagons will be located."));
            EditorGUILayout.PropertyField(vrEnabled, new GUIContent("VR Enabled", "Uses VR-compatible raycasting."));
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Interaction Settings", titleLabelStyle);
            EditorGUILayout.PropertyField(cameraMain, new GUIContent("Camera", "Camera used for interaction."));
            EditorGUILayout.PropertyField(respectOtherUI, new GUIContent("Respect Other UI", "Prevents interaction with hexasphere when an UI element is under pointer."));
            EditorGUILayout.PropertyField(highlightEnabled, new GUIContent("Enable Highlight", "Enables or disables selection of tiles."));
            EditorGUILayout.PropertyField(highlightColor, new GUIContent("Highlight Color", "Main tint color for the highlighted tile."));
            EditorGUILayout.PropertyField(highlightSpeed, new GUIContent("   Highlight Speed", "Speed for the flashing animation."));
            EditorGUILayout.PropertyField(rotationEnabled, new GUIContent("Enable Rotation", "Enables or disables rotation of hexasphere by user drag."));
            EditorGUILayout.PropertyField(rotationSpeed, new GUIContent("   Rotation Speed", "Speed for the rotation."));
            EditorGUILayout.PropertyField(rotationAxisAllowed, new GUIContent("   Rotation Axis", "Allowed rotation axis."));
            if (rotationAxisAllowed.intValue == (int)ROTATION_AXIS_ALLOWED.STRAIGHT) {
                EditorGUILayout.PropertyField(rotationAxisVerticalThreshold, new GUIContent("   Min Pole Distance", "Allowed minimum distance to North or South Pole."));
            }
            EditorGUILayout.PropertyField(rightButtonDrag, new GUIContent("Right Button Drag", "If set to true, user can hold and drag the hexasphere using the right mouse button."));
            if (rightButtonDrag.boolValue)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(rightClickRotates, new GUIContent("Right Click Rotates", "Enables or disables rotation of hexasphere by pressing mouse right button."));
            EditorGUILayout.PropertyField(rightClickRotatingClockwise, new GUIContent("   Clockwise Rotation", "Direction of the rotation."));
            GUI.enabled = true;
            EditorGUILayout.PropertyField(dragThreshold, new GUIContent("Drag Threshold", "Minimum angle rotation to consider dragging has occured."));
            EditorGUILayout.PropertyField(clickDuration, new GUIContent("Click Duration", "Maximum time between button press and release to account for a click."));
            EditorGUILayout.PropertyField(zoomEnabled, new GUIContent("Enable Zoom", "Enables or disables zoom of hexasphere by using mouse wheel or pinch in/out."));
            EditorGUILayout.PropertyField(zoomSpeed, new GUIContent("   Zoom Speed", "Speed for the zoom in/out."));
            EditorGUILayout.PropertyField(zoomDamping, new GUIContent("   Zoom Damping", "Speed for decelerating zoom once wheel has been released."));
            EditorGUILayout.PropertyField(zoomMinDistance, new GUIContent("   Min Distance", "Minimum distance to the hexasphere when zooming in. This is a factor of the radius."));
            EditorGUILayout.PropertyField(zoomMaxDistance, new GUIContent("   Max Distance", "Maximum distance to the hexasphere when zooming out. This is a factor of the radius."));
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Path Finding Settings", titleLabelStyle);
            EditorGUILayout.PropertyField(pathFindingFormula, new GUIContent("Estimation Method", "The estimation method used for getting the path between two tiles."));
            EditorGUILayout.PropertyField(pathFindingSearchLimit, new GUIContent("Search Limit", "Maximum path length."));
            EditorGUILayout.PropertyField(pathFindingUseExtrusion, new GUIContent("Use Extrusion", "If path-finding should use tiles' extrusion (altitude) value as part of their crossing cost."));
            if (pathFindingUseExtrusion.boolValue) {
                EditorGUILayout.PropertyField(pathFindingExtrusionWeight, new GUIContent("   Extrusion Weight", "Weight for the extrusion of each tile. The extrusion value (0..1) is multiplied by this value then added to the tile crossing cost."));
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Tools", titleLabelStyle);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Wireframe")) {
                if (EditorUtility.DisplayDialog("Create Asset", "Current hexasphere wireframe mesh will be exported to project root.", "Ok", "Cancel")) {
                    Transform t = hexa.transform.Find("WireFrame/Wire");
                    if (t != null) {
                        MeshFilter mf = t.GetComponent<MeshFilter>();
                        if (mf != null) {
                            SaveMeshAsset(mf.sharedMesh);
                        }
                    }
                }
            }
            if (GUILayout.Button("Export Model")) {
                if (EditorUtility.DisplayDialog("Create Asset", "Current hexasphere shaded model will be exported to project root.", "Ok", "Cancel")) {
                    Transform t = hexa.transform.Find("ShadedFrame/Shade");
                    if (t != null) {
                        MeshFilter mf = t.GetComponent<MeshFilter>();
                        if (mf != null) {
                            SaveMeshAsset(mf.sharedMesh);
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            if (!Application.isPlaying) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Grid Editor", titleLabelStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Export Config")) {
                    if (EditorUtility.DisplayDialog("Export Grid Settings", "This option will add a Hexasphere Config component to this game object with current tile settings. You can restore this configuration just enabling this new component.", "Ok", "Cancel")) {
                        CreatePlaceholder();
                    }
                }
                if (GUILayout.Button("Reset Tiles")) {
                    if (EditorUtility.DisplayDialog("Reset Grid", "Reset tiles to their default values?", "Ok", "Cancel")) {
                        ResetTiles();
                        GUIUtility.ExitGUI();
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(enableGridEditor, new GUIContent("Enable Editor", "Enables grid editing options in Scene View"));
                EditorGUILayout.EndHorizontal();

                if (enableGridEditor.boolValue) {
                    int selectedCount = tileSelectedIndices.Count;
                    if (targets.Length > 1) {
                        GUILayout.Label("Grid Editor only works with one hexasphere at a time.");
                    } else if (selectedCount == 0) {
                        GUILayout.Label("Click on a tile in Scene View to edit its properties\n(hold Control to select multiple tiles).");
                    } else {
                        // Check that all selected tiles are within range
                        for (int k = 0; k < selectedCount; k++) {
                            if (tileSelectedIndices[k] < 0 || tileSelectedIndices[k] >= hexa.tiles.Length) {
                                tileSelectedIndices.Clear();
                                GUIUtility.ExitGUI();
                                return;
                            }
                        }
                        int tileSelectedIndex = tileSelectedIndices[0];

                        EditorGUILayout.BeginHorizontal();
                        if (selectedCount == 1) {
                            GUILayout.Label("Selected Cell", GUILayout.Width(120));
                            GUILayout.Label(tileSelectedIndex.ToString(), GUILayout.Width(120));
                        } else {
                            GUILayout.Label("Selected Cells", GUILayout.Width(120));
                            sb.Length = 0;
                            for (int k = 0; k < selectedCount; k++) {
                                if (k > 0) {
                                    sb.Append(", ");
                                }
                                sb.Append(tileSelectedIndices[k].ToString());
                            }
                            GUILayout.TextArea(sb.ToString(), GUILayout.ExpandHeight(true));
                        }
                        EditorGUILayout.EndHorizontal();
                        Tile selectedTile = hexa.tiles[tileSelectedIndex];

                        if (selectedCount == 1) {

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("   String Tag", GUILayout.Width(120));
                            tileTag = EditorGUILayout.TextField(tileTag);
                            if (tileTag == selectedTile.tag || (string.IsNullOrEmpty(tileTag) && string.IsNullOrEmpty(selectedTile.tag)))
                                GUI.enabled = false;
                            if (GUILayout.Button("Set Tag", GUILayout.Width(60))) {
                                hexa.SetTileTag(tileSelectedIndex, tileTag);
                            }
                            GUI.enabled = true;
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("   Integer Tag", GUILayout.Width(120));
                            tileTagInt = EditorGUILayout.IntField(tileTagInt, GUILayout.Width(60));
                            if (tileTagInt == selectedTile.tagInt)
                                GUI.enabled = false;
                            if (GUILayout.Button("Set Tag", GUILayout.Width(60))) {
                                hexa.SetTileTag(tileSelectedIndex, tileTagInt);
                            }
                            GUI.enabled = true;
                            EditorGUILayout.EndHorizontal();
                        }

                        bool needsRedraw = false;

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("   Color", GUILayout.Width(120));
                        tileColor = EditorGUILayout.ColorField(tileColor, GUILayout.Width(40));
                        GUILayout.Label("  Texture", GUILayout.Width(60));
                        tileTextureIndex = EditorGUILayout.IntField(tileTextureIndex, GUILayout.Width(40));
                        if (hexa.GetTileColor(tileSelectedIndex, true) == tileColor && hexa.GetTileTextureIndex(tileSelectedIndex) == tileTextureIndex)
                            GUI.enabled = false;
                        if (GUILayout.Button(new GUIContent("Set", "Press ALT+S to quick set"), GUILayout.Width(50))) {
                            for (int k = 0; k < selectedCount; k++) {
                                hexa.SetTileTexture(tileSelectedIndices[k], tileTextureIndex, tileColor, false);
                            }
                            needsRedraw = true;
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button(new GUIContent("Clear", "Press ALT+C to quick clear"), GUILayout.Width(50))) {
                            for (int k = 0; k < selectedCount; k++) {
                                hexa.ClearTile(tileSelectedIndices[k]);
                            }
                            needsRedraw = true;
                        }
                        EditorGUILayout.EndHorizontal();

                        if (needsRedraw) {
                            RefreshGrid();
                            GUIUtility.ExitGUI();
                            return;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Textures", GUILayout.Width(120));
                    EditorGUILayout.EndHorizontal();

                    if (toggleButtonStyleNormal == null) {
                        toggleButtonStyleNormal = "Button";
                        toggleButtonStyleToggled = new GUIStyle(toggleButtonStyleNormal);
                        toggleButtonStyleToggled.normal.background = toggleButtonStyleToggled.active.background;
                    }

                    int textureMax = hexa.textures.Length - 1;
                    while (textureMax >= 1 && hexa.textures[textureMax] == null) {
                        textureMax--;
                    }
                    textureMax++;
                    if (textureMax >= hexa.textures.Length)
                        textureMax = hexa.textures.Length - 1;

                    for (int k = 1; k <= textureMax; k++) {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("  " + k.ToString(), GUILayout.Width(40));
                        hexa.textures[k] = (Texture2D)EditorGUILayout.ObjectField(hexa.textures[k], typeof(Texture2D), false);
                        if (hexa.textures[k] != null) {
                            if (GUILayout.Button(new GUIContent("T", "Texture mode - if enabled, you can paint several tiles just clicking over them."), textureMode == k ? toggleButtonStyleToggled : toggleButtonStyleNormal, GUILayout.Width(20))) {
                                textureMode = textureMode == k ? 0 : k;
                            }
                            if (GUILayout.Button(new GUIContent("X", "Remove texture"), GUILayout.Width(20))) {
                                if (EditorUtility.DisplayDialog("Remove texture", "Are you sure you want to remove this texture?", "Yes", "No")) {
                                    hexa.textures[k] = null;
                                    GUIUtility.ExitGUI();
                                    return;
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

            }
            EditorGUILayout.Separator();



            if (serializedObject.ApplyModifiedProperties() || (Event.current.type == EventType.ExecuteCommand &&
                Event.current.commandName == "UndoRedoPerformed")) {
                foreach (Hexasphere hex in targets) {
                    hex.UpdateMaterialProperties();
                }
                HideEditorMesh();
                SceneView.RepaintAll();
            }
        }

        void OnSceneGUI() {
            if (hexa == null || Application.isPlaying || !hexa.enableGridEditor)
                return;
            Event e = Event.current;
            if (e.type == EventType.Layout)
                return;

            bool gridHit = hexa.CheckRay(HandleUtility.GUIPointToWorldRay(e.mousePosition));

            if (tileHighlightedIndex != hexa.lastHighlightedTileIndex) {
                tileHighlightedIndex = hexa.lastHighlightedTileIndex;
                SceneView.RepaintAll();
            }

            int count = tileSelectedIndices.Count;
            for (int k = 0; k < count; k++) {
                int idx = tileSelectedIndices[k];
                Vector3 pos = hexa.GetTileCenter(idx);
                Handles.color = colorSelection;
                Handles.DrawSolidDisc(pos, hexa.tiles[idx].center, HandleUtility.GetHandleSize(pos) * 0.075f);
            }

            if (tileHighlightedIndex < 0)
                return;

            bool redraw = false;

            if ((e.type == EventType.MouseDown && e.isMouse && e.button == 0) || e.shift) {
                if (gridHit && e.type == EventType.MouseDown)
                    e.Use();
                if (!e.shift && tileSelectedIndices.Contains(tileHighlightedIndex)) {
                    tileSelectedIndices.Remove(tileHighlightedIndex);
                } else {
                    if (!e.shift && !e.control) {
                        tileSelectedIndices.Clear();
                    }
                    if (!tileSelectedIndices.Contains(tileHighlightedIndex)) { 
                        tileSelectedIndices.Add(tileHighlightedIndex);
                    }

                    if (textureMode > 0) {
                        hexa.SetTileTexture(tileHighlightedIndex, textureMode, Color.white);
                        redraw = true;
                    }
                    if (!e.shift) {
                        tileColor = hexa.GetTileColor(tileHighlightedIndex);
                        if (tileColor.a == 0)
                            tileColor = Color.white;
                        tileTextureIndex = hexa.GetTileTextureIndex(tileHighlightedIndex);
                    }
                    tileTag = hexa.GetTileTag(tileHighlightedIndex);
                    tileTagInt = hexa.GetTileTagInt(tileHighlightedIndex);
                    EditorUtility.SetDirty(target);
                }
            }

            if (e.shift) {
                if (e.keyCode == KeyCode.S) {
                    if (tileTextureIndex == 0) {
                        hexa.SetTileColor(tileHighlightedIndex, tileColor);
                    } else {
                        hexa.SetTileTexture(tileHighlightedIndex, tileTextureIndex, tileColor, false);
                    }
                    redraw = true;
                    e.Use();
                } else if (e.keyCode == KeyCode.C) {
                    hexa.ClearTile(tileHighlightedIndex);
                    redraw = true;
                    e.Use();
                }
            }

            if (redraw)
                SceneView.RepaintAll();

            if (gridHit) {
                Selection.activeGameObject = hexa.transform.gameObject;
            }
        }

        #region Utility functions

        Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            TextureFormat tf = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat) ? TextureFormat.RGBAFloat : TextureFormat.RGBA32;
            Texture2D result = new Texture2D(width, height, tf, false);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        void SaveMeshAsset(Mesh mesh) {
            string path = "";
            for (int k = 0; k < 10000; k++) {
                path = "Assets/hexasphere" + k.ToString() + ".asset";
                if (!File.Exists(path))
                    break;
            }
            mesh = Instantiate<Mesh>(mesh);
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void ResetTiles() {
            tileSelectedIndices.Clear();
            tileColor = Color.white;
            hexa.ClearTiles();
            RefreshGrid();
        }

        void RefreshGrid() {
            HideEditorMesh();
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();
        }

        void CreatePlaceholder() {
            HexasphereConfig configComponent = hexa.gameObject.AddComponent<HexasphereConfig>();
            configComponent.textures = hexa.textures;
            configComponent.config = hexa.GetTilesConfigurationData();
            configComponent.enabled = false;
        }

        void HideEditorMesh() {
            Renderer[] rr = hexa.GetComponentsInChildren<Renderer>(true);
            for (int k = 0; k < rr.Length; k++) {
#if UNITY_5_5_OR_NEWER
                EditorUtility.SetSelectedRenderState(rr[k], EditorSelectedRenderState.Hidden);
#else
				EditorUtility.SetSelectedWireframeHidden(rr[k], true);
#endif
            }
        }

        #endregion

        [MenuItem("GameObject/3D Object/Hexasphere", false)]
        static void CreateHexasphereMenuOption(MenuCommand menuCommand) {
            // Create a custom game object
            GameObject go = new GameObject("Hexasphere");
            go.name = "Hexasphere";
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            if (Selection.activeTransform != null) {
                go.transform.SetParent(Selection.activeTransform, false);
                go.transform.localPosition = Misc.Vector3zero;
            }
            go.transform.localRotation = Quaternion.Euler(0, 0, 0);
            go.transform.localScale = new Vector3(1f, 1f, 1f);
            Selection.activeObject = go;
            go.AddComponent<Hexasphere>();
        }


    }

}
