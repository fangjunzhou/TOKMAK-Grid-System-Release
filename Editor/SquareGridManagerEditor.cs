using System;
using System.IO;
using FinTOKMAK.GridSystem.Square;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace FinTOKMAK.GridSystem.Editor
{
    [CustomEditor(typeof(SquareGridManager))]
    public class SquareGridManagerEditor : UnityEditor.Editor
    {
        #region Private Field

        /// <summary>
        /// The manager target of current custom editor.
        /// </summary>
        private SquareGridManager _manager;

        private GUIStyle _greenLabel;

        private GUIStyle _redLabel;

        private bool _foldInit;

        #endregion

        #region Serialized Properties

        private SerializedProperty _mapSizeProperty;

        private SerializedProperty _mapFilePathProperty;

        #endregion

        private void OnEnable()
        {
            // Init GUI Style
            _greenLabel = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    textColor = Color.green
                }
            };
            
            _redLabel = new GUIStyle(EditorStyles.label)
            {
                normal =
                {
                    textColor = Color.red
                }
            };
            
            // Init serialized properties.
            _mapSizeProperty = serializedObject.FindProperty("mapSize");
            _mapFilePathProperty = serializedObject.FindProperty("mapFilePath");
        }

        public override void OnInspectorGUI()
        {
            _manager = (SquareGridManager) target;

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Grid Generator Editor Status: ", EditorStyles.boldLabel);
                if (!_manager.generator.editorMode)
                {
                    EditorGUILayout.LabelField("DISABLED", _redLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("ENABLED", _greenLabel);
                }
                EditorGUILayout.EndHorizontal();
                
                if (!_manager.generator.editorMode)
                {
                    EditorGUILayout.HelpBox("Generator not initialized in editor mode!", MessageType.Warning);
                    if (GUILayout.Button("Initialize Editor Mode"))
                    {
                        _manager.generator.EditorInitialize();
                    }
                    EditorGUILayout.EndVertical();
                    return;
                }
                
                if (GUILayout.Button("Close Editor Mode"))
                {
                    try
                    {
                        _manager.generator.ClearMap();
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                    _manager.generator.EditorTearDown();
                }
            }
            EditorGUILayout.EndVertical();
            
            _foldInit = EditorGUILayout.Foldout(_foldInit, "Manager Operation", EditorStyles.foldoutHeader);

            if (_foldInit)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.PropertyField(_mapSizeProperty, new GUIContent("Initialization Map Size"));
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        var vector2IntValue = _mapSizeProperty.vector2IntValue;
                        if (vector2IntValue.x < 0)
                        {
                            vector2IntValue.x = 0;
                        }
                        if (vector2IntValue.y < 0)
                        {
                            vector2IntValue.y = 0;
                        }
                        _mapSizeProperty.vector2IntValue = vector2IntValue;
                        
                        serializedObject.ApplyModifiedProperties();
                    }
            
                    if (GUILayout.Button("Generate Initialized Map"))
                    {
                        _manager.generator.ClearMap();
                        _manager.generator.GenerateMap<GridElement>(_manager.mapSize.x, _manager.mapSize.y, 1,
                            GridGenerationDirection.Horizontal);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Clear Map"))
            {
                _manager.generator.ClearMap();
            }

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Save/Open Map", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Current Map Path:");
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.SelectableLabel(_mapFilePathProperty.stringValue, EditorStyles.textField,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight),
                        GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Locate", GUILayout.Width(80)) && _mapFilePathProperty.stringValue != null)
                    {
                        string relativePath=  "Assets" + _mapFilePathProperty.stringValue.Substring(Application.dataPath.Length);
                        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(relativePath, typeof(UnityEngine.Object));
                        EditorGUIUtility.PingObject(obj);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Save"))
                    {
                        if (_mapFilePathProperty.stringValue != String.Empty)
                        {
                            // Save to exist file.
                            GridSystemSerializer.Serialize(_manager.generator.gridSystem, _mapFilePathProperty.stringValue);
                        }
                        else
                        {
                            // Save to new file.
                            string filePath =
                                EditorUtility.SaveFilePanel("Save Map", Application.dataPath, "Untitled_map", "map");
                            if (filePath != string.Empty)
                                GridSystemSerializer.Serialize(_manager.generator.gridSystem, filePath);
                            _mapFilePathProperty.stringValue = filePath;
                            serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        }
                        
                        return;
                    }
                    
                    if (GUILayout.Button("Save As"))
                    {
                        string filePath =
                            EditorUtility.SaveFilePanel("Save Map", Application.dataPath, "Untitled_map", "map");
                        if (filePath != string.Empty)
                            GridSystemSerializer.Serialize(_manager.generator.gridSystem, filePath);
                        
                        return;
                    }

                    if (GUILayout.Button("Open"))
                    {
                        _manager.generator.ClearMap();
                        
                        string filePath;
                        if (_mapFilePathProperty.stringValue != String.Empty)
                        {
                            // Read exist file.
                            filePath = _mapFilePathProperty.stringValue;
                        }
                        else
                        {
                            filePath = EditorUtility.OpenFilePanel("Open Map", Application.dataPath, "map");
                            if (filePath != string.Empty)
                            {
                                _mapFilePathProperty.stringValue = filePath;
                                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                            }
                        }
                        if (filePath != string.Empty)
                            _manager.generator.GenerateMap<GridElement>(filePath, GridGenerationDirection.Horizontal);
                        
                        return;
                    }

                    if (GUILayout.Button("Open New"))
                    {
                        _manager.generator.ClearMap();
                        string filePath;
                        if (_mapFilePathProperty.stringValue != String.Empty)
                        {
                            // Read exist file.
                            filePath = _mapFilePathProperty.stringValue;
                            string parentDir = Directory.GetParent(filePath)?.FullName;
                            filePath = EditorUtility.OpenFilePanel("Open Map", parentDir, "map");
                        }
                        else
                        {
                            filePath = EditorUtility.OpenFilePanel("Open Map", Application.dataPath, "map");
                            if (filePath != string.Empty)
                            {
                                _mapFilePathProperty.stringValue = filePath;
                                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                            }
                        }
                        if (filePath != string.Empty)
                            _manager.generator.GenerateMap<GridElement>(filePath, GridGenerationDirection.Horizontal);
                        
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}