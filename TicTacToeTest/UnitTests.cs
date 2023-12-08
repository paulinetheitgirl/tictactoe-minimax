using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TicTacToe
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TakeBoardCenter()
        {
            // Arrange
            int[] boardPositions = new int[9];
            char[] playerForMaxBoard = new char[9];
            char[]  playerForMinBoard = new char[9];
            Array.Fill(playerForMaxBoard, '0');
            Array.Fill(playerForMinBoard, '0');
            Array.Fill(boardPositions, 0);

            // Act
            (int nextMove, int moveValue) = GameLogic.MiniMax(boardPositions, new string(playerForMaxBoard), new string(playerForMinBoard), PlayerState.PLAYER_FOR_MIN);

            Assert.AreEqual(4, nextMove, "Computer's first move did not take center");
        }

        [TestMethod]
        public void MoveToMakeMinPlayerWin()
        {
            // Arrange
            int[] boardPositions = new int[9] { 
                1, 0, -1,
                0, 1, -1,
                0, 0, 0
            };
            char[] playerForMaxBoard = new char[9] {
                '1', '0', '0',
                '0', '1', '0',
                '0', '0', '0'
            };
            char[] playerForMinBoard = new char[9] {
                '0', '0', '1',
                '0', '0', '1',
                '0', '0', '0'
            };

            // Act
            (int nextMove, int moveValue) = GameLogic.MiniMax(boardPositions, new string(playerForMaxBoard), new string(playerForMinBoard), PlayerState.PLAYER_FOR_MIN);

            Assert.AreEqual(8, nextMove, "Computer did not take winning move");
        }
    }
}