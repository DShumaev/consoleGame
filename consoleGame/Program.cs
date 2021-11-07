using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;
using ConsoleTables;


namespace consoleGame
{

    class KeyGenerator
    {
        public static byte[] GenerateKey(int byteSize)
        {
            byte[] secretKey = new Byte[byteSize];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(secretKey);
                return secretKey;
            }
        }
    }

    class HmacGenerator
    {
        private static byte[] ComputeHMAC(byte[] key, byte[] message)
        {
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                return hmac.ComputeHash(message);
            }
        }

        private static byte[] StringEncode(string text)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            return encoding.GetBytes(text);
        }

        public static byte[] GenerateHmac(byte[] key, string message)
        {
            byte[] messageInByte = StringEncode(message);
            byte[] hmacInBytes = ComputeHMAC(key, messageInByte);
            return hmacInBytes;
        }
    }

    class TableGameGenerator
    {
        public static void CreateTable(string[] moves)
        {
            string[][] rules = new string[moves.Length + 1][];

            for (int i = 0; i <= moves.Length; i++)
            {
                rules[i] = new string[moves.Length + 1];
            }
    
            rules[0][0] = "pc \\ user";

            for (int i = 1; i <= moves.Length; i++)
            {
                rules[0][i] = moves[i - 1];
                rules[i][0] = moves[i - 1];
            
                for (int j = 1; j <= moves.Length; j++)
                {
                    rules[i][j] = DetermineWinner.GetWinner(moves[i-1], moves[j-1], moves);
                }
            }

            var table = new ConsoleTable(rules[0]);

            for (int i = 1; i <= moves.Length; i++)
            {
                table.AddRow(rules[i]);
            }

            table.Write();
        } 
    }

    class DetermineWinner
    {
        public static string GetWinner(string ComputerMove, string UserMove, string[] moves)
        {
            int indexUserMove = Array.IndexOf(moves, UserMove);
            int indexComputerMove = Array.IndexOf(moves, ComputerMove);

            if (indexUserMove == indexComputerMove)
            {
                return "draw";
            }

            int countMoves = moves.Length;
            double diff = Math.Floor(countMoves / 2.0);

            if ((indexUserMove + diff) <= (countMoves - 1))
            {
                if (indexComputerMove > indexUserMove && indexComputerMove <= indexUserMove + diff)
                {
                    return "computer";
                }
                else
                {
                    return "user";
                }
            }
            else if ((indexUserMove - diff) >= 0)
            {
                if (indexComputerMove < indexUserMove && indexComputerMove >= indexUserMove - diff)
                {
                    return "user";
                }
                else
                {
                    return "computer";
                }
            }
            return "draw";
        }
    }


    public class Game
    {   
        private string[] moves;
        
        public Game(string[] args)
        {
            moves = args;
        }

        private string BytesToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        private bool IsUserInputCorrect()
        {            
            if (moves.Length < 3 || (moves.Length % 2 == 0))
            {
                Console.WriteLine("Number of entered moves is incorrect. The number of moves must be odd and more than 1");
                Console.WriteLine("For example: 1 2 3 or one two three four five or rock paper scissors");
                Console.WriteLine("Try again!");
                return false;
            }
            HashSet<string> uniqMoves = new HashSet<string>(moves);
            if (uniqMoves.Count != moves.Length)
            {
                Console.WriteLine("Moves cannot be repeated!");
                Console.WriteLine("Try again!");
                return false;
            }
            return true;
        }

        private void PrintMenu()
        {
            Console.WriteLine("Available moves:");
            for (int i = 0; i < moves.Length; i++)
            {
                Console.WriteLine("{0} - {1}", i + 1, moves[i]);
            }
            Console.WriteLine("0 - exit");
            Console.WriteLine("? - help");
        }

        private string ReadUserСhoice()
        {
            try
            {
                Console.Write("Enter your move: ");
                string choice = Console.ReadLine();

                if (choice == "0")
                {
                    CloseProgram();
                }

                if (choice == "?")
                {
                    TableGameGenerator.CreateTable(moves);
                    CloseProgram();
                }

                return moves[int.Parse(choice) - 1];
            }
            catch 
            {
                Console.WriteLine("You entered incorrect values. Try again!");
                return "error";
            } 
        }

        private int generateComputerMove(int min, int max)
        {
            int rand = BitConverter.ToInt32(KeyGenerator.GenerateKey(4), 0); 
            const Decimal OldRange = (Decimal)int.MaxValue - (Decimal)int.MinValue;
            Decimal NewRange = max - min + 1;
            Decimal NewValue = ((Decimal)rand - (Decimal)int.MinValue) / OldRange * NewRange + (Decimal)min;
            return (int)NewValue;
        }

        private void CloseProgram()
        {
                Console.WriteLine("End of program (through 5 seconds)");
                Thread.Sleep(5000);
                Environment.Exit(1);
        }

        public void Start()
        {
            if (!IsUserInputCorrect())
            {
                CloseProgram();
            }
            
            string computerMove = moves[generateComputerMove(1, moves.Length) - 1];
            string userMove = "";
            byte[] secretKey = KeyGenerator.GenerateKey(16);
            byte[] moveHmac = HmacGenerator.GenerateHmac(secretKey, computerMove);
            Console.WriteLine("HMAC: {0}", BytesToString(moveHmac));
            PrintMenu();

            while (true)
            {
                userMove = ReadUserСhoice();
                if (userMove != "error")
                {  
                    break;
                }
            }

            Console.WriteLine("Your move: {0}", userMove);
            Console.WriteLine("Computer move: {0}", computerMove);
            string winner = DetermineWinner.GetWinner(computerMove, userMove, moves);
            
            if (winner == "user")
            {
                Console.WriteLine("You win!");
            }
            else if (winner == "computer")
            {
                 Console.WriteLine("You lose!");
            }
            else
            {
                Console.WriteLine("Draw!");
            }

            Console.WriteLine("HMAC key: {0}", BytesToString(secretKey));
            CloseProgram();
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            new Game(args).Start();
        }
    }
}
