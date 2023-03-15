using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Random = System.Random;

public class Grid : MonoBehaviour
{
    // Multiplayer logic
    private int currentPlayer = -1;
    private int numPlayers = -1;

    private const int NUM_PIECES = 8;
    //where on the board should be pieces be placed
    private const int FIRST_PIECE_BX = 0;
    private int FIRST_PIECE_BY = 7;
    private const int FIRST_PIECE_WX = 0;
    private const int FIRST_PIECE_WY = 0;
    [SerializeField] private int width, height;
    [SerializeField] private ReplayMove replayMove;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private GameObject forestTileDark;
    [SerializeField] private GameObject forestTileLight;
    [SerializeField] private Tile tilePrefab;

    [SerializeField] private Transform cam;

    [SerializeField] private Piece GeneralPiece;
    private Piece selectedPiece;

    [SerializeField] GameObject moveIndicator;

    //  [SerializeField] Camera camera;
    private string playerToplay = "white";
    private Piece[,] positions;
    private Piece[] playerBlack = new Piece[2 * NUM_PIECES];
    private Piece[] playerWhite = new Piece[2 * NUM_PIECES];
    List<Vector3> moves = new List<Vector3>();
    List<Vector3> leagalMoves = new List<Vector3>();

    private bool castleLong = false;
    private bool castleShort = false;
    private bool enPassantWhite = false;
    private bool enPassantBlack = false;

    //Structure to save information about a move
    public struct Move
    {
        public Vector2 originalPos;
        public Vector2 currentPos;
        public Piece piece;
    }
    public struct Move2
    {
        public string pieceName;
        public int originalX;
        public int originalY;
        public int goalX;
        public int goalY;
        public bool capture;
        public bool check;
        public bool enpassant;

    }
    
    private bool replayingGame = false;
    Move lastmove;
    List<Move2> movesPlayed = new List<Move2>();
    private bool startAsBlack = false;
    private bool startAsWhite = false;
    private bool onlineGame = false;

    void Awake()
    {
        
        registerEvents();
        positions = new Piece[width, height];
        FIRST_PIECE_BY = height - 1;
    }

