using Raylib_cs;
using System.Numerics;

namespace TicTacToe
{
    /// <summary>
    /// Player vs computer, using minimax algorithm.
    /// Currently, this assumes that the human player is the max-value player.
    /// </summary>
    internal class GameBoard
    {
        #region Setup
        public const string NAME = "Tic-Tac-Toe by Pauline";
        public const int WIDTH = 500;
        public const int HEIGHT = 500;
        public Raylib_cs.Font ttfFont;

        public void Run()
        {
            Raylib.InitWindow(WIDTH, HEIGHT, NAME);
            // NOTE: Textures/Fonts MUST be loaded after Window initialization (OpenGL context is required)

            Load();

            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                Update(dt);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.WHITE);

                Draw();

                Raylib.EndDrawing();
            }

            Unload();

            Raylib.CloseWindow();
        }
        #endregion

        public Rectangle[] boardPositions = new Rectangle[9];
        public int[] currentBoardPositions;
        public Raylib_cs.Color halfGray = new Raylib_cs.Color(100, 100, 100, 50);
        public Rectangle? currentPosition = null;
        PlayerState currentPlayer = PlayerState.DRAW;
        PlayerState[] players = [PlayerState.PLAYER_FOR_MIN, PlayerState.PLAYER_FOR_MAX];
        char[] playerForMaxBoard;
        char[] playerForMinBoard;
        Dictionary<PlayerState, string> endGameMessages = new Dictionary<PlayerState, string>();
        const string ELLIPSIS = "...";

        // Timer
        public float timer = 0;
        public float timerMax = 1;
        float computerMoveDelaySeconds = 0;
        float computerMoveDelaySecondsMax = 5;

        public int colorBoxIndex;

        // game loop logic
        public bool hasGameStarted = false;
        public bool hasFinishedGuessing = false;
        public bool haveColorsBeenShown = false;
        public bool hasGameEnded = false;
        PlayerState winner = PlayerState.DRAW;

        public GameBoard()
        {
            boardPositions = [
                new Rectangle(100, 100, 99, 99),
                new Rectangle(201, 100, 99, 99),
                new Rectangle(301, 100, 99, 99),
                new Rectangle(100, 201, 99, 99),
                new Rectangle(201, 201, 99, 99),
                new Rectangle(301, 201, 99, 99),
                new Rectangle(100, 301, 99, 99),
                new Rectangle(201, 301, 99, 99),
                new Rectangle(301, 301, 99, 99)
            ];
            currentBoardPositions = new int[boardPositions.Length];
            playerForMaxBoard = new char[9];
            playerForMinBoard = new char[9];
            Array.Fill(playerForMaxBoard, '0');
            Array.Fill(playerForMinBoard, '0');
            endGameMessages.Add(PlayerState.DRAW, "It's a draw!");
            endGameMessages.Add(PlayerState.PLAYER_FOR_MAX, "You win!");
            endGameMessages.Add(PlayerState.PLAYER_FOR_MIN, "Computer wins!");
        }
        
        /// <summary>
        /// Called once during setup
        /// </summary>
        public void Load()
        {
            // Check https://github.com/ChrisDill/Raylib-cs/tree/master/Examples/resources/fonts for all raylib distributable fonts
            ttfFont = Raylib.LoadFontEx("resources/pixantiqua.ttf", 20, null, 250);
            // randomizer
            Random randomer = new Random();
            randomer.Shuffle(players);
            currentPlayer = players[0];
            hasGameStarted = true;
        }

        /// <summary>
        /// Keeps update timing consistent
        /// </summary>
        /// <param name="_deltaTime"></param>
        public void Update(float _deltaTime)
        {
            if (hasGameStarted && !hasGameEnded)
            {
                timer += _deltaTime;
                // human
                if (currentPlayer == PlayerState.PLAYER_FOR_MAX)
                {
                    Vector2 getMouseXY = Raylib.GetMousePosition();

                    currentPosition = null;
                    for (int r = 0; r < boardPositions.Length; r++)
                    {
                        Rectangle rect = boardPositions[r];
                        if (Raylib.CheckCollisionPointRec(getMouseXY, rect) && currentBoardPositions[r] == 0)
                        {
                            currentPosition = rect;
                            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                            {
                                currentBoardPositions[r] = (int)currentPlayer;
                                playerForMaxBoard[r] = '1';
                                (hasGameEnded, winner) = GameLogic
                                    .HasGameEnded(currentBoardPositions, new string(playerForMaxBoard), new string(playerForMinBoard));
                                // switch players
                                currentPlayer = players.Where(p => p != currentPlayer).First();
                                foreach (var item in currentBoardPositions)
                                {
                                    Console.Write("{0},", item);
                                }
                                Console.WriteLine("");
                            }
                        }
                    }
                }
                // computer
                else if (currentPlayer == PlayerState.PLAYER_FOR_MIN)
                {
                    (int nextMove, int moveValue) = GameLogic.MiniMax(currentBoardPositions,
                        new string(playerForMaxBoard),
                        new string(playerForMinBoard),
                        currentPlayer);
                    if (nextMove > -1)
                    {
                        computerMoveDelaySeconds += _deltaTime;
                        if (computerMoveDelaySeconds > computerMoveDelaySecondsMax)
                        {
                            Raylib.SetMousePosition((int)boardPositions[nextMove].X, (int)boardPositions[nextMove].Y);
                            currentBoardPositions[nextMove] = (int)currentPlayer;
                            playerForMinBoard[nextMove] = '1';
                            (hasGameEnded, winner) = GameLogic
                                .HasGameEnded(currentBoardPositions, new string(playerForMaxBoard), new string(playerForMinBoard));
                            // switch players
                            currentPlayer = players.Where(p => p != currentPlayer).First();
                            computerMoveDelaySeconds = 0;
                            foreach (var item in currentBoardPositions)
                            {
                                Console.Write("{0},", item);
                            }
                            Console.WriteLine("");
                        }
                    }
                }
                if (timer > timerMax)
                {
                    // reset
                    timer = 0;
                }
            }
        }

        public void Draw()
        {
            for (int v = 200; v < 400; v = v + 100)
            {
                Raylib.DrawLine(v, 100, v, 400, Raylib_cs.Color.BLACK);
            }
            for (int h = 200; h < 400; h = h + 100)
            {
                Raylib.DrawLine(100, h, 400, h, Raylib_cs.Color.BLACK);
            }
            Raylib.DrawTextEx(ttfFont, "Play TicTacToe with your computer!", new Vector2(10.0f, 25.0f), ttfFont.BaseSize, 2, Color.BLACK);
            Raylib.DrawTextEx(ttfFont, "Player uses X. Computer uses O.", new Vector2(10.0f, 50.0f), ttfFont.BaseSize, 2, Color.BLACK);

            for (int u = 0; u < currentBoardPositions.Length; u++) 
            {
                if (currentBoardPositions[u] == (int)PlayerState.PLAYER_FOR_MAX) 
                {
                    Rectangle boardPos = boardPositions[u];
                    Raylib.DrawText("X", (int) (boardPos.X + (boardPos.Width / 3)), (int)(boardPos.Y + (boardPos.Height / 3)), 48, Color.RED);
                }
                if (currentBoardPositions[u] == (int)PlayerState.PLAYER_FOR_MIN)
                {
                    Rectangle boardPos = boardPositions[u];
                    Raylib.DrawText("O", (int)(boardPos.X + (boardPos.Width / 3)), (int)(boardPos.Y + (boardPos.Height / 3)), 48, Color.GREEN);
                }
            }

            if (!hasGameEnded)
            {
                if (currentPosition != null)
                {
                    Raylib.DrawRectangleRec((Rectangle)currentPosition, halfGray);
                }
                if (currentPlayer == PlayerState.PLAYER_FOR_MAX)
                {
                    Raylib.DrawTextEx(ttfFont, "Your turn", new Vector2(10.0f, 450.0f), ttfFont.BaseSize, 2, Color.BLUE);
                }
                if (currentPlayer == PlayerState.PLAYER_FOR_MIN)
                {
                    Raylib.DrawTextEx(ttfFont, "Computer's turn" + ((int)computerMoveDelaySeconds % 2 == 0 ? ELLIPSIS : ""), new Vector2(10.0f, 450.0f), ttfFont.BaseSize, 2, Color.BLUE);
                }
            }
            else
            {
                Raylib.DrawTextEx(ttfFont, endGameMessages[winner], new Vector2(10.0f, 450), ttfFont.BaseSize, 2, Color.BLUE);
                Raylib.DrawTextEx(ttfFont, "Game Over! Thanks for playing.", new Vector2(10.0f, 475.0f), ttfFont.BaseSize, 2, Color.BLACK);
            }
        }

        public void Unload()
        {
            Raylib.UnloadFont(ttfFont);
        }
    }
}
