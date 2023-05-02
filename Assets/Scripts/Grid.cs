using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using TMPro;

public class Grid : MonoBehaviour
{
    // Multiplayer logic
    private int currentPlayer = -1;
    private int numPlayers = -1;
    private string computerPlayer = null;
    private const int NUM_PIECES = 8;
    //where on the board should be pieces be placed
    private int FIRST_PIECE_BY = 7;
    [SerializeField] private int width, height;
    [SerializeField] private ReplayMove replayMove;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private GameObject forestTileDark;
    [SerializeField] private GameObject forestTileLight;
    [SerializeField] private GameObject boardNotation;
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
        menuAnimator.SetTrigger("NoMenu");
        GenerateGrid();
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
                bool isLigtSquare = (x + y) % 2 != 0;
                spawnedTile.setTileColor(isLigtSquare);
            }
        }

        cam.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10); //Move the camera to the middle of the screen
    }
    //Generate letters and numbers which denote the files and ranks (clarity for game repaly)
    private void generateBoardNotation(float x, float y, string text)
    {
        var spawnedMove = Instantiate(boardNotation, new Vector3(x, y, -1), Quaternion.identity);
        spawnedMove.tag = "BoardGraphic";
        TextMeshPro mText = spawnedMove.GetComponent<TextMeshPro>();
        mText.text = text;
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

        //If we are not replaying a game add the move the the moves played list
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
            movePlayed.checkmate = false;
            movesPlayed.Add(movePlayed);
        }

        resetKingTile(); //if the king was in check, reset the color of the king square to the original tile color

        positions[x, y] = piece;

        positions[piece.GetX(), piece.GetY()] = null;
        piece.SetX(x);
        piece.SetY(y);

        //If we are in an online game sned a message with the move to the other player
        if (onlineGame)
        {
            MakeMoveMsg move = new MakeMoveMsg();
            move.originalX = lastmove.originalPos.x;
            move.originalY = lastmove.originalPos.y;
            move.goalX = x;
            move.goalY = y;
            if (piece.GetPlayer() == "white")
            {
                move.player = 0;
            }
            else
            {
                move.player = 1;
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

    //Function to generate possible pseudo-legal moves of a specific piece
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

    //Pseudo-legal moves for pawns
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
    //Create an indicator at a square on the board, unless occupied by a friendly piece
    private void createIndicator(int x, int y, Piece piece)
    {
        bool isOnBoard = onBoard(x, y);
        //error check
        if (!isOnBoard)
        {
            return;
        }

        Piece pieceAtPos = positions[x, y];
        bool isEmpty = pieceAtPos == null;

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

    //Create move indicators on the board based on the legal moves
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

        Tile kingTile = null;

        if (isInCheck(playerToPlay))
        {
            // if the king is in check the last move has put in check, mark that move for correct notation
            Move2 checkMove = movesPlayed[movesPlayed.Count - 1];
            checkMove.check = true;
            movesPlayed[movesPlayed.Count - 1] = checkMove;

            //Set the king tile to red if the king is in check
            Vector2Int kingPos = findKing(playerToplay);
            kingTile = GameObject.Find("Tile " + kingPos.x + " " + kingPos.y).GetComponent<Tile>();
            kingTile.tileRed();
        }
        //if the king is not in check, check if he can castle
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


        for (int j = 0; j < myMoves.Count; j++)
        {
            bool enW = false;
            bool enB = false;

            //if the move of the piece would take an enemy piece save that piece
            if (positions[myMoves[j].x, myMoves[j].y] != null)
            {
                pieceToTake = positions[myMoves[j].x, myMoves[j].y];
            }
            else
            {
                //en passant special case (capture when moving onto an empty square) check and handle  individually
                if (j == myMoves.Count - 1 && Math.Abs(myMoves[j].x - originalX) == 1)
                {
                    if (playerToPlay == "white" && enPassantWhite == true && isWPawn(piece.name))
                    {
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
            positions[originalX, originalY] = null;

            //After the move has been made generate the moves of the opponent again
            generateAllPseudoLegalMoves(enemyPlayer);

            //After the move has been made check if the king is still in check
            if (!isInCheck(playerToPlay))
            {
                legalMoves.Add(myMoves[j]); // if the king is not in check after the move make the move legal
            }
            //If a piece has been overwritten by the move set the piece back
            if (pieceToTake != null)
            {
                //Handle returning of the board state before the move if the move was en passant
                if (enW)
                {
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
                    if (legalMoves[i] == moveToFind)
                    {
                        moveIndex = i;
                        break;
                    }
                }
                //If the move is not found, remove the castling move from legal moves
                if (moveIndex == -1)
                {
                    legalMoves.RemoveAt(legalMoves.Count - 1);
                }
            }
        }
        else if ((piece.name == "wKing" || piece.name == "bKing") && ((lastMoveX == 6 && (lastMoveY == 0 || (lastMoveY == 7))) || (lastMoveX == 2 && (lastMoveY == 0 || lastMoveY == 7))) && (castleLong || castleShort))
        {
            legalMoves.RemoveAt(legalMoves.Count - 1);
        }

        //Set the possible moves to the legal moves
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
        //mark the last move as checkmate for correct notation
        Move2 move = movesPlayed[movesPlayed.Count - 1];
        move.checkmate = true;
        movesPlayed[movesPlayed.Count - 1] = move;
        return true;
    }
    private int moveNumber = 0;
    //convert the moves played to chess notation and generate replayMove Gameobjects 
    public void generatePlayedMoves(int index)
    {
        int moveIndex = 1;
        Move2 move;
        for (int i = index; i < movesPlayed.Count; i++)
        {
            string text = $"{(moveNumber + 2) / 2} ";

            moveIndex++;
            string notation = convertNotation(movesPlayed[i]);
            //if the move was a castling move, remove the rook move (as castling is technically two moves, we remove one so it effectivelly counts as one)
            if (notation == "O-O" || notation == "O-O-O")
            {
                movesPlayed.RemoveAt(i + 1);
            }
            //Add a number to every even move to denote the turn
            if (moveNumber % 2 != 0)
            {
                text = notation;
            }
            else
            {
                text += notation;
            }
            //Instantantiate the move
            replayMove.generateReplayMove(text, moveNumber++);
            i++;
            //error check
            if (i >= movesPlayed.Count)
            {
                break;
            }
            notation = convertNotation(movesPlayed[i]);
            //if the move was a castling move, remove the rook move (as castling is technically two moves, we remove one so it effectivelly counts as one)
            if (notation == "O-O" || notation == "O-O-O")
            {
                movesPlayed.RemoveAt(i + 1);
            }
            //Instantantiate the move
            replayMove.generateReplayMove(notation, moveNumber++);
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
            //Check all castling condition for queenside castling
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
            //Check all castling condition for kingside castling
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
    }
    // ==== Getters, setters and helper methods ====
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

    //======================================

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
    public void resetNumPlayers()
    {
        numPlayers = -1;
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
        if (move.player != currentPlayer)
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
            if (move.player == 0)
            {
                setPlayerToPlay("black");
            }
            else if (move.player == 1)
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
        for (int x = 0; x < width; x++)
        {
            generateBoardNotation(x, -0.7f, convertToFile(x));
            generateBoardNotation(-0.7f, x, $"{x + 1}");
        }
        replayingGame = true;
        generatePlayedMoves(0);

    }
    public void clearMovesPlayed()
    {
        movesPlayed.Clear();
    }
    public void replayNumMoves(int index)
    {
        replayingGame = true;
        replayMoveIndex = index;
        resetKingTile();
        DestroyIndicators();
        destroyPieces();
        fromFenToBoard(Fens[replayMoveIndex]);
        if (replayMoveIndex > 0)
        {
            resetMove(highlightedMove);
        }
        highlightMove(replayMoveIndex - 1);

    }
    public void replayNextMove()
    {

        if (replayMoveIndex < Fens.Count - 1)
        {
            replayingGame = true;
            resetKingTile();
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
    }
    public void setLastMove()
    {
        if (replayMoveIndex != 0)
        {
            lastmove.piece = positions[movesPlayed[replayMoveIndex - 1].goalPos.x, movesPlayed[replayMoveIndex - 1].goalPos.y];
            lastmove.originalPos = new Vector2Int(movesPlayed[replayMoveIndex - 1].originalPos.x, movesPlayed[replayMoveIndex - 1].originalPos.y);
            lastmove.goalPos = new Vector2Int(movesPlayed[replayMoveIndex - 1].goalPos.x, movesPlayed[replayMoveIndex - 1].goalPos.y);
        }

    }
    //Sets the board to the previous move when repaying a game
    public void replayPrevMove()
    {
        if (replayMoveIndex > 0)
        {
            replayingGame = true;
            resetKingTile();
            DestroyIndicators();
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
    //Destroys all the created assets(board, pieces, move indicators, move replay moves) and resets internal variables and clears lists
    public void destroyAssets()
    {
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
        moveNumber = 0;
        computerPlayer = "";

        DestroyIndicators();
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
        //Destroys the letters and numbers denoting files and ranks in game replay
        GameObject[] boardGraphics = GameObject.FindGameObjectsWithTag("BoardGraphic");
        for (int i = 0; i < boardGraphics.Length; i++)
        {
            Destroy(boardGraphics[i]);
        }
    }
    //Converts a move to FIDE chess notation
    private string convertNotation(Move2 move)
    {
        string notation = null;
        switch (move.pieceName)
        {
            case "wPawn":
            case "bPawn":
                break;
            case "wRook":
            case "bRook":
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
                //Check for castling when the king moves two squares
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
        notation += $"{move.originalPos.y + 1}"; // add one as notation starts at 1 but indexing starts at 0

        if (move.capture)
        {
            notation += "x";
        }

        notation += convertToFile(move.goalPos.x);
        notation += $"{move.goalPos.y + 1}"; // add one as notation starts at 1 but indexing starts at 0
        //Check for pawn promotion
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
    //Number to letter conversion
    private static string convertToFile(int file)
    {
        return ((char)('a' + file)).ToString();
    }

    //Converts the board into a FEN string
    public string convertToFen()
    {
        string fenNotation = "";
        for (int i = height - 1; i >= 0; i--)
        {
            int emptySquares = 0;
            for (int j = 0; j < width; j++)
            {
                //write the number of empty squares in a row
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
                    //Write down the pieces according to FEN
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
            //Denote next rank
            fenNotation += "/";
        }
        //Denote player to play
        if (playerToplay == "white")
        {
            fenNotation += " b";
        }
        else
        {
            fenNotation += " w";
        }

        //Castling rights for white
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
        //Castling rights for black
        castleLong = false;
        castleShort = false;

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

    //Set the board position from a FEN string
    public void fromFenToBoard(string FEN)
    {
        //Split the board part from the rest
        string[] fenParts = FEN.Split(' ');
        //Split by ranks
        string[] fenRows = fenParts[0].Split('/');

        playerToplay = fenParts[1] == "w" ? "white" : "black";
        //Set castling rights
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
                //Skip digits and add that number to the column value
                if (char.IsDigit(fenChar))
                {
                    col += (int)char.GetNumericValue(fenChar);
                }
                else
                {
                    //Create the piece name from the FEN represantation
                    string player = char.IsUpper(fenChar) ? "w" : "b";
                    string type = getPieceType(char.ToLower(fenChar));
                    string piece = player + type;
                    //Create the piece
                    Piece newPiece = CreatePiece(piece, col, row);
                    //if pawns arent on their original squares set them to hasMoved (for legal move generation)
                    if (piece == "wPawn" && row != 1)
                    {
                        newPiece.setHasMoved(true);
                    }
                    else if (piece == "bPawn" && row != height - 2)
                    {
                        newPiece.setHasMoved(true);
                    }
                    //Check if kings have made a move previously and set to hasMoved accordingly (for castling rights)
                    else if (piece == "bKing")
                    {
                        newPiece.setHasMoved(hasKingMoved("bKing"));
                    }
                    else if (piece == "wKing")
                    {
                        newPiece.setHasMoved(hasKingMoved("wKing"));
                    }
                    positions[col, row] = newPiece;
                    col++;
                }
            }
        }
    }
    //Check if any of the previous moves were made by a psecific king
    private bool hasKingMoved(string king)
    {
        for (int i = 0; i < movesPlayed.Count; i++)
        {
            if (i < replayMoveIndex)
            {
                if (movesPlayed[i].pieceName == king)
                {
                    return true;
                }
            }
        }
        return false;
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
    //Destroy all Piece gameobjects
    public void destroyPieces()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        //Destroy all pieces except the general piece that has been initialized as a [Serailized field], always index 0 so start at index 1 to not destroy it
        for (int i = 1; i < pieces.Length; i++)
        {
            Destroy(pieces[i]);
        }
    }
    //Underline a move in game replay
    public void highlightMove(int index)
    {
        ReplayMove move = GameObject.Find($"Move {index}")?.GetComponent<ReplayMove>();
        highlightedMove = index;
        move?.underlineText();
    }
    //Reset the text style of a move in game replay
    public void resetMove(int index)
    {
        ReplayMove move = GameObject.Find($"Move {index}")?.GetComponent<ReplayMove>();
        move?.resetStyle();
    }
    //Destroy the moves in game replay starting from a certain index
    public void destroyMoves(int index)
    {
        for (int i = index; i <= movesPlayed.Count + 1; i++)
        {
            GameObject move = GameObject.Find($"Move {i}");
            Destroy(move);
        }
        //remove the moves from the list as well
        for (int i = index; i < movesPlayed.Count; i++)
        {
            movesPlayed.RemoveAt(i);
        }
        //remove the positions from the FEN string list
        for (int i = Fens.Count - 1; i > index; i--)
        {
            Fens.RemoveAt(i);
        }
        moveNumber = index;
    }

    //=======AI===============

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
    //Generate all legal moves for a player and save them to a list
    public void generateAllLegalMoves(string player)
    {
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
                    legalMoves(a);

                    for (int j = 0; j < moves.Count; j++)
                    {
                        Move move;
                        move.piece = a;
                        move.goalPos = new Vector2Int(moves[j].x, moves[j].y);
                        move.originalPos = new Vector2Int(a.GetX(), a.GetY());
                        allLegalMoves.Add(move);
                    }
                    moves.Clear();
                }
            }
        }
    }
    //Counts the material value of the board
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
    private Move BESTMOVE = new Move();

    //Minimax algorithm with alpha-beta prunning which finds a best move for a player based on an evelation function
    public int Minimax(int depth, bool isMaximizingPlayer, int alpha, int beta)
    {
        //If we reach depth 0 check if the player would be checkmated, otherwise return the material value of the board
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
            //Set the maximum evaluation to the lowest possible value
            int maxEval = -checkmateValue;
            playerToplay = "white";
            //Generate and order all legal moves
            generateAllLegalMoves(playerToplay);
            List<Move> legalMoves = new List<Move>(orderMoves(playerToplay));
            foreach (Move move in legalMoves)
            {
                bool pieceMoved = positions[move.originalPos.x, move.originalPos.y].getHasMoved();
                //Programatically make the move and save the taken piece if there was one
                Piece pieceToTake = makeMove(move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);
                //Recursive call of minimax with decreased depth 
                int eval = Minimax(depth - 1, false, alpha, beta);
                //Unmake the move to set the board back to its position before the move
                unMakeMove(pieceToTake, pieceMoved, move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);
                if (eval > maxEval)
                {
                    maxEval = eval;
                    //if we reached the top level of the tree and our evaluation is better than the current maximum evaluation, save the best move
                    if (depth == treeDepth)
                    {
                        BESTMOVE = move;
                    }
                }
                //update alpha and prune the tree if possible
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
            //Set the minimum evaluation to the highest possible value
            int minEval = checkmateValue;
            playerToplay = "black";

            //Generate and order all legal moves
            generateAllLegalMoves(playerToplay);
            List<Move> legalMoves = new List<Move>(orderMoves(playerToplay));

            foreach (Move move in legalMoves)
            {
                bool pieceMoved = positions[move.originalPos.x, move.originalPos.y].getHasMoved();
                //Programatically make the move and save the taken piece if there was one
                Piece pieceToTake = makeMove(move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);
                //Recursive call of minimax with decreased depth 
                int eval = Minimax(depth - 1, true, alpha, beta);
                //Unmake the move to set the board back to its position before the move
                unMakeMove(pieceToTake, pieceMoved, move.originalPos.x, move.originalPos.y, move.goalPos.x, move.goalPos.y);

                if (eval < minEval)
                {
                    minEval = eval;
                    //if we reached the top level of the tree and our evaluation is less than the current minumum evaluation, save the best move
                    if (depth == treeDepth)
                    {
                        BESTMOVE = move;
                    }
                }
                if (depth == treeDepth && BESTMOVE.piece == null)
                {
                    BESTMOVE = move;
                }
                //update beta and prune the tree if possible
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
        resetKingTile();

        if (positions[goalX, goalY] != null)
        {
            pieceToTake = positions[goalX, goalY];
        }
        //Check for en passant
        else if (playerToPlay == "white" && Math.Abs(originalX - goalX) == 1 && isWPawn(piece.name) && positions[goalX, goalY] == null)
        {
            pieceToTake = positions[goalX, goalY - 1];
            positions[goalX, goalY - 1] = null;
        }
        else if (playerToPlay == "black" && Math.Abs(originalX - goalX) == 1 && isBPawn(piece.name) && positions[goalX, goalY] == null)
        {
            pieceToTake = positions[goalX, goalY + 1];
            positions[goalX, goalY + 1] = null;
        }
        // Set the last move played to the move that is being made
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
        //if the king was in check, reset the color of the king square to the original tile color
        resetKingTile();

        Piece piece = positions[currentX, currentY];
        //check if a piece was taken in the move, return it
        if (takenPiece != null)
        {
            positions[currentX, currentY] = null;
            positions[takenPiece.GetX(), takenPiece.GetY()] = takenPiece;
        }
        //set the place the piece moved to to null
        else
        {
            positions[currentX, currentY] = null;
        }
        //Set the piece back to its original square
        positions[originalX, originalY] = piece;
        piece.setHasMoved(originalPieceMoved);
        piece.SetX(originalX);
        piece.SetY(originalY);

        castleLong = false;
        castleShort = false;
        enPassantBlack = false;
        enPassantWhite = false;

    }
    //Find the tile the king of the player to play and reset the tile to its original colour
    private void resetKingTile()
    {
        Vector2Int kingPos = findKing(playerToplay);
        Tile kingTile = GameObject.Find("Tile " + kingPos.x + " " + kingPos.y).GetComponent<Tile>();
        kingTile.resetColor();
    }
    //Make the move that the AI found as the best
    public void playBestMove(Move move)
    {
        Piece pieceAtPos = positions[move.goalPos.x, move.goalPos.y];
        SetPosition(move.piece, move.goalPos.x, move.goalPos.y);
        if (pieceAtPos != null)
        {
            Destroy(pieceAtPos.gameObject);
        }
        //move the piece on the visible board
        move.piece.setPieceToPos(new Vector3(move.goalPos.x, move.goalPos.y, -1));
        move.piece.setHasMoved(true);
        //Add the FEN string to the list to be able to replay a game
        addFEN();
        playerToplay = "white";
    }

    //Call minimax, play the move and return the move made
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
    //Return the number of all legal moves
    public int numLegalMoves(string player)
    {
        moves.Clear();
        allLegalMoves.Clear();
        generateAllLegalMoves(player);
        return allLegalMoves.Count;
    }
}


