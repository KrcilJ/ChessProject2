using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Animator menuAnimator;
     [SerializeField] private TMP_InputField addressInput;
    public static GameUI Instance {set; get;}

    [SerializeField] private Server server;
    [SerializeField] private Client client;
    
    public GameObject controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
       Instance = this; 
    }

    // Button presses
    public void localGamePress(){
        menuAnimator.SetTrigger("NoMenu");
        // server.init(8007);
        // client.init(8007,"127.0.0.1" );
        controller.GetComponent<Grid>().startGame();
    }
     public void onlineGamePress(){
        menuAnimator.SetTrigger("HostMenu");
       
    }
      public void hostGamePress(){
        server.init(8007);
        client.init( 8007 ,"127.0.0.1");
        menuAnimator.SetTrigger("ConnectionMenu");
        //menuAnimator.SetTrigger("NoMenu");
    }
    //TODO
    //Change to the input field
     public void connectPress(){
        client.init( 8007 ,"127.0.0.1");
        
    }
      public void onlineMenuBack(){
        menuAnimator.SetTrigger("MainMenu");
    }
  public void connectionMenuBack(){
    server.shutdown();
    client.shutdown();
        menuAnimator.SetTrigger("HostMenu");
    }
}
