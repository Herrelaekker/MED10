using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public struct GameData {
    public int trials;
    public float interTrialIntervalSeconds;
    public float inputWindowSeconds;
    public GameState gameState;
    public float noInputReceivedFabAlarm;
    public float fabAlarmVariability;
}

public class Mechanism {
    public string name = "";
    public TrialType trialType;
    public float rate = -1f;
    public int trialsLeft = -1;
    public int trials = -1;
    public UrnEntryBehavior behavior;
}

public class GameDecisionData {
    public TrialType decision;
    public MotorImageryEvent classification;
    public float currentFabAlarm;
}

public struct GameTimers {
    public float inputWindowTimer;
    public float interTrialTimer;
}

public enum InputWindowState {
    Closed,
    Open,
}

public enum GameState {
    Running,
    Paused,
    Stopped,
}

public enum TrialType  {
     AccInput,
     RejInput,
}

public class GameManager : MonoBehaviour
{

    [Header("Trial Setup")]
	[Tooltip("The total number of trials is calculated from the trial counts set here.")]
    public int accTrials = 10;
    public int rejTrials = 0;

    private int trialsTotal = -1;
    private int currentTrial = -1;
    private TrialType trialResult = TrialType.RejInput;

    private Dictionary<string, Mechanism> mechanisms = new Dictionary<string, Mechanism>();

    private bool alarmFired = false;
    private MotorImageryEvent classification;

    [Header("InputWindow Settings")]
    [Tooltip("Length of Window and Inter-trial interval.")]
    [SerializeField]
    private float interTrialIntervalSeconds = 4.5f;
    [SerializeField]
    private float inputWindowSeconds = 1f;
    private float inputWindowTimer = 0.0f;
    private float interTrialTimer = 0.0f;
    private InputWindowState inputWindow = InputWindowState.Closed;
    private int inputIndex = 0;

    private GameState gameState = GameState.Stopped;

    [Serializable]
    public class OnGameStateChanged : UnityEvent<GameData> { }
    public OnGameStateChanged onGameStateChanged;
    [Serializable]
    public class GameDecision : UnityEvent<GameDecisionData> { }
    public GameDecision gameDecision;

    [Serializable]
    public class OnInputWindowChanged : UnityEvent<InputWindowState> { }
    public OnInputWindowChanged onInputWindowChanged;

    [Serializable]
    public class OnGameTimeUpdate : UnityEvent<GameTimers> { }
    public OnGameTimeUpdate onGameTimeUpdate;

    private LoggingManager loggingManager;
    private UrnModel urn;

    InputData bestInputData = null;

    SimBCIInput fabBCIInput;
    OpenBCIInput openBCIInput;

    void Start()
    {
        loggingManager = GameObject.Find("LoggingManager").GetComponent<LoggingManager>();
        urn = GetComponent<UrnModel>();
        SetupMechanisms();
        SetupUrn();
        LogMeta();

        fabBCIInput = FindObjectOfType<SimBCIInput>();
        openBCIInput = FindObjectOfType<OpenBCIInput>();
    }

    private void SetupMechanisms() {
        mechanisms["AccInput"] = new Mechanism {
            name = "AccInput",
            trialType = TrialType.AccInput,
            rate = 0f,
            trials = accTrials,
            trialsLeft = accTrials,
            behavior = UrnEntryBehavior.Persist
        };
        mechanisms["RejInput"] = new Mechanism
        {
            name = "RejInput",
            trialType = TrialType.RejInput,
            rate = 0f,
            trials = rejTrials,
            trialsLeft = rejTrials,
            behavior = UrnEntryBehavior.Override
        };
    }

    private void SetupUrn() {
        trialsTotal = 0;
        foreach (KeyValuePair<string, Mechanism> pair in mechanisms) {
            var m = pair.Value;
            urn.AddUrnEntryType(m.name, m.behavior, m.trials);
            trialsTotal += m.trials;
        }

        urn.NewUrn();
        currentTrial = 0;
    }

