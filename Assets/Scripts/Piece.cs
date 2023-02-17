using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour
{
    [SerializeField] public Sprite bBishop, bRook, bPawn, bQueen, bKing, bKnight;
    [SerializeField] public Sprite wBishop, wRook, wPawn, wQueen, wKing, wKnight;
    [SerializeField] public Sprite moveIndicator;
    public GameObject controller;
    private int xCord = -1;
    private int yCord = -1;
    private bool hasMoved = false;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private string player = "white";
    private string playerToplay = "white";
    private string destinationTag = "DropArea";
 
    public int GetX(){
        return xCord;
    }
    public int GetY(){
        return yCord;
    }
    public void SetX(int x){
        xCord = x;
    }
     public void SetY(int y){
        yCord = y;
    }
    public string GetPlayer(){
        return player;
    }

    public void SetPiece(){
        controller = GameObject.FindGameObjectWithTag("GameController");
          switch (this.name)
        {
            case "bQueen": this.GetComponent<SpriteRenderer>().sprite = bQueen; player = "black"; break;
            case "bKnight": this.GetComponent<SpriteRenderer>().sprite = bKnight; player = "black"; break;
            case "bBishop": this.GetComponent<SpriteRenderer>().sprite = bBishop; player = "black"; break;
            case "bKing": this.GetComponent<SpriteRenderer>().sprite = bKing; player = "black"; break;
            case "bRook": this.GetComponent<SpriteRenderer>().sprite = bRook; player = "black"; break;
            case "bPawn": this.GetComponent<SpriteRenderer>().sprite = bPawn; player = "black"; break;
            case "wQueen": this.GetComponent<SpriteRenderer>().sprite = wQueen; player = "white"; break;
            case "wKnight": this.GetComponent<SpriteRenderer>().sprite = wKnight; player = "white"; break;
            case "wBishop": this.GetComponent<SpriteRenderer>().sprite = wBishop; player = "white"; break;
            case "wKing": this.GetComponent<SpriteRenderer>().sprite = wKing; player = "white"; break;
            case "wRook": this.GetComponent<SpriteRenderer>().sprite = wRook; player = "white"; break;
            case "wPawn": this.GetComponent<SpriteRenderer>().sprite = wPawn; player = "white"; break;
        }
    }
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
   private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }
 

//     public void OnBeginDrag(PointerEventData eventData) {
//         Debug.Log("OnBeginDrag");
//          canvasGroup.blocksRaycasts = false;
//     }


//     public void OnEndDrag(PointerEventData eventData) {
//         Debug.Log("OnEndDrag");
//          canvasGroup.blocksRaycasts = true;
//     }

