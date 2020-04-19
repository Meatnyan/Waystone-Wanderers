using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public enum ReqType
{
    MoreOrEqualTo,
    Exactly,
    LessThanOrEqualTo
}

public enum ReqName
{
    TotalEnemiesKilled,
    HighestMaxHealth,
    HighestHealthRestored,
    HighestLevelBeaten,
    FastestLevelBeatenTime,
    HighestShotsMissedInARow,
    HighestShotsHitInARow,
    MostDamageDealtInSingleHit,
    MostEnemiesKilledRecently
};

public class AchievementManager : MonoBehaviour
{
    public Sprite placeholderSprite;

    public GameObject grayBgObject;

    [Space(20f)]
    public GameObject unlockBgObj;

    public float unlockBgMoveTime;

    [Space(20f)]
    public Image unlockAnimImage;

    [Space(20f)]
    public Text youUnlockedText;

    public float youUnlockedFadeInDur;

    [Space(10f)]
    public Text displayedNameText;

    [Space(20f)]
    public Image unlockedObjImage;

    public float timeBeforeObjFadeIn;

    public float objFadeInDur;

    [Space(10f)]
    public int objFloatMaxFrames;

    public float objFloatFrameThreshold;

    [Space(20f)]
    public GameObject shineParticlesObject;

    [Space(20f)]
    public Sprite[] flashingFrames;

    [Space(10f)]
    public float timeBeforeFlashingFrames;

    [Space(20f)]
    public Sprite[] lockOpeningFrames;

    public float lockOpeningFrameDur;

    [Space(10f)]
    public float timeBeforeLockOpening;

    public float timeAfterLockOpening;

    [Space(20f)]
    public Sprite[] gateSlidingFrames;

    public float gateSlidingFrameDur;

    [System.Serializable]
    public struct DefaultValueForReq
    {
        public ReqName reqName;
        public int defaultValue;
    }

    [Space(30f)]
    public DefaultValueForReq[] defaultValuesForReqs;

    [System.Serializable]
    public struct Achievement
    {
        [System.NonSerialized]
        public int internalID;

        public GameObject unlockedObject;

        public ReqName unlockRequirement;
        public ReqType requirementType;
        public int unlockThreshold;
    }

    [Space(40f)]
#pragma warning disable 0649
    [SerializeField]
    Achievement[] achievements;
#pragma warning restore 0649

    [Space(40f)]
    public float autoUpdateTime;

    [System.NonSerialized]  // VERY IMPORTANT: use System.NonSerialized instead of HideInInspector for public variables that don't want to be serialized (the value of which changes independent of unity)
    public int[] reqs = new int[System.Enum.GetNames(typeof(ReqName)).Length];

    List<Achievement> lockedAchievements = new List<Achievement>();

    Coroutine updateUnlocksCoroutine = null;

    RectTransform unlockBgRectTransform;

    Vector2 unlockBgStartingAnchoredPos;

    AudioSource lockOpeningSound;

    AudioSource gateSlidingSound;

    AudioSource shimmeringSound;

    Sprite startingUnlockAnimFrame;

    LootManager lootManager;

    WorldManager worldManager;

    [HideInInspector]
    public PlayerController playerController;

    static bool alreadyExists = false;

    public List<int> unlockedAchievementInternalIDs = new List<int>();

    [HideInInspector]
    public int highscore = 0;

    //private void OnValidate() // add new requirements' default values to inspector
    //{
    //    int amountOfReqs = System.Enum.GetNames(typeof(ReqName)).Length;
    //    if (defaultValuesForReqs.Length < amountOfReqs)
    //    {
    //        Debug.LogWarning($"defaultValuesForReqs.Length = {defaultValuesForReqs.Length}; amountOfReqs = {amountOfReqs}");

    //        int[] previousDefaultValues = new int[defaultValuesForReqs.Length];
    //        for (int i = 0; i < previousDefaultValues.Length; i++)
    //            previousDefaultValues[i] = defaultValuesForReqs[i].defaultValue;

