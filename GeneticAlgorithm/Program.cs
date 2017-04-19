using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rnd = new Random();
            Stopwatch sw = new Stopwatch();
            int generation = 1;

            Console.WriteLine("Wipe old chromosomes? (y/n)");
            if (Console.ReadLine() == "y")
            {
                DeleteChromosomes();
            }
            Console.WriteLine("Please enter the amount of chromosomes to generate:");
            int chromosomesToGen = Int32.Parse(Console.ReadLine());

            sw.Start();
            for (int i = 0; i < chromosomesToGen; i++)
            {
                Console.Title = "Chromosome: " + (i + 1);
                string chromosome = "";
                string gene = "";
                bool b = true;
                for (int j = 0; j < rnd.Next(10, 41); j++)
                {
                    gene = ConvertToBinary((b) ? (rnd.Next(9)) : (rnd.Next(9, 13)));
                    while (gene.Length != 4)
                    {
                        gene = "0" + gene;
                    }
                    b = !b;
                    chromosome += gene;
                }
                SaveChromosome(chromosome);
            }

            int chromosomes = File.ReadAllLines(@"C:\users\Seth Dolin\Desktop\NeuralNetworks\Chromosomes.txt").Length - 1;

            sw.Stop();
            float timeTaken = (float) + sw.ElapsedMilliseconds / 1000;
            Console.WriteLine("Finished generating " + chromosomesToGen + " chromosomes in " + timeTaken + " seconds");


            Console.WriteLine("Please enter the number that you would like to have an expression for: ");
            int numToApproximate = Int32.Parse(Console.ReadLine());

            Console.WriteLine("Please enter the number of generations that you would like to evolve: ");
            int generationsToEvolve = Int32.Parse(Console.ReadLine());
            Console.WriteLine("Now evolving " + generationsToEvolve + " generations");
            sw.Restart();
            EvolveGenerations(numToApproximate, generationsToEvolve);
            timeTaken = (float)+sw.ElapsedMilliseconds / 1000;

            Console.WriteLine("Finished evolving " + generationsToEvolve + " generations of " + chromosomes + " chromosomes in " + timeTaken + " seconds");

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }

        private static double GetFitnessSum()
        {
            string fileAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\FitnessScores.txt";
            var lines = File.ReadAllLines(fileAddress);
            double sum = 0.0;
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i] != "")
                {
                    sum += Double.Parse(lines[i]);
                }
            }
            return sum;
        }

        private static int WeightedSelector(double fitnessSum)
        {
            int selection = 1;
            Random rnd = new Random();
            double random;
            string fileAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\FitnessScores.txt";
            var lines = File.ReadAllLines(fileAddress);
            double selectionStart = 0.0;
            double selectionEnd = 0.0;
            selectionEnd = Double.Parse(lines[1]);
            for (int i = 1; i < lines.Length; i++)
            {
                random = rnd.NextDouble() * fitnessSum;
                if (lines[i] != "")
                {
                    if (random >= selectionStart && random < selectionEnd)
                    {
                        selection = i;
                        break;
                    }

                    selectionEnd += Double.Parse(lines[i]);
                    selectionStart += Double.Parse(lines[i]);
                }
            }

            return selection;
        }

        private static void Breed()
        {
            string fileAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\Chromosomes.txt";
            var lines = File.ReadAllLines(fileAddress);
            Random rnd = new Random();
            for (int i = 1; i < lines.Length; i++)
            {
                string result = "";
                string chromosome1 = lines[WeightedSelector(GetFitnessSum())];
                string chromosome2 = lines[WeightedSelector(GetFitnessSum())];
                bool c1IsLonger = (chromosome1.Length > chromosome2.Length);

                int crossoverPoint = rnd.Next(((c1IsLonger) ? (chromosome2.Length) : (chromosome1.Length)));
                result = (c1IsLonger ? (chromosome2.Substring(0, crossoverPoint)) : (chromosome1.Substring(0, crossoverPoint))) + (c1IsLonger ? (chromosome1.Substring(crossoverPoint)) : (chromosome2.Substring(crossoverPoint)));
                for (int j = 0; j < result.Length; j++)
                {
                    if (rnd.Next(100) == 1)
                    {
                        result = result.Substring(0, j) + (Int32.Parse(result.Substring(j, 1)) * -1 + 1) + result.Substring(j + 1);
                    }
                }
                lines[i] = result;
            }

            File.WriteAllLines(fileAddress, lines);
        }

        private static void EvolveGenerations(int numToApproximate, int generationsToEvolve)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int chromosomes = File.ReadAllLines(@"C:\users\Seth Dolin\Desktop\NeuralNetworks\Chromosomes.txt").Length - 1;
            float timeTaken;

            for (int i = 1; i < generationsToEvolve + 1; i++)
            {
                Console.Title = "Generation: " + i;
                Console.WriteLine("Now parsing all chromosomes.");
                sw.Restart();
                ParseChromosomes(i);
                timeTaken = (float)+sw.ElapsedMilliseconds / 1000;
                Console.WriteLine("Finished parsing " + chromosomes + " chromosomes in " + timeTaken + " seconds");

                Console.WriteLine("Now evaluating all chromosomes");
                sw.Restart();
                EvaluateChromosomes(i);
                timeTaken = (float)+sw.ElapsedMilliseconds / 1000;
                Console.WriteLine("Finished evaluating " + chromosomes + " chromosomes in " + timeTaken + " seconds");

                Console.WriteLine("Now evaluating fitness for all chromosomes");
                sw.Restart();
                EvaluateFitness(numToApproximate, i);
                timeTaken = (float)+sw.ElapsedMilliseconds / 1000;
                Console.WriteLine("Finished evaluating fitness for " + chromosomes + " chromosomes in " + timeTaken + " seconds");

                Console.WriteLine("Now breeding all chromosomes");
                sw.Restart();
                Breed();
                timeTaken = (float)+sw.ElapsedMilliseconds / 1000;
                Console.WriteLine("Finished breeding " + chromosomes + " chromosomes in " + timeTaken + " seconds");

            }
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
            Console.Clear();
        }

        private static void EvaluateFitness(int numToApproximate, int generation)
        {
            string sourceAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\EvaluatedChromosomes.txt";
            string destAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\FitnessScores.txt";
            var lines = File.ReadAllLines(sourceAddress);
            string[] newLines = new string[lines.Length];
            newLines[0] = "gen: " + generation.ToString();
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i] != "" && Double.Parse(lines[i]) != numToApproximate)
                {
                    newLines[i] = (1.0 / (Double.Parse(lines[i]) - numToApproximate)).ToString();
                }
                else if (lines[i] != "")
                {
                    newLines[i] = "0";
                }
            }

            File.WriteAllLines(destAddress, newLines);
        }

        private static void ParseChromosomes(int generation)
        {
            string sourceAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\Chromosomes.txt";
            string destAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\ParsedChromosomes.txt";
            var lines = File.ReadAllLines(sourceAddress);
            string[] newLines = new string[lines.Length];
            newLines[0] = lines[0];
            for (int i = 1; i < lines.Length; i++)
            {
                string s = FixChromosome(ParseChromosome(lines[i]));
                newLines[i] = s;
            }
            newLines[0] = "gen: " + generation.ToString();
            File.WriteAllLines(destAddress, newLines);
        }

        private static void EvaluateChromosomes(int generation)
        {
            string sourceAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\ParsedChromosomes.txt";
            string destAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\EvaluatedChromosomes.txt";
            var lines = File.ReadAllLines(sourceAddress);
            int length = lines.Length;
            string[] newLines = new string[length + 1];
            double result = 0.0;
            for (int i = 1; i < length; i++)
            {
                result = EvaluateMathString(lines[i]);

                newLines[i] = result.ToString();
            }
            newLines[0] = "gen: " + generation.ToString();
            File.WriteAllLines(destAddress, newLines);
        }

        private static double EvaluateMathString(string s)
        {
            double result = 0.0;
            if (s == "")
            {
                string fileAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\EvaluatedChromosomes.txt";
                var lines = File.ReadAllLines(fileAddress);
                result = Double.Parse(lines[1]);
                return result;
            }
            result = Double.Parse(s.Substring(0, 1));
            for (int i = 1; i < (s.Length - 1); i+=2)
            {
                if (s.Substring(i, 1) == "+")
                {
                    result += Double.Parse(s.Substring(i + 1, 1));
                }
                else if (s.Substring(i, 1) == "-")
                {
                    result -= Double.Parse(s.Substring(i + 1, 1));
                }
                else if (s.Substring(i, 1) == "*")
                {
                    result *= Double.Parse(s.Substring(i + 1, 1));
                }
                else if (s.Substring(i, 1) == "/")
                {
                    result /= Double.Parse(s.Substring(i + 1, 1));
                }
            }
            return result;
        }

        private static string ConvertToBinary(int num)
        {
            return Convert.ToString(num, 2);
        }
        
        private static void SaveChromosome(string chromosome)
        {
            string fileAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\Chromosomes.txt";
            var lines = File.ReadAllLines(fileAddress);
            string[] newLines = new string[lines.Length + 1];
            newLines[lines.Length] = chromosome;
            for (int i = 0; i < lines.Length; i++)
            {
                newLines[i] = lines[i];
            }
            File.WriteAllLines(fileAddress, newLines);
            Console.WriteLine("Chromosome saved with pattern " + chromosome);
        }

        private static void DeleteChromosomes()
        {
            string fileAddress = @"C:\users\Seth Dolin\Desktop\NeuralNetworks\Chromosomes.txt";
            string[] lines = { "" };
            File.WriteAllLines(fileAddress, lines);
            Console.WriteLine("All chromosomes deleted");
        }

        private static string ParseChromosome(string chromosome)
        {
            string parsed = "";
            for (int i = 0; i < chromosome.Length; i+=4)
            {
                string gene = chromosome.Substring(i, 4);
                if (gene == "0000")
                {
                    parsed += "1";
                }
                else if (gene == "0001")
                {
                    parsed += "2";
                }
                else if (gene == "0010")
                {
                    parsed += "3";
                }
                else if (gene == "0011")
                {
                    parsed += "4";
                }
                else if (gene == "0100")
                {
                    parsed += "5";
                }
                else if (gene == "0101")
                {
                    parsed += "6";
                }
                else if (gene == "0110")
                {
                    parsed += "7";
                }
                else if (gene == "0111")
                {
                    parsed += "8";
                }
                else if (gene == "1000")
                {
                    parsed += "9";
                }
                else if (gene == "1001")
                {
                    parsed += "+";
                }
                else if (gene == "1010")
                {
                    parsed += "-";
                }
                else if (gene == "1011")
                {
                    parsed += "*";
                }
                else if (gene == "1100")
                {
                    parsed += "/";
                }
                //default handles 1101, 1110, and 1111
                else
                {
                }
            }
            return parsed;
        }
        
        private static string FixChromosome(string parsedChromosome)
        {
            string result = "";
            bool b = true;
            for (int i = 0; i < (parsedChromosome.Length / 4); i++)
            {
                if ((b) ? (isNum(parsedChromosome.Substring(i, 1))) : (isOperator(parsedChromosome.Substring(i, 1))))
                {
                    result += parsedChromosome.Substring(i, 1);
                    b = !b;
                }
            }
            return result;
        }

        private static bool isNum(string s)
        {
            if (s == "1" || s == "2" || s == "3" || s == "4" || s == "5" || s == "6" || s == "7" || s == "8" || s == "9")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool isOperator(string s)
        {
            if (s == "+" || s == "-" || s == "*" || s == "/")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