    private void LogMeta() {
        Dictionary<string, object> metaLog = new Dictionary<string, object>() {
            {"AccInputTrials", accTrials},
            {"Trials", trialsTotal},
            {"InterTrialInterval_sec", interTrialIntervalSeconds},
            {"InputWindow_sec", inputWindowSeconds},
        };
        loggingManager.Log("Meta", metaLog);
    }

    private void LogEvent(string eventLabel) {
        Dictionary<string, object> gameLog = new Dictionary<string, object>() {
            {"Event", eventLabel},
            {"InputWindow", System.Enum.GetName(typeof(InputWindowState), inputWindow)},
            {"InputWindowOrder", inputIndex},
            {"GameState", System.Enum.GetName(typeof(GameState), gameState)},
        };

        foreach (KeyValuePair<string, Mechanism> pair in mechanisms) {
            var m = pair.Value;
            gameLog[m.name + "TrialsLeft"] = m.trialsLeft;
            gameLog[m.name + "Rate"] = m.rate;
        }

        if (eventLabel == "GameDecision") {
            gameLog["TrialResult"] = trialResult;
        } else {
            gameLog["TrialResult"] = "NA";
        }

        loggingManager.Log("Game", gameLog);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GameState.Running) {
            if (inputWindow == InputWindowState.Closed) {
                alarmFired = false;
                interTrialTimer += Time.deltaTime;
                if (interTrialTimer > interTrialIntervalSeconds && currentTrial < trialsTotal) {
                    interTrialTimer = 0f;
                    inputWindow = InputWindowState.Open;
                    //SetFabAlarmVariability();
                    onInputWindowChanged.Invoke(inputWindow);
                    LogEvent("InputWindowChange");
                } else if (interTrialTimer > interTrialIntervalSeconds) {
                    EndGame();
                }
            } else if (inputWindow == InputWindowState.Open) {
                //Debug.Log("inputwindow is open");
                inputWindowTimer += Time.deltaTime;
                
                if (inputWindowTimer > inputWindowSeconds) {

                    MakeInputDecision(bestInputData, true);

                    bestInputData = null;
                    curClassification = MotorImageryEvent.Rest;
                    alarmFired = false;
                }
            }
        }
        GameTimers gameTimers = new GameTimers();
        gameTimers.interTrialTimer = interTrialTimer;
        gameTimers.inputWindowTimer = inputWindowTimer;
        onGameTimeUpdate.Invoke(gameTimers);
    }

    public GameData createGameData() {
            GameData gameData = new GameData();
            gameData.trials = trialsTotal;
            gameData.interTrialIntervalSeconds = interTrialIntervalSeconds;
            gameData.inputWindowSeconds = inputWindowSeconds;
            gameData.gameState = gameState;
            return gameData;
    }

    void OnApplicationQuit() {
        if(gameState != GameState.Stopped) {
            EndGame();
        }
    }

    public void RunGame() {
        CalculateRecogRate();
        //gameState = GameState.Running;
        GameData gameData = createGameData();
        onGameStateChanged.Invoke(gameData);
        LogEvent("GameRunning");
    }

    public void EndGame() {
        interTrialTimer = 0f;
        if (inputWindow == InputWindowState.Open) {
            CloseInputWindow();
        }
        gameState = GameState.Stopped;
        GameData gameData = createGameData();
        onGameStateChanged.Invoke(gameData);
        LogEvent("GameStopped");
        loggingManager.SaveLog("Game");
        loggingManager.SaveLog("Sample");
        loggingManager.SaveLog("Meta");
        loggingManager.ClearAllLogs();
    }

    public void CalculateRecogRate() {
        var entriesLeft = urn.GetEntriesLeft();

        foreach (KeyValuePair<string, Mechanism> pair in mechanisms) {
            var m = pair.Value;
            m.trialsLeft = entriesLeft[m.name];
        }
        
        var entryResults = urn.GetEntryResults();

        foreach (KeyValuePair<string, Mechanism> pair in mechanisms) {
            var m = pair.Value;
            m.rate = (float) entryResults[m.name] / (float) trialsTotal;
        }

        currentTrial = urn.GetIndex();
    }

    MotorImageryEvent curClassification;
    public void OnInputReceived(InputData inputData) {
        if (bestInputData != null)
            if (bestInputData.classification == MotorImageryEvent.GoldenMotorImagery)
                return;
        curClassification = inputData.classification;

        if (inputWindow == InputWindowState.Closed) {
            // ignore the input.
            return;
        } else {
            if (inputData.classification == MotorImageryEvent.MotorImagery || inputData.classification == MotorImageryEvent.GoldenMotorImagery)
            {
                bestInputData = inputData;
                if (fabBCIInput.enabled)
                {
                    fabBCIInput.SaveMI(curClassification);
                }
                if (openBCIInput.enabled)
                {
                    openBCIInput.SaveMI(curClassification);
                }
            }
            //MakeInputDecision(inputData);
        }
    }

    public void CloseInputWindow() {
        // update the window state.
        inputWindow = InputWindowState.Closed;
        interTrialTimer = 0; //-= (inputWindowSeconds - inputWindowTimer);
        inputWindowTimer = 0f;
        onInputWindowChanged.Invoke(inputWindow);

        if (fabBCIInput.enabled)
        {
            fabBCIInput.SaveMI(MotorImageryEvent.Rest);
        }
        if (openBCIInput.enabled)
        {
            openBCIInput.SaveMI(MotorImageryEvent.Rest);
        }

        LogEvent("InputWindowChange");

        // store the input decision.
        urn.SetEntryResult(System.Enum.GetName(typeof(TrialType), trialResult));

        CalculateRecogRate();
        // Send Decision Data
        GameDecisionData gameDecisionData = new GameDecisionData();
        gameDecisionData.decision = trialResult;
        gameDecisionData.classification = classification;
        gameDecision.Invoke(gameDecisionData);
        LogEvent("GameDecision");
       ////Debug.Log("designedInputOrder: " + designedInputOrder.Count);
       ////Debug.Log("actualInputOrder: " + actualInputOrder.Count);
       ////Debug.Log("Decision: " + System.Enum.GetName(typeof(InputTypes), currentInputDecision));
        //UpdateDesignedInputOrder();
        inputIndex++;
    }

    public void MakeInputDecision(InputData inputData = null, bool windowExpired = false) {

        if (fabBCIInput.enabled)
        {
            fabBCIInput.LogMotorImageryEvent();
        }
        if (openBCIInput.enabled)
        {
            openBCIInput.LogMotorImageryEvent();
        }

        string entry = urn.ReadEntry();
        trialResult = TrialType.RejInput;

        print("trialresult " + trialResult);

        if (inputData != null)
        {
            classification = inputData.classification;

            if (inputData.validity == InputValidity.Accepted)
            {
                trialResult = TrialType.AccInput;
            }
        }
        else
        {
            classification = MotorImageryEvent.Rest;
        }

        CloseInputWindow();
    }

    public void PauseTrial() {
        gameState = GameState.Paused;
    }

    public void ResetTrial() {
        inputWindowTimer = 0f;
        interTrialTimer = 0.001f;
        inputWindow = InputWindowState.Closed;
    }

    public void ResumeTrial() {
        gameState = GameState.Running;
    }

    public void SetInputWindowSeconds(float time) {
        inputWindowSeconds = time;
        GameData gameData = createGameData();
        onGameStateChanged.Invoke(gameData);        
    }

    public void SetInterTrialSeconds(float time) {
        interTrialIntervalSeconds = time;
        GameData gameData = createGameData();
        onGameStateChanged.Invoke(gameData);
    }

    public float GetInterTrialSeconds()
    {
        return interTrialIntervalSeconds;
    }

}
