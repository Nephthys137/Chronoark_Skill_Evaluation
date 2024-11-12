global using System;
global using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Updater_Evaluation
{
    static class Program
    {
        public const string NAME = "Updater_Evaluation";
        public const string VERSION = "1.0.0";
        public const string AUTHOR = "一些微小的代码工作：Bright查卡器&数据库作者：熙熙";
        [STAThread]
        static void Main(string[] args)
        {
            Application.Run(new WinForm());
            bool flag = true;
            string startupPath = Application.StartupPath;
            string User_Data_Path = startupPath + "/User_Data";
            if (args == null || args.Length < 1 || !"UpdaterUserData".Equals(args[0]) || !File.Exists(User_Data_Path + "/User_Data.json"))
            {
                flag = false;
            }
            JsonSerializer jsonSerializer = new();

            DataSet dataSet = MySql.KeyValue("versions");
            Version_Data_List versionData_List = new()
            {
                RECORDS = []
            };
            DataTable dataTable = dataSet.Tables[0];
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                DataRow dataRow = dataTable.Rows[i];
                Version_Data versionData = new()
                {
                    版本 = dataRow[0].ToString(),
                    时间 = dataRow[1].ToString()
                };
                versionData_List.RECORDS.Add(versionData);
            }
            using (StreamWriter streamWriter = new(startupPath + "/Version_Data.json"))
            {
                jsonSerializer.Serialize(streamWriter, versionData_List);
            }
            Dictionary<string, Skill_Evaluation_Data> Skill_Data_Dic = null;
            Dictionary<string, Skill_Evaluation_Data> Historical_Data_Dic = null;
            Skill_Evaluation_Data_List Temp_Evaluation_List = new()
            {
                RECORDS = []
            };
            if (flag)
            {
                using var streamReader = File.OpenText(User_Data_Path + "/User_Data.json");
                Skill_Data_Dic = jsonSerializer.Deserialize<Dictionary<string, Skill_Evaluation_Data>>(new JsonTextReader(streamReader));
                using var streamReader1 = File.OpenText(User_Data_Path + "/Historical_Data.json");
                Historical_Data_Dic = jsonSerializer.Deserialize<Dictionary<string, Skill_Evaluation_Data>>(new JsonTextReader(streamReader1));
            }
            dataSet = MySql.KeyValue("skill_evaluation");
            dataTable = dataSet.Tables[0];
            Skill_Evaluation_Data_List Evaluation_Data_List = new()
            {
                RECORDS = []
            };
            for (int j = 0; j < dataTable.Rows.Count; j++)
            {
                DataRow dataRow = dataTable.Rows[j];
                Skill_Evaluation_Data skill_evaluation;
                skill_evaluation = new()
                {
                    技能名 = dataRow[0].ToString(),
                    评价等级 = dataRow[1].ToString(),
                    评价 = dataRow[2].ToString(),
                    序号 = dataRow[3].ToString(),
                    出现次数 = double.Parse(dataRow[4].ToString()),
                    获得次数 = double.Parse(dataRow[5].ToString()),
                    删除次数 = double.Parse(dataRow[6].ToString()),
                    尝试次数 = double.Parse(dataRow[7].ToString()),
                    通关次数 = double.Parse(dataRow[8].ToString())
                };
                Evaluation_Data_List.RECORDS.Add(skill_evaluation);
                if (flag)
                {
                    if (!Skill_Data_Dic.TryGetValue(skill_evaluation.技能名, out var User_Data))
                        Skill_Data_Dic[skill_evaluation.技能名] = User_Data = new();
                    if (!Historical_Data_Dic.TryGetValue(skill_evaluation.技能名, out var Historical_Data))
                        Historical_Data_Dic[skill_evaluation.技能名] = Historical_Data = new();
#if DEBUG
                    if (skill_evaluation.获得次数 > 200000)
                    {
                        skill_evaluation.GrabProb = 100 * (skill_evaluation.获得次数 / skill_evaluation.出现次数);
                        skill_evaluation.DelProb = 100 * (skill_evaluation.删除次数 / skill_evaluation.获得次数);
                        skill_evaluation.WinProb = 100 * (skill_evaluation.通关次数 / skill_evaluation.尝试次数);
                        Temp_Evaluation_List.RECORDS.Add(skill_evaluation);
                    }

#else
                    if (User_Data.出现次数 > 10)
                    {
                        skill_evaluation.出现次数 += User_Data.出现次数;
                        skill_evaluation.获得次数 += User_Data.获得次数;
                        skill_evaluation.删除次数 += User_Data.删除次数;
                        skill_evaluation.尝试次数 += User_Data.尝试次数;
                        skill_evaluation.通关次数 += User_Data.通关次数;

                        Historical_Data.出现次数 += User_Data.出现次数;
                        Historical_Data.获得次数 += User_Data.获得次数;
                        Historical_Data.删除次数 += User_Data.删除次数;
                        Historical_Data.尝试次数 += User_Data.尝试次数;
                        Historical_Data.通关次数 += User_Data.通关次数;

                        Temp_Evaluation_List.RECORDS.Add(User_Data);
                    }
#endif
                }
            }
#if DEBUG
            throw new Exception("如果你在DEBUG模式,请在此设置断点");
#endif
            using (StreamWriter streamWriter = new(startupPath + "/Skill_Evaluation.json"))
            {
                jsonSerializer.Serialize(streamWriter, Evaluation_Data_List);
            }
            if (flag)
            {
                MySql.UpdateDatabase(Temp_Evaluation_List, versionData_List);
                using (StreamWriter streamWriter = new(User_Data_Path + "/User_Data.json"))
                {
                    jsonSerializer.Serialize(streamWriter, Skill_Data_Dic);
                }
                using (StreamWriter streamWriter = new(User_Data_Path + "/Historical_Data.json"))
                {
                    jsonSerializer.Serialize(streamWriter, Historical_Data_Dic);
                }
            }
            dataSet = MySql.KeyValue("item_evaluation");
            dataTable = dataSet.Tables[0];
            Item_Evaluation_Data_List Item_Data_List = new()
            {
                RECORDS = []
            };
            for (int k = 0; k < dataTable.Rows.Count; k++)
            {
                DataRow dataRow = dataTable.Rows[k];
                Item_Evaluation_Data item_evaluation = new()
                {
                    道具名 = dataRow[0].ToString(),
                    评分 = dataRow[1].ToString(),
                    评价 = dataRow[2].ToString()
                };
                Item_Data_List.RECORDS.Add(item_evaluation);
            }
            using StreamWriter streamWriter1 = new(startupPath + "/Item_Evaluation.json");
            jsonSerializer.Serialize(streamWriter1, Item_Data_List);
            File.WriteAllText(startupPath + "/LatestUpdaterTime.txt", DateTime.Now.ToString("yyyy/%M/%d %H:%m:%s"));
        }
        public static Dictionary<TKey, TElement> ToDictionaryEX<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            Dictionary<TKey, TElement> dictionary = [];
            foreach (TSource item in source)
                dictionary[keySelector(item)] = elementSelector(item);
            return dictionary;
        }
    }
}
