using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CopyItem
{
    public class Copyable
    {
        public Component Component;
        public bool Enabled;

        public Copyable(Component component, bool enabled)
        {
            Component = component;
            Enabled = enabled;
        }
    }

    public GameObject SourceObject { get; protected set; }
    public GameObject DestinationObject { get; set; }
    public bool Enabled { get; set; }

    /// <summary>
    /// List of components and whether they should be copied
    /// </summary>
    public List<Copyable> Copyables { get; protected set; }

    public CopyItem(GameObject sourceObject, GameObject destinationRootObject, List<string> path)
    {
        Copyables = new List<Copyable>();
        SourceObject = sourceObject;
        DestinationObject = GetTargetGameObject(destinationRootObject, path);
        Enabled = DestinationObject != null;
        List<Component> components = SourceObject.GetComponents<Component>().Where(c => !Blacklist.Instance.Exclusions.Contains(c.GetType().Name)).ToList();
        foreach(Component component in components) Copyables.Add(new Copyable(component, true));
    }

    /// <summary>
    /// search the given GameObject for a child GameObject with a name specified in 'path'
    /// </summary>
    /// <param name="gameObject">the parent GameObject to search</param>
    /// <param name="path">the path at which the GameObject should be</param>
    /// <returns>instance of GameObject or null if not found</returns>
    private GameObject GetTargetGameObject(GameObject gameObject, List<string> path)
    {
        if (path[0] == "ROOT") path.RemoveAt(0);

        while (path.Count > 0)
        {
            if (gameObject.transform.childCount > 0)
            {
                bool found = false;

                foreach (Transform child in gameObject.transform)
                {
                    if (child.name == path[0])
                    {
                        path.RemoveAt(0);
                        gameObject = child.gameObject;
                        found = true;
                        break;
                    }
                }

                //target does not exist in children
                if (!found) return null;
            }
            else //no children
            {
                return null;
            }
        }
        return gameObject;
    }

}