    public void startGame()
    {
        playerToplay = "white";
        menuAnimator.SetTrigger("NoMenu");
        GenerateGrid();
        playerWhite = new Piece[]
        {
            CreatePiece("wRook", FIRST_PIECE_WX, FIRST_PIECE_WY),
            CreatePiece("wKnight", FIRST_PIECE_WX + 1, FIRST_PIECE_WY),
            CreatePiece("wBishop", FIRST_PIECE_WX + 2, FIRST_PIECE_WY),
            CreatePiece("wQueen", FIRST_PIECE_WX + 3, FIRST_PIECE_WY),
            CreatePiece("wKing", FIRST_PIECE_WX + 4, FIRST_PIECE_WY),
            CreatePiece("wBishop",FIRST_PIECE_WX + 5, FIRST_PIECE_WY),
            CreatePiece("wKnight",FIRST_PIECE_WX + 6, FIRST_PIECE_WY),
            CreatePiece("wRook", FIRST_PIECE_WX + 7, FIRST_PIECE_WY),
            CreatePiece("wPawn", FIRST_PIECE_WX, FIRST_PIECE_WY + 1),
            CreatePiece("wPawn", FIRST_PIECE_WX + 1, FIRST_PIECE_WY + 1),
            CreatePiece("wPawn", FIRST_PIECE_WX + 2, FIRST_PIECE_WY + 1),
            CreatePiece("wPawn", FIRST_PIECE_WX + 3, FIRST_PIECE_WY + 1),
            CreatePiece("wPawn", FIRST_PIECE_WX + 4, FIRST_PIECE_WY + 1),
            CreatePiece("wPawn", FIRST_PIECE_WX + 5, FIRST_PIECE_WY + 1),
            CreatePiece("wPawn", FIRST_PIECE_WX + 6, FIRST_PIECE_WY + 1),
            CreatePiece("wPawn", FIRST_PIECE_WX + 7, FIRST_PIECE_WY + 1)
        };
        playerBlack = new Piece[]
        {
            CreatePiece("bRook", FIRST_PIECE_BX, FIRST_PIECE_BY),
            CreatePiece("bKnight", FIRST_PIECE_BX + 1, FIRST_PIECE_BY),
            CreatePiece("bBishop", FIRST_PIECE_BX + 2, FIRST_PIECE_BY),
            CreatePiece("bQueen", FIRST_PIECE_BX + 3, FIRST_PIECE_BY),
            CreatePiece("bKing", FIRST_PIECE_BX + 4, FIRST_PIECE_BY),
            CreatePiece("bBishop", FIRST_PIECE_BX + 5, FIRST_PIECE_BY),
            CreatePiece("bKnight", FIRST_PIECE_BX + 6, FIRST_PIECE_BY),
            CreatePiece("bRook", FIRST_PIECE_BX + 7, FIRST_PIECE_BY),
            CreatePiece("bPawn", FIRST_PIECE_BX, FIRST_PIECE_BY - 1),
            CreatePiece("bPawn", FIRST_PIECE_BX + 1, FIRST_PIECE_BY - 1),
            CreatePiece("bPawn", FIRST_PIECE_BX + 2, FIRST_PIECE_BY - 1),
            CreatePiece("bPawn", FIRST_PIECE_BX + 3, FIRST_PIECE_BY - 1),
            CreatePiece("bPawn", FIRST_PIECE_BX + 4, FIRST_PIECE_BY - 1),
            CreatePiece("bPawn", FIRST_PIECE_BX + 5, FIRST_PIECE_BY - 1),
            CreatePiece("bPawn", FIRST_PIECE_BX + 6, FIRST_PIECE_BY - 1),
            CreatePiece("bPawn", FIRST_PIECE_BX + 7, FIRST_PIECE_BY - 1)
        };

        for (int i = 0; i < 2 * NUM_PIECES; i++)
        {
            positions[playerWhite[i].GetX(), playerWhite[i].GetY()] = playerWhite[i];
            positions[playerBlack[i].GetX(), playerBlack[i].GetY()] = playerBlack[i];
        }
        //Rotate the camera based on what player should be on the bottom
        if (currentPlayer == 1)
        {
            cam.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        }
        else
        {
            cam.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    void GenerateGrid()
    {
        //Generate the grid squares
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                Random random = new Random();
                int randomNumber = random.Next(0, 5);

                bool isLigt = (x + y) % 2 != 0;
                if (isLigt)
                {
                    // Instantiate(forestTileLight, new Vector3(x, y), Quaternion.Euler(0f, 0f, 90f * randomNumber));
                    //Instantiate(forestTileLight, new Vector3(x, y), Quaternion.Euler(0f, 0f, 90f * randomNumber));
                }
                else
                {
                    // Instantiate(forestTileDark, new Vector3(x, y), Quaternion.Euler(0f, 0f, 90f * randomNumber));
                }
                //Assign the correct colors the the squres
                spawnedTile.isLight(isLigt);
            }
        }

        cam.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10); //Move the camera to the middle of the screen
    }
   
    //Set the position of the piece so the program knows where a piece has moved
    public void SetPosition(Piece piece, int x, int y)
    {
        //Set values of the move played as the last move
        lastmove.piece = piece;
        lastmove.originalPos = new Vector2(piece.GetX(), piece.GetY());
        lastmove.currentPos = new Vector2(x, y);
        
        if(!replayingGame){
            Move2 movePlayed;
            movePlayed.pieceName = piece.name;
            movePlayed.originalX = piece.GetX();
            movePlayed.originalY = piece.GetY();
            movePlayed.goalX = x;
            movePlayed.goalY = y;
            if(positions[x,y] != null) {
                movePlayed.capture = true;
            }
            else{
                movePlayed.capture = false;
            }
            movePlayed.check = false;
            movePlayed.enpassant = false;
            movesPlayed.Add(movePlayed);
        }
       
        Vector3 kingPos = findKing(playerToplay);
        Tile kingTile = GameObject.Find("Tile " + (int)kingPos.x + " " + (int)kingPos.y).GetComponent<Tile>();
        kingTile.resetColor(); //if the king was in check, reset the color of the king square to the original tile color
        positions[x, y] = piece;
        positions[piece.GetX(), piece.GetY()] = null;
        piece.SetX(x);
        piece.SetY(y);

        //Send a message with the move to the other player
        if (onlineGame)
        {
            MakeMoveMsg move = new MakeMoveMsg();
            move.originalX = (int)lastmove.originalPos.x;
            move.originalY = (int)lastmove.originalPos.y;
            move.goalX = x;
            move.goalY = y;
            if (piece.GetPlayer() == "white")
            {
                move.team = 0;
            }
            else
            {
                move.team = 1;
            }
            Client.Instance.sendToServer(move);
        }

    }

    private Piece CreatePiece(string name, int x, int y)
    {
        Piece obj = null;
        //If we are playing as black we need to rotate our pieces as we rotated our camera leading to correct orientation of the pieces
        if (startAsBlack)
        {
            obj = Instantiate(GeneralPiece, new Vector3(x, y, -1), Quaternion.Euler(0f, 0f, 180f));
        }
        //Otherwise just create the piece normally
        else
        {
            obj = Instantiate(GeneralPiece, new Vector3(x, y, -1), Quaternion.identity);
        }
        Piece piece = obj.GetComponent<Piece>();
        piece.name = name;
        piece.GetComponent<Collider2D>().enabled = true;
        piece.SetX(x);
        piece.SetY(y);
        piece.SetPiece();
        return obj;
    }

    //Function to generate possible moves of a piece
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
                createIndicator(x + 1, y, piece);
                createIndicator(x - 1, y, piece);
                createIndicator(x + 1, y + 1, piece);
                createIndicator(x - 1, y - 1, piece);
                createIndicator(x + 1, y - 1, piece);
                createIndicator(x - 1, y + 1, piece);
                createIndicator(x, y + 1, piece);
                createIndicator(x, y - 1, piece);
                //Just check this if the player is on their turn as the function also checks for checks
                if (piece.GetPlayer() == getPlayerToPlay())
                {
                    canCastle(piece);
                }
                // Debug.Log("after methods");
                // for(int i = 0; i < moves.Count; i++) {
                //     Debug.Log(moves[i]);
                // }
                break;

        }
    }

    //Create moves in a straight line or a diagonal
    private void createLineIndicator(int xStep, int yStep, Piece piece)
    {
        int x = piece.GetX() + xStep;
        int y = piece.GetY() + yStep;
        //Create moves on empty squares
        while (onBoard(x, y) && positions[x, y] == null)
        {
            moves.Add(new Vector3(x, y, -1));
            x += xStep;
            y += yStep;
        }
        //Create a move if there is an enemy piece at the end of the straight line
        if (onBoard(x, y) && piece.GetPlayer() != positions[x, y].GetPlayer())
        {
            moves.Add(new Vector3(x, y, -1));
        }
    }


    private void createPawnIndicator(int x, int yStep, Piece piece)
    {
        int y = piece.GetY() + yStep;
        bool isOnBoard = onBoard(x, y);
        bool isEmpty = positions[x, y] == null;
        //Pawn can move two squares as its first move
        if (isOnBoard && isEmpty && piece.GetPlayer() == "white" && piece.getHasMoved() == false)
        {
            if (positions[x, y] == null)
            {
                moves.Add(new Vector3(x, y, -1));
            }
            if (positions[x, y + 1] == null)
            {
                moves.Add(new Vector3(x, y + 1, -1));
            }
        }
        else if (isOnBoard && isEmpty && piece.GetPlayer() == "black" && piece.getHasMoved() == false)
        {
            if (positions[x, y] == null)
            {
                moves.Add(new Vector3(x, y, -1));
            }
            if (positions[x, y - 1] == null)
            {
                moves.Add(new Vector3(x, y - 1, -1));
            }
        }
        //Otherwise just move one square forward
        else if (isOnBoard && isEmpty)
        {
            moves.Add(new Vector3(x, y, -1));

        }
        //Check if we can take a piece and create a move if we can
        if (onBoard(x + 1, y) && positions[x + 1, y] != null && piece.GetPlayer() != positions[x + 1, y].GetPlayer())
        {
            moves.Add(new Vector3(x + 1, y, -1));

        }
        if (onBoard(x - 1, y) && positions[x - 1, y] != null && piece.GetPlayer() != positions[x - 1, y].GetPlayer())
        {
            moves.Add(new Vector3(x - 1, y, -1));
        }
        float movementY = Math.Abs(lastmove.currentPos.y - lastmove.originalPos.y);
        float xDiff = piece.GetX() - lastmove.currentPos.x;
        //Check if en Passant is possible
        if (piece.GetPlayer() == getPlayerToPlay())
        {
            if (piece.GetPlayer() == "white")
            {
                if (piece.GetY() == height - 4 && lastmove.piece.name == "bPawn" && movementY == 2)
                {
                    if (xDiff == 1 || xDiff == -1)
                    {
                        moves.Add(new Vector3(lastmove.currentPos.x, height - 3, -1));
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
        Piece pieceAtPos = positions[x, y];
        bool isEmpty = pieceAtPos == null;
        if (isOnBoard)
        {
            if (isEmpty || pieceAtPos.GetPlayer() != piece.GetPlayer())
            {
                moves.Add(new Vector3(x, y, -1));
            }
        }
    }

    //Checks if the corrdinates are on the board
    private bool onBoard(int x, int y)
    {
        return x < width && x >= 0 && y < height && y >= 0;
    }

    //Used to destroy all the move indicator game objects
    public void DestroyIndicators()
    {
        GameObject[] moveIndicators = GameObject.FindGameObjectsWithTag("MoveIndicator");
        for (int i = 0; i < moveIndicators.Length; i++)
        {
            Destroy(moveIndicators[i]);
        }
    }

    //Create move indicators based on the legal moves
    public void makeIndicators()
    {
        for (int i = 0; i < moves.Count; i++)
        {
            Instantiate(moveIndicator, moves[i], Quaternion.identity);
        }
    }
    //Find the king of the specified player
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
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
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

    //Checks if the king is in check by checking if it can be "taken" by an enemy piece
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

        List<Vector3> legalMoves = new List<Vector3>();
        List<Vector3> myMoves = new List<Vector3>(moves); //save the current pieces possible moves
        Piece pieceToTake = null;

        clearMoves();
        //Generate all possible moves for the enemy pieces - these will be now saved in the moves array
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                Piece a = positions[i, k];
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
            Move2 checkMove = movesPlayed[movesPlayed.Count - 1];
            checkMove.check = true;
            movesPlayed[movesPlayed.Count - 1] = checkMove;
           Debug.Log( movesPlayed[movesPlayed.Count - 1].check);
            if(piece.name == "wKing" || piece.name == "bKing"){
                myMoves.RemoveAt(myMoves.Count - 1);
            }
            Vector3 kingPos = findKing(playerToplay);
            kingTile = GameObject.Find("Tile " + (int)kingPos.x + " " + (int)kingPos.y).GetComponent<Tile>();
            kingTile.tileRed();
        }
        //Save the original position of the piece we want to move
        Vector2 originalPos = new Vector2();
        originalPos.x = piece.GetX();
        originalPos.y = piece.GetY();
        //Debug.Log("my moves");
        for (int j = 0; j < myMoves.Count; j++)
        {
            //Debug.Log(piece + " " + myMoves[j] + " " + myMoves.Count);
            //if the move of the piece would take an enemy piece save that piece
            if (positions[(int)myMoves[j].x, (int)myMoves[j].y] != null)
            {
                pieceToTake = positions[(int)myMoves[j].x, (int)myMoves[j].y];
            }
            if (playerToPlay == "white" && enPassantWhite == true && piece.name == "wPawn" && j == myMoves.Count - 1)
            {
                pieceToTake = positions[(int)myMoves[myMoves.Count - 1].x, (int)myMoves[myMoves.Count - 1].y - 1];
                positions[(int)myMoves[myMoves.Count - 1].x, (int)myMoves[myMoves.Count - 1].y - 1] = null;
            }
            else if (playerToPlay == "black" && enPassantBlack == true && piece.name == "bPawn" && j == myMoves.Count - 1)
            {
                pieceToTake = positions[(int)myMoves[myMoves.Count - 1].x, (int)myMoves[myMoves.Count - 1].y + 1];
                positions[(int)myMoves[myMoves.Count - 1].x, (int)myMoves[myMoves.Count - 1].y + 1] = null;
            }
            //Make the move on the board programatically (the board does not visually change)
            positions[(int)myMoves[j].x, (int)myMoves[j].y] = piece;
            positions[(int)originalPos.x, (int)originalPos.y] = null;

            clearMoves();
            //After the move has been made generate the moves of the opponent again
            for (int i = 0; i < width; i++)
            {
                for (int k = 0; k < height; k++)
                {
                    Piece a = positions[i, k];
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
                if (playerToPlay == "white" && enPassantWhite == true && piece.name == "wPawn" && j == myMoves.Count - 1)
                {
                    positions[(int)myMoves[myMoves.Count - 1].x, (int)myMoves[myMoves.Count - 1].y - 1] = pieceToTake;
                }
                else if (playerToPlay == "black" && enPassantBlack == true && piece.name == "bPawn" && j == myMoves.Count - 1)
                {
                    positions[(int)myMoves[myMoves.Count - 1].x, (int)myMoves[myMoves.Count - 1].y + 1] = pieceToTake;
                }
                else
                {
                    positions[(int)myMoves[j].x, (int)myMoves[j].y] = pieceToTake;
                }

                pieceToTake = null;
            }
            //if the move was on an empty square set the squere to null
            else
            {
                positions[(int)myMoves[j].x, (int)myMoves[j].y] = null;
            }
            //Set the piece to its original position
            positions[(int)originalPos.x, (int)originalPos.y] = piece;
            // if (legalMoves.Count == 0)
            // {
            //     moves = legalMoves;
            //     return;
            // }
        }

        int lastMoveX = -1;
        int lastMoveY = -1;

        if (legalMoves.Count >= 1)
        {
            lastMoveX = (int)legalMoves[legalMoves.Count - 1].x;
            lastMoveY = (int)legalMoves[legalMoves.Count - 1].y;
        }
        //Check if castling is legal (the king cannot go through check thus the tile next to him has to be a legal move for castling to be legal)
        if ((piece.name == "wKing" || piece.name == "bKing") && legalMoves.Count >= 2)
        {
            if (((lastMoveX == 6 && (lastMoveY == 0 || lastMoveY == 7)) || (lastMoveX == 2 && (lastMoveY == 0 || lastMoveY == 7))) && (castleLong || castleShort))
            {
                int moveIndex = -1;
                Vector3 moveToFind;
                Vector3 castleMove1 = new Vector3(3, lastMoveY, -1);
                Vector3 castleMove2 = new Vector3(5, lastMoveY, -1);
                if (lastMoveX == 6)
                {
                    moveToFind = castleMove2;
                }
                else
                {
                    moveToFind = castleMove1;
                }
                //Try to find the move which would make castling legal
                for (int i = 0; i < legalMoves.Count; i++)
                {
                    Debug.Log("Legal move" + i + legalMoves[i]);
                    if (legalMoves[i] == moveToFind)
                    {
                        Debug.Log("Found Castle move" + legalMoves[i]);
                        moveIndex = i;
                        break;
                    }
                }
                //If the move is not found, remove the castling move from legal moves
                if (moveIndex == -1)
                {
                    Debug.Log("move removed");
                    legalMoves.RemoveAt(legalMoves.Count - 1);
                }
            }
        }
        else if ((piece.name == "wKing" || piece.name == "bKing") && ((lastMoveX == 6 && (lastMoveY == 0 || (lastMoveY == 7))) || (lastMoveX == 2 && (lastMoveY == 0 || lastMoveY == 7))) && (castleLong || castleShort))
        {
            Debug.Log("move removed");
            legalMoves.RemoveAt(legalMoves.Count - 1);
        }
        //Set the possible moves to the legal moves
        moves = legalMoves;
    }

    //Check for checkmate by checking if a player has any legal moves
    public bool checkmate(string player)
    {
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                Piece a = positions[i, k];
                if (a != null && a.GetPlayer() == player)
                {
                    GenerateIndicators(a);
                    legalMoves(a);
                    if (moves.Count != 0)
                    {
                        clearMoves();
                        return false;
                    }
                }
            }
        }
        //If the game is online send a message to the other player
        if (onlineGame)
        {
            GameOverMsg gameOverMsg = new GameOverMsg();
            if (player == "white")
            {
                gameOverMsg.team = 1;
            }
            else
            {
                gameOverMsg.team = 0;
            }
            Client.Instance.sendToServer(gameOverMsg);
        }
        int moveIndex = 1;
        for (int i = 0; i < movesPlayed.Count; i++)
        {
            string text = $"{moveIndex } ";
            moveIndex++;
            text+=convertNotation(movesPlayed[i]);
            //replayMove.generateReplayMoveIndex($"{i}");
            replayMove.generateReplayMove(text);
            i++;
            if(i >= movesPlayed.Count) {
                break;
            }
            text = convertNotation(movesPlayed[i]);
            replayMove.generateReplayMove(text);
        }
        return true;
    }

    //Check if its possible to castle (without checking if the king is in check)
    private void canCastle(Piece king)
    {
        int kingX = king.GetX();
        int kingY = king.GetY();
        if (king.getHasMoved() == false)
        {
            Piece initialRookPos = positions[kingX - 4, kingY];

            if (initialRookPos != null && (positions[kingX - 1, kingY] == null && positions[kingX - 2, kingY] == null
            && positions[kingX - 3, kingY] == null && initialRookPos.getHasMoved() == false && (initialRookPos.name == "wRook" || initialRookPos.name == "bRook")))
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    if (moves[i].x == kingX - 1 && moves[i].y == kingY)
                    {
                        createIndicator(kingX - 2, kingY, king);
                        castleLong = true;
                        break;
                    }
                    else { 
                        
                    }
                }
            }
            else
            {
                castleLong = false;
            }
            if (positions[kingX + 3, kingY] != null && (positions[kingX + 1, kingY] == null && positions[kingX + 2, kingY] == null && positions[kingX + 3, kingY].getHasMoved() == false
                && (positions[kingX + 3, kingY].name == "wRook" || positions[kingX + 3, kingY].name == "bRook")))
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
            else
            {
                castleShort = false;
            }
        }
    }

    public void clearMoves()
    {
        moves.Clear();
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

    public int getHeight()
    {
        return height;
    }
    public bool getCastleShort()
    {
        return castleShort;
    }

    public bool getCastleLong()
    {
        return castleLong;
    }

    public void setCastleShort(bool value)
    {
        castleShort = value;
    }

    public void setcastleLong(bool value)
    {
        castleLong = value;
    }

    public bool getenPassantWhite()
    {
        return enPassantWhite;
    }

    public void setEnpassantBlack(bool value)
    {
        enPassantBlack = value;
    }

    public bool getenPassantBlack()
    {
        return enPassantBlack;
    }

    public void setEnpassantWhite(bool value)
    {
        enPassantWhite = value;
    }

    public int getPlayerTeam()
    {
        return currentPlayer;
    }
    public bool getOnlineGame()
    {
        return onlineGame;
    }


    //Register for online messages
    private void registerEvents()
    {
        //These functions get called when the corresponding message is received
        NetUtility.S_WELCOME += onWelcomeServer;
        NetUtility.C_WELCOME += onWelcomeClient;
        NetUtility.C_START_GAME += onStartGameClient;
        NetUtility.S_MAKE_MOVE += onMakeMoveServer;
        NetUtility.C_MAKE_MOVE += onMakeMoveClient;
        NetUtility.S_GAME_OVER += onGameOverServer;
        NetUtility.C_GAME_OVER += onGameOverClient;
    }

    private void onWelcomeServer(Message msg, NetworkConnection connection)
    {
        onlineGame = true;
        WelcomeMsg welcome = msg as WelcomeMsg;
        welcome.player = ++numPlayers;
        Server.Instance.sendToClient(connection, welcome);
        //Broadcast the start game message if both players are connected
        if (numPlayers == 1)
        {
            Server.Instance.broadcast(new StartGameMsg());
        }
    }
    private void onMakeMoveServer(Message msg, NetworkConnection connection)
    {
        //Broadcast the message to the client
        Server.Instance.broadcast(msg);
    }
    private void onWelcomeClient(Message msg)
    {
        onlineGame = true;
        WelcomeMsg welcome = msg as WelcomeMsg;
        currentPlayer = welcome.player;
    }

    private void onStartGameClient(Message msg)
    {
        //Check which player we are
        if (currentPlayer == 1)
        {
            startAsBlack = true;
        }
        startGame();
    }
    private void onMakeMoveClient(Message msg)
    {
        MakeMoveMsg move = msg as MakeMoveMsg;
        if (move.team != currentPlayer)
        {
            Piece piece = positions[move.originalX, move.originalY];

            if (piece == null)
            {
                return;
            }
            //Check if a piece should be taken
            Piece pieceToTake = positions[move.goalX, move.goalY];
            if (pieceToTake != null)
            {
                Destroy(pieceToTake.gameObject);
            }
            //Handle enpassant
            int xDiff = Math.Abs(move.originalX - move.goalX);
            if (piece.name == "wPawn" && xDiff == 1)
            {
                pieceToTake = positions[move.goalX, move.goalY - 1];
                if (pieceToTake != null)
                {
                    Destroy(pieceToTake.gameObject);
                }
            }
            if (piece.name == "bPawn" && xDiff == 1)
            {
                pieceToTake = positions[move.goalX, move.goalY + 1];
                if (pieceToTake != null)
                {
                    Destroy(pieceToTake.gameObject);
                }
            }
            //Handle queen promotion
            if (piece.name == "wPawn" && move.goalY == height - 1)
            {
                piece.name = "wQueen";
                piece.SetPiece();
            }
            if (piece.name == "bPawn" && move.goalY == 0)
            {
                piece.name = "bQueen";
                piece.SetPiece();
            }
            //Move the piece to the position
            piece.transform.position = new Vector3(move.goalX, move.goalY, -1);
            SetPosition(piece, move.goalX, move.goalY);

            if (move.team == 0)
            {
                setPlayerToPlay("black");
            }
            else if (move.team == 1)
            {
                setPlayerToPlay("white");
            }
        }
    }
    private void onGameOverServer(Message msg, NetworkConnection connection)
    {
        Server.Instance.broadcast(msg);
    }
    private void onGameOverClient(Message msg)
    {
        //Call the correct game over function based on who won and lost
        GameOverMsg gameOverMsg = msg as GameOverMsg;
        if (gameOverMsg.team == 0 && currentPlayer == gameOverMsg.team)
        {
            GeneralPiece.gameOver("white", 1);
        }
        else if (gameOverMsg.team == 1 && currentPlayer == gameOverMsg.team)
        {
            GeneralPiece.gameOver("black", 1);
        }
        else if (gameOverMsg.team == 0 && currentPlayer != gameOverMsg.team)
        {
            GeneralPiece.gameOver("black", 0);
        }
        else if (gameOverMsg.team == 1 && currentPlayer != gameOverMsg.team)
        {
            GeneralPiece.gameOver("white", 0);
        }
    }
    private void unregisterEvents()
    {
        NetUtility.S_WELCOME -= onWelcomeServer;
        NetUtility.C_WELCOME -= onWelcomeClient;
        NetUtility.C_START_GAME -= onStartGameClient;
        NetUtility.S_MAKE_MOVE -= onMakeMoveServer;
        NetUtility.C_MAKE_MOVE -= onMakeMoveClient;
        NetUtility.S_GAME_OVER -= onGameOverServer;
        NetUtility.C_GAME_OVER -= onGameOverClient;
    }

    private int replayMoveIndex = 0;
    public void replayGame(){
        destroyAssets();
        startGame();
        replayingGame = true;
        //Show a back and forward button

    }

    public void replayNextMove(){
        int i = replayMoveIndex;
        Piece piece = getPosition( movesPlayed[i].originalX,movesPlayed[i].originalY);
        piece.transform.position = new Vector3(movesPlayed[i].goalX, movesPlayed[i].goalY, -1);
        Piece pieceAtPos = positions[movesPlayed[i].goalX, movesPlayed[i].goalY];
        Debug.Log(pieceAtPos);
        if(pieceAtPos != null) {
            pieceAtPos.transform.position = new Vector3(movesPlayed[i].goalX, movesPlayed[i].goalY, -100);;
        }
        SetPosition(piece, movesPlayed[i].goalX, movesPlayed[i].goalY );
        
        replayMoveIndex++;
    }
    public void destroyAssets()
    {
        //unregisterEvents();
        startAsBlack = false;
        startAsWhite = false;
        onlineGame = false;
        numPlayers = -1;
        currentPlayer = -1;
        lastmove.piece = null;

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("DropArea");
        for (int i = 0; i < tiles.Length; i++)
        {

            Destroy(tiles[i]);
        }
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        //Destroy all pieces except the general piece that has been initialized as a [Serailized field], always index 0 so start at index 1 to not destry it
        for (int i = 1; i < pieces.Length; i++)
        {
            Destroy(pieces[i]);
        }
        GameObject[] boardGraphics = GameObject.FindGameObjectsWithTag("BoardGraphic");
        for (int i = 0; i < boardGraphics.Length; i++)
        {

            Destroy(boardGraphics[i]);
        }
    }
    private string convertNotation(Move2 move){
        string notation = null;
         switch (move.pieceName)
        {
            case "wPawn":
            case "bPawn":
                break;
            case "wRook":
            case "BRook":
                notation+="R";
                break;
            case "wQueen":
            case "bQueen":
                notation+="Q";
                break;
            case "wBishop":
            case "bBishop":
                notation+="B";
                break;
            case "wKnight":
            case "bKnight":
                notation+="N";
                break;
            case "wKing":
            case "bKing":
                notation+="K";
                break;

        }
        if(move.capture) {
             notation+="x";
        }
        if(move.check) {
             notation+="+";
        }
        notation+=convertToFile(move.originalX);
        notation+=$"{move.originalY + 1}";
        notation+=convertToFile(move.goalX);
        notation+=$"{move.goalY + 1}";
        return notation;
    }
    private static string convertToFile(int file)
    {
        return ((char)('a' + file)).ToString();
    }
}
