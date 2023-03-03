using System.Collections;
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
    [SerializeField] private int width, height;

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

    public struct Move
    {
        public Vector2 originalPos;
        public Vector2 currentPos;
        public Piece piece;
    }

    Move lastmove;
    private bool startAsBlack = false;
    private bool startAsWhite = false;
    private bool onlineGame = false;
    void Start() { }

    void Awake()
    {
        registerEvents();
        positions = new Piece[width, height];
    }

    public void startGame()
    {
        playerToplay = "white";
        menuAnimator.SetTrigger("NoMenu");
        GenerateGrid();
        playerWhite = new Piece[]
        {
            CreatePiece("wRook", width - 8, 0),
            CreatePiece("wKnight", width - 7, 0),
            CreatePiece("wBishop", width - 6, 0),
            CreatePiece("wQueen", width - 5, 0),
            CreatePiece("wKing", width - 4, 0),
            CreatePiece("wBishop", width - 3, 0),
            CreatePiece("wKnight",width - 2, 0),
            CreatePiece("wRook", width - 1, 0),
            CreatePiece("wPawn", width - 8, 1),
            CreatePiece("wPawn", width - 7, 1),
            CreatePiece("wPawn", width - 6, 1),
            CreatePiece("wPawn", width - 5, 1),
            CreatePiece("wPawn", width - 4, 1),
            CreatePiece("wPawn", width - 3, 1),
            CreatePiece("wPawn", width - 2, 1),
            CreatePiece("wPawn", width - 1, 1)
        };
        playerBlack = new Piece[]
        {
            CreatePiece("bRook", width - 8, height - 1),
            CreatePiece("bKnight", width - 7, height - 1),
            CreatePiece("bBishop", width - 6, height - 1),
            CreatePiece("bQueen", width - 5, height - 1),
            CreatePiece("bKing", width - 4, height - 1),
            CreatePiece("bBishop", width - 3, height - 1),
            CreatePiece("bKnight", width - 2, height - 1),
            CreatePiece("bRook", width - 1, height - 1),
            CreatePiece("bPawn", width - 8, height - 2),
            CreatePiece("bPawn", width - 7, height - 2),
            CreatePiece("bPawn", width - 6, height - 2),
            CreatePiece("bPawn", width - 5, height - 2),
            CreatePiece("bPawn", width - 4, height - 2),
            CreatePiece("bPawn", width - 3, height - 2),
            CreatePiece("bPawn", width - 2, height - 2),
            CreatePiece("bPawn", width - 1, height - 2)
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
        Instantiate(forestTileLight, new Vector3(0, 0), Quaternion.Euler(0f, 0f, 90f * 0));
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
                    Instantiate(forestTileDark, new Vector3(x, y), Quaternion.Euler(0f, 0f, 90f * randomNumber));
                }
                spawnedTile.isLight(isLigt);
            }
        }

        cam.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10); //Move the camera to the middle of the screen
    }

    public void SetPosition(Piece piece, int x, int y)
    {
        //Set values of the move played as the last move
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

    private void createLineIndicator(int xStep, int yStep, Piece piece)
    {
        int x = piece.GetX() + xStep;
        int y = piece.GetY() + yStep;
        while (onBoard(x, y) && positions[x, y] == null)
        {
            moves.Add(new Vector3(x, y, -1));
            //Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
            x += xStep;
            y += yStep;
        }
        if (onBoard(x, y) && piece.GetPlayer() != positions[x, y].GetPlayer())
        {
            moves.Add(new Vector3(x, y, -1));
            // Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
        }
    }

    private void createPawnIndicator(int x, int yStep, Piece piece)
    {
        int y = piece.GetY() + yStep;
        bool isOnBoard = onBoard(x, y);
        bool isEmpty = positions[x, y] == null;
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

            // Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
            //Instantiate(moveIndicator, new Vector3(x, y + 1, -1), Quaternion.identity);
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
            && positions[x + 1, y] != null
            && piece.GetPlayer() != positions[x + 1, y].GetPlayer()
        )
        {
            moves.Add(new Vector3(x + 1, y, -1));
            // Instantiate(moveIndicator, new Vector3(x + 1, y, -1), Quaternion.identity);
        }
        if (
            onBoard(x - 1, y)
            && positions[x - 1, y] != null
            && piece.GetPlayer() != positions[x - 1, y].GetPlayer()
        )
        {
            moves.Add(new Vector3(x - 1, y, -1));
            // Instantiate(moveIndicator, new Vector3(x - 1, y, -1), Quaternion.identity);
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
                // Instantiate(moveIndicator, new Vector3(x, y, -1), Quaternion.identity);
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

    public void clearMoves()
    {
        moves.Clear();
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
            Debug.Log(piece + " " + myMoves[j] + " " + myMoves.Count);
            //if the move of the piece would take an enemy piece save that piece
            if (positions[(int)myMoves[j].x, (int)myMoves[j].y] != null)
            {
                pieceToTake = positions[(int)myMoves[j].x, (int)myMoves[j].y];
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
        return true;
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

    //Check if its possible to castle (without checking if the king is in check)
    private void canCastle(Piece king)
    {
        int kingX = king.GetX();
        int kingY = king.GetY();
        if (king.getHasMoved() == false)
        {
            Piece initialRookPos = positions[kingX - 4, kingY];

            if (initialRookPos != null && (positions[kingX - 1, kingY] == null && positions[kingX - 2, kingY] == null
            && positions[kingX - 3, kingY] == null && initialRookPos.getHasMoved() == false && (initialRookPos.name == "wRook" ||
            initialRookPos.name == "bRook")))
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    if (moves[i].x == kingX - 1 && moves[i].y == kingY)
                    {
                        createIndicator(kingX - 2, kingY, king);
                        castleLong = true;
                        break;
                    }
                    else { }
                }
            }
            else
            {
                castleLong = false;
            }
            if (positions[kingX + 3, kingY] != null
                && (
                    positions[kingX + 1, kingY] == null
                    && positions[kingX + 2, kingY] == null
                    && positions[kingX + 3, kingY].getHasMoved() == false
                    && (
                        positions[kingX + 3, kingY].name == "wRook"
                        || positions[kingX + 3, kingY].name == "bRook"
                    )
                )
            )
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

    private void registerEvents()
    {
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

        if (numPlayers == 1)
        {
            Server.Instance.broadcast(new StartGameMsg());
        }
    }
    private void onMakeMoveServer(Message msg, NetworkConnection connection)
    {
        Server.Instance.broadcast(msg);
    }
    private void onWelcomeClient(Message msg)
    {
        onlineGame = true;
        WelcomeMsg welcome = msg as WelcomeMsg;
        currentPlayer = welcome.player;
        Debug.Log($"My assigned team is {welcome.player}");
        // Client.Instance.sendToServer(welcome);
    }

    private void onStartGameClient(Message msg)
    {
        if (currentPlayer == 1)
        {
            startAsBlack = true;
        }
        startGame();
    }
    private void onMakeMoveClient(Message msg)
    {
        MakeMoveMsg move = msg as MakeMoveMsg;
        //Debug.Log($"{move.team}, {move.originalX}, {move.originalY} --> {move.goalX}, {move.goalY} ");
        if (move.team != currentPlayer)
        {
            Piece piece = positions[move.originalX, move.originalY];

            if (piece == null)
            {
                return;
            }
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

    public int getPlayerTeam()
    {
        return currentPlayer;
    }
    public bool getOnlineGame()
    {
        return onlineGame;
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
}