    //        defaultValuesForReqs = new DefaultValueForReq[amountOfReqs];
    //        for (int i = 0; i < defaultValuesForReqs.Length; i++)
    //        {
    //            defaultValuesForReqs[i].reqName = (ReqName)i;
    //            if (i <= previousDefaultValues.Length - 1)
    //                defaultValuesForReqs[i].defaultValue = previousDefaultValues[i];
    //        }
    //    }
    //}

    void CreateNewSavefile()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Create(Application.persistentDataPath + ExtConst.savefileOneDirectory);

        Save save = new Save();
        for (int i = 0; i < save.reqs.Length; i++)
            save.reqs[i] = defaultValuesForReqs[i].defaultValue;
        // unlockedAchievementIDs and highscore start at correct values

        binaryFormatter.Serialize(fileStream, save);
        fileStream.Close();
    }

    public void UpdateSaveFile()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Open(Application.persistentDataPath + ExtConst.savefileOneDirectory, FileMode.Open);

        Save save = new Save();

        save.reqs = reqs;
        save.unlockedAchievementInternalIDs = unlockedAchievementInternalIDs;
        save.highscore = highscore;

        binaryFormatter.Serialize(fileStream, save);
    }

    private void Awake()
    {
        if (!alreadyExists)
        {
            alreadyExists = true;
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < achievements.Length; i++)
                achievements[i].internalID = i;


            if (!File.Exists(Application.persistentDataPath + ExtConst.savefileOneDirectory))
                CreateNewSavefile();


            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Open(Application.persistentDataPath + ExtConst.savefileOneDirectory, FileMode.Open);

            Save save = (Save)binaryFormatter.Deserialize(fileStream);
            fileStream.Close();

            for (int i = 0; i < save.reqs.Length; i++)
                reqs[i] = save.reqs[i];

            for (int i = save.reqs.Length; i < reqs.Length; i++)
                reqs[i] = defaultValuesForReqs[i].defaultValue;

            lockedAchievements = new List<Achievement>(achievements);

            for (int i = 0; i < save.unlockedAchievementInternalIDs.Count; i++)
                UnlockAchievementByInternalID(save.unlockedAchievementInternalIDs[i]);

            highscore = save.highscore;


            worldManager = FindObjectOfType<WorldManager>();
            lootManager = FindObjectOfType<LootManager>();

            unlockBgRectTransform = unlockBgObj.GetComponent<RectTransform>();
            unlockBgStartingAnchoredPos = unlockBgRectTransform.anchoredPosition;
            lockOpeningSound = GetComponents<AudioSource>()[0];
            gateSlidingSound = GetComponents<AudioSource>()[1];
            shimmeringSound = GetComponents<AudioSource>()[2];
            startingUnlockAnimFrame = unlockAnimImage.sprite;

            StartCoroutine(AutoUpdater());
        }
        else
            Destroy(gameObject);
    }

    ObjDisplayInfo UnlockAchievementByInternalID(int internalID)
    {
        GameObject unlockedObj = null;

        for (int i = 0; i < lockedAchievements.Count; i++)
            if (lockedAchievements[i].internalID == internalID)
            {
                unlockedObj = lockedAchievements[i].unlockedObject;
                lockedAchievements.RemoveAt(i);
                break;
            }


        unlockedAchievementInternalIDs.Add(internalID);


        StatModifier statModifier = unlockedObj.GetComponent<StatModifier>();
        if(statModifier != null)
            for (int i = 0; i < lootManager.currentTreasureItemPool.Count; i++)
            {
                StatModifier curStatModifier = lootManager.currentTreasureItemPool[i].GetComponent<StatModifier>();
                if (curStatModifier.internalName == statModifier.internalName)
                {
                    curStatModifier.unlocked = true;
                    return new ObjDisplayInfo(curStatModifier.GetComponent<SpriteRenderer>().sprite, curStatModifier.displayedName);
                }
            }

        Shooter shooter = unlockedObj.GetComponent<Shooter>();
        if (shooter != null)
            for (int i = 0; i < lootManager.currentWoodenChestWeaponPool.Count; i++)
            {
                Shooter curShooter = lootManager.currentWoodenChestWeaponPool[i].GetComponent<Shooter>();
                if (curShooter != null && curShooter.internalName == shooter.internalName)
                {
                    curShooter.unlocked = true;
                    return new ObjDisplayInfo(curShooter.GetComponent<SpriteRenderer>().sprite, curShooter.displayedName);
                }
            }

        MeleeAttacker meleeAttacker = unlockedObj.GetComponent<MeleeAttacker>();
        if (meleeAttacker != null)
            for (int i = 0; i < lootManager.currentWoodenChestWeaponPool.Count; i++)
            {
                MeleeAttacker curMeleeAttacker = lootManager.currentWoodenChestWeaponPool[i].GetComponent<MeleeAttacker>();
                if (curMeleeAttacker != null && curMeleeAttacker.internalName == meleeAttacker.internalName)
                {
                    curMeleeAttacker.unlocked = true;
                    return new ObjDisplayInfo(curMeleeAttacker.GetComponent<SpriteRenderer>().sprite, curMeleeAttacker.displayedName);
                }
            }


        return new ObjDisplayInfo(placeholderSprite, "Displayed name not set");
    }

    struct ObjDisplayInfo
    {
        public Sprite sprite;
        public string displayedName;

        public ObjDisplayInfo(Sprite sprite, string displayedName)
        {
            this.sprite = sprite;
            this.displayedName = displayedName;
        }
    }

    IEnumerator UpdateUnlocks()
    {
        List<int> newlyUnlockedAchievementInternalIDs = new List<int>();

        //Debug.LogWarning("lockedAchievements.Count = " + lockedAchievements.Count + "; reqs[(int)ReqIDs.totalEnemiesKilled] = " + reqs[(int)ReqIDs.totalEnemiesKilled]);

        foreach(Achievement lockedAchievement in lockedAchievements)
        {
            int curReqValue = reqs[(int)lockedAchievement.unlockRequirement];
            switch (lockedAchievement.requirementType)
            {
                case ReqType.MoreOrEqualTo:
                    if (curReqValue >= lockedAchievement.unlockThreshold)
                        newlyUnlockedAchievementInternalIDs.Add(lockedAchievement.internalID);
                    break;
                case ReqType.Exactly:
                    if (curReqValue == lockedAchievement.unlockThreshold)
                        newlyUnlockedAchievementInternalIDs.Add(lockedAchievement.internalID);
                    break;
                case ReqType.LessThanOrEqualTo:
                    if (curReqValue <= lockedAchievement.unlockThreshold)
                        newlyUnlockedAchievementInternalIDs.Add(lockedAchievement.internalID);
                    break;
            }
        }

        if (newlyUnlockedAchievementInternalIDs.Count > 0)
        {
            // pause the game before unlocking anim
            float latestTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            playerController.allowControl = false;


            ObjDisplayInfo[] objDisplayInfos = new ObjDisplayInfo[newlyUnlockedAchievementInternalIDs.Count];
            for (int i = 0; i < newlyUnlockedAchievementInternalIDs.Count; i++)
                objDisplayInfos[i] = UnlockAchievementByInternalID(newlyUnlockedAchievementInternalIDs[i]); // removes from locked achievements, adds to unlocked achievements, unlocks object

            UpdateSaveFile();

            for (int unlockingStep = 0; unlockingStep < newlyUnlockedAchievementInternalIDs.Count; unlockingStep++)
            {
                Sprite unlockedObjSprite = objDisplayInfos[unlockingStep].sprite;
                string unlockedObjDisplayedName = objDisplayInfos[unlockingStep].displayedName;


                unlockedObjImage.sprite = null;
                unlockedObjImage.preserveAspect = true;
                unlockedObjImage.sprite = unlockedObjSprite;
                displayedNameText.text = unlockedObjDisplayedName;

                // necessary for CrossFadeAlpha()
                youUnlockedText.canvasRenderer.SetAlpha(0f);
                unlockedObjImage.canvasRenderer.SetAlpha(0f);
                displayedNameText.canvasRenderer.SetAlpha(0f);


                grayBgObject.SetActive(true);
                unlockBgObj.SetActive(true);
                unlockAnimImage.enabled = true;
                unlockAnimImage.sprite = startingUnlockAnimFrame;


                for(int i = 3; i >= 0; --i)
                {
                    unlockBgRectTransform.anchoredPosition = new Vector2(unlockBgStartingAnchoredPos.x, unlockBgStartingAnchoredPos.y - 200f * i);
                    yield return new WaitForSecondsRealtime(unlockBgMoveTime);
                }

                yield return new WaitForSecondsRealtime(timeBeforeFlashingFrames);
                for (int flashID = 0; flashID < flashingFrames.Length; flashID++)
                {
                    unlockAnimImage.sprite = flashingFrames[flashID];
                    yield return null;
                }

                unlockAnimImage.sprite = startingUnlockAnimFrame;


                yield return new WaitForSecondsRealtime(timeBeforeLockOpening);
                lockOpeningSound.Play();
                for(int openingID = 0; openingID < lockOpeningFrames.Length; openingID++)
                {
                    yield return new WaitForSecondsRealtime(lockOpeningFrameDur);
                    unlockAnimImage.sprite = lockOpeningFrames[openingID];
                }
                yield return new WaitForSecondsRealtime(timeAfterLockOpening);


                gateSlidingSound.Play();

                for (int slidingID = 0; slidingID < gateSlidingFrames.Length; slidingID++)
                {
                    unlockAnimImage.sprite = gateSlidingFrames[slidingID];
                    if (slidingID == 3)
                    {
                        shimmeringSound.Play();

                        youUnlockedText.CrossFadeAlpha(1f, youUnlockedFadeInDur, true);
                    }
                    yield return new WaitForSecondsRealtime(gateSlidingFrameDur / (1 + slidingID * 0.25f));
                }
                unlockAnimImage.enabled = false;

                yield return new WaitForSecondsRealtime(timeBeforeObjFadeIn);
                shineParticlesObject.SetActive(true);
                unlockedObjImage.CrossFadeAlpha(1f, objFadeInDur, true);
                displayedNameText.CrossFadeAlpha(1f, objFadeInDur, true);

                int directionMultiplier = 1;
                int curFloatFrame = 0;
                // wait until one of these buttons is pressed
                while (!Input.GetButtonDown("Submit") && !Input.GetKeyDown(KeyCode.Escape) && !Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKeyDown(KeyCode.Mouse1))
                {
                    if(curFloatFrame != 0 && curFloatFrame % objFloatFrameThreshold == 0)
                        unlockedObjImage.rectTransform.anchoredPosition = new Vector2(unlockedObjImage.rectTransform.anchoredPosition.x, unlockedObjImage.rectTransform.anchoredPosition.y + 1 * directionMultiplier);
                    curFloatFrame++;
                    if(curFloatFrame >= objFloatMaxFrames)
                    {
                        curFloatFrame = 0;
                        directionMultiplier *= -1;
                    }
                    yield return null;
                }


                shimmeringSound.Stop();
                shineParticlesObject.SetActive(false);
            }


            // disable the unlocking graphics
            unlockBgObj.SetActive(false);
            grayBgObject.SetActive(false);

            // unpause after every achievement is done unlocking
            Time.timeScale = latestTimeScale;
            playerController.allowControl = true;
        }

        updateUnlocksCoroutine = null;
    }

    IEnumerator AutoUpdater()
    {
        while(true)
        {
            if(updateUnlocksCoroutine == null && worldManager.levelIsLoaded && !playerController.pauseMenu.paused)
                updateUnlocksCoroutine = StartCoroutine(UpdateUnlocks());

            yield return new WaitForSeconds(autoUpdateTime);
        }
    }
}