using UnityEngine;

//[CreateAssetMenu(fileName = "Blacklist", menuName = "Component Copier/Blacklist Asset")]
public class Blacklist : ScriptableObject
{
    public string[] Exclusions;

    private static Blacklist _instance;

    public static Blacklist Instance
    {
        get {
            if(_instance == null) _instance = Resources.Load<Blacklist>("ComponentCopier/Blacklist");
            return _instance;
        }
    }
}