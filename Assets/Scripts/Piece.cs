using UnityEngine;
using TMPro;
public class Piece : MonoBehaviour
{
    [SerializeField] public Sprite bBishop, bRook, bPawn, bQueen, bKing, bKnight;
    [SerializeField] public Sprite wBishop, wRook, wPawn, wQueen, wKing, wKnight;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_Text lostText;
    public GameObject controller;
    private int xCord = -1;
    private int yCord = -1;
    private bool hasMoved = false;
    private RectTransform rectTransform;
    private string player = "white";
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
        //Assign the correct sprite to the pieces
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
    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        //get object references
        rectTransform = GetComponent<RectTransform>();
        controller = GameObject.FindGameObjectWithTag("GameController");
        grid = controller.GetComponent<Grid>();
    }

    Vector3 offset;
    //Show the according screen when a player looses
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
        //Logic to not allow online players to move the other pieces when it is not their turn
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
        //Clean up
        grid.DestroyIndicators();
        grid.clearMoves();
        //Set the offset from the piece to the mouse (this prevents snapping of the piece onto the mouse)
        offset = rectTransform.position - MouseWorldPosition();
        //Check if the player who wants to play is mated
        if (grid.checkmate(playerToPlay))
        {
            gameOver(playerToPlay, 0);
            return;
        }
        //Disable the collider of the piece we are dragging, otherwise we would always hit the piece with the raycast and not the object bellow it
        rectTransform.GetComponent<Collider2D>().enabled = false;
        grid.clearMoves();

        
        if (this != null)
        {
            //Generate the legal moves for the specific piece
            grid.GenerateIndicators(this);
            grid.legalMoves(this);
            grid.makeIndicators();
        }   
    }

    void OnMouseDrag()
    {
        //Change the position of the piece to the mouse position
        rectTransform.position = MouseWorldPosition() + offset;
    }

    void OnMouseUp()
    {

        var rayOrigin = Camera.main.transform.position;
        var rayDirection = MouseWorldPosition() - Camera.main.transform.position;
        
        //Cast a ray from the camera to the mouse to see where a piece is moving
        RaycastHit2D hitInfo;
        hitInfo = Physics2D.Raycast(MouseWorldPosition(), Vector2.zero);
        //Find the move indicators (legal moves)
        GameObject[] indicators = GameObject.FindGameObjectsWithTag("MoveIndicator");
        bool legalMove = false;
        bool takePiece = false;
        //Check if the move was made on top of the board
        if (hitInfo)
        {
            //Check if we are moving on an empty tile
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
            //Otherwise we are taking a piece
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
                //if the movec was legal change the position of the piece to the position of the square it is moving to
                rectTransform.position = hitInfo.transform.position;
                //Check if the move was a castling move and move the corresponding rook if the was a castling move
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
                //Check if the move en passant, if it was destroy the pawn behind the en passant move
                if (grid.getenPassantWhite() && grid.getPosition(this.GetX(), this.GetY() - 1) != null)
                {
                    Destroy(grid.getPosition(this.GetX(), this.GetY() - 1).gameObject);
                    if (!grid.getOnlineGame())
                    {
                        grid.setEnpassantWhite(false);
                    }

                }
                if (grid.getenPassantBlack() && grid.getPosition(this.GetX(), this.GetY() + 1) != null)
                {
                    Destroy(grid.getPosition(this.GetX(), this.GetY() + 1).gameObject);
                    if (!grid.getOnlineGame())
                    {
                        grid.setEnpassantBlack(false);
                    }
                }
                setHasMoved(true);
                //Clean up
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

            //Logic for when we are taking a piece
            if (takePiece && grid.getPosition((int)hitInfo.transform.position.x, (int)hitInfo.transform.position.y).GetPlayer() != GetPlayer())
            {
                //move the piece to the square
                rectTransform.position = hitInfo.transform.position;
                //Destroy the piece that was on the square
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
            //Otherwise return the piece to its original position
            else
            {
                rectTransform.position = new Vector3(GetX(), GetY(), -1);
            }
            //Handle queen promotion
            if (this.name == "wPawn" && GetY() == grid.getHeight() - 1)
            {

                this.name = "wQueen";
                SetPiece();
            }
            else if (this.name == "bPawn" && GetY() == 0)
            {
                this.name = "bQueen";
                SetPiece();
            }
        }
        //Otherwise return the piece to its original position
        else
        {
            rectTransform.position = new Vector3(GetX(), GetY(), -1);
        }
        //Re-enable the disabled collider
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
