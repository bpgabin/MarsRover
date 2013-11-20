using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class GUISystem : MonoBehaviour {

	public Camera mainCamera;
    public enum ButtonType { rover, arrowUp, arrowLeft, arrowRight, grab, drop, blank }

    public GUISkin Ourskin;
    public AudioClip[] songs;
    public AudioClip[] effects;

    private delegate void GUIFunction();
    private GUIFunction currentGUI;
    private GUIFunction pausedGUI;
    private GUIFunction menuGUI;
    private ColonyManager colonyManager;
    private int songIndex = 0;

    private MarsBase currentBase = null;

    private bool dragging = false;
    private Vector2 scrollPosition = Vector2.zero;
    private ButtonType draggingButton;
    private Dictionary<int, Texture> drillTextures;
    private Dictionary<int, Texture> refineryTextures;
    private Dictionary<ButtonType, Texture> buttonTextures;
    private Dictionary<ButtonType, MarsBase.Rover.ActionType> buttonToAction;
    private Dictionary<MarsBase.Rover.ActionType, ButtonType> actionToButton;

    private AudioClip robotGrab;
    private AudioClip buttonPressSound;
    private AudioClip dragClick;

    private int instructionsPage = 0;

    void Start() {
        audio.clip = songs[0];
        audio.Play();
        Invoke("PlayNextSong", songs[0].length);

        robotGrab = effects[0];
        buttonPressSound = effects[1];
        dragClick = effects[2];

        buttonTextures = new Dictionary<ButtonType, Texture>();
        buttonTextures[ButtonType.blank] = Resources.Load("Textures/blankIcon") as Texture;
        buttonTextures[ButtonType.drop] = Resources.Load("Textures/dropIcon") as Texture;
        buttonTextures[ButtonType.grab] = Resources.Load("Textures/grabIcon") as Texture;
        buttonTextures[ButtonType.arrowUp] = Resources.Load("Textures/forwardIcon") as Texture;
        buttonTextures[ButtonType.arrowLeft] = Resources.Load("Textures/rotateLeftIcon") as Texture;
        buttonTextures[ButtonType.arrowRight] = Resources.Load("Textures/rotateRightIcon") as Texture;
        buttonTextures[ButtonType.rover] = Resources.Load("Textures/rover") as Texture;

        buttonToAction = new Dictionary<ButtonType, MarsBase.Rover.ActionType>();
        buttonToAction[ButtonType.arrowUp] = MarsBase.Rover.ActionType.forward;
        buttonToAction[ButtonType.arrowLeft] = MarsBase.Rover.ActionType.turnLeft;
        buttonToAction[ButtonType.arrowRight] = MarsBase.Rover.ActionType.turnRight;
        buttonToAction[ButtonType.grab] = MarsBase.Rover.ActionType.grab;
        buttonToAction[ButtonType.drop] = MarsBase.Rover.ActionType.drop;
        buttonToAction[ButtonType.blank] = MarsBase.Rover.ActionType.none;

        actionToButton = new Dictionary<MarsBase.Rover.ActionType, ButtonType>();
        actionToButton[MarsBase.Rover.ActionType.forward] = ButtonType.arrowUp;
        actionToButton[MarsBase.Rover.ActionType.turnLeft] = ButtonType.arrowLeft;
        actionToButton[MarsBase.Rover.ActionType.turnRight] = ButtonType.arrowRight;
        actionToButton[MarsBase.Rover.ActionType.grab] = ButtonType.grab;
        actionToButton[MarsBase.Rover.ActionType.drop] = ButtonType.drop;
        actionToButton[MarsBase.Rover.ActionType.none] = ButtonType.blank;

        drillTextures = new Dictionary<int, Texture>();
        drillTextures[1] = Resources.Load("Textures/bucketwheelexcavator_small_red") as Texture;
        drillTextures[2] = Resources.Load("Textures/bucketwheelexcavator_small_green") as Texture;
        drillTextures[3] = Resources.Load("Textures/bucketwheelexcavator_small_blue") as Texture;

        refineryTextures = new Dictionary<int, Texture>();
        refineryTextures[1] = Resources.Load("Textures/refinery_small_red") as Texture;
        refineryTextures[2] = Resources.Load("Textures/refinery_small_green") as Texture;
        refineryTextures[3] = Resources.Load("Textures/refinery_small_blue") as Texture;

        currentGUI = MainMenuGUI;
    }

    void PlayNextSong() {
        audio.Stop();
        songIndex++;
        if (songIndex >= songs.Length)
            songIndex = 0;
        audio.clip = songs[songIndex];
        audio.Play();
        Invoke("PlayNextSong", audio.clip.length);
    }

    void OnGUI() {
        currentGUI();
    }

	public void SwitchToGame(){
		currentGUI = ColonyGUI;
	}

	void DrawNothing(){

	}

    void Update() {
        if (currentGUI == ColonyGUI || currentGUI == BaseGUI) {
            if (colonyManager.auditing) {
                Time.timeScale = 0;
                pausedGUI = currentGUI;
                currentGUI = AuditGUI;
            }
        }
    }

    void AuditGUI() {
        GUI.skin = Ourskin;
        GUI.Box(new Rect(Screen.width / 2 - 200, 50, 400, 500), GUIContent.none);

        GUI.Label(new Rect(Screen.width / 2 - 90, 55, 200, 40), "performance audit");

        GUI.skin = null;
        GUI.Box(new Rect(Screen.width / 2 - 180, 80, 360, 420), GUIContent.none);
        GUI.skin = Ourskin;

        GUI.Label(new Rect(Screen.width / 2 - 150, 100, 300, 25), "GENERAL STATS");
        GUI.Label(new Rect(Screen.width / 2 - 150, 125, 300, 25), "money: " + colonyManager.money.ToString());
        GUI.Label(new Rect(Screen.width / 2 - 150, 150, 300, 25), "total iron sold: " + colonyManager.totalIronSold.ToString());
        GUI.Label(new Rect(Screen.width / 2 - 150, 175, 300, 25), "times audited: " + (colonyManager.timesAudited).ToString());

        GUI.Label(new Rect(Screen.width / 2 - 150, 225, 300, 25), "AUDIT");
        GUI.Label(new Rect(Screen.width / 2 - 150, 250, 300, 25), "iron goal: " + colonyManager.lastAuditGoal.ToString());
        GUI.Label(new Rect(Screen.width / 2 - 150, 275, 300, 25), "iron sold since last audit: " + colonyManager.lastAuditAmount.ToString());
        GUI.Label(new Rect(Screen.width / 2 - 150, 300, 325, 25), "strikes: " + colonyManager.strikes.ToString());
        if (colonyManager.lastAuditAmount >= colonyManager.lastAuditGoal) {
            GUI.Label(new Rect(Screen.width / 2 - 150, 325, 400, 60), "GOOD JOB!\nCONTINUE YOUR SUCCESS.");
        }
        else if(colonyManager.strikes >= 3) {
            GUI.Label(new Rect(Screen.width / 2 - 150, 325, 400, 60), "THREE STRIKES!\nYOU'RE FIRED!");
        }
        else {
            GUI.Label(new Rect(Screen.width / 2 - 150, 325, 400, 60), "YOU'VE EARNED A STRIKE!\nYOU MUST IMPROVE.");
        }
        

        

        // Game Failed Condition
        if (colonyManager.strikes >= 3) {
            if (GUI.Button(new Rect(Screen.width / 2 - 50, 510, 100, 30), "main menu")) {
                KongregateAPI kongAPI = gameObject.GetComponent<KongregateAPI>();
                kongAPI.SubmitStats("gameCompleted", 1);
                kongAPI.SubmitStats("mostTotalIron", colonyManager.totalIronSold);
                kongAPI.SubmitStats("mostAuditTimes", colonyManager.timesAudited);

                DestroyImmediate(colonyManager);
                colonyManager = null;
                Time.timeScale = 1;
                currentGUI = MainMenuGUI;
            }
        }
        else {
            GUI.Label(new Rect(Screen.width / 2 - 150, 400, 400, 25), "NEW GOAL");
            GUI.Label(new Rect(Screen.width / 2 - 150, 425, 400, 25), "new iron goal: " + colonyManager.auditGoal.ToString());

            if (GUI.Button(new Rect(Screen.width / 2 - 50, 510, 100, 30), "resume")) {
                Time.timeScale = 1;
                colonyManager.auditing = false;
                currentGUI = pausedGUI;
                KongregateAPI kongAPI = gameObject.GetComponent<KongregateAPI>();
                kongAPI.SubmitStats("mostTotalIron", colonyManager.totalIronSold);
                kongAPI.SubmitStats("mostAuditTimes", colonyManager.timesAudited);
            }
        }

    }

    void MainMenuGUI() {
        GUI.skin = Ourskin;
		GUI.DrawTexture(new Rect(Screen.width / 2 - 176, 100, 352, 148), Resources.Load("Textures/redrover") as Texture);
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 70, Screen.height / 2 + 50 , 140, 200));
        if (GUILayout.Button("Start")) {
            colonyManager = gameObject.AddComponent<ColonyManager>();
			currentGUI = ColonyGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        if (GUILayout.Button("Instructions")) {
            menuGUI = currentGUI;
            currentGUI = InstructionsGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        if (GUILayout.Button("Options")) {
            menuGUI = currentGUI;
            currentGUI = OptionsMenuGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        if (GUILayout.Button("Credits")) {
            menuGUI = currentGUI;
            currentGUI = CreditsGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        GUILayout.EndArea();
    }

    void PauseMenuGUI() {
        GUI.skin = Ourskin;
        GUI.DrawTexture(new Rect(Screen.width / 2 - 176, 100, 352, 148), Resources.Load("Textures/redrover") as Texture);
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 70, Screen.height / 2 + 50, 140, 200));
        if (GUILayout.Button("Resume")) {
            currentGUI = pausedGUI;
            Time.timeScale = 1;
            audio.PlayOneShot(buttonPressSound);
        }
        if (GUILayout.Button("New Game")) {
            DestroyImmediate(colonyManager);
            colonyManager = gameObject.AddComponent<ColonyManager>();
            Time.timeScale = 1;
            currentGUI = ColonyGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        if (GUILayout.Button("Instructions")) {
            menuGUI = currentGUI;
            currentGUI = InstructionsGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        if (GUILayout.Button("Options")) {
            menuGUI = currentGUI;
            currentGUI = OptionsMenuGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        if (GUILayout.Button("Credits")) {
            menuGUI = currentGUI;
            currentGUI = CreditsGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        GUILayout.EndArea();
    }

    void InstructionsGUI() {
        GUI.skin = Ourskin;
        //GUI.DrawTexture(new Rect(Screen.width / 2 - 176, 100, 352, 148), Resources.Load("Textures/redrover") as Texture);

        GUI.Box(new Rect(26, 26, 908, 548), GUIContent.none);

        if (instructionsPage == 0) {
            GUI.DrawTexture(new Rect(30, 30, 900, 540), Resources.Load("Textures/baseInstructions") as Texture);
        }
        else if (instructionsPage == 1) {
            GUI.DrawTexture(new Rect(30, 30, 900, 540), Resources.Load("Textures/programmingInstructions") as Texture);
        }
        else if (instructionsPage == 2) {
            GUI.DrawTexture(new Rect(30, 30, 900, 540), Resources.Load("Textures/productionCycle") as Texture);
        }
        else if (instructionsPage == 3) {
            GUI.DrawTexture(new Rect(30, 30, 900, 540), Resources.Load("Textures/overview") as Texture);
        }
        
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 120, Screen.height - 60, 240, 200));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (instructionsPage > 0) {
            if (GUILayout.Button("Prev")) {
                instructionsPage--;
                audio.PlayOneShot(buttonPressSound);
            }
        }
        else GUILayout.FlexibleSpace();
        if (GUILayout.Button("Return", GUILayout.Width(100))) {
            instructionsPage = 0;
            currentGUI = menuGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        if (instructionsPage < 3) {
            if (GUILayout.Button("Next")) {
                instructionsPage++;
                audio.PlayOneShot(buttonPressSound);
            }
        }
        else GUILayout.FlexibleSpace();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void OptionsMenuGUI() {
        GUI.skin = Ourskin;
        GUI.DrawTexture(new Rect(Screen.width / 2 - 176, 100, 352, 148), Resources.Load("Textures/redrover") as Texture);
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 120, Screen.height / 2 + 50, 240, 200));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Options");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Box("global volume: " + string.Format("{0:0.00}", AudioListener.volume), GUILayout.Width(230));
        AudioListener.volume = GUILayout.HorizontalSlider(AudioListener.volume, 0.0f, 1.0f);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Return", GUILayout.Width(100))) {
            currentGUI = menuGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void CreditsGUI() {
        GUI.skin = Ourskin;
        GUI.DrawTexture(new Rect(Screen.width / 2 - 176, 35, 352, 148), Resources.Load("Textures/redrover") as Texture);
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 350));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Credits");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Art");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Richard Vallejos");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Ruben Telles");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Programming");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Brian Gabin");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Music");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Kevin McLeod");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Box("incompetech.com");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Return")) {
            currentGUI = menuGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        GUILayout.EndArea();
    }

    void ColonyGUI() {
        GUI.skin = Ourskin;

        List<Rect> baseRects = new List<Rect>();

        // Draw Background
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/marsbackground_02") as Texture);

        GUI.Box(new Rect(0, 0, 230, 100), GUIContent.none);
        GUI.Label(new Rect(0 + 40, 8, 220, 30), "IRON: " + colonyManager.iron.ToString());
        GUI.Label(new Rect(0 + 40, 28, 220, 30), "MONEY: $" + colonyManager.money.ToString());
        GUI.Label(new Rect(0 + 40, 48, 220, 30), "IRON SOLD: " + colonyManager.ironSoldSinceAudit.ToString());
        GUI.Label(new Rect(0 + 40, 68, 240, 30), "IRON GOAL: " + colonyManager.auditGoal.ToString());

        GUI.Box(new Rect(Screen.width - 230, 0, 230, 130), GUIContent.none);
        GUI.Box(new Rect(Screen.width - 230 + 15, 30, 200, 40), GUIContent.none);
        GUI.Box(new Rect(Screen.width - 230 + 15, 80, 200, 40), GUIContent.none);
        GUI.Label(new Rect(Screen.width - 230 + 115 - 25, 5, 80, 30), "timers");

        GUI.color = Color.Lerp(Color.red, Color.green, (colonyManager.currentSpaceElevatorTime / colonyManager.spaceElevatorTime));
        GUI.Box(new Rect(Screen.width - 230 + 15 + 6, 30 + 6, (colonyManager.currentSpaceElevatorTime / colonyManager.spaceElevatorTime) * 188, 28), GUIContent.none, "ProgressBar");
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 230 + 66, 38, 125, 40), "trade ship");

        GUI.color = Color.Lerp(Color.red, Color.green, (colonyManager.currentAuditTime / colonyManager.auditTime));
        GUI.Box(new Rect(Screen.width - 230 + 15 + 6, 80 + 6, (colonyManager.currentAuditTime / colonyManager.auditTime) * 188, 28), GUIContent.none, "ProgressBar");
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 230 + 94, 88, 125, 40), "audit");

        if (GUI.Button(new Rect(Screen.width - 50, Screen.height - 30, 50, 30), "Menu")) {
            Time.timeScale = 0;
            pausedGUI = currentGUI;
            currentGUI = PauseMenuGUI;
            audio.PlayOneShot(buttonPressSound);
        }

        GUI.DrawTexture(new Rect(0, -120, 960, 540), Resources.Load("Textures/spaceElevator") as Texture);

        Texture drillTexture = Resources.Load("Textures/bucketwheelexcavator_small") as Texture;
        baseRects.Add(new Rect(30, 230, drillTexture.width, drillTexture.height));
        baseRects.Add(new Rect(190, 350, drillTexture.width, drillTexture.height));
        baseRects.Add(new Rect(380, 360, drillTexture.width, drillTexture.height));
        baseRects.Add(new Rect(560, 350, drillTexture.width, drillTexture.height));
        baseRects.Add(new Rect(740, 230, drillTexture.width, drillTexture.height));

        for (int i = 0; i < baseRects.Count; i++) {
            if (i < colonyManager.bases.Count) {
                if (colonyManager.bases[i].running) {
                    GUI.color = Color.green;
                    GUI.Label(new Rect(baseRects[i].x + 60, baseRects[i].y + 100, 200, 80), "running");
                    GUI.DrawTexture(baseRects[i], drillTexture);
                }
                else {
                    GUI.color = Color.red;
                    GUI.Label(new Rect(baseRects[i].x + 60, baseRects[i].y + 100, 200, 80), "stopped");
                    GUI.DrawTexture(baseRects[i], drillTexture);
                }
            }
            else if (i == colonyManager.bases.Count) {
                GUI.color = Color.white;
                GUI.Label(new Rect(baseRects[i].x + 40, baseRects[i].y + 100, 200, 80), "click to buy\n$" + colonyManager.costs[ColonyManager.ShopItems.miningBase].ToString());
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                GUI.DrawTexture(baseRects[i], drillTexture);
            }
            else {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                GUI.DrawTexture(baseRects[i], drillTexture);
            }
        }
        GUI.color = Color.white;

        if (Event.current.type == EventType.mouseDown) {
            for (int i = 0; i < baseRects.Count; i++) {
                if (baseRects[i].Contains(Event.current.mousePosition)) {
                    if (i < colonyManager.bases.Count) {
                        currentBase = colonyManager.bases[i];
                        currentGUI = BaseGUI;
                        audio.PlayOneShot(buttonPressSound);
                    }
                    else if(i == colonyManager.bases.Count) {
                        if (colonyManager.money >= colonyManager.costs[ColonyManager.ShopItems.miningBase]) {
                            colonyManager.AddBase();
                            audio.PlayOneShot(buttonPressSound);
                        }
                    }
                }
            }
        }
    }

    void BaseGUI() {
        GUI.depth = 3;
        GUI.skin = Ourskin;
        // Track each interactive button's Rect
        Dictionary<ButtonType, Rect> rects = new Dictionary<ButtonType, Rect>();
        Rect dropRect = new Rect(0, 0, 0, 0);

        List<Rect> actionRects = new List<Rect>();

        // Draw Background
        //GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/marsbackground_01") as Texture);
        switch (currentBase.baseNumber) {
            case MarsBase.BaseNumber.baseOne:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level1") as Texture);
                break;
            case MarsBase.BaseNumber.baseTwo:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level2") as Texture);
                break;
            case MarsBase.BaseNumber.baseThree:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level3") as Texture);
                break;
            case MarsBase.BaseNumber.baseFour:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level4") as Texture);
                break;
            case MarsBase.BaseNumber.baseFive:
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Resources.Load("Textures/level5") as Texture);
                break;
        }
         
        // Context Panel
        if (currentBase.selectedRover == null) {
            string title = "";
            switch (currentBase.baseNumber) {
                case MarsBase.BaseNumber.baseOne:
                    title = "alpha";
                    break;
                case MarsBase.BaseNumber.baseTwo:
                    title = "beta";
                    break;
                case MarsBase.BaseNumber.baseThree:
                    title = "gamma";
                    break;
                case MarsBase.BaseNumber.baseFour:
                    title = "delta";
                    break;
                case MarsBase.BaseNumber.baseFive:
                    title = "epsilon";
                    break;
            }
            GUI.Box(new Rect(Screen.width - 260, 10, 250, 520), "mining base " + title);
            rects[ButtonType.rover] = new Rect(Screen.width - 260 + 250 / 2 - 15 + 30, 30 + 264 + 100, 30, 30);

            GUI.skin = null;
            GUI.Box(new Rect(Screen.width - 250, 50, 230, 100), GUIContent.none);
            GUI.Box(new Rect(Screen.width - 250, 170, 230, 130), GUIContent.none);
            GUI.Box(new Rect(Screen.width - 250, 330, 230, 120), GUIContent.none);
            GUI.skin = Ourskin;
            //GUI.Label(new Rect(Screen.width - 250 + 115 - 25, 55, 80, 30), "stats");
            GUI.Label(new Rect(Screen.width - 250 + 40, 58, 220, 30), "IRON: " + colonyManager.iron.ToString());
            GUI.Label(new Rect(Screen.width - 250 + 40, 78, 220, 30), "MONEY: $" + colonyManager.money.ToString());
            GUI.Label(new Rect(Screen.width - 250 + 40, 98, 220, 30), "IRON SOLD: " + colonyManager.ironSoldSinceAudit.ToString());
            GUI.Label(new Rect(Screen.width - 250 + 40, 118, 240, 30), "IRON GOAL: " + colonyManager.auditGoal.ToString());

            if (currentBase.selectedBuilding == null) {
                GUI.Label(new Rect(Screen.width - 250 + 30, 360, 300, 60), "buy rover\n$" + colonyManager.costs[ColonyManager.ShopItems.rover].ToString());
                if (colonyManager.money < colonyManager.costs[ColonyManager.ShopItems.rover])
                    GUI.color = new Color(1f, 1f, 1f, 0.5f);
                GUI.DrawTexture(rects[ButtonType.rover], buttonTextures[ButtonType.rover]);
                GUI.color = Color.white;
            }
            else {
                if (currentBase.selectedBuilding.buildingType == MarsBase.Building.BuildingType.mine) {
                    Texture drillTex;
                    if(currentBase.selectedBuilding.buildingLevel != 3)
                        drillTex = drillTextures[currentBase.selectedBuilding.buildingLevel + 1];
                    else
                        drillTex = drillTextures[currentBase.selectedBuilding.buildingLevel];
                    Rect bRect = new Rect(Screen.width - 245, 335, drillTex.width, drillTex.height);
                    GUI.DrawTexture(bRect, drillTex);
                }
                else if (currentBase.selectedBuilding.buildingType == MarsBase.Building.BuildingType.processingPlant) {
                    Texture refineryTex;
                    if (currentBase.selectedBuilding.buildingLevel != 3)
                        refineryTex = refineryTextures[currentBase.selectedBuilding.buildingLevel + 1];
                    else
                        refineryTex = refineryTextures[currentBase.selectedBuilding.buildingLevel];
                    Rect bRect = new Rect(Screen.width - 235, 350, refineryTex.width, refineryTex.height);
                    GUI.DrawTexture(bRect, refineryTex);
                }
                int cost = 0;
                if (currentBase.selectedBuilding.buildingLevel == 1) cost = colonyManager.costs[ColonyManager.ShopItems.firstUpgrade];
                else if (currentBase.selectedBuilding.buildingLevel == 2) cost = colonyManager.costs[ColonyManager.ShopItems.secondUpgrade];

                GUI.Label(new Rect(Screen.width - 90, 390, 80, 30), ((cost == 0) ? "" : "$" + cost.ToString()));

                GUI.enabled = (currentBase.selectedBuilding.buildingLevel < 3 && colonyManager.money >= cost);
                if (GUI.Button(new Rect(Screen.width - 110, 420, 84, 24), "Upgrade")) {
                    currentBase.selectedBuilding.buildingLevel++;
                }
                GUI.enabled = true;
            }

            GUI.Box(new Rect(Screen.width - 260 + 25, 200, 200, 40), GUIContent.none);
            GUI.Box(new Rect(Screen.width - 260 + 25, 250, 200, 40), GUIContent.none);
            GUI.Label(new Rect(Screen.width - 250 + 115 - 25, 170, 80, 30), "timers");

            GUI.color = Color.Lerp(Color.red, Color.green, (colonyManager.currentSpaceElevatorTime / colonyManager.spaceElevatorTime));
            GUI.Box(new Rect(Screen.width - 260 + 25 + 6, 200 + 6, (colonyManager.currentSpaceElevatorTime / colonyManager.spaceElevatorTime) * 188, 28), GUIContent.none, "ProgressBar");
            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width - 260 + 76, 208, 125, 40), "trade ship");

            GUI.color = Color.Lerp(Color.red, Color.green, (colonyManager.currentAuditTime / colonyManager.auditTime));
            GUI.Box(new Rect(Screen.width - 260 + 25 + 6, 250 + 6, (colonyManager.currentAuditTime / colonyManager.auditTime) * 188, 28), GUIContent.none, "ProgressBar");
            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width - 260 + 104, 258, 125, 40), "audit");
        }
        else {
            GUI.Box(new Rect(Screen.width - 260, 10, 250, 520), "PROGRAMMING");
            int size = 36, distance = 1, offset = 15, start = 260, yPos = 33;
            rects[ButtonType.arrowUp] = new Rect(Screen.width - start + offset, yPos, size, size);
            rects[ButtonType.arrowLeft] = new Rect(Screen.width - start + offset + size * 1 + distance * 1, yPos, size, size);
            rects[ButtonType.arrowRight] = new Rect(Screen.width - start + offset + size * 2 + distance * 2, yPos, size, size);
            rects[ButtonType.grab] = new Rect(Screen.width - start + offset + size * 3 + distance * 3, yPos, size, size);
            rects[ButtonType.drop] = new Rect(Screen.width - start + offset + size * 4 + distance * 4, yPos, size, size);
            rects[ButtonType.blank] = new Rect(Screen.width - start + offset + size * 5 + distance * 5, yPos, size, size);

            foreach (KeyValuePair<ButtonType, Rect> entry in rects) {
                GUI.DrawTexture(entry.Value, buttonTextures[entry.Key]);
            }

            dropRect = new Rect(Screen.width - 250, 80, 230, 410);
            GUI.skin = null;
            GUI.Box(dropRect, GUIContent.none);
            GUI.skin = Ourskin;

            GUILayout.BeginArea(new Rect(Screen.width - 235, 495, 200, 40));
            GUILayout.BeginHorizontal();
            Ourskin.GetStyle("Button").fontSize = 14;
            GUI.enabled = (!currentBase.running && !currentBase.crashed);
            if (GUILayout.Button("rotate")) {
                switch (currentBase.selectedRover.direction) {
                    case MarsBase.Direction.north:
                        currentBase.selectedRover.direction = MarsBase.Direction.west;
                        currentBase.selectedRover.startDirection = MarsBase.Direction.west;
                        break;
                    case MarsBase.Direction.east:
                        currentBase.selectedRover.direction = MarsBase.Direction.north;
                        currentBase.selectedRover.startDirection = MarsBase.Direction.north;
                        break;
                    case MarsBase.Direction.west:
                        currentBase.selectedRover.direction = MarsBase.Direction.south;
                        currentBase.selectedRover.startDirection = MarsBase.Direction.south;
                        break;
                    case MarsBase.Direction.south:
                        currentBase.selectedRover.direction = MarsBase.Direction.east;
                        currentBase.selectedRover.startDirection = MarsBase.Direction.east;
                        break;
                }
                audio.PlayOneShot(buttonPressSound);
            }
            GUI.enabled = currentBase.selectedRover.actionsSize > 0 && !currentBase.running && !currentBase.crashed;
            if (GUILayout.Button("clear")) {
                currentBase.selectedRover.ClearActions();
                audio.PlayOneShot(buttonPressSound);
            }
            GUI.enabled = true;
            GUI.enabled = (!currentBase.running && !currentBase.crashed);
            if (GUILayout.Button("sell")) {
                colonyManager.SellItem(ColonyManager.SellItems.rover);
                currentBase.RemoveRover(currentBase.selectedRover);
                currentBase.selectedRover = null;
                audio.PlayOneShot(buttonPressSound);
                return;
            }
            GUI.enabled = true;
            Ourskin.GetStyle("Button").fontSize = 0;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // Draw Selected Rover's Actions
            if (currentBase.selectedRover.actionsSize > 0) {
                ReadOnlyCollection<MarsBase.Rover.ActionType> actions = currentBase.selectedRover.actions;
                GUILayout.BeginArea(new Rect(Screen.width - 245, 85, 230, 400));
                if (actions.Count > 45)
                    scrollPosition = GUI.BeginScrollView(new Rect(0, 0, 235, 410), scrollPosition, new Rect(0, 0, 215, 410 + ((actions.Count - 41) / 5) * 45));
                int colPos = 0;
                int rowPos = 0;
                int actionDistance = 5;
                int actionSize = 40;

                for (int i = 0; i < actions.Count; i++) {
                    Rect newActionRect = new Rect((actionSize + actionDistance) * colPos, (actionSize + actionDistance) * rowPos, actionSize, actionSize);
                    actionRects.Add(new Rect((actionSize + actionDistance) * colPos + (Screen.width - 245), (actionSize + actionDistance) * rowPos + 85, actionSize, actionSize));
                    if (i == currentBase.selectedRover.currentActionIndex) {
                        if (currentBase.selectedRover.crashed) GUI.color = Color.red;
                        else if(currentBase.running) GUI.color = Color.green;
                    }
                    GUI.DrawTexture(newActionRect, buttonTextures[actionToButton[actions[i]]]);
                    GUI.color = Color.white;
                    colPos++;
                    if (colPos == 5) {
                        colPos = 0;
                        rowPos++;
                    }
                }
                if (actions.Count > 45)
                    GUI.EndScrollView();
                GUILayout.EndArea();
            }
        }

        // Buttons to change current scene or UI
        GUILayout.BeginArea(new Rect(Screen.width - 260, Screen.height - 64, 250, 30));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Main Base", GUILayout.Width(120))) {
            currentBase = null;
            currentGUI = ColonyGUI;
            audio.PlayOneShot(buttonPressSound);
            return;
        }
        if (GUILayout.Button("Menu", GUILayout.Width(120))) {
            Time.timeScale = 0;
            pausedGUI = currentGUI;
            currentGUI = PauseMenuGUI;
            audio.PlayOneShot(buttonPressSound);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width - 260 + 125 - 50, Screen.height - 32, 100, 30));
        if (!currentBase.running && !currentBase.crashed) {
            GUI.enabled = !currentBase.crashed;
            if (GUILayout.Button("start")) {
                currentBase.StartSim();
                if (!colonyManager.running) colonyManager.StartSim();
                audio.PlayOneShot(buttonPressSound);
            }
            GUI.enabled = true;
        }
        else if (currentBase.running) {
            if (GUILayout.Button("stop")) {
                currentBase.StopSim();
                currentBase.ResetBoard();
                audio.PlayOneShot(buttonPressSound);
            }
        }
        else {
            if (GUILayout.Button("reset")) {
                currentBase.ResetBoard();
                audio.PlayOneShot(buttonPressSound);
            }
        }
        GUILayout.EndArea();

        // Draw Board
        DrawBase();

        // Handle Custom Interaction
        if (!currentBase.running) {
            bool crashCheck = true;
            if (currentBase.selectedRover != null) crashCheck = !currentBase.selectedRover.crashed;
            if (crashCheck) {
                if (!dragging && Event.current.type == EventType.mouseDown) {
                    // Start to drag buy rover
                    foreach (KeyValuePair<ButtonType, Rect> entry in rects) {
                        if (entry.Value.Contains(Event.current.mousePosition)) {
                            if (entry.Key == ButtonType.rover) {
                                if (colonyManager.money >= colonyManager.costs[ColonyManager.ShopItems.rover]) {
                                    dragging = true;
                                    draggingButton = entry.Key;
                                    break;
                                }
                            }
                            else {
                                dragging = true;
                                draggingButton = entry.Key;
                                break;
                            }
                        }
                    }
                    // Remove Actions
                    if (currentBase.selectedRover != null) {
                        for (int i = 0; i < actionRects.Count; i++) {
                            if (actionRects[i].Contains(Event.current.mousePosition)) {
                                currentBase.selectedRover.RemoveAction(i);
                                audio.PlayOneShot(dragClick);
                                break;
                            }
                        }
                    }
                }
                else if (dragging && Event.current.type == EventType.mouseUp) {
                    dragging = false;
                    if (currentBase.selectedRover != null) {
                        bool missedActions = true;
                        for (int i = 0; i < actionRects.Count; i++) {
                            if (actionRects[i].Contains(Event.current.mousePosition)) {
                                currentBase.selectedRover.AddActionAt(buttonToAction[draggingButton], i);
                                audio.PlayOneShot(dragClick);
                                missedActions = false;
                                break;
                            }
                        }
                        if (missedActions) {
                            if (dropRect.Contains(Event.current.mousePosition)) {
                                currentBase.selectedRover.AddAction(buttonToAction[draggingButton]);
                                audio.PlayOneShot(dragClick);
                            }
                        }
                    }
                    else {
                        // Drop the buy rover
                        Vector2 pos = Event.current.mousePosition;

                        int x = Mathf.FloorToInt(pos.x / 30);
                        int y = Mathf.FloorToInt(pos.y / 30) + 1;

                        if (x >= 0 && x < MarsBase.GRID_WIDTH && y >= 0 && y <= MarsBase.GRID_HEIGHT) {
                            if (currentBase.board[x, MarsBase.GRID_HEIGHT - y].tileType == MarsBase.GridTile.TileType.open) {
                                currentBase.BuyPart(draggingButton, x, MarsBase.GRID_HEIGHT - y);
                                // Assume it's a rover for now
                                colonyManager.BuyItem(ColonyManager.ShopItems.rover);
                                audio.PlayOneShot(dragClick);
                            }
                        }
                    }
                }
            }
        }

        // Draw Dragged Object
        if (dragging) {
            Vector2 mousePosition = Event.current.mousePosition;
            Rect origRect = rects[draggingButton];
            Rect dragRect = new Rect(mousePosition.x - origRect.width / 2, mousePosition.y - origRect.height / 2, origRect.width, origRect.height);
            GUI.DrawTexture(dragRect, buttonTextures[draggingButton]);
        }
    }

    void DrawBase() {
        //GUI.skin = Ourskin;
        Dictionary<Rect, MarsBase.Rover> roverRects = new Dictionary<Rect, MarsBase.Rover>();
        Dictionary<Rect, MarsBase.Building> buildingRects = new Dictionary<Rect, MarsBase.Building>();

        GUILayout.BeginArea(new Rect(-1, -1, 691, 600));
        // Iterate through the grid
        for (int j = MarsBase.GRID_HEIGHT - 1; j >= 0; j--) {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < MarsBase.GRID_WIDTH; i++) {
                switch (currentBase.board[i, j].tileType) {
                    case MarsBase.GridTile.TileType.open:
                        GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
                        GUI.Box(new Rect(30 * i, 30 * (MarsBase.GRID_HEIGHT - j - 1), 31, 31), GUIContent.none, "Grid");
                        GUI.color = Color.white;
                        break;
                    case MarsBase.GridTile.TileType.building:
                        if (currentBase.board[i, j].buildingSprite) {
                            MarsBase.Building building = currentBase.board[i, j].building;
                            if (building.buildingType == MarsBase.Building.BuildingType.mine) {
                                Texture drillTex = drillTextures[building.buildingLevel];
                                Rect newBuildingRect =new Rect((30 * (i - 4)) + (180 - drillTex.width) / 2, (30 * (MarsBase.GRID_HEIGHT - j - 3)) + (120 - drillTex.height) / 2, drillTex.width, drillTex.height);
                                buildingRects[newBuildingRect] = building;
                                GUI.DrawTexture(newBuildingRect, drillTex);
                            }
                            else if (building.buildingType == MarsBase.Building.BuildingType.processingPlant) {
                                Texture refineryTex = refineryTextures[building.buildingLevel];
                                Rect newBuildingRect = new Rect((30 * (i - 3)) + (120 - refineryTex.width) / 2, (30 * (MarsBase.GRID_HEIGHT - j - 3)) + (90 - refineryTex.height) / 2 + 5, refineryTex.width, refineryTex.height);
                                buildingRects[newBuildingRect] = building;
                                GUI.DrawTexture(newBuildingRect, refineryTex);
                            }
                            else if (building.buildingType == MarsBase.Building.BuildingType.tramStation) {
                                Texture tramTexture = Resources.Load("Textures/tram_small") as Texture;
                                Texture tramCarTexture = Resources.Load("Textures/TramCar_Empty") as Texture;
                                Rect newBuildingRect = new Rect((30 * (i - 1)) + 10, (30 * (MarsBase.GRID_HEIGHT - j - 1)), tramTexture.width, tramTexture.height);
                                //buildingRects[newBuildingRect] = building;
                                GUI.DrawTexture(newBuildingRect, tramTexture);
                                GUI.DrawTexture(new Rect((30 * (i - 1)) + 6 + 26, (30 * (MarsBase.GRID_HEIGHT - j - 1)) + 22, tramCarTexture.width, tramCarTexture.height), tramCarTexture);
                            }
                        }
                        break;    
                    case MarsBase.GridTile.TileType.wall:
                        break;
                    case MarsBase.GridTile.TileType.rover:
                        MarsBase.Rover rover = currentBase.board[i, j].rover;
                        MarsBase.Direction direction = rover.direction;

                        if (currentBase.selectedRover == rover) {
                            GUI.color = new Color(0f, 1f, 0f, 0.5f);
                            GUI.Box(new Rect(30 * i, 30 * (MarsBase.GRID_HEIGHT - j - 1), 31, 31), GUIContent.none, "Grid");
                            GUI.color = Color.white;
                        }
                        else {
                            GUI.color = new Color(1f, 1f, 1f, 0.1f);
                            GUI.Box(new Rect(30 * i, 30 * (MarsBase.GRID_HEIGHT - j - 1), 31, 31), GUIContent.none, "Grid");
                            GUI.color = Color.white;
                        }
                        Rect newRoverRect = new Rect((30 * i) + 1, (30 * (MarsBase.GRID_HEIGHT - j - 1)) + 2, 29, 28);
                        roverRects[newRoverRect] = rover;
                        if (rover.crashed) GUI.color = Color.red;
                        switch (direction) {
                            case MarsBase.Direction.north:
                                GUI.DrawTexture(newRoverRect, buttonTextures[ButtonType.rover]);
                                break;
                            case MarsBase.Direction.east:
                                GUIUtility.RotateAroundPivot(90, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                GUI.DrawTexture(newRoverRect, buttonTextures[ButtonType.rover]);
                                GUIUtility.RotateAroundPivot(-90, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                break;
                            case MarsBase.Direction.south:
                                GUIUtility.RotateAroundPivot(180, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                GUI.DrawTexture(newRoverRect, buttonTextures[ButtonType.rover]);
                                GUIUtility.RotateAroundPivot(-180, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                break;
                            case MarsBase.Direction.west:
                                GUIUtility.RotateAroundPivot(-90, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                GUI.DrawTexture(newRoverRect, buttonTextures[ButtonType.rover]);
                                GUIUtility.RotateAroundPivot(90, new Vector2(newRoverRect.x + newRoverRect.width / 2, newRoverRect.y + newRoverRect.height / 2));
                                break;
                        }
                        GUI.color = new Color(1f, 1f, 1f, 1f);

                        // Draw Resource Icons
                        if (rover.resource == MarsBase.ResourceType.rawIron || rover.resource == MarsBase.ResourceType.doubleRawIron || rover.resource == MarsBase.ResourceType.tripleRawIron) {
                            Rect iconRect = new Rect(newRoverRect.x, newRoverRect.y, 16, 16);
                            GUI.DrawTexture(iconRect, Resources.Load("Textures/rock_icon") as Texture);
                        }
                        else if (rover.resource == MarsBase.ResourceType.refinedIron || rover.resource == MarsBase.ResourceType.doubleRefinedIron || rover.resource == MarsBase.ResourceType.tripleRefinedIron) {
                            Rect iconRect = new Rect(newRoverRect.x, newRoverRect.y, 16, 16);
                            GUI.DrawTexture(iconRect, Resources.Load("Textures/iron_icon") as Texture);
                        }
                        break;
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();

        if (Event.current.type == EventType.mouseDown) {
            bool foundRover = false;
            foreach (KeyValuePair<Rect, MarsBase.Rover> entry in roverRects) {
                if (entry.Key.Contains(Event.current.mousePosition)) {
                    foundRover = true;
                    if (currentBase.selectedRover != entry.Value) {
                        currentBase.selectedRover = entry.Value;
                        currentBase.selectedBuilding = null;
                    }
                    else currentBase.selectedRover = null;
                    break;
                }
            }
            if (!foundRover) {
                foreach (KeyValuePair<Rect, MarsBase.Building> entry in buildingRects) {
                    if (entry.Key.Contains(Event.current.mousePosition)) {
                        if (currentBase.selectedBuilding != entry.Value) {
                            currentBase.selectedBuilding = entry.Value;
                            currentBase.selectedRover = null;
                        }
                        else currentBase.selectedBuilding = null;
                    }
                }
            }
        }
    }
}