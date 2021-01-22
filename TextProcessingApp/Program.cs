using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepMorphy;

namespace TextProcessingApp
{
    class Program
    {
        static readonly MorphAnalyzer Morph = new MorphAnalyzer(true);

        private const string FilePath = "file.txt";
        static void Main(string[] args)
        {
            List<(string word, int reps)> GetRepetitions(string plainText, string[] exceptGrams = null)
            {
                var rawWords = plainText.ToLower().Split(new[] { '!', '/', '?', '.', ',', ':', ';', '-', '=', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                
                var infinitives = Morph.Parse(rawWords).Select(t => t.BestTag).ToList();
                var filteredLemmas = infinitives.Where(t => exceptGrams == null || !exceptGrams.Intersect(t.Grams).Any()).Select(t => t.Lemma).ToList();

                var result = filteredLemmas.GroupBy(t => t).Select(t => (word: t.Key, reps: t.Count())).OrderByDescending(tuple => tuple.Item2).ToList();
                return result;
            }

            void Write((string, int) tuple) =>
                Console.WriteLine(tuple.Item2 > 1
                    ? $"\t\t\"{tuple.Item1}\" повторяется {tuple.Item2} раз."
                    : $"\t\t\"{tuple.Item1}\" не повторяется");

            List<(string, int)> totalTop = new List<(string, int)>();

            using (var reader = new StreamReader(FilePath))
            {
                int lineCnt = 0;
                while (!reader.EndOfStream)
                {
                    //line = абзац
                    var line = reader.ReadLine();

                    lineCnt++;
                    Console.WriteLine($"Абзац {lineCnt}");

                    //sentence = предложение
                    if (line != null)
                    {
                        int sentCnt = 0;
                        foreach (var sentence in line.ToLower().Split('.').Where(t => t.Trim().Any()))
                        {
                            sentCnt++;
                            Console.WriteLine($"\tПредложение {sentCnt}");

                            var firstFive = GetRepetitions(sentence,new []{ "предл", "союз" }).Take(5).ToList();
                            totalTop.AddRange(firstFive);
                            firstFive.ForEach(Write);
                            Console.WriteLine();
                        }
                    }

                }
            }

            Console.WriteLine("\nИтого:");
            totalTop.GroupBy(t => t.Item1).Select(t => (t.Key, t.Sum(tuple => tuple.Item2))).OrderByDescending(tuple => tuple.Item2).Take(5).ToList().ForEach(Write);

            Console.ReadKey();
        }
    }
}
