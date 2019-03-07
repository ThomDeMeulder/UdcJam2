using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public sealed class Player : Singleton<Player>, IEventListener<PlayerTeleportEvent>
{
    [Header("Game Settings")]
    [SerializeField]
    private int startGameAfterAmount = 5;
    [SerializeField]
    private Text countdownText;
    [SerializeField]
    private Text buttonClickText;
    [SerializeField]
    private float textDistance = 5.0f;
    [SerializeField]
    private float yOffset = 2.0f;
    [SerializeField]
    private float gameLength = 60.0f;
    [SerializeField]
    private float countdownFrom = 5.0f;

    public int Points { get; set; }
    public GameState GameState { get; private set; }

    public void AddPoints(int points) => Points = Points + points;

    private Transform mainCamera;
    private int[] skippedControllerValues = { 16, 17, 14, 15, 19, 11, 10, 12, 13, 18 };
    private Vector3 buttonClickVelocity = Vector3.zero;
    private bool firstButtonClickTextFrame = true;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
        XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
        EventManager<PlayerTeleportEvent>.AddListener(this);
    }

    private void Update()
    {
        if (GameState == GameState.Menu)
        {
            if (firstButtonClickTextFrame)
            {
                firstButtonClickTextFrame = false;
                UpdateTextRotationAndPositionOnce(buttonClickText);
            }
            UpdateTextRotationAndPosition(buttonClickText, ref buttonClickVelocity);

            if (AnyKeyDown())
            {
                buttonClickText.gameObject.SetActive(false);
                countdownText.gameObject.SetActive(true);
                GameState = GameState.PreStart;
                EventManager<PreGameStartEvent>.CallEvent(new PreGameStartEvent());

                StartCoroutine(StartGameBeginCountdown(StartCoroutine(UpdateCountdownText())));
            }
        }
    }

    private bool AnyKeyDown()
    {
        for (var i = 0; i < 20; i++)
        {
            if (IsWrongButtonNumber(i)) continue;
            if (Input.GetKeyDown("joystick button " + i) || Input.GetKeyDown("joystick 1 button " + i)) return true;
        }
        return false;
    }

    private bool IsWrongButtonNumber(int number)
    {
        for (var i = 0; i < skippedControllerValues.Length; i++)
        {
            if (skippedControllerValues[i] == number) return true;
        }
        return false;
    }

    private IEnumerator StartGameBeginCountdown(Coroutine startTextUpdater)
    {
        for (var i = startGameAfterAmount; i >= 0; i--)
        {
            countdownText.text = i == 0 ? "Go!" : i.ToString();
            yield return new WaitForSeconds(1);
        }

        GameState = GameState.Start;
        countdownText.gameObject.SetActive(false);
        StopCoroutine(startTextUpdater);
        StartCoroutine(StartGameEndCountdown());
        EventManager<GameStartEvent>.CallEvent(new GameStartEvent());
    }

    private IEnumerator UpdateCountdownText()
    {
        var velocity = Vector3.zero;
        UpdateTextRotationAndPositionOnce(countdownText);

        while (true)
        {
            UpdateTextRotationAndPosition(countdownText, ref velocity);
            yield return null;
        }
    }

    private void UpdateTextRotationAndPosition(Text text, ref Vector3 velocity)
    {
        var textTransform = text.transform;
        textTransform.forward = Vector3.SmoothDamp(textTransform.forward, mainCamera.transform.forward, ref velocity, 0.2f);
        textTransform.position = transform.position - -textTransform.forward * textDistance;

        var position = textTransform.position;
        position.y += yOffset;
        textTransform.position = position;
    }

    public void Teleport(Vector3 position, float rotation)
    {
        transform.position = position;
        transform.Rotate(0.0f, rotation, 0.0f);
    }

    private void UpdateTextRotationAndPositionOnce(Text text)
    {
        var tempTransform = text.transform;
        tempTransform.forward = mainCamera.transform.forward;
        tempTransform.position = transform.position - -tempTransform.forward * textDistance;
    }

    private IEnumerator StartGameEndCountdown()
    {
        yield return new WaitForSeconds(gameLength);

        GameState = GameState.PreEnd;
        firstButtonClickTextFrame = true;
        countdownText.gameObject.SetActive(true);
        var coroutine = StartCoroutine(UpdateCountdownText());
        EventManager<PreGameEndEvent>.CallEvent(new PreGameEndEvent());

        for (var i = startGameAfterAmount; i >= 0; i--)
        {
            countdownText.text = i == 0 ? "Stop!" : i.ToString();
            if (i == 0)
            {
                GameState = GameState.End;
                EventManager<GameEndEvent>.CallEvent(new GameEndEvent());
                yield return new WaitForSeconds(3.0f);
            }
            else yield return new WaitForSeconds(1.0f);
        }
        
        countdownText.text = $"Score:\n{Points}";

        yield return new WaitForSeconds(6.0f);

        GameState = GameState.Menu;
        countdownText.gameObject.SetActive(false);
        buttonClickText.gameObject.SetActive(true);
        StopCoroutine(coroutine);
    }

    public void OnEvent(PlayerTeleportEvent eventData)
    {
        if (countdownText.IsActive()) UpdateTextRotationAndPositionOnce(countdownText);
        if (buttonClickText.IsActive()) UpdateTextRotationAndPositionOnce(buttonClickText);
    }
}

public enum GameState
{
    Menu,
    PreStart,
    Start,
    PreEnd,
    End
}
