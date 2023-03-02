using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
public class Piece : MonoBehaviour
{
    [SerializeField] public Sprite bBishop, bRook, bPawn, bQueen, bKing, bKnight;
    [SerializeField] public Sprite wBishop, wRook, wPawn, wQueen, wKing, wKnight;
    [SerializeField] public Sprite moveIndicator;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_Text lostText;
    public GameObject controller;
    private int xCord = -1;
    private int yCord = -1;
    private bool hasMoved = false;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private string player = "white";
    private string playerToplay = "white";
    private string destinationTag = "DropArea";
    private Grid grid;
    public int GetX()
    {
        return xCord;
    }
    public int GetY()
    {
        return yCord;
    }
    public void SetX(int x)
    {
        xCord = x;
    }
    public void SetY(int y)
    {
        yCord = y;
    }
    public string GetPlayer()
    {
        return player;
    }

    public void SetPiece()
    {
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
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        controller = GameObject.FindGameObjectWithTag("GameController");
        grid = controller.GetComponent<Grid>();
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
    public void gameOver(string player, int iWon)
    {
        if (iWon == 0)
        {
            lostText.text = "Player " + player + " lost";
        }
        else if (iWon == 1)
        {
            lostText.text = "Player " + player + " won";
        }
        menuAnimator.SetTrigger("lostScreen");
    }
    void OnMouseDown()
    {
        player = GetPlayer();
        string playerToPlay = grid.getPlayerToPlay();
        int pTP;
        if (playerToPlay == "white")
        {
            pTP = 0;
        }
        else
        {
            pTP = 1;
        }
        if (player != playerToPlay)
        {

            return;
        }
        if (grid.getOnlineGame())
        {
            if (pTP != grid.getPlayerTeam())
            {
                return;
            }
        }
        grid.DestroyIndicators();
        grid.clearMoves();
        offset = rectTransform.position - MouseWorldPosition();
        if (grid.checkmate(playerToPlay))
        {
            gameOver(playerToPlay, 0);
            return;
        }
        rectTransform.GetComponent<Collider2D>().enabled = false;
        grid.clearMoves();

        if (this != null)
        {
            Debug.Log("Actual moves");
            grid.GenerateIndicators(this);

            grid.legalMoves(this);
            grid.makeIndicators();
        }
        // for (int i = 0; i < 8; i++)
        // {
        //     for (int k = 0; k < 8; k++)
        // {
        //    Piece a = grid.getPosition(i,k);
        //    if (a != null && a.GetPlayer() == grid.getPlayerToPlay())
        //    {
        //     grid.GenerateIndicators(a);
        //    }
        // } 
        // }
        //grid.makeIndicators();      

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
        if (hitInfo)
        {
            if (hitInfo.transform.tag == destinationTag)
            {
                for (int i = 0; i < indicators.Length; i++)
                {
                    if (hitInfo.transform.position.x == indicators[i].transform.position.x && hitInfo.transform.position.y == indicators[i].transform.position.y)
                    {
                        legalMove = true;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < indicators.Length; i++)
                {
                    if (hitInfo.transform.position.x == indicators[i].transform.position.x && hitInfo.transform.position.y == indicators[i].transform.position.y)
                    {
                        takePiece = true;
                        break;
                    }
                }
            }

            if (legalMove)
            {
                //Debug.Log("move");
                rectTransform.position = hitInfo.transform.position;
                //Debug.Log(grid.getCastleShort());
                if (grid.getCastleLong() && (int)rectTransform.position.x == this.GetX() - 2)
                {
                    Piece rook = grid.getPosition(this.GetX() - 4, this.GetY()).GetComponent<Piece>();
                    rook.rectTransform.position = new Vector3(rectTransform.position.x + 1, rectTransform.position.y, -1);
                    grid.SetPosition(rook, (int)rectTransform.position.x + 1, (int)rectTransform.position.y);
                    grid.setcastleLong(false);
                }
                if (grid.getCastleShort() && (int)rectTransform.position.x == this.GetX() + 2)
                {
                    Piece rook = grid.getPosition(this.GetX() + 3, this.GetY()).GetComponent<Piece>();
                    rook.rectTransform.position = new Vector3(rectTransform.position.x - 1, rectTransform.position.y, -1);
                    grid.SetPosition(rook, (int)rectTransform.position.x - 1, (int)rectTransform.position.y);
                    grid.setCastleShort(false);
                }
                grid.SetPosition(this, (int)rectTransform.position.x, (int)rectTransform.position.y);
                if (grid.getenPassantWhite() && grid.getPosition(this.GetX(), this.GetY() - 1).gameObject != null)
                {
                    Destroy(grid.getPosition(this.GetX(), this.GetY() - 1).gameObject);
                    if (!grid.getOnlineGame())
                    {
                        grid.setEnpassantWhite(false);
                    }

                }
                if (grid.getenPassantBlack() && grid.getPosition(this.GetX(), this.GetY() + 1).gameObject != null)
                {
                    Destroy(grid.getPosition(this.GetX(), this.GetY() + 1).gameObject);
                    if (!grid.getOnlineGame())
                    {
                        grid.setEnpassantBlack(false);
                    }
                }
                setHasMoved(true);
                grid.clearMoves();
                grid.DestroyIndicators();
                if (player == "white")
                {
                    grid.setPlayerToPlay("black");
                }
                else
                {
                    grid.setPlayerToPlay("white");
                }
            }


            if (takePiece && grid.getPosition((int)hitInfo.transform.position.x, (int)hitInfo.transform.position.y).GetPlayer() != GetPlayer())
            {
                //Debug.Log("take");
                rectTransform.position = hitInfo.transform.position;
                Destroy(hitInfo.transform.gameObject);
                grid.SetPosition(this, (int)rectTransform.position.x, (int)rectTransform.position.y);
                setHasMoved(true);
                grid.clearMoves();
                grid.DestroyIndicators();
                if (player == "white")
                {
                    grid.setPlayerToPlay("black");
                }
                else
                {
                    grid.setPlayerToPlay("white");
                }
            }
            else
            {
                rectTransform.position = new Vector3(GetX(), GetY(), -1);
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
        else
        {
            rectTransform.position = new Vector3(GetX(), GetY(), -1);
        }
        rectTransform.GetComponent<Collider2D>().enabled = true;
    }

    Vector3 MouseWorldPosition()
    {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }

    public void setHasMoved(bool moved)
    {
        hasMoved = moved;
    }
    public bool getHasMoved()
    {
        return hasMoved;
    }
}
