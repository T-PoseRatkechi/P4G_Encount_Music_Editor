using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static P4G_Encount_Music_Editor.EnemyEnums;

namespace P4G_Encount_Music_Editor
{
    class OutputListOption : EncounterMenuOptions
    {
        public override string Name => "Output Encounters List";

        protected override void RunEncount()
        {
            OutputEncounterList();
        }

        private void OutputEncounterList()
        {
            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                string listFilePath = $@"{currentDir}\{_currentGame.Name}_Encounters List.txt";

                Encounter[] encounters = GetEncountersList();

                StringBuilder listText = new StringBuilder();

                for (int i = 0, total = encounters.Length; i < total; i++)
                {
                    listText.AppendLine($"Encounter Index: {i} Song Index: {encounters[i].MusicId}");
                    foreach (ushort enemyId in encounters[i].Units)
                    {
                        switch (_currentGame.Name)
                        {
                            case GameTitle.P4G:
                                listText.AppendLine(((P4_EnemiesID)enemyId).ToString());
                                break;
                            case GameTitle.P5:
                                listText.AppendLine(((P5_EnemiesID)enemyId).ToString());
                                break;
                            default:
                                break;
                        }
                    }
                }

                File.WriteAllText(listFilePath, listText.ToString());
                Console.WriteLine($"Encounters List Created: {listFilePath}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem writing encounter list!");
            }
        }
    }
}
