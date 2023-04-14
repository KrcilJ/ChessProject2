using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Random = System.Random;
using System.Text;

public class Grid : MonoBehaviour
{
    // Multiplayer logic
    private int currentPlayer = -1;
    private int numPlayers = -1;
    private string computerPlayer = null;
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
    private string playerToplay = null;
    private Piece[,] positions;
    private Piece[] playerBlack = new Piece[2 * NUM_PIECES];
    private Piece[] playerWhite = new Piece[2 * NUM_PIECES];
    // List<Vector3> moves = new List<Vector3>();
    List<Vector2Int> moves = new List<Vector2Int>();
    List<Vector2Int> captures = new List<Vector2Int>();
    private string startingFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w -- --";
    private bool castleLong = false;
    private bool castleShort = false;
    private bool enPassantWhite = false;
    private bool enPassantBlack = false;

    const int pawnValue = 100;
    const int knightValue = 300;
    const int bishopValue = 300;
    const int rookValue = 500;
    const int queenValue = 900;
    const int checkmateValue = 9999;
    private int treeDepth = 4;
    List<Move> allLegalMoves = new List<Move>();
    //Structure to save information about a move


    public struct Move
    {
        public Vector2Int originalPos;
        public Vector2Int goalPos;
        public Piece piece;
    }
    public struct Move2
    {
        public string pieceName;
        public Vector2Int originalPos;
        public Vector2Int goalPos;
        public bool capture;
        public bool check;
        public bool checkmate;
        public bool enpassant;
        public bool castle;
    }

    private bool replayingGame = false;
    private Move lastmove;
    List<Move2> movesPlayed = new List<Move2>();
    private bool startAsBlack = false;
    private bool startAsWhite = false;
    private bool onlineGame = false;
    List<string> Fens = new List<string>();
    void Awake()
    {
        Fens.Add(startingFEN);

        registerEvents();
        positions = new Piece[width, height];
        FIRST_PIECE_BY = height - 1;
    }

