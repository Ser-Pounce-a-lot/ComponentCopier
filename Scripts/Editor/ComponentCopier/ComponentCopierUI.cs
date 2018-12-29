using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class ComponentCopierUI : EditorWindow
{
    #region Variables

    protected ComponentCopier Copier;
    protected List<AnimBool> foldStates;
    protected bool componentsChecked;
    protected int copiedCount = -1;
    protected const int checkboxWidth = 11;

    #endregion

    #region Methods

    #region Constructor/destructor/events

    [MenuItem("Tools/Component Copier/Edit Blacklist")]
    static void EditBlacklist()
    {
        Blacklist blacklist = Resources.FindObjectsOfTypeAll<Blacklist>().FirstOrDefault();
        Selection.activeObject = blacklist;
    }

    [MenuItem("Tools/Component Copier/Open Component Copier")]
    static void Init()
    {
        ComponentCopierUI copier = (ComponentCopierUI)GetWindow(typeof(ComponentCopierUI));
        copier.titleContent = new GUIContent("Component Copier");
        copier.Initialize();
        copier.Show();
    }

    /// <summary>
    /// Acts as a constructor of this class.
    /// </summary>
    public void Initialize()
    {
        foldStates = new List<AnimBool>();
        Copier = new ComponentCopier();
    }

    /// <summary>
    /// Unsubscribe events on destroyal.
    /// </summary>
    protected void OnDestroy()
    {
        Unsubscribe();
    }

    /// <summary>
    /// Subscribe the Repaint method to all valueChanged events.
    /// </summary>
    protected void Subscribe()
    {
        foreach (AnimBool state in foldStates) state.valueChanged.AddListener(Repaint);
    }

    /// <summary>
    /// Unsubscribe the Repaint method from all valueChanged events.
    /// </summary>
    protected void Unsubscribe()
    {
        foreach (AnimBool state in foldStates) state.valueChanged.RemoveListener(Repaint);
    }

    #endregion

    #region GUI

    protected void OnGUI()
    {
        Copier.SourceRootObject = (GameObject)EditorGUILayout.ObjectField("From", Copier.SourceRootObject, typeof(GameObject), true);
        Copier.DestinationRootObject = (GameObject)EditorGUILayout.ObjectField("To", Copier.DestinationRootObject, typeof(GameObject), true);

        if (Copier.SourceRootObject != null && Copier.DestinationRootObject != null)
        {
            if (Copier.SourceRootObject == Copier.DestinationRootObject)
            {
                EditorGUILayout.HelpBox("Source and destination object cannot be the same object", MessageType.Error);
                componentsChecked = false;
                copiedCount = -1;
            }
            else
            {
                if (GUILayout.Button(new GUIContent("Check components"))) CheckComponents();
            }
        }
        else
        {
            componentsChecked = false;
            copiedCount = -1;
        }

        EditorGUILayout.Space();

        DrawCopyItems();
    }

    protected void DrawCopyItems()
    {
        if (Copier.CopyItems == null || Copier.SourceRootObject == null || Copier.DestinationRootObject == null || !componentsChecked) return;

        for (int i = 0; i < Copier.CopyItems.Count; i++)
        {
            AnimBool foldState = foldStates[i];
            CopyItem copyItem = Copier.CopyItems[i];
            EditorGUILayout.BeginHorizontal();
            {
                foldState.target = EditorGUILayout.Toggle(foldState.target, EditorStyles.foldout, GUILayout.Width(checkboxWidth));
                copyItem.Enabled = EditorGUILayout.Toggle("", copyItem.Enabled, GUILayout.Width(checkboxWidth));
                EditorGUILayout.LabelField("From", GUILayout.Width(32));
                EditorGUILayout.ObjectField(copyItem.SourceObject, typeof(GameObject), true);
                EditorGUILayout.LabelField("to", GUILayout.Width(14));
                copyItem.DestinationObject = (GameObject)EditorGUILayout.ObjectField(copyItem.DestinationObject, typeof(GameObject), true);
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(foldState.faded))
            {
                EditorGUI.indentLevel += 3;
                EditorGUILayout.BeginVertical();
                {
                    for (int j = 0; j < copyItem.Copyables.Count; j++)
                    {
                        CopyItem.Copyable copyable = copyItem.Copyables[j];
                        EditorGUILayout.BeginHorizontal();
                        {
                            copyable.Enabled = EditorGUILayout.ToggleLeft(GetComponentIcon(copyable.Component.GetType()), copyable.Enabled);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= 3;
            }
            EditorGUILayout.EndFadeGroup();
        }

        if (GUILayout.Button(new GUIContent("Copy components")))
        {
            List<Component> newComponents = Copier.CopyComponents();
            Copier.UpdateReferences(newComponents);
            copiedCount = newComponents.Count;
        }

        if (copiedCount >= 0) EditorGUILayout.HelpBox(string.Format("Succesfully copied {0} components from {1} to {2}.", copiedCount, Copier.SourceRootObject.name, Copier.DestinationRootObject.name), MessageType.Info);
    }

    protected GUIContent GetComponentIcon(Type type)
    {
        GUIContent content = EditorGUIUtility.ObjectContent(null, type);
        if (content.image == null) content = EditorGUIUtility.ObjectContent(null, typeof(Behaviour));
        content.text = type.Name;
        return content;
    }

    #endregion

    #region Logic

    /// <summary>
    /// Initialize UI animations and subscribe to events.
    /// </summary>
    protected void Reinitialize()
    {
        if (foldStates != null) OnDestroy();
        foldStates = new List<AnimBool>();
        if (Copier.CopyItems != null)
        {
            foreach (CopyItem copyItem in Copier.CopyItems)
            {
                AnimBool foldState = new AnimBool(false);
                foldStates.Add(foldState);
            }

            Subscribe();
        }
    }

    protected void CheckComponents()
    {
        Copier.FetchCopyItems();
        Reinitialize();
        componentsChecked = true;
    }

    #endregion

    #endregion
}