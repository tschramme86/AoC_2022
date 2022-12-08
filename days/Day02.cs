using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day02
    {
        enum RPC
        {
            NotYetDecided = 0,
            Rock = 1,
            Paper = 2,
            Scissors = 3
        }
        enum GameResult
        {
            Loss = 0,
            Draw = 3,
            Win = 6
        }

        public static void Solve()
        {
            Console.WriteLine("*** 2nd December ***");

            var lines = File.ReadAllLines("data\\d2.txt");
            var totalScoreWrongPlaying = 0;
            var totalScoreCorrectPlaying = 0;

            foreach (var line in lines)
            {
                var gameActions = line.Split(' ');
                var opponentAction = gameActions[0] switch { 
                    "A" => RPC.Rock,
                    "B" => RPC.Paper,
                    "C" => RPC.Scissors
                };

                // wrong playing, story part one
                {
                    var myAction = gameActions[1] switch
                    {
                        "X" => RPC.Rock,
                        "Y" => RPC.Paper,
                        "Z" => RPC.Scissors
                    };

                    var gameResult = CalcResult(opponentAction, myAction);

                    var score = (int)myAction + (int)gameResult;
                    totalScoreWrongPlaying += score;
                }

                // Correct playing, story part two
                {
                    var expectedResult = gameActions[1] switch
                    {
                        "X" => GameResult.Loss,
                        "Y" => GameResult.Draw,
                        "Z" => GameResult.Win
                    };

                    var myAction = RPC.NotYetDecided;
                    switch(expectedResult)
                    {
                        case GameResult.Loss:
                            myAction = opponentAction switch
                            {
                                RPC.Scissors => RPC.Paper,
                                RPC.Paper => RPC.Rock,
                                RPC.Rock => RPC.Scissors,
                            };
                            break;

                        case GameResult.Win:
                            myAction = opponentAction switch
                            {
                                RPC.Scissors => RPC.Rock,
                                RPC.Paper => RPC.Scissors,
                                RPC.Rock => RPC.Paper
                            };
                            break;

                        case GameResult.Draw:
                            myAction = opponentAction;
                            break;
                    }

                    Debug.Assert(myAction != RPC.NotYetDecided);
                    Debug.Assert(expectedResult == CalcResult(opponentAction, myAction));

                    var score = (int)myAction + (int)expectedResult;
                    totalScoreCorrectPlaying += score;
                }
            }

            Console.WriteLine($"Total rounds: {lines.Length}");
            Console.WriteLine($"Total score (wrong playing, story part one): {totalScoreWrongPlaying}");
            Console.WriteLine($"Total score (correct playing, story part two): {totalScoreCorrectPlaying}");
        }

        static GameResult CalcResult(RPC opponentAction, RPC myAction)
        {
            var gameResult = GameResult.Loss;
            if (opponentAction == myAction)
            {
                gameResult = GameResult.Draw;
            }
            else if (
                (myAction == RPC.Rock && opponentAction == RPC.Scissors) ||
                (myAction == RPC.Scissors && opponentAction == RPC.Paper) ||
                (myAction == RPC.Paper && opponentAction == RPC.Rock))
            {
                gameResult = GameResult.Win;
            }

            return gameResult;
        }
    }
}