    public void startGame()
    {
        //playerToplay = "white";
        menuAnimator.SetTrigger("NoMenu");
        GenerateGrid();
        // playerWhite = new Piece[]
        // {
        //     CreatePiece("wRook", FIRST_PIECE_WX, FIRST_PIECE_WY),
        //     CreatePiece("wKnight", FIRST_PIECE_WX + 1, FIRST_PIECE_WY),
        //     CreatePiece("wBishop", FIRST_PIECE_WX + 2, FIRST_PIECE_WY),
        //     CreatePiece("wQueen", FIRST_PIECE_WX + 3, FIRST_PIECE_WY),
        //     CreatePiece("wKing", FIRST_PIECE_WX + 4, FIRST_PIECE_WY),
        //     CreatePiece("wBishop",FIRST_PIECE_WX + 5, FIRST_PIECE_WY),
        //     CreatePiece("wKnight",FIRST_PIECE_WX + 6, FIRST_PIECE_WY),
        //     CreatePiece("wRook", FIRST_PIECE_WX + 7, FIRST_PIECE_WY),
        //     CreatePiece("wPawn", FIRST_PIECE_WX, FIRST_PIECE_WY + 1),
        //     CreatePiece("wPawn", FIRST_PIECE_WX + 1, FIRST_PIECE_WY + 1),
        //     CreatePiece("wPawn", FIRST_PIECE_WX + 2, FIRST_PIECE_WY + 1),
        //     CreatePiece("wPawn", FIRST_PIECE_WX + 3, FIRST_PIECE_WY + 1),
        //     CreatePiece("wPawn", FIRST_PIECE_WX + 4, FIRST_PIECE_WY + 1),
        //     CreatePiece("wPawn", FIRST_PIECE_WX + 5, FIRST_PIECE_WY + 1),
        //     CreatePiece("wPawn", FIRST_PIECE_WX + 6, FIRST_PIECE_WY + 1),
        //     CreatePiece("wPawn", FIRST_PIECE_WX + 7, FIRST_PIECE_WY + 1)
        // };
        // playerBlack = new Piece[]
        // {
        //     CreatePiece("bRook", FIRST_PIECE_BX, FIRST_PIECE_BY),
        //     CreatePiece("bKnight", FIRST_PIECE_BX + 1, FIRST_PIECE_BY),
        //     CreatePiece("bBishop", FIRST_PIECE_BX + 2, FIRST_PIECE_BY),
        //     CreatePiece("bQueen", FIRST_PIECE_BX + 3, FIRST_PIECE_BY),
        //     CreatePiece("bKing", FIRST_PIECE_BX + 4, FIRST_PIECE_BY),
        //     CreatePiece("bBishop", FIRST_PIECE_BX + 5, FIRST_PIECE_BY),
        //     CreatePiece("bKnight", FIRST_PIECE_BX + 6, FIRST_PIECE_BY),
        //     CreatePiece("bRook", FIRST_PIECE_BX + 7, FIRST_PIECE_BY),
        //     CreatePiece("bPawn", FIRST_PIECE_BX, FIRST_PIECE_BY - 1),
        //     CreatePiece("bPawn", FIRST_PIECE_BX + 1, FIRST_PIECE_BY - 1),
        //     CreatePiece("bPawn", FIRST_PIECE_BX + 2, FIRST_PIECE_BY - 1),
        //     CreatePiece("bPawn", FIRST_PIECE_BX + 3, FIRST_PIECE_BY - 1),
        //     CreatePiece("bPawn", FIRST_PIECE_BX + 4, FIRST_PIECE_BY - 1),
        //     CreatePiece("bPawn", FIRST_PIECE_BX + 5, FIRST_PIECE_BY - 1),
        //     CreatePiece("bPawn", FIRST_PIECE_BX + 6, FIRST_PIECE_BY - 1),
        //     CreatePiece("bPawn", FIRST_PIECE_BX + 7, FIRST_PIECE_BY - 1)
        // };

        // for (int i = 0; i < 2 * NUM_PIECES; i++)
        // {
        //     positions[playerWhite[i].GetX(), playerWhite[i].GetY()] = playerWhite[i];
        //     positions[playerBlack[i].GetX(), playerBlack[i].GetY()] = playerBlack[i];
        // }
        fromFenToBoard(startingFEN);

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
                spawnedTile.isLight(isLigt);
            }
        }

        cam.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10); //Move the camera to the middle of the screen
    }
    public void nullPosition(int x, int y)
    {
        positions[x, y] = null;
    }
    //Set the position of the piece so the program knows where a piece has moved
    public void SetPosition(Piece piece, int x, int y)
    {

        //Set values of the move played as the last move
        lastmove.piece = piece;
        lastmove.originalPos = new Vector2Int(piece.GetX(), piece.GetY());
        lastmove.goalPos = new Vector2Int(x, y);

        if (!replayingGame)
        {
            Move2 movePlayed;
            movePlayed.pieceName = piece.name;
            movePlayed.originalPos = new Vector2Int(piece.GetX(), piece.GetY());
            movePlayed.goalPos = new Vector2Int(x, y);

            if (positions[x, y] != null)
            {
                movePlayed.capture = true;
            }
            else
            {
                movePlayed.capture = false;
            }
            movePlayed.check = false;
            movePlayed.enpassant = false;
            movePlayed.castle = false;
            movePlayed.checkmate = false;
            movesPlayed.Add(movePlayed);
        }

        Vector2Int kingPos = findKing(playerToplay);
        Tile kingTile = GameObject.Find("Tile " + kingPos.x + " " + kingPos.y).GetComponent<Tile>();
        kingTile.resetColor(); //if the king was in check, reset the color of the king square to the original tile color

        positions[x, y] = piece;

        positions[piece.GetX(), piece.GetY()] = null;
        piece.SetX(x);
        piece.SetY(y);
        // if (!replayingGame)
        // {
        //     Fens.Add(convertToFen());
        // }
        //Send a message with the move to the other player
        if (onlineGame)
        {
            MakeMoveMsg move = new MakeMoveMsg();
            move.originalX = lastmove.originalPos.x;
            move.originalY = lastmove.originalPos.y;
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

                    // canCastle(piece);
                    // if (castleLong)
                    // {
                    //     createIndicator(x - 2, y, piece);
                    // }
                    // if (castleShort)
                    // {
                    //     createIndicator(x + 2, y, piece);
                    // }
                }

                break;

        }

    }

    //Create moves in a straight line or a diagonal
    private void createLineIndicator(int xStep, int yStep, Piece piece)
    {
        int x = piece.GetX() + xStep;
        int y = piece.GetY() + yStep;
        bool isOnBoard = onBoard(x, y);

        //Create moves on empty squares
        while (onBoard(x, y) && positions[x, y] == null)
        {
            moves.Add(new Vector2Int(x, y));
            x += xStep;
            y += yStep;
        }
        //Create a move if there is an enemy piece at the end of the straight line
        if (onBoard(x, y) && piece.GetPlayer() != positions[x, y].GetPlayer())
        {
            moves.Add(new Vector2Int(x, y));
            captures.Add(new Vector2Int(x, y));
        }
    }


    private void createPawnIndicator(int x, int yStep, Piece piece)
    {

        int y = piece.GetY() + yStep;
        bool isOnBoard = onBoard(x, y);
        if (!isOnBoard)
        {
            return;
        }
        bool isEmpty = positions[x, y] == null;
        bool hasMoved = piece.getHasMoved();
        string playerPiece = piece.GetPlayer();
        //Pawn can move two squares as its first move
        if (isEmpty && !hasMoved)
        {
            if (playerPiece == "white")
            {
                if (positions[x, y] == null)
                {
                    moves.Add(new Vector2Int(x, y));
                }
                if (positions[x, y + 1] == null)
                {
                    moves.Add(new Vector2Int(x, y + 1));
                }
            }
            else if (playerPiece == "black")
            {
                if (positions[x, y] == null)
                {
                    moves.Add(new Vector2Int(x, y));
                }
                if (positions[x, y - 1] == null)
                {
                    moves.Add(new Vector2Int(x, y - 1));
                }
            }
        }
        //Otherwise just move one square forward
        else if (isEmpty)
        {
            moves.Add(new Vector2Int(x, y));
        }
        //Check if we can take a piece and create a move if we can
        if (onBoard(x + 1, y) && positions[x + 1, y] != null && playerPiece != positions[x + 1, y].GetPlayer())
        {
            moves.Add(new Vector2Int(x + 1, y));
            captures.Add(new Vector2Int(x + 1, y));
        }
        if (onBoard(x - 1, y) && positions[x - 1, y] != null && playerPiece != positions[x - 1, y].GetPlayer())
        {
            moves.Add(new Vector2Int(x - 1, y));
            captures.Add(new Vector2Int(x - 1, y));
        }

        if (lastmove.piece == null)
        {
            return;
        }
        float movementY = Math.Abs(lastmove.goalPos.y - lastmove.originalPos.y);
        float xDiff = Math.Abs(piece.GetX() - lastmove.goalPos.x);
        //Check if en Passant is possible
        if (playerPiece == getPlayerToPlay())
        {
            if (playerPiece == "white")
            {
                if (piece.GetY() == height - 4 && isBPawn(lastmove.piece.name) && movementY == 2 && positions[lastmove.goalPos.x, height - 3] == null)
                {
                    if (xDiff == 1)
                    {
                        moves.Add(new Vector2Int(lastmove.goalPos.x, height - 3));
                        captures.Add(new Vector2Int(lastmove.goalPos.x, height - 3));
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
            else
            {
                if (piece.GetY() == 3 && isWPawn(lastmove.piece.name) && movementY == 2 && positions[lastmove.goalPos.x, 2] == null)
                {
                    if (xDiff == 1)
                    {
                        moves.Add(new Vector2Int(lastmove.goalPos.x, 2));
                        captures.Add(new Vector2Int(lastmove.goalPos.x, 2));
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
            if (isEmpty)
            {
                moves.Add(new Vector2Int(x, y));
            }
            else if (pieceAtPos.GetPlayer() != piece.GetPlayer())
            {
                moves.Add(new Vector2Int(x, y));
                captures.Add(new Vector2Int(x, y));
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
            Instantiate(moveIndicator, new Vector3(moves[i].x, moves[i].y, -1), Quaternion.identity);
        }
    }
    //Find the king of the specified player
    public Vector2Int findKing(string player)
    {
        string kingToFind = (player == "white") ? "wKing" : "bKing";
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (positions[i, j] != null)
                {
                    if (positions[i, j].name == kingToFind)
                    {
                        return new Vector2Int(i, j);
                    }
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    //Checks if the king is in check by checking if it can be "taken" by an enemy piece
    public bool isInCheck(string player)
    {
        Vector2Int kingPos = findKing(player);
        for (int i = 0; i < captures.Count; i++)
        {
            if (captures[i] == kingPos)
            {
                return true;
            }
        }
        return false;
    }

    public void legalMoves(Piece piece)
    {

        string playerToPlay = piece.GetPlayer();
        string enemyPlayer = playerToPlay == "white" ? "black" : "white";

        List<Vector2Int> legalMoves = new List<Vector2Int>();
        List<Vector2Int> myMoves = new List<Vector2Int>(moves); //save the current pieces possible moves

        Piece pieceToTake = null;
        int originalX = piece.GetX();
        int originalY = piece.GetY();
        //Generate all possible moves for the enemy pieces - these will be now saved in the moves array
        generateAllPseudoLegalMoves(enemyPlayer);
        // Debug.Log(convertToFen());
        // Debug.Log($"Legal moves for {playerToPlay} with enemy {enemyPlayer} for {piece.name}");
        // Debug.Log($"Number of legal moves for playerTOPLay {allLegalMoves.Count}");
        Tile kingTile = null;
        //Set the king tile to red if the king is in check
        if (isInCheck(playerToPlay))
        {
            Move2 checkMove = movesPlayed[movesPlayed.Count - 1];
            checkMove.check = true;
            movesPlayed[movesPlayed.Count - 1] = checkMove;
            //Debug.Log(movesPlayed[movesPlayed.Count - 1].pieceName + "checking piece");
            // if (piece.name == "wKing" || piece.name == "bKing" && (castleLong || castleShort))
            // {
            //     print("removing move");
            //     print(convertToFen2());
            //     castleLong = false;
            //     castleShort = false;
            //     myMoves.RemoveAt(myMoves.Count - 1);
            // }
            Vector2Int kingPos = findKing(playerToplay);
            kingTile = GameObject.Find("Tile " + kingPos.x + " " + kingPos.y).GetComponent<Tile>();
            kingTile.tileRed();
        }
        else if (piece.name == "wKing" || piece.name == "bKing")
        {
            clearMoves();
            GenerateIndicators(piece);
            canCastle(piece);
            if (castleLong)
            {
                myMoves.Add(new Vector2Int(originalX - 2, originalY));
                createIndicator(originalX - 2, originalY, piece);
            }
            if (castleShort)
            {
                myMoves.Add(new Vector2Int(originalX + 2, originalY));
                createIndicator(originalX + 2, originalY, piece);
            }
        }
        //Save the original position of the piece we want to move


        // Debug.Log($"Original X {originalX} and {originalY}");
        //Debug.Log("my moves");

        for (int j = 0; j < myMoves.Count; j++)
        {
            bool enW = false;
            bool enB = false;
            //Debug.Log(piece + " " + myMoves[j] + " " + myMoves.Count);
            //if the move of the piece would take an enemy piece save that piece

            if (positions[myMoves[j].x, myMoves[j].y] != null)
            {
                pieceToTake = positions[myMoves[j].x, myMoves[j].y];
            }
            else
            {
                if (j == myMoves.Count - 1 && Math.Abs(myMoves[j].x - originalX) == 1)
                {
                    if (playerToPlay == "white" && enPassantWhite == true && isWPawn(piece.name))
                    {
                        // && Math.Abs(myMoves[j].x - originalX) == 1 && lastmove.isBPawn(piece.name) && lastmove.goalPos.y == originalY && Math.Abs(lastmove.goalPos.y - lastmove.originalPos.y) == 2 && lastmove.goalPos.x == myMoves[j].x && Math.Abs(originalX - lastmove.goalPos.x) == 1
                        enW = true;
                        pieceToTake = positions[myMoves[j].x, myMoves[j].y - 1];
                        positions[myMoves[j].x, myMoves[j].y - 1] = null;
                    }
                    else if (playerToPlay == "black" && enPassantBlack == true && isBPawn(piece.name))
                    {
                        enB = true;
                        pieceToTake = positions[myMoves[myMoves.Count - 1].x, myMoves[myMoves.Count - 1].y + 1];
                        positions[myMoves[myMoves.Count - 1].x, myMoves[myMoves.Count - 1].y + 1] = null;
                    }
                }
            }

            //Make the move on the board programatically (the board does not visually change)
            positions[myMoves[j].x, myMoves[j].y] = piece;
            // piece.SetX(myMoves[j].x);
            // piece.SetY(myMoves[j].y);
            positions[originalX, originalY] = null;

            //After the move has been made generate the moves of the opponent again
            generateAllPseudoLegalMoves(enemyPlayer);

            //Debug.Log($"Number of pseudo legal moves for enemy {moves.Count}");
            //After the move has been made check if the king is still in check
            if (!isInCheck(playerToPlay))
            {
                legalMoves.Add(myMoves[j]); // if the king is not in check after the move make the move legal
            }
            // else
            // {
            //     Debug.Log($"{playerToPlay}Would be in check removing move for piece {piece.name}");
            // }
            //If a piece has been overwritten by the move set the piece back
            if (pieceToTake != null)
            {
                if (enW)
                {

                    //&& Math.Abs(myMoves[j].x - originalX) == 1 && lastmove.isBPawn(piece.name) && lastmove.goalPos.y == originalY &&
                    //Math.Abs(lastmove.goalPos.y - lastmove.originalPos.y) == 2 && lastmove.goalPos.x == myMoves[j].x && Math.Abs(originalX - lastmove.goalPos.x) == 1
                    positions[myMoves[j].x, myMoves[j].y] = null;
                    positions[myMoves[j].x, myMoves[j].y - 1] = pieceToTake;
                    enW = false;

                }
                else if (enB)
                {
                    positions[myMoves[j].x, myMoves[j].y] = null;
                    positions[myMoves[j].x, myMoves[j].y + 1] = pieceToTake;
                    enB = false;
                }
                else
                {
                    positions[myMoves[j].x, myMoves[j].y] = pieceToTake;
                }

                pieceToTake = null;
            }
            //if the move was on an empty square set the squere to null
            else
            {
                positions[myMoves[j].x, myMoves[j].y] = null;
            }
            //Set the piece to its original position
            positions[originalX, originalY] = piece;
            // piece.SetX(originalX);
            // piece.SetY(originalY);
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
            lastMoveX = legalMoves[legalMoves.Count - 1].x;
            lastMoveY = legalMoves[legalMoves.Count - 1].y;
        }
        //Check if castling is legal (the king cannot go through check thus the tile next to him has to be a legal move for castling to be legal)
        if ((piece.name == "wKing" || piece.name == "bKing") && legalMoves.Count >= 2)
        {
            if (((lastMoveX == 6 && (lastMoveY == 0 || lastMoveY == 7)) || (lastMoveX == 2 && (lastMoveY == 0 || lastMoveY == 7))) && (castleLong || castleShort))
            {
                int moveIndex = -1;
                Vector2Int moveToFind;
                Vector2Int castleMove1 = new Vector2Int(3, lastMoveY);
                Vector2Int castleMove2 = new Vector2Int(5, lastMoveY);
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
                    // Debug.Log("Legal move" + i + legalMoves[i]);
                    if (legalMoves[i] == moveToFind)
                    {
                        //Debug.Log("Found Castle move" + legalMoves[i]);
                        moveIndex = i;
                        break;
                    }
                }
                //If the move is not found, remove the castling move from legal moves
                if (moveIndex == -1)
                {
                    //Debug.Log("move removed");
                    legalMoves.RemoveAt(legalMoves.Count - 1);
                }
            }
        }
        else if ((piece.name == "wKing" || piece.name == "bKing") && ((lastMoveX == 6 && (lastMoveY == 0 || (lastMoveY == 7))) || (lastMoveX == 2 && (lastMoveY == 0 || lastMoveY == 7))) && (castleLong || castleShort))
        {
            //Debug.Log("move removed");
            legalMoves.RemoveAt(legalMoves.Count - 1);
        }
        //Set the possible moves to the legal moves
        //clearMoves();

        moves = new List<Vector2Int>(legalMoves);


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
                    moves.Clear();
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
        // moves.Clear();
        // allLegalMoves.Clear();
        // generateAllLegalMoves(player);
        // if (allLegalMoves.Count != 0)
        // {
        //     moves.Clear();
        //     allLegalMoves.Clear();
        //     return false;
        // }
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

        Move2 move = movesPlayed[movesPlayed.Count - 1];
        move.checkmate = true;
        movesPlayed[movesPlayed.Count - 1] = move;
        return true;
    }
    private int moveNuber = 0;
    public void generatePlayedMoves(int index)
    {

        int moveIndex = 1;
        Move2 move;
        for (int i = index; i < movesPlayed.Count; i++)
        {

            string text = $"{(moveNuber + 2) / 2} ";

            moveIndex++;
            string notation = convertNotation(movesPlayed[i]);
            //if the move was a castling move, remove the rook move (as castling is technically two moves, we remove one so it effectivelly counts as one)
            if (notation == "O-O" || notation == "O-O-O")
            {
                movesPlayed.RemoveAt(i + 1);
                move = movesPlayed[i];
                move.castle = true;
                movesPlayed[i] = move;

                // if (index == 0)
                // {
                //     Fens.RemoveAt(i + 1);
                // }
                // else
                // {
                //     Fens.RemoveAt(Fens.Count - 2);
                // }
                // Debug.Log($"INDEX {index}");
                // for (int j = 0; j < Fens.Count; j++)
                // {
                //     Debug.Log(Fens[j]);
                // }
                // Debug.Log($"INDEX {index}");
            }
            if (moveNuber % 2 != 0)
            {
                text = notation;
            }
            else
            {
                text += notation;
            }

            replayMove.generateReplayMove(text, moveNuber++);
            i++;
            if (i >= movesPlayed.Count)
            {
                break;
            }
            notation = convertNotation(movesPlayed[i]);
            if (notation == "O-O" || notation == "O-O-O")
            {
                movesPlayed.RemoveAt(i + 1);

                move = movesPlayed[i];
                move.castle = true;
                movesPlayed[i] = move;

                // if (index == 0)
                // {
                //     Fens.RemoveAt(i + 1);
                // }
                // else
                // {
                //     Fens.RemoveAt(Fens.Count - 2);
                // }

            }
            replayMove.generateReplayMove(notation, moveNuber++);
        }
    }
    //Check if its possible to castle (without checking if the king is in check)
    private void canCastle(Piece king)
    {

        if (king.getHasMoved() == false)
        {
            int kingX = king.GetX();
            int kingY = king.GetY();
            Piece initialRookPos = positions[kingX - 4, kingY];

            if (initialRookPos != null && (positions[kingX - 1, kingY] == null && positions[kingX - 2, kingY] == null
            && positions[kingX - 3, kingY] == null && initialRookPos?.getHasMoved() == false && (initialRookPos?.name == "wRook" || initialRookPos?.name == "bRook")))
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    if (moves[i].x == kingX - 1 && moves[i].y == kingY)
                    {

                        castleLong = true;
                        break;
                    }
                    else
                    {
                        castleLong = false;
                    }
                }
            }
            else
            {
                castleLong = false;
            }
            initialRookPos = positions[kingX + 3, kingY];
            if (initialRookPos != null && (positions[kingX + 1, kingY] == null && positions[kingX + 2, kingY] == null && initialRookPos?.getHasMoved() == false
                && (initialRookPos?.name == "wRook" || initialRookPos?.name == "bRook")))
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    if (moves[i].x == kingX + 1 && moves[i].y == kingY)
                    {

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
        // else
        // {
        //     castleShort = false;
        //     castleLong = false;
        // }
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
    public void setReplaingGame(bool value)
    {
        replayingGame = value;
    }
    public bool getReplaingGame()
    {
        return replayingGame;
    }
    public int getReplayMoveIndex()
    {
        return replayMoveIndex;
    }
    public int getNumMoves()
    {
        return movesPlayed.Count;
    }
    public void addFEN()
    {
        Fens.Add(convertToFen());
    }
    public void setComputerPlayer(string player)
    {
        computerPlayer = player;
    }
    public string getComputerPlayer()
    {
        return computerPlayer;
    }
    public int getLegalMovesCount()
    {
        int count = allLegalMoves.Count;
        allLegalMoves.Clear();
        return count;

    }
    public void setDepth(int depth)
    {
        treeDepth = depth;
    }
    public bool isWPawn(string name)
    {
        return name == "wPawn";
    }
    public bool isBPawn(string name)
    {
        return name == "bPawn";
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
            if (xDiff == 1 && positions[move.goalX, move.goalY] == null)
            {
                if (isWPawn(piece.name))
                {
                    pieceToTake = positions[move.goalX, move.goalY - 1];
                    if (pieceToTake != null)
                    {
                        Destroy(pieceToTake.gameObject);
                    }
                }
                if (isBPawn(piece.name))
                {
                    pieceToTake = positions[move.goalX, move.goalY + 1];
                    if (pieceToTake != null)
                    {
                        Destroy(pieceToTake.gameObject);
                    }
                }
            }

            //Handle queen promotion
            if (isWPawn(piece.name) && move.goalY == height - 1)
            {
                piece.name = "wQueen";
                piece.SetPiece();
            }
            else if (isBPawn(piece.name) && move.goalY == 0)
            {
                piece.name = "bQueen";
                piece.SetPiece();
            }
            //Move the piece to the position
            piece.transform.position = new Vector3(move.goalX, move.goalY, -1);
            piece.setHasMoved(true);
            SetPosition(piece, move.goalX, move.goalY);
            addFEN();
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
    public void replayGame()
    {

        destroyAssets();
        startGame();
        replayingGame = true;
        generatePlayedMoves(0);
        //Show a back and forward button

    }
    public void clearMovesPlayed()
    {
        movesPlayed.Clear();
    }
    public void replayNumMoves(int index)
    {
        // destroyAssets();
        // startGame();
        replayingGame = true;
        replayMoveIndex = index;
        DestroyIndicators();
        destroyPieces();
        fromFenToBoard(Fens[replayMoveIndex]);
        if (replayMoveIndex > 0)
        {
            resetMove(highlightedMove);
        }
        highlightMove(replayMoveIndex - 1);
        // for (int i = 0; i < index; i++)
        // {
        //     Piece piece = getPosition(movesPlayed[i].originalX, movesPlayed[i].originalY);
        //     piece.transform.position = new Vector3(movesPlayed[i].goalX, movesPlayed[i].goalY, -1);
        //     Piece pieceAtPos = positions[movesPlayed[i].goalX, movesPlayed[i].goalY];
        //     if (pieceAtPos != null)
        //     {
        //         pieceAtPos.transform.position = new Vector3(movesPlayed[i].goalX, movesPlayed[i].goalY, -100); ;
        //     }
        //     if (piece.name == "wQueen" && movesPlayed[i].goalY == height - 1)
        //     {

        //         piece.name = "wQueen";
        //         piece.SetPiece();
        //     }
        //     else if (piece.name == "bQueen" && movesPlayed[i].goalY == 0)
        //     {
        //         piece.name = "bQueen";
        //         piece.SetPiece();
        //     }
        //     SetPosition(piece, movesPlayed[i].goalX, movesPlayed[i].goalY);
        // }
        // if (index % 2 == 0)
        // {
        //     playerToplay = "white";
        // }
        // else
        // {
        //     playerToplay = "black";
        // }
    }
    public void replayNextMove()
    {

        if (replayMoveIndex < Fens.Count - 1)
        {
            replayingGame = true;
            destroyPieces();
            DestroyIndicators();
            if (replayMoveIndex > 0)
            {
                resetMove(highlightedMove);
            }
            highlightMove(replayMoveIndex);


            replayMoveIndex++;
            fromFenToBoard(Fens[replayMoveIndex]);


        }
        // if (replayMoveIndex < movesPlayed.Count)
        // {
        //     int i = replayMoveIndex;
        //     Piece piece = getPosition(movesPlayed[i].originalX, movesPlayed[i].originalY);
        //     piece.transform.position = new Vector3(movesPlayed[i].goalX, movesPlayed[i].goalY, -1);
        //     Piece pieceAtPos = positions[movesPlayed[i].goalX, movesPlayed[i].goalY];
        //     // Debug.Log(pieceAtPos);
        //     if (pieceAtPos != null)
        //     {
        //         pieceAtPos.transform.position = new Vector3(movesPlayed[i].goalX, movesPlayed[i].goalY, -100); ;
        //     }
        //     SetPosition(piece, movesPlayed[i].goalX, movesPlayed[i].goalY);
        //     replayMoveIndex++;
        //     if (movesPlayed[i].castle)
        //     {
        //         replayNextMove();
        //     }
        //     if (replayMoveIndex % 2 == 0)
        //     {
        //         playerToplay = "white";
        //     }
        //     else
        //     {
        //         playerToplay = "black";
        //     }
        // }
    }
    public void setLastMove()
    {
        lastmove.piece = positions[movesPlayed[replayMoveIndex - 1].goalPos.x, movesPlayed[replayMoveIndex - 1].goalPos.y];
        lastmove.originalPos = new Vector2Int(movesPlayed[replayMoveIndex - 1].originalPos.x, movesPlayed[replayMoveIndex - 1].originalPos.y);
        lastmove.goalPos = new Vector2Int(movesPlayed[replayMoveIndex - 1].goalPos.x, movesPlayed[replayMoveIndex - 1].goalPos.y);
    }
    public void replayPrevMove()
    {
        if (replayMoveIndex > 0)
        {
            replayingGame = true;

            DestroyIndicators();
            // replayNumMoves(replayMoveIndex - 1);
            //replayMoveIndex--;
            destroyPieces();
            replayMoveIndex--;
            if (replayMoveIndex < movesPlayed.Count)
            {
                resetMove(highlightedMove);
            }
            if (replayMoveIndex != 0)
            {
                highlightMove(replayMoveIndex - 1);
            }

            fromFenToBoard(Fens[replayMoveIndex]);
        }

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
        clearMoves();
        captures.Clear();
        allLegalMoves.Clear();
        replayingGame = false;
        replayMoveIndex = 0;
        moveNuber = 0;
        computerPlayer = "";


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
    private string convertNotation(Move2 move)
    {
        string notation = null;
        switch (move.pieceName)
        {
            case "wPawn":
            case "bPawn":
                break;
            case "wRook":
            case "BRook":
                notation += "R";
                break;
            case "wQueen":
            case "bQueen":
                notation += "Q";
                break;
            case "wBishop":
            case "bBishop":
                notation += "B";
                break;
            case "wKnight":
            case "bKnight":
                notation += "N";
                break;
            case "wKing":
            case "bKing":
                if (Math.Abs(move.goalPos.x - move.originalPos.x) == 2)
                {
                    if (move.goalPos.x > 4)
                    {
                        notation = "O-O";

                    }
                    else
                    {
                        notation = "O-O-O";
                    }
                    return notation;
                }
                notation += "K";
                break;

        }


        notation += convertToFile(move.originalPos.x);
        notation += $"{move.originalPos.y + 1}";

        if (move.capture)
        {
            notation += "x";
        }

        notation += convertToFile(move.goalPos.x);
        notation += $"{move.goalPos.y + 1}";
        if (move.pieceName == "wPawn" && move.goalPos.y == height - 1)
        {
            notation += "Q";
        }
        else if (move.pieceName == "bPawn" && move.goalPos.y == 0)
        {
            notation += "Q";
        }
        if (move.check)
        {
            notation += "+";
        }
        if (move.checkmate)
        {
            notation += "+";
        }
        return notation;
    }
    private static string convertToFile(int file)
    {
        return ((char)('a' + file)).ToString();
    }

    public string convertToFen()
    {
        string fenNotation = "";
        for (int i = height - 1; i >= 0; i--)
        {
            int emptySquares = 0;
            for (int j = 0; j < width; j++)
            {


                if (positions[j, i] == null)
                {
                    emptySquares++;
                    if (emptySquares == 8)
                    {
                        fenNotation += $"{emptySquares}";
                    }
                }
                else
                {
                    string name = positions[j, i].name;
                    if (emptySquares != 0)
                    {
                        fenNotation += $"{emptySquares}";
                        emptySquares = 0;
                    }

                    switch (name)
                    {

                        case "wPawn":
                            fenNotation += "P";
                            break;
                        case "bPawn":
                            fenNotation += "p";
                            break;
                        case "wRook":
                            fenNotation += "R";
                            break;
                        case "bRook":
                            fenNotation += "r";
                            break;
                        case "wQueen":
                            fenNotation += "Q";
                            break;
                        case "bQueen":
                            fenNotation += "q";
                            break;
                        case "wBishop":
                            fenNotation += "B";
                            break;
                        case "bBishop":
                            fenNotation += "b";
                            break;
                        case "wKnight":
                            fenNotation += "N";
                            break;
                        case "bKnight":
                            fenNotation += "n";
                            break;
                        case "wKing":
                            fenNotation += "K";
                            break;
                        case "bKing":
                            fenNotation += "k";
                            break;

                    }
                }
            }

            fenNotation += "/";
        }
        if (playerToplay == "white")
        {
            fenNotation += " b";
        }
        else
        {
            fenNotation += " w";
        }
        castleLong = false;
        castleShort = false;
        Vector2Int kingPos = findKing("white");
        Piece king = positions[kingPos.x, kingPos.y];
        clearMoves();
        GenerateIndicators(king);
        if (king.GetPlayer() != playerToplay)
        {
            canCastle(king);
        }
        clearMoves();
        if (castleShort)
        {
            fenNotation += " K";
            castleShort = false;
        }
        else
        {
            fenNotation += " -";
        }
        if (castleLong)
        {
            fenNotation += "Q";
            castleLong = false;
        }
        else
        {
            fenNotation += "-";
        }

        kingPos = findKing("black");
        king = positions[kingPos.x, kingPos.y];
        GenerateIndicators(king);
        if (king.GetPlayer() != playerToplay)
        {
            canCastle(king);
        }
        if (castleShort)
        {
            fenNotation += " k";
        }
        else
        {
            fenNotation += " -";
        }
        if (castleLong)
        {
            fenNotation += "q";
        }
        else
        {
            fenNotation += "-";
        }
        return fenNotation;
    }

    public string convertToFen2()
    {

        string fenNotation = "";
        for (int i = height - 1; i >= 0; i--)
        {
            int emptySquares = 0;
            for (int j = 0; j < width; j++)
            {
                if (positions[j, i] == null)
                {
                    emptySquares++;
                    if (emptySquares == 8)
                    {
                        fenNotation += $"{emptySquares}";
                    }
                }
                else
                {
                    string name = positions[j, i].name;
                    if (emptySquares != 0)
                    {
                        fenNotation += $"{emptySquares}";
                        emptySquares = 0;
                    }
                    switch (name)
                    {
                        case "wPawn":
                            fenNotation += "P";
                            break;
                        case "bPawn":
                            fenNotation += "p";
                            break;
                        case "wRook":
                            fenNotation += "R";
                            break;
                        case "bRook":
                            fenNotation += "r";
                            break;
                        case "wQueen":
                            fenNotation += "Q";
                            break;
                        case "bQueen":
                            fenNotation += "q";
                            break;
                        case "wBishop":
                            fenNotation += "B";
                            break;
                        case "bBishop":
                            fenNotation += "b";
                            break;
                        case "wKnight":
                            fenNotation += "N";
                            break;
                        case "bKnight":
                            fenNotation += "n";
                            break;
                        case "wKing":
                            fenNotation += "K";
                            break;
                        case "bKing":
                            fenNotation += "k";
                            break;
                    }
                }
            }

            fenNotation += "/";
        }
        return fenNotation;
    }
    public void fromFenToBoard(string FEN)
    {
        //Split the board part from the rest
        string[] fenParts = FEN.Split(' ');
        string[] fenRows = fenParts[0].Split('/');

        playerToplay = fenParts[1] == "w" ? "white" : "black";
        if (playerToplay == "white")
        {
            castleShort = fenParts[2].Contains("K");
            castleLong = fenParts[2].Contains("Q");
        }
        else
        {
            castleShort = fenParts[3].Contains("k");
            castleLong = fenParts[3].Contains("q");
        }

        for (int row = 0; row < 8; row++)
        {
            string fenRow = fenRows[7 - row];
            int col = 0;

            foreach (char fenChar in fenRow)
            {
                if (char.IsDigit(fenChar))
                {
                    col += (int)char.GetNumericValue(fenChar);
                }
                else
                {
                    string player = char.IsUpper(fenChar) ? "w" : "b";
                    string type = getPieceType(char.ToLower(fenChar));
                    string piece = player + type;
                    Piece newPiece = CreatePiece(piece, col, row);
                    if (piece == "wPawn" && row != 1)
                    {
                        newPiece.setHasMoved(true);
                    }
                    else if (piece == "bPawn" && row != height - 2)
                    {
                        newPiece.setHasMoved(true);
                    }
                    else if (piece == "bKing")
                    {
                        for (int i = 0; i < movesPlayed.Count; i++)
                        {
                            if (movesPlayed[i].pieceName == "bKing")
                            {
                                newPiece.setHasMoved(true);
                                break;
                            }
                        }
                    }
                    else if (piece == "wKing")
                    {
                        for (int i = 0; i < movesPlayed.Count; i++)
                        {
                            if (movesPlayed[i].pieceName == "wKing")
                            {
                                newPiece.setHasMoved(true);
                                break;
                            }
                        }
                    }
                    positions[col, row] = newPiece;
                    col++;
                }
            }
        }
    }

    private string getPieceType(char fenChar)
    {
        switch (fenChar)
        {
            case 'p': return "Pawn";
            case 'n': return "Knight";
            case 'b': return "Bishop";
            case 'r': return "Rook";
            case 'q': return "Queen";
            case 'k': return "King";
            default: throw new ArgumentException($"Invalid FEN character: {fenChar}");
        }
    }
    private int highlightedMove = -1;
    public void destroyPieces()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        //Destroy all pieces except the general piece that has been initialized as a [Serailized field], always index 0 so start at index 1 to not destry it
        for (int i = 1; i < pieces.Length; i++)
        {
            Destroy(pieces[i]);
        }
    }
    public void highlightMove(int index)
    {
        ReplayMove move = GameObject.Find($"Move {index}")?.GetComponent<ReplayMove>();
        highlightedMove = index;
        move?.underlineText();
    }
    public void resetMove(int index)
    {
        ReplayMove move = GameObject.Find($"Move {index}")?.GetComponent<ReplayMove>();
        move?.resetStyle();
    }
    public void destroyMoves(int index)
    {
        for (int i = index; i <= movesPlayed.Count + 1; i++)
        {
            // movesPlayed?.RemoveAt(i);
            GameObject move = GameObject.Find($"Move {i}");
            Destroy(move);
        }
        //Debug.Log("Before removing");
        // for (int i = 0; i < movesPlayed.Count; i++)
        // {
        //     Debug.Log(movesPlayed[i].pieceName);
        // }
        // Debug.Log("Removeing");

        for (int i = index; i < movesPlayed.Count; i++)
        {
            //Debug.Log("Removing move " + movesPlayed[i]);
            movesPlayed.RemoveAt(i);

        }
        // for (int i = 0; i < movesPlayed.Count; i++)
        // {
        //     Debug.Log(movesPlayed[i].pieceName);
        // }
        for (int i = Fens.Count - 1; i > index; i--)
        {
            Fens.RemoveAt(i);
        }
        moveNuber = index;
    }

    //=======AI================

    //Generate all pseudo legal moves (not taking into account for check) for a player
    public void generateAllPseudoLegalMoves(string player)
    {
        captures.Clear();
        moves.Clear();
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                Piece a = positions[i, k];
                if (a != null && a.GetPlayer() == player)
                {
                    GenerateIndicators(a);
                }
            }
        }
    }
    public void generateAllLegalMoves(string player)
    {
        // string boardBeforeAIMove = convertToFen2();

        allLegalMoves.Clear();
        moves.Clear();
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                Piece a = positions[k, i];
                if (a != null && a.GetPlayer() == player)
                {
                    enPassantBlack = false;
                    enPassantWhite = false;
                    castleLong = false;
                    castleShort = false;
                    GenerateIndicators(a);
                    // Debug.Log($"{a.name} moves count " + moves.Count);
                    legalMoves(a);
                    // Debug.Log($"{a.name}moves count " + moves.Count);
                    // Debug.Log("ALL lEGAL MOVES CALL AFTE LEGAM MOVES");

                    for (int j = 0; j < moves.Count; j++)
                    {
                        //Debug.Log(moves[j]);
                        Move move;
                        move.piece = a;
                        move.goalPos = new Vector2Int(moves[j].x, moves[j].y);
                        move.originalPos = new Vector2Int(a.GetX(), a.GetY());
                        allLegalMoves.Add(move);
                    }

                    moves.Clear();
                    // if (convertToFen2() != boardBeforeAIMove)
                    // {
                    //     print($"FROM {a} equal {boardBeforeAIMove} // {convertToFen2()}");

                    // }
                    // moves.Clear();
                }
            }
        }

    }

    public void playRandomMove()
    {
        Random rnd = new Random();
        int num = rnd.Next(0, allLegalMoves.Count);
        if (allLegalMoves.Count == 0)
        {
            GeneralPiece.gameOver("white", 1);
            return;
        }
        if (positions[allLegalMoves[num].goalPos.x, allLegalMoves[num].goalPos.y] != null)
        {
            Destroy(positions[allLegalMoves[num].goalPos.x, allLegalMoves[num].goalPos.y].gameObject);
        }
        SetPosition(allLegalMoves[num].piece, allLegalMoves[num].goalPos.x, allLegalMoves[num].goalPos.y);
        allLegalMoves[num].piece.setPieceToPos(new Vector3(allLegalMoves[num].goalPos.x, allLegalMoves[num].goalPos.y, -1));
        allLegalMoves[num].piece.setHasMoved(true);
        allLegalMoves.Clear();
        playerToplay = "white";
    }

    public int countMaterial()
    {
        int material = 0;
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                Piece piece = positions[i, k];
                if (piece != null)
                {
                    string pieceColor = piece.name.Substring(0, 1);
                    //Set the multipler to 1 for white pieces and -1 for black pieces
                    int multiplayer = pieceColor == "b" ? -1 : 1;
                    string pieceName = piece.name.Substring(1);
                    switch (pieceName)
                    {
                        case "Pawn":
                            material += multiplayer * pawnValue;
                            break;
                        case "Knight":
                            material += multiplayer * knightValue;
                            break;
                        case "Bishop":
                            material += multiplayer * bishopValue;
                            break;
                        case "Rook":
                            material += multiplayer * rookValue;
                            break;
                        case "Queen":
                            material += multiplayer * queenValue;
                            break;
                    }
                }
            }
        }
        return material;
    }

    public Move bestMove()
    {
        //List<Vector2> positionsBeforeAI = new List<Vector2>();
        string boardBeforeAIMove = convertToFen();
        generateAllLegalMoves("black");
        List<Move> blackMoves = new List<Move>(allLegalMoves);
        shuffleList(blackMoves);
        //Debug.Log(blackMoves.Count);
        Move bestMove = new Move();
        int score = 0;
        int opponentMinMaxScore = 999;
        foreach (Move move in blackMoves)
        {

            bool piece1Moved = positions[move.originalPos.x, move.originalPos.y].getHasMoved();
            Piece pieceToTake1 = makeMove(move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);

            playerToplay = "white";
            generateAllLegalMoves("white");

            List<Move> opponentMoves = new List<Move>(allLegalMoves);
            // Debug.Log(opponentMoves.Count);
            int opponentMaxScore = -checkmateValue;
            foreach (Move enemyMove in opponentMoves)
            {

                bool piece2Moved = positions[enemyMove.originalPos.x, enemyMove.originalPos.y].getHasMoved();
                Piece pieceToTake2 = makeMove(enemyMove.originalPos.x, enemyMove.originalPos.y, enemyMove.goalPos.x, enemyMove.goalPos.y);


                if (checkmate("black"))
                {

                    // Debug.Log("black would be checkamted");
                    // Debug.Log(convertToFen());
                    score = checkmateValue;
                }
                else
                {
                    score = countMaterial();
                }
                allLegalMoves.Clear();
                if (score > opponentMaxScore)
                {
                    opponentMaxScore = score;
                }

                unMakeMove(pieceToTake2, piece2Moved, enemyMove.originalPos.x, enemyMove.originalPos.y, enemyMove.goalPos.x, enemyMove.goalPos.y);

            }
            if (opponentMaxScore < opponentMinMaxScore)
            {
                opponentMinMaxScore = opponentMaxScore;
                bestMove = move;
            }


            unMakeMove(pieceToTake1, piece1Moved, move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);
        }
        //List<Vector2> positionsAfterAI = new List<Vector2>();
        string boardAfterAIMove = convertToFen();
        if (boardAfterAIMove.Split(" ")[0] != boardBeforeAIMove.Split(" ")[0])
        {
            print($"{boardAfterAIMove.Split(" ")[0]}{boardBeforeAIMove.Split(" ")[0]}");
            print("boards are not equal");
        }
        // for (int i = 0; i < width; i++)
        // {
        //     for (int k = 0; k < height; k++)
        //     {
        //         Piece piece = positions[i, k];
        //         if (piece != null)
        //         {
        //             positionsAfterAI.Add(new Vector2(piece.GetX(), piece.GetY()));
        //         }
        //     }
        // }
        // for (int i = 0; i < positionsAfterAI.Count; i++)
        // {
        //     if (positionsAfterAI[i] != positionsBeforeAI[i])
        //     {
        //         print($"X,Y are not equal{positionsAfterAI[i]} and {positionsBeforeAI[i]}");
        //     }
        // }
        return bestMove;
    }
    private Move BESTMOVE = new Move();

    //Minimax algorithm with alpha-beta prunning which finds a best move for a player based on an evelation function
    public int Minimax(int depth, bool isMaximizingPlayer, int alpha, int beta)
    {
        if (depth == 0)
        {
            if (isMaximizingPlayer)
            {

                if (checkmate("white"))
                {
                    return -checkmateValue;
                }
            }
            else
            {
                if (checkmate("black"))
                {
                    return checkmateValue;
                }
            }
            return countMaterial();
        }

        if (isMaximizingPlayer)
        {
            int maxEval = -checkmateValue;
            playerToplay = "white";

            generateAllLegalMoves(playerToplay);
            // List<Move> legalMoves = new List<Move>(allLegalMoves);
            // shuffleList(legalMoves);
            List<Move> legalMoves = new List<Move>(orderMoves(playerToplay));

            foreach (Move move in legalMoves)
            {
                string boardBeforeAIMove = convertToFen2();
                bool piece1Moved = positions[move.originalPos.x, move.originalPos.y].getHasMoved();
                // print($"MAKING MOVE {move.originalPos.x}, {move.originalPos.y}, {move.goalPos.x}, {move.goalPos.y}");
                Piece pieceToTake1 = makeMove(move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);
                int eval = Minimax(depth - 1, false, alpha, beta);

                //print($"UNMAKING MOVE {move.originalPos.x}, {move.originalPos.y}, {move.goalPos.x}, {move.goalPos.y}");
                unMakeMove(pieceToTake1, piece1Moved, move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);
                if (convertToFen2() != boardBeforeAIMove)
                {
                    print($"DEPTH {depth} boards are not equal {boardBeforeAIMove} // {convertToFen2()}");
                }
                if (eval > maxEval)
                {
                    maxEval = eval;
                    if (depth == treeDepth)
                    {
                        BESTMOVE = move;
                    }
                }
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = checkmateValue;
            playerToplay = "black";

            generateAllLegalMoves(playerToplay);

            // List<Move> legalMoves = new List<Move>(allLegalMoves);
            // shuffleList(legalMoves);
            List<Move> legalMoves = new List<Move>(orderMoves(playerToplay));
            foreach (Move move in legalMoves)
            {
                bool piece1Moved = positions[move.originalPos.x, move.originalPos.y].getHasMoved();
                string boardBeforeAIMove = convertToFen2();
                //print($"MAKING MOVE {move.originalPos.x}, {move.originalPos.y}, {move.goalPos.x}, {move.goalPos.y}");
                Piece pieceToTake1 = makeMove(move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);
                int eval = Minimax(depth - 1, true, alpha, beta);

                // print($"UNMAKING MOVE {move.originalPos.x}, {move.originalPos.y}, {move.goalPos.x}, {move.goalPos.y}");
                unMakeMove(pieceToTake1, piece1Moved, move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);
                if (convertToFen2() != boardBeforeAIMove)
                {
                    print($"DEPTH {depth} boards are not equal {boardBeforeAIMove} // {convertToFen2()}");
                }
                if (eval < minEval)
                {
                    minEval = eval;
                    if (depth == treeDepth)
                    {
                        //print($"settiing move to piece{move.piece.name}");
                        BESTMOVE = move;
                    }
                }
                if (depth == treeDepth && BESTMOVE.piece == null)
                {
                    print($"last settiing move to piece{move.piece.name}");
                    BESTMOVE = move;
                }
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }
            return minEval;
        }
    }
    bool wQPromotion = false;
    bool bQPromotion = false;
    public Piece makeMove(int originalX, int originalY, int goalX, int goalY)
    {
        Piece pieceToTake = null;
        string playerToPlay = positions[originalX, originalY].GetPlayer();
        Piece piece = positions[originalX, originalY];

        //if the king was in check, reset the color of the king square to the original tile color
        Vector2Int kingPos = findKing(playerToplay);
        Tile kingTile = GameObject.Find("Tile " + kingPos.x + " " + kingPos.y).GetComponent<Tile>();
        kingTile.resetColor();

        if (positions[goalX, goalY] != null)
        {
            pieceToTake = positions[goalX, goalY];
        }
        //Check for en passant
        else if (playerToPlay == "white" && Math.Abs(originalX - goalX) == 1 && isWPawn(piece.name) && positions[goalX, goalY] == null)
        {
            //&& lastmove.isBPawn(piece.name) && lastmove.goalPos.y == originalY &&
            //Math.Abs(lastmove.goalPos.y - lastmove.originalPos.y) == 2 && lastmove.goalPos.x == goalX && Math.Abs(originalX - lastmove.goalPos.x) == 1
            pieceToTake = positions[goalX, goalY - 1];
            positions[goalX, goalY - 1] = null;
        }
        else if (playerToPlay == "black" && Math.Abs(originalX - goalX) == 1 && isBPawn(piece.name) && positions[goalX, goalY] == null)
        {
            //&& lastmove.isWPawn(piece.name) && lastmove.goalPos.y == originalY 
            //&& Math.Abs(lastmove.goalPos.y - lastmove.originalPos.y) == 2 && lastmove.goalPos.x == goalX && Math.Abs(originalX - lastmove.goalPos.x) == 1
            pieceToTake = positions[goalX, goalY + 1];
            positions[goalX, goalY + 1] = null;
        }
        // if (isWPawn(piece.name) && goalY == 7)
        // {
        //     piece.name = "wQueen";
        //     piece.SetPiece();
        //     wQPromotion = true;
        // }
        // else if (isBPawn(piece.name) && goalY == 0)
        // {
        //     piece.name = "bQueen";
        //     piece.SetPiece();
        //     bQPromotion = true;
        // }
        // else
        // {
        //     wQPromotion = true;
        //     bQPromotion = true;
        // }
        lastmove.goalPos = new Vector2Int(goalX, goalY);
        lastmove.originalPos = new Vector2Int(originalX, originalY);
        lastmove.piece = piece;

        //Make the move on the board programatically (the board does not visually change)
        positions[goalX, goalY] = piece;
        piece.setHasMoved(true);
        positions[originalX, originalY] = null;
        piece.SetX(goalX);
        piece.SetY(goalY);

        return pieceToTake;
    }
    public void unMakeMove(Piece takenPiece, bool originalPieceMoved, int originalX, int originalY, int currentX, int currentY)
    {
        Vector2Int kingPos = findKing(playerToplay);
        Tile kingTile = GameObject.Find("Tile " + kingPos.x + " " + kingPos.y).GetComponent<Tile>();
        kingTile.resetColor();
        Piece piece = positions[currentX, currentY];
        if (takenPiece != null)
        {
            positions[currentX, currentY] = null;
            positions[takenPiece.GetX(), takenPiece.GetY()] = takenPiece;
        }
        else
        {
            positions[currentX, currentY] = null;
        }

        positions[originalX, originalY] = piece;
        piece.setHasMoved(originalPieceMoved);
        piece.SetX(originalX);
        piece.SetY(originalY);
        // if (wQPromotion)
        // {
        //     piece.name = "wPawn";
        //     piece.SetPiece();
        // }
        // else if (bQPromotion)
        // {
        //     piece.name = "bPawn";
        //     piece.SetPiece();
        // }
        castleLong = false;
        castleShort = false;
        enPassantBlack = false;
        enPassantWhite = false;

    }
    public void playBestMove(Move move)
    {
        Piece pieceAtPos = positions[move.goalPos.x, move.goalPos.y];
        SetPosition(move.piece, move.goalPos.x, move.goalPos.y);
        if (pieceAtPos != null)
        {
            Destroy(pieceAtPos.gameObject);
        }
        move.piece.setPieceToPos(new Vector3(move.goalPos.x, move.goalPos.y, -1));
        move.piece.setHasMoved(true);
        addFEN();
        playerToplay = "white";
    }

    public Move getBestMove()
    {
        BESTMOVE = new Move();
        Minimax(treeDepth, false, -checkmateValue, checkmateValue);
        playBestMove(BESTMOVE);
        return BESTMOVE;
    }

    //Randomize the positions of elements in a list
    void shuffleList<T>(List<T> list)
    {
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    //Orders the legal moves so captures are first in the list
    private List<Move> orderMoves(string player)
    {
        List<Move> orderedMoves = new List<Move>();

        generateAllPseudoLegalMoves(player);
        if (captures.Count == 0)
        {
            shuffleList(allLegalMoves);
            return new List<Move>(allLegalMoves);
        }
        for (int i = 0; i < allLegalMoves.Count; i++)
        {
            for (int j = 0; j < captures.Count; j++)
            {
                if (allLegalMoves[i].goalPos == captures[j])
                {
                    orderedMoves.Add(allLegalMoves[i]);
                    captures.RemoveAt(j);
                    break;

                }
            }
        }

        orderedMoves.AddRange(allLegalMoves);
        return orderedMoves;
    }

}


