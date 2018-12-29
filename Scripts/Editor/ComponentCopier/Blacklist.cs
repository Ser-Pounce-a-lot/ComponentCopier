using System.Linq;
using UnityEngine;

//[CreateAssetMenu(fileName = "Blacklist", menuName = "Component Copier/Blacklist Asset")]
public class Blacklist : ScriptableObject
{
    public string[] Exclusions;

    public static Blacklist Instance
    {
        get { return Resources.FindObjectsOfTypeAll<Blacklist>().FirstOrDefault(); }
    }
}