//     public void OnPointerDown(PointerEventData eventData) {
//         Debug.Log("OnPointerDown");
//     }
//     public void OnDrag(PointerEventData eventData){         
//         rectTransform.position = GetMousePos();       
//    }
//    void OnMouseUp()
//     {
//         Debug.Log("mouse ip");
//         var rayOrigin = Camera.main.transform.position;
//         var rayDirection = GetMousePos() - Camera.main.transform.position;
//         RaycastHit hitInfo;
//         if(Physics.Raycast(rayOrigin, rayDirection, out hitInfo))
//         {
//             if(hitInfo.transform.tag == destinationTag)
//             {
//                 rectTransform.position = hitInfo.transform.position;
//             }
//         }
//         rectTransform.GetComponent<Collider2D>().enabled = true;
//     }
//    private Vector3 GetMousePos(){
//     var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//     mousePos.z = 0;
//     return mousePos;
//    }
 Vector3 offset;
 
    void OnMouseDown()
    {
        player = GetPlayer();
        string playerToPlay = controller.GetComponent<Grid>().getPlayerToPlay();
        int pTP;
        if(playerToPlay == "white") {
            pTP = 0;
        } else {
            pTP = 1;
        }
        if(player != playerToPlay){
           
            return;
        }
         if(controller.GetComponent<Grid>().getOnlineGame()) {
                if(pTP != controller.GetComponent<Grid>().getPlayerTeam()) {
                     return;
                }
            }
        controller.GetComponent<Grid>().DestroyIndicators();
        controller.GetComponent<Grid>().clearMoves();
        offset = rectTransform.position - MouseWorldPosition();    
        rectTransform.GetComponent<Collider2D>().enabled = false;
        if(this != null){
                controller.GetComponent<Grid>().GenerateIndicators(this);
                controller.GetComponent<Grid>().legalMoves(this);
                controller.GetComponent<Grid>().makeIndicators();
        }
        // for (int i = 0; i < 8; i++)
        // {
        //     for (int k = 0; k < 8; k++)
        // {
        //    Piece a = controller.GetComponent<Grid>().getPosition(i,k);
        //    if (a != null && a.GetPlayer() == controller.GetComponent<Grid>().getPlayerToPlay())
        //    {
        //     controller.GetComponent<Grid>().GenerateIndicators(a);
        //    }
        // } 
        // }
        //controller.GetComponent<Grid>().makeIndicators();      
        
    }
 
    void OnMouseDrag()
    {
        rectTransform.position = MouseWorldPosition() + offset;
    }
 
    void OnMouseUp()
    {

        var rayOrigin = Camera.main.transform.position;
        var rayDirection = MouseWorldPosition() - Camera.main.transform.position;
         
        RaycastHit2D hitInfo;
        hitInfo = Physics2D.Raycast(MouseWorldPosition(), Vector2.zero);
        GameObject[] indicators = GameObject.FindGameObjectsWithTag("MoveIndicator");
        bool legalMove = false;
        bool takePiece = false;
        if(hitInfo)
        {
            
            if(hitInfo.transform.tag == destinationTag )
            {
               for (int i = 0; i < indicators.Length; i++)
               {
                if(hitInfo.transform.position.x == indicators[i].transform.position.x && hitInfo.transform.position.y == indicators[i].transform.position.y){
                    legalMove = true;
                    break;
                }                
               }
            }
            else{
                 for (int i = 0; i < indicators.Length; i++)
               {
                if(hitInfo.transform.position.x == indicators[i].transform.position.x && hitInfo.transform.position.y == indicators[i].transform.position.y){
                    takePiece = true;
                    break;
                }                
               }                    
            }
            
            if(legalMove){
                //Debug.Log("move");
                rectTransform.position = hitInfo.transform.position;
                //Debug.Log(controller.GetComponent<Grid>().getCastleShort());
                if(controller.GetComponent<Grid>().getCastleLong() && (int)rectTransform.position.x == this.GetX() - 2){
                    Piece rook = controller.GetComponent<Grid>().getPosition(this.GetX() - 4, this.GetY()).GetComponent<Piece>();
                    rook.rectTransform.position = new Vector3(rectTransform.position.x + 1,rectTransform.position.y, -1 );
                    controller.GetComponent<Grid>().SetPosition(rook, (int)rectTransform.position.x + 1, (int)rectTransform.position.y);
                    controller.GetComponent<Grid>().setcastleLong(false);
                }  
                if(controller.GetComponent<Grid>().getCastleShort() && (int)rectTransform.position.x == this.GetX() + 2){
                    Piece rook = controller.GetComponent<Grid>().getPosition(this.GetX() + 3, this.GetY()).GetComponent<Piece>();
                    rook.rectTransform.position = new Vector3(rectTransform.position.x - 1,rectTransform.position.y, -1 );
                    controller.GetComponent<Grid>().SetPosition(rook, (int)rectTransform.position.x - 1, (int)rectTransform.position.y);
                    controller.GetComponent<Grid>().setCastleShort(false);
                }
                controller.GetComponent<Grid>().SetPosition(this,(int)rectTransform.position.x, (int)rectTransform.position.y );
                if(controller.GetComponent<Grid>().getenPassantWhite() && controller.GetComponent<Grid>().getPosition(this.GetX(), this.GetY()-1).gameObject != null){
                    Destroy(controller.GetComponent<Grid>().getPosition(this.GetX(), this.GetY()-1).gameObject);
                    controller.GetComponent<Grid>().setEnpassantWhite(false);
                }
                if(controller.GetComponent<Grid>().getenPassantBlack() && controller.GetComponent<Grid>().getPosition(this.GetX(), this.GetY()+1).gameObject != null){
                    Destroy(controller.GetComponent<Grid>().getPosition(this.GetX(), this.GetY()+1).gameObject);
                    controller.GetComponent<Grid>().setEnpassantBlack(false);
                }
                setHasMoved(true);
                controller.GetComponent<Grid>().clearMoves();
                controller.GetComponent<Grid>().DestroyIndicators();
                if(player == "white"){                  
                     controller.GetComponent<Grid>().setPlayerToPlay("black");
                }
                else{
                   controller.GetComponent<Grid>().setPlayerToPlay("white");
                }
            }
               
            
            if( takePiece && controller.GetComponent<Grid>().getPosition((int)hitInfo.transform.position.x, (int)hitInfo.transform.position.y).GetPlayer() != GetPlayer()){
                //Debug.Log("take");
                rectTransform.position = hitInfo.transform.position;
                Destroy(hitInfo.transform.gameObject);
                controller.GetComponent<Grid>().SetPosition(this,(int)rectTransform.position.x, (int)rectTransform.position.y );
                setHasMoved(true);
                controller.GetComponent<Grid>().clearMoves();
                controller.GetComponent<Grid>().DestroyIndicators();
               if(player == "white"){                  
                     controller.GetComponent<Grid>().setPlayerToPlay("black");
                }
                else{
                   controller.GetComponent<Grid>().setPlayerToPlay("white");
                }
            }
            else{
                rectTransform.position = new Vector3(GetX() , GetY(), - 1);
            }
            if (this.name == "wPawn" && GetY() == 7)
            {
                //Debug.Log("White queen promotion");
                this.name = "wQueen";
                SetPiece();
            }
            else if (this.name == "bPawn" && GetY() == 0)
            {
                //Debug.Log("Black queen promotion");
                this.name = "bQueen";
                SetPiece();
            }
        }
        rectTransform.GetComponent<Collider2D>().enabled = true;
    }
 
    Vector3 MouseWorldPosition()
    {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    public void setHasMoved(bool moved){
        hasMoved = moved;
    }
    public bool getHasMoved(){
        return hasMoved;
    }
}
