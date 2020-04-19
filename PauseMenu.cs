using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour {

    public GameObject primaryStatsObj;

    public Text[] primaryStatTotalTexts;

    public Text[] primaryStatMultiTexts;

    [Space(10f)]
    public AudioMixer audioMixer;

    [Space(10f)]
    public GameOverController gameOverController;

    public GameObject pauseMenuBox;

    public Text pausedText;

    public GameObject optionsMenuBox;

    public Text optionsText;

    public GameObject optionsMenuBoxContent;

    public GameObject fpsCounter;

    public GameObject selectionPointer;

    public Text masterVolumePercentText;

    public Text soundsVolumePercentText;

    public Text musicVolumePercentText;

    public Slider masterVolumeSlider;

    public Slider soundsVolumeSlider;

    public Slider musicVolumeSlider;

    public Toggle fullscreenToggle;

    public Toggle fpsCounterToggle;

    public float timeBetweenBoxMoves;

    List<GameObject> pauseMenuSelectables = new List<GameObject>();

    GameObject selectedSelectable;

    Button selectedButton;

    Slider selectedSlider;

    Toggle selectedToggle;

    List<GameObject> optionsMenuSelectables = new List<GameObject>();

    RectTransform selectedSelectableRectTransform;

    [HideInInspector]
    public Image pauseMenuBgImage;

    RectTransform pauseMenuBoxRectTransform;

    RectTransform optionsMenuBoxRectTransform;

    RectTransform selectionPointerRectTransform;

    RectTransform sliderTextRectTransform;

    bool optionsMenuBoxIsDone = false;

    [System.NonSerialized]
    public bool paused = false;

    bool readyToResume = true;

    float latestTimeScale;

    PlayerController playerController;

    Vector2 positionOne;

    Vector2 positionTwo;

    AudioSource[] sounds;

    AudioSource startSound;

    AudioSource resumeSound;

    AudioSource changeBoxSound;

    AudioSource soundsTestSound;

    bool queueSoundsTest = false;

    bool playSoundsTest = false;

    int selectedSelectableIndex = 0;

    float sliderRange;

    float sliderValueIncrement;

    RestartManager restartManager;

    WorldManager worldManager;

    const string masterVolume = "masterVolume";

    const string volume_master = "volume_master";

    const string soundsVolume = "soundsVolume";

    const string volume_sounds = "volume_sounds";

    const string musicVolume = "musicVolume";

    const string volume_music = "volume_music";

    const string fullscreen_enabled = "fullscreen_enabled";

    const string fpscounter_enabled = "fpscounter_enabled";

    void Awake () {
        restartManager = FindObjectOfType<RestartManager>();
        worldManager = FindObjectOfType<WorldManager>();
        sounds = GetComponents<AudioSource>();
        startSound = sounds[0];
        resumeSound = sounds[1];
        changeBoxSound = sounds[2];
        soundsTestSound = sounds[3];
        pauseMenuBgImage = GetComponent<Image>();
        pauseMenuBoxRectTransform = pauseMenuBox.GetComponent<RectTransform>();
        optionsMenuBoxRectTransform = optionsMenuBox.GetComponent<RectTransform>();
        selectionPointerRectTransform = selectionPointer.GetComponent<RectTransform>();

        foreach(Transform child in optionsMenuBoxContent.transform)
        {
            if (child.GetComponent<Button>() != null || child.GetComponent<Slider>() != null || child.GetComponent<Toggle>() != null)
                optionsMenuSelectables.Add(child.gameObject);
        }

        foreach(Transform child in pauseMenuBox.transform)
        {
            if (child.GetComponent<Button>() != null)
                pauseMenuSelectables.Add(child.gameObject);
        }

        playerController = FindObjectOfType<PlayerController>();
        positionOne = new Vector2(Screen.currentResolution.width * 0.3333f, 0f);
        positionTwo = new Vector2(Screen.currentResolution.width * 0.1667f, 0f);
        pauseMenuBgImage.enabled = false;
        pauseMenuBox.SetActive(false);
        optionsMenuBox.SetActive(false);
        selectionPointer.SetActive(false);

        primaryStatsObj.SetActive(false);


        SetAndSaveVolume(masterVolume, PlayerPrefs.HasKey(volume_master) ? DisplayedVolumeToActualVolume(PlayerPrefs.GetInt(volume_master)) : 0f);   // 100%
        SetAndSaveVolume(soundsVolume, PlayerPrefs.HasKey(volume_sounds) ? DisplayedVolumeToActualVolume(PlayerPrefs.GetInt(volume_sounds)) : -10f);   // 80%
        SetAndSaveVolume(musicVolume, PlayerPrefs.HasKey(volume_music) ? DisplayedVolumeToActualVolume(PlayerPrefs.GetInt(volume_music)) : -20f);   // 60%

        SetAndSaveFullscreenEnabled(PlayerPrefs.HasKey(fullscreen_enabled) ? System.Convert.ToBoolean(PlayerPrefs.GetInt(fullscreen_enabled)) : true);  // 1
        SetAndSaveFPSCounterEnabled(PlayerPrefs.HasKey(fpscounter_enabled) ? System.Convert.ToBoolean(PlayerPrefs.GetInt(fpscounter_enabled)) : false); // 0
    }

    void Update () {
        if (!queueSoundsTest && playSoundsTest)
        {
            soundsTestSound.Play();
            playSoundsTest = false;
        }
        if(queueSoundsTest)
        {
            playSoundsTest = true;
            queueSoundsTest = false;
        }

		if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!paused && playerController.allowControl)
                StartCoroutine(StartPauseMenu());
            if (readyToResume)
                StartCoroutine(ResumeGameplay());
            if (optionsMenuBoxIsDone)
                StartCoroutine(ExitOptionsMenu());
        }
        if(selectionPointer.activeSelf)
        {
            if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (pauseMenuBox.activeSelf)
                {
                    if ((selectedSelectableIndex + 1) <= pauseMenuSelectables.Count - 1)
                        selectedSelectableIndex++;
                    else
                        selectedSelectableIndex = 0;
                    selectedSelectable = pauseMenuSelectables[selectedSelectableIndex];
                }

                if (optionsMenuBox.activeSelf)
                {
                    if ((selectedSelectableIndex + 1) <= optionsMenuSelectables.Count - 1)
                        selectedSelectableIndex++;
                    else
                        selectedSelectableIndex = 0;
                    selectedSelectable = optionsMenuSelectables[selectedSelectableIndex];
                }
            }

            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                if(pauseMenuBox.activeSelf)
                {
                    if ((selectedSelectableIndex - 1) >= 0)
                        selectedSelectableIndex--;
                    else
                        selectedSelectableIndex = pauseMenuSelectables.Count - 1;
                    selectedSelectable = pauseMenuSelectables[selectedSelectableIndex];
                }

                if (optionsMenuBox.activeSelf)
                {
                    if ((selectedSelectableIndex - 1) >= 0)
                        selectedSelectableIndex--;
                    else
                        selectedSelectableIndex = optionsMenuSelectables.Count - 1;
                    selectedSelectable = optionsMenuSelectables[selectedSelectableIndex];
                }
            }

            if (selectedSelectableRectTransform != selectedSelectable.GetComponent<RectTransform>())
            {
                selectedSelectableRectTransform = selectedSelectable.GetComponent<RectTransform>();
                UpdateSelectionPointerRectTransformAnchoredPosition();
            }

            if(Input.GetButtonDown("Submit"))
            {
                selectedButton = selectedSelectable.GetComponent<Button>();
                selectedToggle = selectedSelectable.GetComponent<Toggle>();
                if(selectedButton != null)
                    selectedButton.onClick.Invoke();
                if (selectedToggle != null)
                    selectedToggle.isOn = !selectedToggle.isOn;

                selectedButton = null;
                selectedToggle = null;
            }

            if(Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("TargetingHorizontal") != 0)
            {
                selectedSlider = selectedSelectable.GetComponent<Slider>();
                if(selectedSlider != null)
                {
                    sliderRange = selectedSlider.maxValue - selectedSlider.minValue;
                    sliderValueIncrement = sliderRange / 100f;
                    if (Input.GetAxisRaw("Horizontal") > 0 || Input.GetAxisRaw("TargetingHorizontal") > 0)
                        selectedSlider.value += sliderValueIncrement;
                    else
                        selectedSlider.value -= sliderValueIncrement;
                }

                selectedSlider = null;
            }
        }
	}

    public void UpdateSelectionPointerRectTransformAnchoredPosition()
    {
        if (selectedSelectableIndex == 0)
            selectionPointerRectTransform.anchoredPosition = new Vector2(selectedSelectableRectTransform.anchoredPosition.x - (selectedSelectableRectTransform.sizeDelta.x * 0.65f), selectedSelectableRectTransform.anchoredPosition.y + (selectedSelectableRectTransform.sizeDelta.y * 0.05f));
        else
        {
            if (selectedSelectable.GetComponent<Button>() != null)
                selectionPointerRectTransform.anchoredPosition = new Vector2(selectedSelectableRectTransform.anchoredPosition.x - (selectedSelectableRectTransform.sizeDelta.x * 0.5f), selectedSelectableRectTransform.anchoredPosition.y + (selectedSelectableRectTransform.sizeDelta.y * 0.05f));
            if(selectedSelectable.GetComponent<Toggle>() != null)
                selectionPointerRectTransform.anchoredPosition = new Vector2(selectedSelectableRectTransform.anchoredPosition.x - (selectedSelectableRectTransform.sizeDelta.x * 0.82f), selectedSelectableRectTransform.anchoredPosition.y + (selectedSelectableRectTransform.sizeDelta.y * 0.05f));
            if (selectedSelectable.GetComponent<Slider>() != null)
            {
                sliderTextRectTransform = selectedSelectableRectTransform.GetComponentInChildren<Text>().GetComponent<RectTransform>();
                selectionPointerRectTransform.anchoredPosition = new Vector2(selectedSelectableRectTransform.anchoredPosition.x + sliderTextRectTransform.anchoredPosition.x - (sliderTextRectTransform.sizeDelta.x * 1.1f), selectedSelectableRectTransform.anchoredPosition.y + sliderTextRectTransform.anchoredPosition.y + (sliderTextRectTransform.sizeDelta.y * 0.05f));
            }
        }
    }

    int ActualVolumeToDisplayedVolume(float actualVolume)
    {
        return Mathf.RoundToInt(100f + (actualVolume * 2f));
    }

    float DisplayedVolumeToActualVolume(int displayedVolume)
    {
        return (displayedVolume - 100f) / 2f;
    }

    void SetAndSaveVolume(string volumeName, float actualVolume)
    {
        audioMixer.SetFloat(volumeName, actualVolume);

        int displayedVolume = ActualVolumeToDisplayedVolume(actualVolume);
        string displayedVolumePercentText = $"{displayedVolume}%";

        switch(volumeName)
        {
            case masterVolume:
                masterVolumePercentText.text = displayedVolumePercentText;
                masterVolumeSlider.SetValueWithoutNotify(actualVolume);
                PlayerPrefs.SetInt(volume_master, displayedVolume);
                break;
            case soundsVolume:
                soundsVolumePercentText.text = displayedVolumePercentText;
                soundsVolumeSlider.SetValueWithoutNotify(actualVolume);
                PlayerPrefs.SetInt(volume_sounds, displayedVolume);
                break;
            case musicVolume:
                musicVolumePercentText.text = displayedVolumePercentText;
                musicVolumeSlider.SetValueWithoutNotify(actualVolume);
                PlayerPrefs.SetInt(volume_music, displayedVolume);
                break;
        }

        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float actualVolume)
    {
        SetAndSaveVolume(masterVolume, actualVolume);

        queueSoundsTest = true;
    }

    public void SetSoundsVolume(float actualVolume)
    {
        SetAndSaveVolume(soundsVolume, actualVolume);

        queueSoundsTest = true;
    }

    public void SetMusicVolume(float actualVolume)
    {
        SetAndSaveVolume(musicVolume, actualVolume);
    }

    void SetAndSaveFullscreenEnabled(bool fullscreenToggled)
    {
        Screen.fullScreen = fullscreenToggled;

        fullscreenToggle.SetIsOnWithoutNotify(fullscreenToggled);

        PlayerPrefs.SetInt(fullscreen_enabled, fullscreenToggled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool fullscreenToggled)
    {
        SetAndSaveFullscreenEnabled(fullscreenToggled);
    }

    void SetAndSaveFPSCounterEnabled(bool fpsCounterToggled)
    {
        fpsCounter.SetActive(fpsCounterToggled);

        fpsCounterToggle.SetIsOnWithoutNotify(fpsCounterToggled);

        PlayerPrefs.SetInt(fpscounter_enabled, fpsCounterToggled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetFPSCounter(bool fpsCounterToggled)
    {
        SetAndSaveFPSCounterEnabled(fpsCounterToggled);
    }

    public void GoToOptionsMenu()
    {
        StartCoroutine(StartOptionsMenu());
    }

    public void GoToPauseMenu()
    {
        StartCoroutine(ExitOptionsMenu());
    }

    public void GoResume()
    {
        StartCoroutine(ResumeGameplay());
    }

    public void Exit()
    {
        gameOverController.Exit();
    }

    IEnumerator StartPauseMenu()
    {
        if (playerController.allowControl)
        {
            latestTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            playerController.allowControl = false;
            startSound.Play();
            playerController.audioManager.Play("PausedMusic");

            primaryStatsObj.SetActive(true);
            for (int i = 0; i < primaryStatTotalTexts.Length; i++)
            {
                primaryStatTotalTexts[i].text = GenericExtensions.ToStringWithTwoDecimals(10f * playerController.GetPrimaryStatValueByID((PrimaryStat)i, StatRetrieveType.Total));
                primaryStatMultiTexts[i].text = $"x{GenericExtensions.ToStringWithTwoDecimals(playerController.GetPrimaryStatValueByID((PrimaryStat)i, StatRetrieveType.Multiplier))}";
            }
        }
        else
            changeBoxSound.Play();

        paused = true;
        readyToResume = false;

        pauseMenuBgImage.enabled = true;
        pauseMenuBox.SetActive(true);
        pausedText.enabled = false;
        foreach (GameObject selectable in pauseMenuSelectables)
            selectable.SetActive(false);
        selectionPointer.SetActive(false);
        pauseMenuBoxRectTransform.anchoredPosition = positionOne;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        pauseMenuBoxRectTransform.anchoredPosition = positionTwo;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        pauseMenuBoxRectTransform.anchoredPosition = Vector2.zero;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        pausedText.enabled = true;
        foreach (GameObject selectable in pauseMenuSelectables)
            selectable.SetActive(true);
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        selectedSelectableIndex = 0;
        selectedSelectable = pauseMenuSelectables[0];
        selectedSelectableRectTransform = selectedSelectable.GetComponent<RectTransform>();
        selectionPointer.SetActive(true);
        UpdateSelectionPointerRectTransformAnchoredPosition();
        readyToResume = true;
    }

    IEnumerator StartOptionsMenu()
    {
        changeBoxSound.Play();
        readyToResume = false;
        optionsMenuBoxIsDone = false;
        pausedText.enabled = false;
        foreach (GameObject selectable in pauseMenuSelectables)
            selectable.SetActive(false);
        selectionPointer.SetActive(false);
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        pauseMenuBoxRectTransform.anchoredPosition = -positionTwo;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        pauseMenuBoxRectTransform.anchoredPosition = -positionOne;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        pauseMenuBox.SetActive(false);
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        optionsMenuBox.SetActive(true);
        optionsText.enabled = false;
        foreach (GameObject selectable in optionsMenuSelectables)
            selectable.SetActive(false);
        optionsMenuBoxRectTransform.anchoredPosition = positionOne;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        optionsMenuBoxRectTransform.anchoredPosition = positionTwo;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        optionsMenuBoxRectTransform.anchoredPosition = Vector2.zero;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        optionsText.enabled = true;
        foreach (GameObject selectable in optionsMenuSelectables)
            selectable.SetActive(true);
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        selectedSelectableIndex = 0;
        selectedSelectable = optionsMenuSelectables[0];
        selectedSelectableRectTransform = selectedSelectable.GetComponent<RectTransform>();
        selectionPointer.SetActive(true);
        UpdateSelectionPointerRectTransformAnchoredPosition();
        optionsMenuBoxIsDone = true;
    }

    IEnumerator ExitOptionsMenu()
    {
        optionsMenuBoxIsDone = false;
        optionsText.enabled = false;
        foreach (GameObject selectable in optionsMenuSelectables)
            selectable.SetActive(false);
        selectionPointer.SetActive(false);
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        optionsMenuBoxRectTransform.anchoredPosition = -positionTwo;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        optionsMenuBoxRectTransform.anchoredPosition = -positionOne;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        optionsMenuBox.SetActive(false);
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        StartCoroutine(StartPauseMenu());
    }

    IEnumerator ResumeGameplay()
    {
        primaryStatsObj.SetActive(false);

        resumeSound.Play();
        playerController.audioManager.Stop("PausedMusic");
        pausedText.enabled = false;
        foreach (GameObject selectable in pauseMenuSelectables)
            selectable.SetActive(false);
        selectionPointer.SetActive(false);
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        pauseMenuBoxRectTransform.anchoredPosition = positionTwo;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        pauseMenuBoxRectTransform.anchoredPosition = positionOne;
        yield return new WaitForSecondsRealtime(timeBetweenBoxMoves);
        Time.timeScale = latestTimeScale;
        playerController.allowControl = true;
        paused = false;
        pauseMenuBgImage.enabled = false;
        pauseMenuBox.SetActive(false);
    }
}
