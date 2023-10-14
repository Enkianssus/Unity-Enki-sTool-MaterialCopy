using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VInspector.Libs;
using Object = UnityEngine.Object;

public class CopyMaterials : EditorWindow
{
    [SerializeField]
    public Renderer[] arrayCopy;
    [SerializeField]
    public Renderer[] arrayPaste;
    [SerializeField]
    public List<Renderer> missMatchedCopyList = new List<Renderer>();
    public List<Renderer> missMatchedPasteList = new List<Renderer>();
    private GameObject goCopy;
    private GameObject goPaste;
    private string tip="you can put MeshRenderer SkinMeshRenderer or the parent of them";
    private MessageType _messageType = MessageType.Info;
    private bool isMissMatched = false;
    Vector2 scrollPosition = Vector2.zero;
    
    [MenuItem("Tools/Enki/CopyMaterials")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CopyMaterials));
    }
    
    private Editor _editor;
    private void OnGUI()
    {
        Vector2 windowSize = new Vector2(position.width, position.height);
        GUILayout.Label("CopyMaterials",EditorStyles.boldLabel);
        goCopy = (GameObject)EditorGUILayout.ObjectField("Copy", goCopy,typeof(GameObject),true);
        goPaste = (GameObject)EditorGUILayout.ObjectField("Paste", goPaste,typeof(GameObject),true);
        if (GUILayout.Button("Copy"))
        {
            StartCopy();
        }
        Rect lastControlRect = GUILayoutUtility.GetLastRect();
        var otherControlHeight = lastControlRect.y + lastControlRect.height;
        EditorGUILayout.HelpBox(tip, _messageType);
        //GUILayout.Label(tip,EditorStyles.helpBox);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, true, true,  GUILayout.Width(windowSize.x),  GUILayout.Height(windowSize.y-otherControlHeight)); 
        if (!_editor) _editor = Editor.CreateEditor(this);
        else _editor.OnInspectorGUI();
        GUILayout.EndScrollView();
    }

    private void Clear()
    {
        missMatchedCopyList.Clear();
        missMatchedPasteList.Clear();
    }
    void StartCopy()
    {
        Clear();
        _messageType = MessageType.Info;
        if (goCopy == null)
        {
            tip = "Copy GameObject is null";
            _messageType = MessageType.Error;
            return;
        }

        if (goPaste == null)
        {
            tip = "Paste GameObject is null";
            _messageType = MessageType.Error;
            return;
        }
        if (goCopy.TryGetComponent(out Renderer renderCopy))
        {
            if (goPaste.TryGetComponent(out Renderer renderPaste))
            {
                CopyRendererMaterials(renderCopy,renderPaste);
                tip = "Single Copy Finished";
                return;
            }
            else
            {
                tip = "paste GameObject have no renderer";
                _messageType = MessageType.Error;
                return;
            }
        }
        else
        {
            arrayCopy = goCopy.GetComponentsInChildren<Renderer>();
            arrayPaste = goPaste.GetComponentsInChildren<Renderer>();
            //arrayCopy = arrayCopy.OrderBy(r => r.gameObject.name).ToArray();
            //arrayPaste = arrayPaste.OrderBy(r => r.gameObject.name).ToArray();
            Dictionary<string, Renderer> dicCopy = new Dictionary<string, Renderer>();
            Dictionary<string, Renderer> dicPaste = new Dictionary<string, Renderer>();
            //List<string> missing = new List<string>();
            bool hasMissingTarget=false;
            int matchedNum = 0;
            foreach (var v in arrayCopy)
            {
                dicCopy.Add(v.gameObject.name,v);
            }

            foreach (var v in arrayPaste)
            {
                dicPaste.Add(v.gameObject.name,v);
            }

            foreach (var kv in dicCopy)
            {
                if (dicPaste.TryGetValue(kv.Key, out var vPaste))
                {
                    CopyRendererMaterials(kv.Value,vPaste);
                    matchedNum++;
                }
                else
                {
                    tip = hasMissingTarget ? tip+" , "+kv.Key : "These Renderer didn't find Paste Target[ "+kv.Key;
                    //tip += kv.Key;
                    //tip += "  ";
                    hasMissingTarget = true;
                    missMatchedCopyList.Add(kv.Value);
                }
            }

            foreach (var kv in dicPaste)
            {
                if(!dicCopy.ContainsKey(kv.Key)) missMatchedPasteList.Add(kv.Value);
            }

            _messageType = !((arrayCopy.Length == arrayPaste.Length) && (arrayPaste.Length == matchedNum))
                ? MessageType.Warning
                : MessageType.Info;
            _messageType = hasMissingTarget ? MessageType.Error : _messageType;

            tip = hasMissingTarget ? tip+" ] " : "Copy Finished! ";
            
            tip += $"\n find copy renderers:{arrayCopy.Length} Paste renderers:{arrayPaste.Length},Matched:{matchedNum}";
        }
        //Mesh mesh = null;
        // switch (renderer)
        // {
        //     case MeshRenderer mr: mesh = mr.GetComponent<MeshFilter>().sharedMesh; break;
        //     case SkinnedMeshRenderer smr: mesh = smr.sharedMesh; ; break;
        //     default:
        //         break; // do nothing if not a supported renderer(e.g. particle system's renderer)
        // }
        
        
    }

    void CopyRendererMaterials(Renderer copy,Renderer paste)
    {
        Material[] materialsA = copy.sharedMaterials;
        Material[] materialsB = new Material[materialsA.Length];
        for (int i = 0; i < materialsA.Length; i++)
        {
            materialsB[i] = new Material(materialsA[i]);
        }
        paste.sharedMaterials = materialsB;
    }
}
