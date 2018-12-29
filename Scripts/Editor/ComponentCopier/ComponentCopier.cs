using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ComponentCopier
{
    #region Variables

    public GameObject SourceRootObject { get; set; }
    public GameObject DestinationRootObject { get; set; }
    public List<CopyItem> CopyItems { get; protected set; }

    #endregion

    #region Methods

    /// <summary>
    /// Fetch all the components and store them as CopyItems
    /// </summary>
    public void FetchCopyItems()
    {
        CopyItems = GetAllComponents(SourceRootObject);
    }

    /// <summary>
    /// Get all the Components on the specified GameObject
    /// </summary>
    /// <param name="gameObject">the GameObject of which to get the Components</param>
    /// <returns>A List of CopyItems the contains a mapping between source and destination GameObjects and also the Components that should be copied</returns>
    protected List<CopyItem> GetAllComponents(GameObject gameObject)
    {
        List<CopyItem> copyItems = new List<CopyItem>();
        List<string> path = GetPath(gameObject);
        if (gameObject.GetComponents<Component>().Where(c => !Blacklist.Instance.Exclusions.Contains(c.GetType().Name)).ToList().Count > 0)
            copyItems.Add(new CopyItem(gameObject, DestinationRootObject, path));

        foreach (Transform transform in gameObject.transform)//foreach child
            copyItems.AddRange(GetAllComponents(transform.gameObject));

        return copyItems;
    }

    /// <summary>
    /// get the relative path of the given GameObject, which resides in either the source or destination hierarchy.
    /// </summary>
    /// <param name="gameObject">the GameObject to find the path of</param>
    /// <returns>null if not found, or a List of strings containing the path to the specified GameObject</returns>
    protected List<string> GetPath(GameObject gameObject)
    {
        List<string> path = new List<string>();
        while (gameObject.transform.parent != null && !IsRoot(gameObject))
        {
            path.Insert(0, gameObject.name);
            gameObject = gameObject.transform.parent.gameObject;
        }

        if (IsRoot(gameObject))
            path.Insert(0, "ROOT");

        return path;
    }

    /// <summary>
    /// check if the given gameobject is the gameobject that is being copied from or to.
    /// </summary>
    /// <param name="gameObject">the gameobject to check</param>
    /// <returns>true if it is either the gameobject that is being copied from or to, else false</returns>
    protected bool IsRoot(GameObject gameObject)
    {
        return gameObject.GetInstanceID() == SourceRootObject.GetInstanceID() || gameObject.GetInstanceID() == DestinationRootObject.GetInstanceID();
    }

    /// <summary>
    /// Copy the components
    /// </summary>
    /// <returns>a list of the new components</returns>
    public List<Component> CopyComponents()
    {
        List<Component> newComponents = new List<Component>();

        foreach (CopyItem copyItem in CopyItems)
        {
            if (!copyItem.Enabled || copyItem.DestinationObject == null) continue;

            foreach (CopyItem.Copyable copyable in copyItem.Copyables)
            {
                if (!copyable.Enabled) continue;
                Component newComponent = CopyComponent(copyable.Component, copyItem.DestinationObject);
                if (newComponent != null) newComponents.Add(newComponent);
            }
        }

        return newComponents;
    }

    /// <summary>
    /// makes a copy of the given component and adds it to the destination GameObject
    /// </summary>
    /// <param name="original">the component to copy</param>
    /// <param name="destination">the GameObject to copy it to</param>
    protected Component CopyComponent(Component original, GameObject destination)
    {
        Component copy = destination.AddComponent(original.GetType());
        EditorUtility.CopySerialized(original, copy);
        return copy;
    }

    /// <summary>
    /// update any reference types 
    /// </summary>
    /// <param name="components">list of copied components</param>
    public void UpdateReferences(List<Component> components)
    {
        foreach (Component component in components)
        {
            FieldInfo[] fields = GetReferenceFields(component);
            foreach (FieldInfo field in fields)
            {
                if (!typeof(IEnumerable).IsAssignableFrom(field.FieldType) || field.FieldType == typeof(Transform)) // if not ienumerable type
                {
                    Component componentRef = (Component)field.GetValue(component);
                    if (componentRef == null) continue;
                    Type fieldType = field.FieldType;

                    List<string> path = GetPath(componentRef.gameObject);
                    GameObject newGameObject = GetGameObject(DestinationRootObject, path);
                    Component[] newComponentRefs = newGameObject.GetComponents(fieldType);
                    if (newComponentRefs.Length != 1)
                    {
                        Debug.LogWarningFormat("{0} {1} components were found on {2}.", newComponentRefs.Length, fieldType.Name, newGameObject.name);
                    }

                    if (newComponentRefs.Length > 0)
                    {
                        field.SetValue(component, newComponentRefs.FirstOrDefault());
                    }
                }
                else
                {
                    if (field.FieldType.IsArray)
                    {
                        //TODO find a component that uses an array-backed field, so that I can implement it
                        Debug.LogError("Tried updating references for array " + field.Name + " which is not yet implemented.");
                    }
                    else //collection
                    {
                        IList values = (IList)field.GetValue(component);
                        Type fieldType = values.GetType().GetGenericArguments()[0];
                        for (int i = 0; i < values.Count; i++)
                        {
                            Component componentRef = (Component)values[i];
                            if (componentRef == null) continue;

                            List<string> path = GetPath(componentRef.gameObject);
                            GameObject newGameObject = GetGameObject(DestinationRootObject, path);
                            Component[] refs = newGameObject.GetComponents(fieldType);
                            if (refs.Length != 1)
                                Debug.LogWarningFormat("{0} {1} components were found on {2}.", refs.Length, fieldType.Name, newGameObject.name);

                            if (refs.Length > 0)
                                values[i] = refs.FirstOrDefault();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// get all the fields of a component that have a reference to another component
    /// </summary>
    /// <param name="component">the component to get the references from</param>
    /// <returns>the fields which contain references to other components</returns>
    protected FieldInfo[] GetReferenceFields(Component component)
    {
        Type type = component.GetType();
        FieldInfo[] fields = type.GetFields().Where(f => !f.IsLiteral).ToArray();
        List<FieldInfo> fieldInfos = new List<FieldInfo>();
        fieldInfos.AddRange(fields.Where(f => f.FieldType.IsSubclassOf(typeof(Component), true)));//get all fields that reference components
        fieldInfos.AddRange(fields.Where(f => typeof(IEnumerable).IsAssignableFrom(f.FieldType) && //get all enumerable fields that reference components
            f.FieldType.GetGenericArguments().Length > 0 &&
            f.FieldType.GetGenericArguments()[0].IsSubclassOf(typeof(Component), true)));
        return fieldInfos.ToArray();
    }

    /// <summary>
    /// Try and find a GameObject in the specified hierarchy with a given path
    /// </summary>
    /// <param name="target">the hierarchy to search the GameObject in</param>
    /// <param name="path">the path to the GameObject</param>
    /// <returns>the GameObject or null if not found</returns>
    protected GameObject GetGameObject(GameObject target, List<string> path)
    {
        if (path[0] == "ROOT") path.RemoveAt(0);

        while (path.Count > 0)
        {
            Transform next = null;
            foreach (Transform child in target.transform)
            {
                if (child.name == path[0])
                {
                    next = child;
                    path.RemoveAt(0);
                    break;
                }
            }

            if (next == null) break;
            else target = next.gameObject;
        }

        return target;
    }

    #endregion
}

public static class TypeExtensions
{
    /// <summary>
    /// Check if the current type is a derived of the specified type
    /// </summary>
    /// <param name="type">the current type</param>
    /// <param name="other">the type to compare to</param>
    /// <param name="includeSelf">should this method return true if current and compared type are the same?</param>
    public static bool IsSubclassOf(this Type type, Type other, bool includeSelf = false)
    {
        if (!includeSelf) return type.IsSubclassOf(other);
        return type.IsSubclassOf(other) || type == other;
    }
}