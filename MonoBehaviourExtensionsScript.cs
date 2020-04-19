using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourExtensionsScript : MonoBehaviour
{
}

/* If implemented properly, this could apparently be used to create a function that does both DontDestroyOnLoad and .Add to a List<GameObject>, but for now this'll be done manually
using UnityEngine;
using System.Collections.Generic;

public static class MonoBehaviourExtensions
{
    private static List<GameObject> tracker;

    public static void TrackedDontDestroyOnLoad(this MonoBehaviour m, GameObject go)
    {
        if (tracker == null)
        {
            tracker = new List<GameObject>();
        }

        tracker.Add(go);
        m.DontDestroyOnLoad(go);
    }

    public static void DestroyAllDDOL()
    {
        if (tracker == null)
        {
            return;
        }
        foreach (var go in tracker)
        {
            Destroy(go);
        }
    }
*/