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
        //menuAnimator.SetTrigger("NoMenu");
        menuAnimator.SetTrigger("replayGame");
        // nextButton.SetActive(true);
        // prevButton.SetActive(true);
        // scrollArea.SetActive(true);
        grid.replayGame();
    }
    public void replayGameMainMenuPress()
    {
        menuAnimator.SetTrigger("MainMenu");
        // nextButton.SetActive(true);
        // prevButton.SetActive(true);
        // scrollArea.SetActive(true);
        grid.destroyMoves(0);
        grid.clearMovesPlayed();
        grid.destroyAssets();
    }
    public void gameOverMainMenuPress()
    {
        menuAnimator.SetTrigger("MainMenu");
        Client.Instance.shutdown();
        Server.Instance.shutdown();
        grid.destroyAssets();
    }
    public void localGamePress()
    {
        menuAnimator.SetTrigger("NoMenu");
        // server.init(8007);
        // client.init(8007,"127.0.0.1" );
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
        //menuAnimator.SetTrigger("NoMenu");
    }
    //TODO
    //Change to the input field
    public void connectPress()
    {
        client.init(8007, "127.0.0.1");

    }
    public void onlineMenuBack()
    {
        menuAnimator.SetTrigger("MainMenu");
    }
    public void connectionMenuBack()
    {
        server.shutdown();
        client.shutdown();
        menuAnimator.SetTrigger("HostMenu");
    }
}
