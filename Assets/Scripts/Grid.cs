using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private int width,
        height;

    [SerializeField]
    private Tile tilePrefab;

    [SerializeField]
    private Transform cam;

    [SerializeField]
    private Piece GeneralPiece;
    private Piece selectedPiece;
    [SerializeField]
    GameObject moveIndicator;
    private string playerToplay = "white";
    private Piece[,] positions = new Piece[8, 8];
    private Piece[] playerBlack = new Piece[16];
    private Piece[] playerWhite = new Piece[16];
    List<Vector3> moves = new List<Vector3>();
    List<Vector3> leagalMoves = new List<Vector3>();

    void Start()
    {
        GenerateGrid();
        playerWhite = new Piece[]
        {
            CreatePiece("wRook", 0, 0),
            CreatePiece("wKnight", 1, 0),
            CreatePiece("wBishop", 2, 0),
            CreatePiece("wQueen", 3, 0),
            CreatePiece("wKing", 4, 0),
            CreatePiece("wBishop", 5, 0),
            CreatePiece("wKnight", 6, 0),
            CreatePiece("wRook", 7, 0),
            CreatePiece("wPawn", 0, 1),
            CreatePiece("wPawn", 1, 1),
            CreatePiece("wPawn", 2, 1),
            CreatePiece("wPawn", 3, 1),
            CreatePiece("wPawn", 4, 1),
            CreatePiece("wPawn", 5, 1),
            CreatePiece("wPawn", 6, 1),
            CreatePiece("wPawn", 7, 1)
        };
        playerBlack = new Piece[]
        {
            CreatePiece("bRook", 0, 7),
            CreatePiece("bKnight", 1, 7),
            CreatePiece("bBishop", 2, 7),
            CreatePiece("bQueen", 3, 7),
            CreatePiece("bKing", 4, 7),
            CreatePiece("bBishop", 5, 7),
            CreatePiece("bKnight", 6, 7),
            CreatePiece("bRook", 7, 7),
            CreatePiece("bPawn", 0, 6),
            CreatePiece("bPawn", 1, 6),
            CreatePiece("bPawn", 2, 6),
            CreatePiece("bPawn", 3, 6),
            CreatePiece("bPawn", 4, 6),
            CreatePiece("bPawn", 5, 6),
            CreatePiece("bPawn", 6, 6),
            CreatePiece("bPawn", 7, 6)
        };

        for (int i = 0; i < 16; i++)
        {
            positions[playerWhite[i].GetX(), playerWhite[i].GetY()] = playerWhite[i];
            positions[playerBlack[i].GetX(), playerBlack[i].GetY()] = playerBlack[i];
        }
        for (int j = 0; j < 8; j++)
        {
            for (int k = 0; k < 8; k++)
            {
                Debug.Log(positions[j, k]);
            }
        }
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                bool isLigt = (x + y) % 2 != 0;
                spawnedTile.isLight(isLigt);
            }
        }

        cam.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10);
    }

    public void SetPosition(Piece piece, int x, int y)
    {
        positions[x, y] = piece;
        positions[piece.GetX(), piece.GetY()] = null;
        piece.SetX(x);
        piece.SetY(y);
    }

    private Piece CreatePiece(string name, int x, int y)
    {
        Piece obj = Instantiate(GeneralPiece, new Vector3(x, y), Quaternion.identity);
        Piece piece = obj.GetComponent<Piece>(); //We have access to the Piece, we need the script
        piece.name = name; //This is a built in variable that Unity has, so we did not have to declare it before
        piece.SetX(x);
        piece.SetY(y);
        piece.SetPiece(); //It has everything set up so it can now Activate()
        return obj;
    }

    public void GenerateIndicators(Piece piece)
    {
        selectedPiece = piece;
        int x = piece.GetX(),
            y = piece.GetY();
        switch (piece.name)
        {
            case "wPawn":
                createPawnIndicator(x, 1, piece);
                break;
            case "bPawn":
                createPawnIndicator(piece.GetX(), -1, piece);
                break;
            case "wRook":
            case "bRook":
                createLineIndicator(0, -1, piece);
                createLineIndicator(0, 1, piece);
                createLineIndicator(1, 0, piece);
                createLineIndicator(-1, 0, piece);
                break;
            case "wQueen":
            case "bQueen":
                createLineIndicator(0, -1, piece);
                createLineIndicator(0, 1, piece);
                createLineIndicator(1, 0, piece);
                createLineIndicator(-1, 0, piece);
                createLineIndicator(1, 1, piece);
                createLineIndicator(-1, 1, piece);
                createLineIndicator(-1, -1, piece);
                createLineIndicator(1, -1, piece);
                break;
            case "wBishop":
            case "bBishop":
                createLineIndicator(1, 1, piece);
                createLineIndicator(-1, 1, piece);
                createLineIndicator(-1, -1, piece);
                createLineIndicator(1, -1, piece);
                break;
            case "wKnight":
            case "bKnight":

                createIndicator(x + 1, y + 2, piece);
                createIndicator(x - 1, y + 2, piece);
                createIndicator(x + 2, y + 1, piece);
                createIndicator(x - 2, y + 1, piece);
                createIndicator(x - 1, y - 2, piece);
                createIndicator(x + 1, y - 2, piece);
                createIndicator(x - 2, y - 1, piece);
                createIndicator(x + 2, y - 1, piece);
                break;
            case "wKing":
            case "bKing":
                createIndicator(x + 1, y, piece);
                createIndicator(x - 1, y, piece);
                createIndicator(x + 1, y + 1, piece);
                createIndicator(x - 1, y - 1, piece);
                createIndicator(x + 1, y - 1, piece);
                createIndicator(x - 1, y + 1, piece);
                createIndicator(x + 1, y + 1, piece);
                createIndicator(x, y + 1, piece);
                createIndicator(x, y - 1, piece);
                break;
        }
    }

    private void createLineIndicator(int xStep, int yStep, Piece piece)
    {
        int x = piece.GetX() + xStep;
        int y = piece.GetY() + yStep;
        while (onBoard(x, y) && getPosition(x, y) == null)
        {
            moves.Add(new Vector3(x, y, -1));
            //Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
            x += xStep;
            y += yStep;
        }
        if (onBoard(x, y) && piece.GetPlayer() != getPosition(x, y).GetPlayer())
        {
            moves.Add(new Vector3(x, y, -1));
            // Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
        }
    }

    private void createPawnIndicator(int x, int yStep, Piece piece)
    {
        int y = piece.GetY() + yStep;
        bool isOnBoard = onBoard(x, y);
        bool isEmpty = getPosition(x, y) == null;
        if (isOnBoard && isEmpty && piece.GetPlayer() == "white" && piece.GetY() == 1)
        {
            if (getPosition(x, y) == null)
            {
                moves.Add(new Vector3(x, y, -1));
            }
            if (getPosition(x, y + 1) == null)
            {
                moves.Add(new Vector3(x, y + 1, -1));
            }

            // Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
            //Instantiate(moveIndicator, new Vector3(x, y + 1, -1), Quaternion.identity);
        }
        else if (isOnBoard && isEmpty && piece.GetPlayer() == "black" && piece.GetY() == 6)
        {
            if (getPosition(x, y) == null)
            {
                moves.Add(new Vector3(x, y, -1));
            }
            if (getPosition(x, y - 1) == null)
            {
                moves.Add(new Vector3(x, y - 1, -1));
            }

            // Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
            // Instantiate(moveIndicator, new Vector3(x, y - 1, -1), Quaternion.identity);
        }
        else if (isOnBoard && isEmpty)
        {
            moves.Add(new Vector3(x, y, -1));
            // Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
        }
        if (
            onBoard(x + 1, y)
            && getPosition(x + 1, y) != null
            && piece.GetPlayer() != getPosition(x + 1, y).GetPlayer()
        )
        {
            moves.Add(new Vector3(x + 1, y, -1));
            // Instantiate(moveIndicator, new Vector3(x + 1, y, -1), Quaternion.identity);
        }
        if (
            onBoard(x - 1, y)
            && getPosition(x - 1, y) != null
            && piece.GetPlayer() != getPosition(x - 1, y).GetPlayer()
        )
        {
            moves.Add(new Vector3(x - 1, y, -1));
            // Instantiate(moveIndicator, new Vector3(x - 1, y, -1), Quaternion.identity);
        }
    }

    private void createIndicator(int x, int y, Piece piece)
    {
        bool isOnBoard = onBoard(x, y);
        if (!isOnBoard)
        {
            return;
        }
        Piece pieceAtPos = getPosition(x, y);
        bool isEmpty = pieceAtPos == null;
        if (isOnBoard)
        {
            if (isEmpty || pieceAtPos.GetPlayer() != piece.GetPlayer())
            {
                // Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
                moves.Add(new Vector3(x, y, -1));
            }
        }
    }

    private bool onBoard(int x, int y)
    {
        return x < 8 && x >= 0 && y < 8 && y >= 0;
    }

    public void DestroyIndicators()
    {
        GameObject[] moveIndicators = GameObject.FindGameObjectsWithTag("MoveIndicator");
        for (int i = 0; i < moveIndicators.Length; i++)
        {
            Destroy(moveIndicators[i]);
        }
    }

    public void makeIndicators()
    {
        for (int i = 0; i < moves.Count; i++)
        {
            Instantiate(moveIndicator, moves[i], Quaternion.identity);
        }
    }

    public void clearMoves()
    {
        moves.Clear();
    }

    public Vector3 findKing(string player)
    {
        string kingToFind = "";
        if (player == "white")
        {
            kingToFind = "wKing";
        }
        else
        {
            kingToFind = "bKing";
        }
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (positions[i, j] != null)
                {
                    if (positions[i, j].name == kingToFind)
                    {
                        return new Vector3(i, j, -1);
                    }
                }

            }
        }
        return new Vector3(-1, -1, -1);
    }

    public bool isInCheck(string player)
    {
        Vector3 kingPos = findKing(player);
        //Debug.Log(kingPos);
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i] == kingPos)
            {
                return true;
            }
        }
        return false;
    }

    public void legalMoves(Piece piece)
    {
        List<Vector3> legalMoves = new List<Vector3>(); ;
        List<Vector3> myMoves = new List<Vector3>(moves);
        Piece pieceToTake  = null;
        clearMoves();
        for (int i = 0; i < 8; i++)
        {
            for (int k = 0; k < 8; k++)
            {
                Piece a = getPosition(i, k);
                if (a != null && a.GetPlayer() != getPlayerToPlay())
                {
                    GenerateIndicators(a);
                }
            }
        }
        if(!isInCheck(getPlayerToPlay())){
            moves = myMoves;
            return;
        }
        Vector2 originalPos = new Vector2();

        originalPos.x = piece.GetX();
        originalPos.y = piece.GetY();
        for (int j = 0; j < myMoves.Count; j++)
        {
            if(positions[(int)myMoves[j].x, (int)myMoves[j].y] != null) {
               pieceToTake = getPosition((int)myMoves[j].x, (int)myMoves[j].y);
            } 
            positions[(int)myMoves[j].x, (int)myMoves[j].y] = piece;
             positions[(int)originalPos.x, (int)originalPos.y] = null;
            //SetPosition(piece, (int)myMoves[j].x, (int)myMoves[j].y);
            clearMoves();
            for (int i = 0; i < 8; i++)
            {
                for (int k = 0; k < 8; k++)
                {
                    Piece a = getPosition(i, k);
                    if (a != null && a.GetPlayer() != getPlayerToPlay())
                    {
                        GenerateIndicators(a);
                    }
                }
            }
            if (!isInCheck(getPlayerToPlay()))
            {
                legalMoves.Add(myMoves[j]);
            }
            if(pieceToTake != null) {
                Debug.Log(pieceToTake);
              // SetPosition(pieceToTake, (int)myMoves[j].x, (int)myMoves[j].y);
               positions[(int)myMoves[j].x, (int)myMoves[j].y] = pieceToTake;
                Debug.Log(getPosition((int)myMoves[j].x, (int)myMoves[j].y));
                pieceToTake = null;
            }
            else{
                positions[(int)myMoves[j].x, (int)myMoves[j].y] = null;
            }
              positions[(int)originalPos.x, (int)originalPos.y] = piece;
           // SetPosition(piece, (int)originalPos.x, (int)originalPos.y);
           // Debug.Log(getPosition((int)myMoves[j].x, (int)myMoves[j].y));
        }
        moves = legalMoves;
      
    }

    public Piece getPosition(int x, int y)
    {
        return positions[x, y];
    }

    public string getPlayerToPlay()
    {
        return playerToplay;
    }

    public void setPlayerToPlay(string player)
    {
        playerToplay = player;
    }
}
