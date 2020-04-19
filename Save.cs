using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Save
{
    public int[] reqs = new int[System.Enum.GetNames(typeof(ReqName)).Length];

    public List<int> unlockedAchievementInternalIDs = new List<int>();

    public int highscore = 0;
}
