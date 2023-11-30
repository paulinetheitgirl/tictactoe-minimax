namespace TicTacToe
{
    enum PlayerState
    {
        PLAYER_FOR_MAX = 1,
        DRAW = 0,
        PLAYER_FOR_MIN = -1
    }

    internal class GameLogic
    {
        /// <summary>
        /// Integral representations of possible win patterns in TictacToe.
        /// Ex., {111000000} means the first row of the board,
        /// {000111000} means the second row of the board,
        /// and so on.
        /// </summary>
        static readonly int[] winPatterns = [
            Convert.ToInt32("111000000", 2), // row 1
            Convert.ToInt32("000111000", 2), // row 2
            Convert.ToInt32("000000111", 2), // row 3
            Convert.ToInt32("100100100", 2), // col 1
            Convert.ToInt32("010010010", 2), // col 2
            Convert.ToInt32("001001001", 2), // col 3
            Convert.ToInt32("100010001", 2), // diagonal left to right
            Convert.ToInt32("001010100", 2) // diagonal right to left
        ];

        internal static PlayerState CheckWinner(string playerForMaxBoard, string playerForMinBoard)
        {
            int playerForMaxBoardInt = Convert.ToInt32(playerForMaxBoard, 2);
            int playerForMinBoardInt = Convert.ToInt32(playerForMinBoard, 2);
            foreach (int winPattern in winPatterns)
            {
                /**
                 * Do a bitwise And first because there can be other positions on the board
                 * which don't form a win pattern
                 */
                if ((winPattern & playerForMaxBoardInt) == winPattern)
                {
                    return PlayerState.PLAYER_FOR_MAX;
                }
                if ((winPattern & playerForMinBoardInt) == winPattern)
                {
                    return PlayerState.PLAYER_FOR_MIN;
                }
            }

            return PlayerState.DRAW;
        }
        internal static (bool, PlayerState) HasGameEnded(int[] currentBoardPositions, string playerForMaxBoard, string playerForMinBoard)
        {
            int moveCount = currentBoardPositions.Length - currentBoardPositions.Where(u => u == 0).Count();
            if (moveCount < 5) // optimization: not possible to win in less than 5 moves
            { 
                return (false, PlayerState.DRAW);
            }
            PlayerState winner = CheckWinner(playerForMaxBoard, playerForMinBoard);
            if (((PlayerState[])[PlayerState.PLAYER_FOR_MAX, PlayerState.PLAYER_FOR_MIN]).Contains(winner))
            {
                return (true, winner);
            }
            if (moveCount < currentBoardPositions.Length)
            {
                return (false, PlayerState.DRAW);
            }
            return (true, PlayerState.DRAW);
        }

        static (int[], string, string) applyMoveToBoard(int[] boardPositionsForMove,
            int moveIndex,
            string playerForMaxBoard,
            string playerForMinBoard,
            PlayerState player)
        {
            int[] boardCopy = new int[boardPositionsForMove.Length];
            Array.Copy(boardPositionsForMove, boardCopy, boardPositionsForMove.Length);
            boardCopy[moveIndex] = (int)player;
            string playerBoardToUpdate = player == PlayerState.PLAYER_FOR_MAX ? playerForMaxBoard : playerForMinBoard;
            char[] tempPlayerBoard = playerBoardToUpdate.ToCharArray();
            tempPlayerBoard[moveIndex] = '1';

            return (boardCopy,
                player == PlayerState.PLAYER_FOR_MAX ? new string(tempPlayerBoard) : playerForMaxBoard,
                player == PlayerState.PLAYER_FOR_MIN ? new string(tempPlayerBoard) : playerForMinBoard);
        }
        internal static (int, int) MiniMax(int[] currentBoardPositions,
            string playerForMaxBoard,
            string playerForMinBoard,
            PlayerState currentPlayer)
        {
            /**
             * Optimization: The best first move is the center.
             * https://medium.com/analytics-vidhya/artificial-intelligence-at-play-connect-four-minimax-algorithm-explained-3b5fc32e4a4f
             */
            int usedBoardPositions = currentBoardPositions.Where(u => u != 0).Count();
            if (currentBoardPositions[4] != 1 && usedBoardPositions < 2)
            {
                return (4, (int)PlayerState.DRAW);
            }
            /**
             * Optimization: For the second move, any board position works
             */
            if (usedBoardPositions == 1)
            {
                Random randomer = new Random();
                int[] boardPosIndexes = currentBoardPositions.Select((v, i) => new { Index = i, Value = v })
                    .Where(u => u.Value == 0)
                    .Select(u => u.Index)
                    .ToArray();
                randomer.Shuffle(boardPosIndexes);
                int randomBoardPosition = boardPosIndexes[0];
                return (randomBoardPosition, (int)currentPlayer);
            }

            (bool gameEnded, PlayerState winner) = HasGameEnded(currentBoardPositions, playerForMaxBoard, playerForMinBoard);
            if (gameEnded)
            {
                return (-1, (int)winner);
            }
            int nextMoveIndex = -1;
            // logic for player whose goal is the max-value state
            if (currentPlayer == PlayerState.PLAYER_FOR_MAX)
            { 
                int maxGoalValue = int.MinValue;
                nextMoveIndex = -1;
                int[] emptyMovePosIndexes = currentBoardPositions.Select((v, i) => new { Index = i, Value = v })
                    .Where(u => u.Value == 0)
                    .Select(u => u.Index)
                    .ToArray();
                foreach (int index in emptyMovePosIndexes)
                {
                    (int[] boardWithNextMove, string updatedPlayerForMaxBoard, string updatedPlayerForMinBoard) = applyMoveToBoard(currentBoardPositions,
                        index,
                        playerForMaxBoard,
                        playerForMinBoard,
                        currentPlayer);
                    (int nextMove, int moveValue) = MiniMax(boardWithNextMove,
                        updatedPlayerForMaxBoard,
                        updatedPlayerForMinBoard,
                        PlayerState.PLAYER_FOR_MIN);
                    if (moveValue > maxGoalValue)
                    {
                        nextMoveIndex = index;
                        maxGoalValue = moveValue;
                    }
                }
                return (nextMoveIndex, maxGoalValue);
            }
            // logic for player whose goal is the min-value state
            if (currentPlayer == PlayerState.PLAYER_FOR_MIN)
            {
                int minGoalValue = int.MaxValue;
                nextMoveIndex = -1;
                int[] emptyMovePosIndexes = currentBoardPositions.Select((v, i) => new { Index = i, Value = v })
                    .Where(u => u.Value == 0)
                    .Select(u => u.Index)
                    .ToArray();
                foreach (int index in emptyMovePosIndexes) {
                    (int[] boardWithNextMove, string updatedPlayerForMaxBoard, string updatedPlayerForMinBoard) = applyMoveToBoard(currentBoardPositions,
                        index,
                        playerForMaxBoard,
                        playerForMinBoard,
                        currentPlayer);
                    (int nextMove, int moveValue) = MiniMax(boardWithNextMove,
                        updatedPlayerForMaxBoard,
                        updatedPlayerForMinBoard,
                        PlayerState.PLAYER_FOR_MAX);
                    if (moveValue < minGoalValue)
                    {
                        nextMoveIndex = index;
                        minGoalValue = moveValue;
                    }
                }
                return (nextMoveIndex, minGoalValue);
            }

            return (-1, 0);
        }
    }
}
