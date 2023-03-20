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
    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
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
        controller.GetComponent<Grid>().replayGame();
    }
    public void replayGameMainMenuPress()
    {
        menuAnimator.SetTrigger("MainMenu");
        // nextButton.SetActive(true);
        // prevButton.SetActive(true);
        // scrollArea.SetActive(true);
        controller.GetComponent<Grid>().destroyAssets();
    }
    public void gameOverMainMenuPress()
    {
        menuAnimator.SetTrigger("MainMenu");
        Client.Instance.shutdown();
        Server.Instance.shutdown();
        controller.GetComponent<Grid>().destroyAssets();
    }
    public void localGamePress()
    {
        menuAnimator.SetTrigger("NoMenu");
        // server.init(8007);
        // client.init(8007,"127.0.0.1" );
        controller.GetComponent<Grid>().startGame();
    }
    public void onlineGamePress()
    {
        menuAnimator.SetTrigger("HostMenu");

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
