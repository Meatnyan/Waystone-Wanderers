using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class UICanvasController : MonoBehaviour {

    public GameObject primaryStatDisplayerObj;

    public GameObject armorIconObj;

    public Text armorText;

    public Color negativeArmorColor;

    public GameObject adrenalGlandCountdownObj;

    public GameObject fadeOutOverlayObj;

    public GameObject pauseMenuObj;

    public GameObject gameOverObj;

    public GameObject devConsoleObj;

    public Text commandCommentText;

    public Text enemiesRemainingText;

    [Header("Dev Console Settings")]
    public byte numOfDevKeyPressesRequired;

    public float timeBetweenDevKeyPresses;

    byte devKeyPresses = 0;

    [Space(20f)]
    public Color commandSuccessColor;

    public Color commandFailureColor;

    RectTransform devConsoleRectTransform;

    [HideInInspector]
    public bool devConsoleIsFullyOpen = false;

    bool devConsoleIsTransitioning = false;

    float lastDevKeyPressTime = Mathf.Infinity;

    InputField devConsoleInputField;

    WorldManager worldManager;

    float latestTimeScale = 1f;

    PlayerController playerController;

    LootManager lootManager;

    public class CommandWithParameters
    {
        public string command;
        public List<string> parameters;

        public CommandWithParameters(string newCommand, List<string> newParameters)
        {
            command = newCommand;
            parameters = newParameters;
        }
    }

    List<CommandWithParameters> registeredCommandsWithParameters = new List<CommandWithParameters>();

    int curRegdCommandWithParamsID = -1;

    bool latestCommandWasHandled = false;

    private void Awake()
    {
        devConsoleRectTransform = devConsoleObj.GetComponent<RectTransform>();
        devConsoleInputField = devConsoleObj.GetComponent<InputField>();
        worldManager = FindObjectOfType<WorldManager>();
        playerController = FindObjectOfType<PlayerController>();
        lootManager = FindObjectOfType<LootManager>();
        commandCommentText.text = "";
        devConsoleInputField.text = "";

        playerController.uiCanvasController = this;
        playerController.Armor = playerController.Armor;    // makes the armor icon/text display properly on startup
    }

    private void Update()
    {
        if(worldManager.levelIsLoaded && !devConsoleIsTransitioning && Input.GetKeyDown(KeyCode.BackQuote))
        {
            devKeyPresses++;
            lastDevKeyPressTime = Time.time;
            if(devKeyPresses == numOfDevKeyPressesRequired)
            {
                devKeyPresses = 0;
                if (devConsoleIsFullyOpen)
                    StartCoroutine(DevConsoleCloser());
                else
                    StartCoroutine(DevConsoleOpener());
            }
        }

        if (Time.time > lastDevKeyPressTime + timeBetweenDevKeyPresses)
            devKeyPresses = 0;

        if(devConsoleIsFullyOpen && registeredCommandsWithParameters.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (!latestCommandWasHandled)
                {
                    CommandWithParameters curCmdWithParams = SplitCurrentCommand();
                    registeredCommandsWithParameters.Add(curCmdWithParams);

                    curRegdCommandWithParamsID = registeredCommandsWithParameters.Count - 2;

                    SetInputFieldTextToCurRegdCommandWithParameters();

                    latestCommandWasHandled = true;
                }
                else if (curRegdCommandWithParamsID > 0)
                {
                    curRegdCommandWithParamsID--;

                    SetInputFieldTextToCurRegdCommandWithParameters();
                }
            }
            else if(Input.GetKeyDown(KeyCode.DownArrow) && latestCommandWasHandled && curRegdCommandWithParamsID < registeredCommandsWithParameters.Count - 1)
            {
                curRegdCommandWithParamsID++;

                SetInputFieldTextToCurRegdCommandWithParameters();
            }
        }
    }

    void SetInputFieldTextToCurRegdCommandWithParameters()
    {
        devConsoleInputField.text = registeredCommandsWithParameters[curRegdCommandWithParamsID].command;

        for (int i = 0; i < registeredCommandsWithParameters[curRegdCommandWithParamsID].parameters.Count; ++i)
            devConsoleInputField.text += " " + registeredCommandsWithParameters[curRegdCommandWithParamsID].parameters[i];
    }

    public void TrySplitCurrentCommandAndSend()
    {
        CommandWithParameters curCmdWithParams = SplitCurrentCommand();
        if (curCmdWithParams.command != "")
        {
            TrySendCommand(curCmdWithParams);
            registeredCommandsWithParameters.Add(curCmdWithParams);
        }
    }

    CommandWithParameters SplitCurrentCommand() // does not add to registered commands
    {
        bool commandDetected = false;
        for (int i = 0; i < devConsoleInputField.text.Length; ++i)
            if (devConsoleInputField.text[i] != ' ')
            {
                commandDetected = true;
                break;
            }

        if (commandDetected)
        {
            string command = "";
            List<string> parameters = new List<string>();
            bool commandIsDone = false;
            string parameterString = "";
            bool firstSpaceAfterWord = true;

            for (int inputCharID = 0; inputCharID < devConsoleInputField.text.Length; ++inputCharID)
            {
                if (!commandIsDone)
                {
                    if (devConsoleInputField.text[inputCharID] != ' ')
                        command += devConsoleInputField.text[inputCharID];
                    else if (command != "")
                        commandIsDone = true;
                }
                else
                {
                    if (devConsoleInputField.text[inputCharID] != ' ')
                    {
                        parameterString += devConsoleInputField.text[inputCharID];
                        if (inputCharID == devConsoleInputField.text.Length - 1)
                        {
                            parameters.Add(parameterString.ToLowerInvariant());
                            break;
                        }
                        firstSpaceAfterWord = false;
                    }
                    else if (!firstSpaceAfterWord)
                    {
                        parameters.Add(parameterString.ToLowerInvariant());
                        parameterString = "";
                        firstSpaceAfterWord = true;
                    }
                }
            }

            command = command.ToLowerInvariant();

            return new CommandWithParameters(command, parameters);
        }
        else return new CommandWithParameters("", new List<string>());
    }

    void TrySendCommand(CommandWithParameters commandWithParameters)
    {
        string command = commandWithParameters.command;
        List<string> parameters = commandWithParameters.parameters;

        switch(command) // both command and parameters are all lowercase already
        {
            case "spawnenemy":
                if (parameters.Count > 0)
                {
                    GameObject enemyObject = null;
                    for (int i = 0; i < worldManager.enemies.Length; ++i)
                        if (worldManager.enemies[i].GetComponent<EnemyController>().internalName == parameters[0])
                        {
                            enemyObject = worldManager.enemies[i];
                            break;
                        }
                    if (enemyObject == null)
                        ShowFailureMessage($"Unrecognized enemy name \"{parameters[0]}\"");
                    else
                    {
                        SpawnObjectAtSpecifiedAmount(enemyObject, parameters.Count > 1 ? parameters[1] : null, out int amountSpawned);

                        ShowSuccessMessage($"Successfully spawned enemy \"{parameters[0]}\" ({amountSpawned})");
                    }
                }
                else
                    ShowFailureMessage("No enemy name specified");
                break;


            case "spawnitem":
                if (parameters.Count > 0)
                    if (lootManager.SpawnSpecificItemOnNewPedestal(parameters[0], GenericExtensions.GetMousePositionInWorld()))
                        ShowSuccessMessage($"Successfully spawned item \"{parameters[0]}\"");
                    else
                        ShowFailureMessage($"Unrecognized item name \"{parameters[0]}\"");
                else
                    ShowFailureMessage("No item name specified");
                break;


            case "spawnweapon":
                if (parameters.Count > 0)
                {
                    GameObject weaponObject = null;
                    for(int i = 0; i < lootManager.currentWoodenChestWeaponPool.Count; ++i)
                    {
                        MeleeAttacker meleeAttacker = lootManager.currentWoodenChestWeaponPool[i].GetComponentInChildren<MeleeAttacker>();
                        Shooter shooter = lootManager.currentWoodenChestWeaponPool[i].GetComponent<Shooter>();
                        if((meleeAttacker != null && meleeAttacker.internalName == parameters[0]) || (shooter != null && shooter.internalName == parameters[0]))
                        {
                            weaponObject = lootManager.currentWoodenChestWeaponPool[i];
                            break;
                        }
                    }
                    if (weaponObject == null)
                        ShowFailureMessage($"Unrecognized weapon name \"{parameters[0]}\"");
                    else
                    {
                        SpawnObjectAtSpecifiedAmount(weaponObject, parameters.Count > 1 ? parameters[1] : null, out int amountSpawned);

                        ShowSuccessMessage($"Successfully spawned weapon \"{parameters[0]}\" ({ amountSpawned})");
                    }
                }
                else
                    ShowFailureMessage("No weapon name specified");
                break;


            case "spawnpickup":
                if (parameters.Count > 0)
                {
                    GameObject pickupObject = null;
                    for (int i = 0; i < lootManager.currentWoodenChestPickupPool.Count; ++i)
                    {
                        if (lootManager.currentWoodenChestPickupPool[i].GetComponent<Pickup>().internalName == parameters[0])
                        {
                            pickupObject = lootManager.currentWoodenChestPickupPool[i];
                            break;
                        }
                    }
                    if (pickupObject == null)
                        ShowFailureMessage($"Unrecognized pickup name \"{parameters[0]}\"");
                    else
                    {
                        SpawnObjectAtSpecifiedAmount(pickupObject, parameters.Count > 1 ? parameters[1] : null, out int amountSpawned);

                        ShowSuccessMessage($"Successfully spawned pickup \"{parameters[0]}\" ({amountSpawned})");
                    }
                }
                else
                    ShowFailureMessage("No pickup name specified");
                break;


            case "skiplevel":
                if (parameters.Count == 0)
                    if (worldManager.waystoneIsPresent)
                        worldManager.waystoneController.transform.position = playerController.transform.position;
                    else
                        worldManager.SpawnWaystone(playerController.transform.position);
                else
                    ShowFailureMessage($"The command \"{command}\" does not accept any parameters");

                break;


            case "moveplayer":
                Vector2 prevPos = playerController.transform.position;
                playerController.transform.position = GenericExtensions.GetMousePositionInWorld();
                ShowSuccessMessage($"Successfully moved player from {prevPos} to {(Vector2)playerController.transform.position}");
                break;


            case "toggle":
            case "t":
                switch(parameters[0])
                {
                    case "aggro":
                    case "agr":
                    playerController.enableEnemyAggro = !playerController.enableEnemyAggro;
                    ShowSuccessMessage($"Enemy aggro is now {(playerController.enableEnemyAggro ? "Enabled" : "Disabled")}");
                        break;

                    default:
                        ShowFailureMessage($"Unrecognized value to toggle \"{parameters[0]}\"");
                        break;
                }
                break;


            case "modify":
            case "mod":
                if (parameters.Count == 2)
                {
                    char mathSign = parameters[1][0];
                    string numberAsString = parameters[1].Remove(0, 1);
                    if (IsMathSign(mathSign))
                    {
                        if (IsNumber(numberAsString))
                        {
                            PropertyInfo[] propertyInfos = typeof(PlayerController).GetProperties();    // TO-DO: also make it work with fields
                            for (int i = 0; i < propertyInfos.Length; i++)
                            {
                                if (propertyInfos[i].Name.ToLowerInvariant() == parameters[0])
                                {
                                    float oldPropertyValue = (float)propertyInfos[i].GetValue(playerController);
                                    propertyInfos[i].SetValue(playerController, ParseValueMod(oldPropertyValue, mathSign, numberAsString));
                                    ShowSuccessMessage($"{propertyInfos[i].Name} value modified from {oldPropertyValue} to {(float)propertyInfos[i].GetValue(playerController)}");
                                    break;
                                }
                                else if (i == propertyInfos.Length - 1)
                                    ShowFailureMessage($"Property named \"{parameters[0]}\" not found");
                            }
                        }
                        else
                            ShowFailureMessage($"Value after math sign in parameter \"{parameters[1]}\" is not a number");
                    }
                    else
                        ShowFailureMessage($"First character of parameter \"{parameters[1]}\" is not an acceptable math sign");
                }
                else
                    ShowFailureMessage($"Mod requires a property name and a math operation");
                break;


            case "help":
            case "h":
                if (parameters.Count == 0)
                    ShowSuccessMessage($"Commands list: help [1/2/3...]; Command help: help [commandname]");
                else if (parameters.Count == 1)
                {
                    if (IsNumber(parameters[0]))
                        switch (parameters[0])
                        {
                            case "1":
                                ShowSuccessMessage("1: spawnenemy spawnitem spawnweapon spawnpickup skiplevel moveplayer");
                                break;
                            case "2":
                                ShowSuccessMessage("2: toggle/t modify/mod");
                                break;

                            default:
                                ShowFailureMessage($"Help page {parameters[0]} does not exist. Maximum is 2");
                                break;
                        }
                    else
                        switch (parameters[0])
                        {
                            case "spawnenemy":
                                ShowSuccessMessage("SPAWNENEMY enemyname [count]; spawnenemy sandraider 2");
                                break;
                            case "spawnitem":
                                ShowSuccessMessage("SPAWNITEM itemname; spawnitem thecookie");
                                break;
                            case "spawnweapon":
                                ShowSuccessMessage("SPAWNWEAPON weaponname [count]; spawnweapon assaultrifle 2");
                                break;
                            case "spawnpickup":
                                ShowSuccessMessage("SPAWNPICKUP pickupname [count]; spawnpickup healthlargepickup 2");
                                break;
                            case "skiplevel":
                                ShowSuccessMessage("SKIPLEVEL; skiplevel");
                                break;
                            case "moveplayer":
                                ShowSuccessMessage("MOVEPLAYER; moveplayer; moves to mouse position");
                                break;
                            case "toggle":
                            case "t":
                                ShowSuccessMessage("TOGGLE/T propertyname; toggle aggro");
                                break;
                            case "modify":
                            case "mod":
                                ShowSuccessMessage("MODIFY/MOD property signthennumber; mod attackspeedflat *3.5");
                                break;

                            default:
                                ShowFailureMessage($"No help available on command named \"{parameters[0]}\"");
                                break;
                        }
                }
                else
                    ShowFailureMessage("Help only accepts 1 parameter: page number or command name");
                break;


            default:
                ShowFailureMessage($"Unrecognized command \"{command}\"");
                break;
        }

        latestCommandWasHandled = false;

        devConsoleInputField.text = "";
        devConsoleInputField.ActivateInputField();
        devConsoleInputField.Select();
    }

    bool IsNumber(string str)
    {
        bool hasDot = false;
        for(int i = 0; i < str.Length; i++)
        {
            switch(str[i])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    continue;

                case '.':
                    hasDot = !hasDot;
                    if (!hasDot || i == str.Length - 1)
                        return false;
                    continue;

                default:
                    return false;
            }
        }
        return true;
    }

    bool IsMathSign(char sign)
    {
        return "+-*/=".Contains(sign.ToString());
    }

    float ParseValueMod(float value, char operation, string number)
    {
        float num = float.Parse(number);
        switch(operation)
        {
            case '+':
                value += num;
                break;
            case '-':
                value -= num;
                break;
            case '*':
                value *= num;
                break;
            case '/':
                value /= num;
                break;
            case '=':
                value = num;
                break;
        }
        return value;
    }

    void SpawnObjectAtSpecifiedAmount(GameObject obj, string amountAsString, out int amountToSpawn)
    {
        if (amountAsString != null)
        {
            try
            {
                amountToSpawn = int.Parse(amountAsString, NumberStyles.None);
            }
            catch (FormatException)
            {
                amountToSpawn = 1;
            }
            catch (OverflowException)
            {
                amountToSpawn = 999;
            }
            if (amountToSpawn > 999)
                amountToSpawn = 999;
            else if (amountToSpawn == 0)
                amountToSpawn = 1;
        }
        else
            amountToSpawn = 1;

        for (int i = 0; i < amountToSpawn; ++i)
            Instantiate(obj, GenericExtensions.GetMousePositionInWorld(), Quaternion.identity, null);
    }

    void ShowSuccessMessage(string message)
    {
        commandCommentText.color = commandSuccessColor;
        commandCommentText.text = message;
    }

    void ShowFailureMessage(string message)
    {
        commandCommentText.color = commandFailureColor;
        commandCommentText.text = message;
    }

    IEnumerator DevConsoleOpener()
    {
        playerController.allowControl = false;

        latestTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        devConsoleObj.SetActive(true);
        devConsoleIsTransitioning = true;

        for (int i = 0; i < 4; ++i)
        {
            devConsoleRectTransform.anchoredPosition = new Vector2(devConsoleRectTransform.anchoredPosition.x, devConsoleRectTransform.anchoredPosition.y + 20);
            yield return null;
        }

        devConsoleInputField.ActivateInputField();
        devConsoleInputField.Select();

        devConsoleIsTransitioning = false;
        devConsoleIsFullyOpen = true;
    }

    IEnumerator DevConsoleCloser()
    {
        commandCommentText.text = "";
        devConsoleInputField.text = "";
        devConsoleInputField.DeactivateInputField();

        devConsoleIsFullyOpen = false;
        devConsoleIsTransitioning = true;

        for (int i = 0; i < 4; ++i)
        {
            devConsoleRectTransform.anchoredPosition = new Vector2(devConsoleRectTransform.anchoredPosition.x, devConsoleRectTransform.anchoredPosition.y - 20);
            yield return null;
        }

        devConsoleIsTransitioning = false;
        devConsoleObj.SetActive(false);

        Time.timeScale = latestTimeScale;

        playerController.allowControl = true;
    }
}