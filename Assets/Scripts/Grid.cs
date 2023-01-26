using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private int width, height;

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

    private bool castleLong = false;
    private bool castleShort = false;
    private bool enPassantWhite = false;
    private bool enPassantBlack = false;
    public struct Move
    {
        public Vector2 originalPos;
        public Vector2 currentPos;
        public Piece piece;
    }
    Move lastmove;
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

        cam.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10); //Move the camera to the middle of the screen
    }

    public void SetPosition(Piece piece, int x, int y)
    {
        lastmove.piece = piece;
        lastmove.originalPos = new Vector2(piece.GetX(), piece.GetY());
        lastmove.currentPos = new Vector2(x, y);
        Vector3 kingPos = findKing(playerToplay);
        Tile kingTile = GameObject.Find("Tile " + (int)kingPos.x + " " + (int)kingPos.y).GetComponent<Tile>();
        kingTile.resetColor(); //if the king was in check, reset the color of the king square to the original tile color
        positions[x, y] = piece;
        positions[piece.GetX(), piece.GetY()] = null;
        piece.SetX(x);
        piece.SetY(y);
    }

    private Piece CreatePiece(string name, int x, int y)
    {
        Piece obj = Instantiate(GeneralPiece, new Vector3(x, y), Quaternion.identity);
        Piece piece = obj.GetComponent<Piece>();
        piece.name = name;
        piece.SetX(x);
        piece.SetY(y);
        piece.SetPiece();
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
                createPawnIndicator(x, -1, piece);
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

                // Debug.Log("after methods");
                // for(int i = 0; i < moves.Count; i++) {
                //     Debug.Log(moves[i]);
                // }
                createIndicator(x + 1, y, piece);
                createIndicator(x - 1, y, piece);
                createIndicator(x + 1, y + 1, piece);
                createIndicator(x - 1, y - 1, piece);
                createIndicator(x + 1, y - 1, piece);
                createIndicator(x - 1, y + 1, piece);
                createIndicator(x + 1, y + 1, piece);
                createIndicator(x, y + 1, piece);
                createIndicator(x, y - 1, piece);
                if(piece.GetPlayer() == getPlayerToPlay()) {
                   canCastle(piece); 
                }
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
        if (onBoard(x + 1, y) && getPosition(x + 1, y) != null && piece.GetPlayer() != getPosition(x + 1, y).GetPlayer())
        {
            moves.Add(new Vector3(x + 1, y, -1));
            // Instantiate(moveIndicator, new Vector3(x + 1, y, -1), Quaternion.identity);
        }
        if (onBoard(x - 1, y) && getPosition(x - 1, y) != null && piece.GetPlayer() != getPosition(x - 1, y).GetPlayer())
        {
            moves.Add(new Vector3(x - 1, y, -1));
            // Instantiate(moveIndicator, new Vector3(x - 1, y, -1), Quaternion.identity);
        }
        float movementY = Math.Abs(lastmove.currentPos.y - lastmove.originalPos.y);
        float xDiff = piece.GetX() - lastmove.currentPos.x;
        if (piece.GetPlayer() == getPlayerToPlay())
        {
            if (piece.GetPlayer() == "white")
            {
                if (piece.GetY() == 4 && lastmove.piece.name == "bPawn" && movementY == 2)
                {
                    if (xDiff == 1 || xDiff == -1)
                    {
                        moves.Add(new Vector3(lastmove.currentPos.x, 5, -1));
                        enPassantWhite = true;
                    }
                    else
                    {
                        enPassantWhite = false;
                    }
                }
                else
                {
                    enPassantWhite = false;
                }
            }
            else if (piece.GetPlayer() == "black")
            {
                if (piece.GetY() == 3 && lastmove.piece.name == "wPawn" && movementY == 2)
                {
                    if (xDiff == 1 || xDiff == -1)
                    {
                        moves.Add(new Vector3(lastmove.currentPos.x, 2, -1));
                        enPassantBlack = true;
                    }
                    else
                    {
                        enPassantBlack = false;
                    }
                }
                else
                {
                    enPassantBlack = false;
                }
            }
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
        string playerToPlay = getPlayerToPlay();
        List<Vector3> legalMoves = new List<Vector3>(); ;

        List<Vector3> myMoves = new List<Vector3>(moves); //save the current pieces possible moves
        Piece pieceToTake = null;
        clearMoves();
        //Generate all possible moves for the enemy pieces
        for (int i = 0; i < 8; i++)
        {
            for (int k = 0; k < 8; k++)
            {
                Piece a = getPosition(i, k);
                if (a != null && a.GetPlayer() != playerToPlay)
                {
                    GenerateIndicators(a);
                }
            }
        }
        Tile kingTile = null;
        //Set the king tile to red if the king is in check
        if (isInCheck(playerToPlay))
        {
            Vector3 kingPos = findKing(playerToplay);
            kingTile = GameObject.Find("Tile " + (int)kingPos.x + " " + (int)kingPos.y).GetComponent<Tile>();
            kingTile.tileRed();
        }
        //Save the original position of the piece
        Vector2 originalPos = new Vector2();
        originalPos.x = piece.GetX();
        originalPos.y = piece.GetY();
        //Debug.Log("my moves");
        for (int j = 0; j < myMoves.Count; j++)
        {
            //Debug.Log(myMoves[j]);
            //if the move of the piece would take an enemy piece save that piece
            if (positions[(int)myMoves[j].x, (int)myMoves[j].y] != null)
            {
                pieceToTake = getPosition((int)myMoves[j].x, (int)myMoves[j].y);
            }
            //Make the move on the board (programatically the board does not change)
            positions[(int)myMoves[j].x, (int)myMoves[j].y] = piece;
            positions[(int)originalPos.x, (int)originalPos.y] = null;

            clearMoves();
            for (int i = 0; i < 8; i++)
            {
                for (int k = 0; k < 8; k++)
                {
                    Piece a = getPosition(i, k);
                    if (a != null && a.GetPlayer() != playerToPlay)
                    {
                        GenerateIndicators(a);
                    }
                }
            }
            //After the move has been made check if the king is still in check
            if (!isInCheck(playerToPlay))
            {
                legalMoves.Add(myMoves[j]); // if the king is not in check after the move make the move legal
            }
            //If a piece has been overwritten by the move set the piece back
            if (pieceToTake != null)
            {
                // SetPosition(pieceToTake, (int)myMoves[j].x, (int)myMoves[j].y);
                positions[(int)myMoves[j].x, (int)myMoves[j].y] = pieceToTake;
                pieceToTake = null;
            }
            //if the move was on an empty square set the squere to null
            else
            {
                positions[(int)myMoves[j].x, (int)myMoves[j].y] = null;
            }
            //Set the piece to its original position
            positions[(int)originalPos.x, (int)originalPos.y] = piece;

        }
        if ((piece.name == "wKing" || piece.name == "bKing") && legalMoves.Count >= 2)
        {
            if ( ((legalMoves[legalMoves.Count - 1].x == 6 && legalMoves[legalMoves.Count - 1].y == 0) || (legalMoves[legalMoves.Count - 1].x == 2 && legalMoves[legalMoves.Count - 1].y == 0)) && (castleLong || castleShort))
            {
                int moveIndex = -1;
                Vector3 castleMove1 = new Vector3(3, legalMoves[legalMoves.Count - 1].y, -1);
                Vector3 castleMove2 = new Vector3(5, legalMoves[legalMoves.Count - 1].y, -1);
                for (int i = 0; i < legalMoves.Count; i++)
                {
                    if (legalMoves[i] == castleMove1 || legalMoves[i] == castleMove2)
                    {
                        moveIndex = i;
                        break;
                    }
                }
                if (moveIndex == -1)
                {
                    Debug.Log("move removed");
                    legalMoves.RemoveAt(legalMoves.Count - 1);
                    // Debug.Log("move" + legalMoves[moveIndex]);
                    // if ((legalMoves[moveIndex].x == legalMoves[legalMoves.Count - 1].x - 1 && legalMoves[moveIndex].y == legalMoves[legalMoves.Count - 1].y) || (legalMoves[moveIndex].x == legalMoves[legalMoves.Count - 1].x + 1 && legalMoves[moveIndex].y == legalMoves[legalMoves.Count - 1].y))
                    // {

                    // }
                    // else
                    // {
                    //     
                    // }
                }
            }
        }
        else if((piece.name == "wKing" || piece.name == "bKing") && ((legalMoves[legalMoves.Count - 1].x == 6 && legalMoves[legalMoves.Count - 1].y == 0) || (legalMoves[legalMoves.Count - 1].x == 2 && legalMoves[legalMoves.Count - 1].y == 0)) && (castleLong || castleShort)){
            Debug.Log("move removed");
            legalMoves.RemoveAt(legalMoves.Count - 1);
        }
        //Set the possible moves to the legal moves
        moves = legalMoves;

        // Debug.Log("Legal moves");
        // for (int i = 0; i < legalMoves.Count; i++)
        // {
        //     Debug.Log(legalMoves[i]);
        // }
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

    private void canCastle(Piece king)
    {
        int kingX = king.GetX();
        int kingY = king.GetY();
        if (king.getHasMoved() == false)
        {
            if (king.GetPlayer() == getPlayerToPlay() && getPosition(kingX - 4, kingY) != null && (getPosition(kingX - 1, kingY) == null && getPosition(kingX - 2, kingY) == null && getPosition(kingX - 3, kingY) == null && getPosition(kingX - 4, kingY).getHasMoved() == false && (getPosition(kingX - 4, kingY).name == "wRook" || getPosition(kingX - 4, kingY).name == "bRook")))
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    if (moves[i].x == kingX - 1 && moves[i].y == kingY)
                    {
                        createIndicator(kingX - 2, kingY, king);
                        castleLong = true;
                        break;
                    }
                    else
                    {

                    }
                }
            }
            else
            {
                castleLong = false;
            }
            if (king.GetPlayer() == getPlayerToPlay() && getPosition(kingX + 3, kingY)!=null && (getPosition(kingX + 1, kingY) == null && getPosition(kingX + 2, kingY) == null && getPosition(kingX + 3, kingY).getHasMoved() == false && (getPosition(kingX + 3, kingY).name == "wRook" || getPosition(kingX + 3, kingY).name == "bRook")))
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    if (moves[i].x == kingX + 1 && moves[i].y == kingY)
                    {
                        createIndicator(kingX + 2, kingY, king);
                        castleShort = true;
                        break;
                    }
                    else
                    {
                        castleShort = false;
                    }
                }
            }
            else {
                castleShort = false;
            }
        }

    }
    public bool getCastleShort()
    {
        return castleShort;
    }
    public bool getCastleLong()
    {
        return castleLong;
    }
    public void setCastleShort(bool value){
        castleShort = value;
    }
    public void setcastleLong(bool value){
        castleLong = value;
    }
    public bool getenPassantWhite()
    {
        return enPassantWhite;
    }
    public void setEnpassantBlack(bool value){
        enPassantBlack = value;
    }
    public bool getenPassantBlack()
    {
        return enPassantBlack;
    }
     public void setEnpassantWhite(bool value){
        enPassantWhite = value;
    }
}
