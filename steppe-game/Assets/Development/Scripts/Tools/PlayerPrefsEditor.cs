#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class PlayerPrefsEditor
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs cleared!");
    }
}

#endif