using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WordleSolver
{
    public class Guess
    {
        public string Word { get; set; }
        public int[] State { get; set; } = new int[] { 0, 0, 0, 0, 0 }; 
        public bool IsPossibleGuess(string guess)
        {
            for (int i = 0; i < 5; i++) 
            {
                if (
                    //Exclude words that are missing known letters
                    (State[i] == 2 && Word[i] != guess[i])

                    //Exclude words that are missing present letters
                 || (State[i] == 1 && !guess.Contains(Word[i]))

                    //Exclude words that have present letters in the wrong location
                 || (State[i] == 1 && Word[i] == guess[i])

                    //Exclude words that have known incorrect letters EXCLUDING existing correct letters (to handle words with multiples of the same letter)
                 || (State[i] == 0 && guess.Where((f, idx) => State[idx] != 2 || f != Word[idx]).Contains(Word[i]))
                   )
                    return false;
            }
            return true;
        }
    }

    class Program
    {

        static string[] words;
        static Dictionary<char, int>[] letterFrequency;
        static void Main(string[] args)
        {
            words = System.IO.File.ReadAllLines("FiveLetterWords.txt");
            ComputeLetterFrequency();
            var guesses = new Guess[6];

            //First guess excludes duplicate letters to maximize rate of guessing a letter
            try
            {
                int[] lastState;
                guesses[0] = PickWord(0, guesses, f => f.Distinct().Count() == f.Count());
                lastState = guesses[0].State = Guess(guesses[0].Word);
                guesses[1] = PickWord(1, guesses);
                lastState = guesses[1].State = Guess(guesses[1].Word);
                guesses[2] = PickWord(2, guesses);
                lastState = guesses[2].State = Guess(guesses[2].Word);
                guesses[3] = PickWord(3, guesses);
                lastState = guesses[3].State = Guess(guesses[3].Word);
                guesses[4] = PickWord(4, guesses);
                lastState = guesses[4].State = Guess(guesses[4].Word);
                guesses[5] = PickWord(5, guesses);
                lastState = guesses[5].State = Guess(guesses[5].Word);
            }
            catch (LolThisIsADumbWayToTriggerAWinException)
            {
                Console.WriteLine("Yay we win");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("OH NOOOOOOOOO");
            Console.ReadLine();

        }

        static Guess PickWord(int round, Guess[] guesses, Func<string, bool> heuristic = null)
        {
            heuristic = heuristic ?? (f => true);
            var excludeNull = guesses.Take(round).Where(f => f != null);
            var previousGuesses = excludeNull.Select(f => f.Word);

            var lastGuess = excludeNull.LastOrDefault();
            int[] unknownLetters = new int[5] { 1, 1, 1, 1, 1 };
            if (lastGuess != null)
            {
                unknownLetters = lastGuess.State.Select((f, idx) => f == 2 ? 0 : 1).ToArray();
            }

            var word = words
                .Except(previousGuesses)
                .Where(heuristic)
                .Where(f => excludeNull.All(v => v.IsPossibleGuess(f)))
                .MaxBy(f =>
                    (unknownLetters[0] == 1 ? letterFrequency[0][f[0]] : 0) +
                    (unknownLetters[1] == 1 ? letterFrequency[1][f[1]] : 0) +
                    (unknownLetters[2] == 1 ? letterFrequency[2][f[2]] : 0) +
                    (unknownLetters[3] == 1 ? letterFrequency[3][f[3]] : 0) +
                    (unknownLetters[4] == 1 ? letterFrequency[4][f[4]] : 0))
                .FirstOrDefault();
            return new Guess()
            {
                Word = word
            };
        }

        static int[] Guess(string guess)
        {
            Console.WriteLine("Enter guess: " + guess + ", then enter the results.");
            var result = Console.ReadLine().Trim();
            while (result.Any(f => f != '0' && f != '1' && f != '2') || result.Length != 5)
            {
                Console.WriteLine("Invalid results.  Enter 5 numbers, 0 = miss, 1 = wrong spot, 2 = correct.");
                result = Console.ReadLine().Trim();
            }
            if (result == "22222")
                throw new LolThisIsADumbWayToTriggerAWinException();
            return result.Select(f => f == '0' ? 0 : f == '1' ? 1 : 2).ToArray();
        }

        static void ComputeLetterFrequency()
        {
            letterFrequency = new Dictionary<char, int>[5] { MakeCharDict(), MakeCharDict(), MakeCharDict(), MakeCharDict(), MakeCharDict() };
            foreach (var word in words)
            {
                for (int i = 0; i < 5; i++)
                {
                    letterFrequency[i][word[i]]++;
                }
            }

        }

        static Dictionary<char, int> MakeCharDict() => Enumerable.Range(97, 26).Select(f => (char)f).ToDictionary(f => f, _ => 0);

        public class LolThisIsADumbWayToTriggerAWinException : Exception { }

    }
}
