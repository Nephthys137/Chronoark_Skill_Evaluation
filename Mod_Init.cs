global using System;
global using System.Collections.Generic;
using ChronoArkMod;
using ChronoArkMod.ModData;
using ChronoArkMod.ModData.Settings;
using ChronoArkMod.Plugin;
using HarmonyLib;
using I2.Loc;
using Newtonsoft.Json;
using Steamworks;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace ChronoArk_Evaluation
{
    [PluginConfig(GUID, NAME, VERSION)]
    public class Mod_Init : ChronoArkPluginMonoBehaviour
    {
        public const string GUID = "Com.Bright.ChronoArk_Evaluation";
        public const string NAME = "ChronoArk_Evaluation";
        public const string VERSION = "1.0.0";
        public const string AUTHOR = "一些微小的代码工作：Bright查卡器&数据库作者：熙熙";

        public void Awake()
        {
            ModInfo modInfo = ModManager.getModInfo(NAME);
            Database_Path = modInfo.DirectoryName + "/Assemblies/Database";
            User_Data_Path = Database_Path + "/User_Data";
            IsChinese = LocalizationManager.CurrentLanguage.StartsWith("Chinese");
            if (IsChinese)
                Evaluation_Bool = modInfo.GetSetting<ToggleSetting>("Evaluation_Bool").Value;
            UpdaterUserData_Bool = modInfo.GetSetting<ToggleSetting>("UpdaterUserData_Bool").Value;
            CheckUpdaterTime = modInfo.GetSetting<SliderSetting>("CheckUpdaterTime").Value;
            harmony.PatchAll(typeof(HOOK));

            Updater(modInfo);
            DirectoryInfo directoryInfo = new(User_Data_Path);
            string SteamID = SteamUser.GetSteamID().ToString();
            if (!File.Exists(User_Data_Path + "/SteamUserID") || !File.ReadAllText(User_Data_Path + "/SteamUserID").Equals(SteamID))
            {
                NewUser();
                if (directoryInfo.Exists)
                    directoryInfo.Delete(true);
                directoryInfo.Create();
                File.WriteAllText(User_Data_Path + "/SteamUserID", SteamID);
            }

            StartCoroutine(InternetAccessCheck((bool flag) =>
            {
                if (!flag)
                {
                    Debug.LogWarning("No Internet");
                    return;
                }
                UpdaterData(modInfo);
            }));
        }
        static void UpdaterData(ModInfo modInfo = null)
        {
            if (DateTime.ParseExact(File.ReadAllText(Database_Path + "/LatestUpdaterTime.txt"), "yyyy/%M/%d %H:%m:%s", CultureInfo.InvariantCulture).AddDays(CheckUpdaterTime) < DateTime.Now)
            {
                Process process = new();
                process.StartInfo.FileName = Database_Path + "/Updater_Evaluation.exe";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (UpdaterUserData_Bool)
                    process.StartInfo.Arguments = "UpdaterUserData";
                process.Start();
                if (modInfo == null)
                    return;
                Task.Run(() =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    while (true)
                    {
                        if (process.HasExited)
                        {
                            break;
                        }
                        else if (stopwatch.ElapsedMilliseconds > 60000)
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                });
                Updater(modInfo);
            }
        }
        public void OnDestroy()
        {
            harmony.UnpatchSelf();
            OnApplicationQuit();
        }
        public void OnApplicationQuit()
        {
            JsonSerializer jsonSerializer = new();
            using var streamWriter = new StreamWriter(User_Data_Path + "/User_Data.json");
            jsonSerializer.Serialize(streamWriter, User_Data_Dic);
            using var streamWriter1 = new StreamWriter(User_Data_Path + "/Historical_Data.json");
            jsonSerializer.Serialize(streamWriter1, Historical_Data_Dic);
            UpdaterData();
        }
        static void Updater(ModInfo modInfo)
        {
            JsonSerializer jsonSerializer = new();
            using var streamReader = File.OpenText(Database_Path + "/Skill_Evaluation.json");
            Evaluation_Data_List = jsonSerializer.Deserialize<Skill_Evaluation_Data_List>(new JsonTextReader(streamReader));
            if (File.Exists(User_Data_Path + "/User_Data.json"))
            {
                using var streamReader2 = File.OpenText(User_Data_Path + "/User_Data.json");
                User_Data_Dic = jsonSerializer.Deserialize<Dictionary<string, Skill_Evaluation_Data>>(new JsonTextReader(streamReader2));
                foreach (var skill_evaluation in Evaluation_Data_List.RECORDS)
                {
                    var User_Data = Core.GetOrCreateEvaluationData(skill_evaluation.技能名);
                    skill_evaluation.出现次数 += User_Data.出现次数;
                    skill_evaluation.获得次数 += User_Data.获得次数;
                    skill_evaluation.删除次数 += User_Data.删除次数;
                    skill_evaluation.尝试次数 += User_Data.尝试次数;
                    skill_evaluation.通关次数 += User_Data.通关次数;
                }
                using var streamReader3 = File.OpenText(User_Data_Path + "/Historical_Data.json");
                Historical_Data_Dic = jsonSerializer.Deserialize<Dictionary<string, Skill_Evaluation_Data>>(new JsonTextReader(streamReader3));
            }
            Skill_Data_Dic = Evaluation_Data_List.RECORDS.ToDictionaryEX((x) => { x.UpdaterData(); return x.技能名; }, (x) => x);
            using var streamReader4 = File.OpenText(Database_Path + "/Item_Evaluation.json");
            Item_Data_List = jsonSerializer.Deserialize<Item_Evaluation_Data_List>(new JsonTextReader(streamReader4));
            Item_Data_Dic = Item_Data_List.RECORDS.ToDictionaryEX((x) => x.道具名, (x) => x);
            using var streamReader5 = File.OpenText(Database_Path + "/Version_Data.json");
            Version_Data_List versionData_List = jsonSerializer.Deserialize<Version_Data_List>(new JsonTextReader(streamReader5));
            var version_Data = versionData_List.RECORDS[0];

            if (IsChinese)
            {
                SetTranslation(modInfo, "ChronoArk_Evaluation/Title", "卡牌评价与胜率统计 版本:" + version_Data.版本);
                SetTranslation(modInfo, "ChronoArk_Evaluation/Description", "卡牌评价与胜率统计\n数据库最后更新于:" + version_Data.时间);
            }
            else
            {
                SetTranslation(modInfo, "ChronoArk_Evaluation/Title", "Card Win Rate Statistics\nVersion:" + version_Data.版本);
                SetTranslation(modInfo, "ChronoArk_Evaluation/Description", "Card Win Rate Statistics\nDataBases Latest Updater Time:" + version_Data.时间);
            }
        }
        static void SetTranslation(ModInfo modInfo, string term, string Translation)
        {
            LanguageSourceData sourceData = modInfo.localizationInfo.MainFile;
            int languageIndex = sourceData.GetLanguageIndex(LocalizationManager.CurrentLanguage, true, false);
            TermData termData = sourceData.GetTermData(term, false);
            termData.Languages[languageIndex] = Translation;
        }
        static void NewUser()
        {
            User_Data_Dic = [];
            Historical_Data_Dic = [];
            foreach (var skill_evaluation in Evaluation_Data_List.RECORDS)
            {
                User_Data_Dic[skill_evaluation.技能名] = new();
                Historical_Data_Dic[skill_evaluation.技能名] = new();
            }
            File.WriteAllText(Database_Path + "/LatestUpdaterTime.txt", DateTime.Now.AddDays(-14).ToString("yyyy/%M/%d %H:%m:%s"));
        }
        static IEnumerator InternetAccessCheck(Action<bool> callback, int timeOut = 4)
        {
            string url = "https://weibo.com/";
            UnityWebRequest request = new(url)
            {
                timeout = timeOut
            };
            yield return request.SendWebRequest();
            if (request.isNetworkError)
            {
                callback(false);
            }
            else
            {
                callback(true);
            }
        }
        private static readonly Harmony harmony = new(GUID);
        private static string Database_Path;
        private static string User_Data_Path;

        public static Item_Evaluation_Data_List Item_Data_List;
        public static Dictionary<string, Item_Evaluation_Data> Item_Data_Dic;

        public static Skill_Evaluation_Data_List Evaluation_Data_List;
        public static Dictionary<string, Skill_Evaluation_Data> Skill_Data_Dic;
        public static Dictionary<string, Skill_Evaluation_Data> User_Data_Dic;
        public static Dictionary<string, Skill_Evaluation_Data> Historical_Data_Dic;

        public static bool Evaluation_Bool;
        public static bool UpdaterUserData_Bool;
        public static float CheckUpdaterTime;
        public static bool IsChinese;
    }
}