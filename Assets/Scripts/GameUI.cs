using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    public static GameUI Instance { set; get; }
    [SerializeField] private Server server;
    [SerializeField] private Client client;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject prevButton;
    [SerializeField] private GameObject scrollArea;
    private GameObject controller;
    private Grid grid;
    // Start is called before the first frame update
    void Start()
    {
        //get object references
        controller = GameObject.FindGameObjectWithTag("GameController");
        grid = controller.GetComponent<Grid>();
    }

    private void Awake()
    {
        Instance = this;
    }
    // Button presses
    public void replayGamePress()
    {
        menuAnimator.SetTrigger("replayGame");
        grid.replayGame();
    }
    public void replayGameMainMenuPress()
    {
        menuAnimator.SetTrigger("MainMenu");
        //Destroy all assets and clear the movesPlayed
        cleanUp();
    }
    public void gameOverMainMenuPress()
    {
        menuAnimator.SetTrigger("MainMenu");
        shutDownClientAndServer();
        grid.destroyAssets();
    }
    public void globalMainMenuPress()
    {
        menuAnimator.SetTrigger("MainMenu");
        //Clean up all resources
        cleanUp();
    }
    public void localGamePress()
    {
        menuAnimator.SetTrigger("NoMenu");
        grid.startGame();
    }
    public void onlineGamePress()
    {
        menuAnimator.SetTrigger("HostMenu");

    }
    public void aiGamePress()
    {
        menuAnimator.SetTrigger("aiMenu");
    }
    public void trivialGamePress()
    {
        grid.setDepth(1);
        startAiGame();
    }
    public void mediumGamePress()
    {
        grid.setDepth(2);
        startAiGame();
    }
    public void hardGamePress()
    {
        grid.setDepth(4);
        startAiGame();
    }
    public void aiMenuBackPress()
    {
        menuAnimator.SetTrigger("MainMenu");
    }
    private void startAiGame()
    {
        menuAnimator.SetTrigger("noMenu");
        grid.setComputerPlayer("black");
        grid.startGame();
    }
    public void hostGamePress()
    {
        server.init(8007);
        client.init(8007, "127.0.0.1");
        menuAnimator.SetTrigger("ConnectionMenu");
    }

    public void connectPress()
    {
        client.shutdown();
        client.init(8007, "127.0.0.1");
    }
    public void onlineMenuBack()
    {
        menuAnimator.SetTrigger("MainMenu");
    }
    public void connectionMenuBack()
    {
        shutDownClientAndServer();
        grid.resetNumPlayers();
        menuAnimator.SetTrigger("HostMenu");
    }
    private void shutDownClientAndServer()
    {
        Client.Instance.shutdown();
        Server.Instance.shutdown();
    }
    private void cleanUp()
    {
        shutDownClientAndServer();
        //Destroy all the ReplayMove gameobjects
        grid.destroyMoves(0);
        grid.clearMovesPlayed();
        grid.destroyAssets();
    }
}
