using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static P4G_Encount_Music_Editor.EnemyEnums;

namespace P4G_Encount_Music_Editor
{
    class CollectionCreationOption : EncounterMenuOptions
    {
        public override string Name => "Collection Creation";

        protected override void RunEncount()
        {
            Encounter[] encounters = GetEncountersList();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Collection Creation");
            Console.ResetColor();

            Console.WriteLine("Enter search term \"inaba\" to exit and save matches to file...");

            Dictionary<ushort, string> encounterMatches = new Dictionary<ushort, string>();

            while (true)
            {
                string searchString = ConsolePrompt.PromptString("Search Term").ToLower();
                if (searchString.Equals("inaba"))
                    break;

                string[] searchTerms = searchString.Split(' ');
                bool searchByOccurence = false;

                if (searchTerms.Length == 1)
                {
                    Console.WriteLine("Match encounters that contain only ONE instance (y) or any amount (n) of Search Term?");
                    searchByOccurence = ConsolePrompt.PromptYN("(y/n)");
                }

                int totalMatches = 0;
                for (int i = 0, total = encounters.Length; i < total; i++)
                {
                    Encounter currentEncounter = encounters[i];

                    bool foundMatch = false;

                    if (searchTerms.Length == 1)
                    {
                        if (searchByOccurence)
                        {
                            if (ContainsUnitTerm(currentEncounter.Units, searchTerms[0], 1))
                            {
                                Console.WriteLine("Found match!");
                                foundMatch = true;
                            }
                        }
                        else
                        {
                            if (ContainsUnitTerm(currentEncounter.Units, searchTerms[0]))
                            {
                                Console.WriteLine("Found match!");
                                foundMatch = true;
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Searching for multiple terms...");
                        foundMatch = true;
                        // if multiple terms, check for each term in current encounter units
                        foreach (var term in searchTerms)
                        {
                            int count = searchTerms.Count(s => s.Equals(term));
                            // if encounter does not contain a term match not made
                            if (!ContainsUnitTerm(currentEncounter.Units, term, count))
                            {
                                foundMatch = false;
                                break;
                            }
                        }
                    }

                    // add to encounters list if match was found
                    if (foundMatch)
                    {
                        totalMatches++;
                        ushort matchKey = (ushort)i;
                        if (!encounterMatches.ContainsKey(matchKey))
                        {
                            Console.WriteLine($"EncounterID: {i} - Added match to collection list!");
                            StringBuilder enemiesList = new StringBuilder();
                            enemiesList.Append("//");
                            foreach (ushort enemyId in currentEncounter.Units)
                                enemiesList.Append($"{GetEnemyName(enemyId)}, ");
                            enemiesList.Append('\n');

                            encounterMatches.Add((ushort)i, enemiesList.ToString());
                        }
                    }
                }

                Console.WriteLine($"Total Matches: {totalMatches}");
            }

            string currentDir = Directory.GetCurrentDirectory();
            string collectionName = ConsolePrompt.PromptString("Collection Name (Lowercase)").ToLower();
            string collectionFilePath = $@"{currentDir}\collections\{collectionName}.enc";
            bool addToFile = false;

            if (File.Exists(collectionFilePath))
            {
                Console.WriteLine("Collection exists! Append to collection (y) or overwrite (n)?");
                addToFile = ConsolePrompt.PromptYN("(y/n)");
            }

            // write or overwrite collection file
            if (!File.Exists(collectionFilePath) || !addToFile)
            {
                StringBuilder collectionText = new StringBuilder();

                foreach (var match in encounterMatches)
                {
                    collectionText.AppendLine(match.Key.ToString());
                    collectionText.AppendLine(match.Value);
                }

                try
                {
                    File.WriteAllText(collectionFilePath, collectionText.ToString());
                    Console.WriteLine($"Collectione created! File: {collectionFilePath}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem writing collection to file!");
                }
            }
            else
            {
                // add to existing collection
                try
                {
                    string[] originalCollectionLines = File.ReadAllLines(collectionFilePath);
                    StringBuilder newCollectionLines = new StringBuilder();

                    // list of existing ids
                    List<ushort> existingIds = new List<ushort>();

                    // parse collection for existing ids
                    foreach (string line in originalCollectionLines)
                    {
                        newCollectionLines.AppendLine(line);
                        if (line.StartsWith("/") || line.Length < 1)
                            continue;
                        existingIds.Add(ushort.Parse(line));
                    }

                    foreach (var match in encounterMatches)
                    {
                        if (!existingIds.Contains(match.Key))
                        {
                            newCollectionLines.AppendLine(match.Key.ToString());
                            newCollectionLines.AppendLine($"{match.Value}");
                            Console.WriteLine($"EncounterID: {match.Key} added!");
                        }
                    }

                    File.WriteAllText(collectionFilePath, newCollectionLines.ToString());
                    Console.WriteLine($"Collectione edited! File: {collectionFilePath}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem adding to collection!");
                }
            }
        }

        private bool ContainsUnitTerm(ushort[] units, string term)
        {
            foreach (ushort unit in units)
            {
                string unitName = GetEnemyName(unit).ToLower();
                if (unitName.Contains(term))
                    return true;
            }

            return false;
        }

        private bool ContainsUnitTerm(ushort[] units, string term, int count)
        {
            int matchCount = 0;
            foreach (ushort unit in units)
            {
                string unitName = GetEnemyName(unit).ToLower();
                if (unitName.Contains(term))
                    matchCount++;
            }

            if (matchCount == count)
                return true;
            else
                return false;
        }

        private string GetEnemyName(ushort enemyId)
        {
            if (_currentGame.Name == GameTitle.P4G)
                return ((P4_EnemiesID)enemyId).ToString();
            else if (_currentGame.Name == GameTitle.P5)
                return ((P5_EnemiesID)enemyId).ToString();
            else
                return null;
        }
    }
}
