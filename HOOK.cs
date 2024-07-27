using GameDataEditor;
using HarmonyLib;
using I2.Loc;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChronoArk_Evaluation
{
    public static class HOOK
    {
        [HarmonyPatch(typeof(ToolTipWindow), nameof(ToolTipWindow.itemTooltip))]
        [HarmonyPostfix]
        static void ItemTooltip(ToolTipWindow __instance, Transform trans, ItemBase Item, GameObject __result)
        {
            if (Mod_Init.Evaluation_Bool && Mod_Init.Item_Data_Dic.TryGetValue(Item.itemkey, out var item_Evaluation_Data))
                ToolTipWindow.PlusTooltipsView_Mod("Item Evaluation", item_Evaluation_Data.ToString(), __result.gameObject.transform.Find("PlusTooltip"));
        }
        [HarmonyPatch(typeof(SkillToolTip), nameof(SkillToolTip.Input))]
        [HarmonyPostfix]
        static void Input(SkillToolTip __instance, Skill Skill, Stat _stat, ToolTipWindow.SkillTooltipValues skillvalues, bool View)
        {
            if (Mod_Init.Skill_Data_Dic.TryGetValue(Skill.MySkill.KeyID, out var skill_Evaluation_Data))
                __instance.PlusTooltipsView(Mod_Init.Evaluation_Bool ? "Skill Evaluation" : "WinProb", skill_Evaluation_Data.ToString());
        }
        [HarmonyPatch(typeof(SkillView), nameof(SkillView.Init))]
        [HarmonyPostfix]
        static void Init(SkillView __instance, GDESkillData _data, int _index)
        {
            if (Mod_Init.Skill_Data_Dic.TryGetValue(_data.KeyID, out var skill_Evaluation_Data))
                __instance.PlusTooltipsView(Mod_Init.Evaluation_Bool ? "<b>Skill Evaluation</b>" : "<b>WinProb</b>", skill_Evaluation_Data.ToString());
        }
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.TryNumPlus))]
        [HarmonyPostfix]
        static void TryNumPlus()
        {
            Core.OnEndGame();
        }
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.QuitSave))]
        [HarmonyPostfix]
        static void QuitSave()
        {
            if (!(SceneManager.GetActiveScene().name == "Main") && !(SceneManager.GetActiveScene().name == "MainOption") && (!(SceneManager.GetActiveScene().name == "Battle") || !(BattleSystem.instance != null) || BattleSystem.instance.ActWindow.On))
                Core.OnEndGame();
        }
        [HarmonyPatch(typeof(PauseWindow), nameof(PauseWindow.QuitGame))]
        [HarmonyPrefix]
        static bool QuitGame(PauseWindow __instance)
        {
            if (PlayData.GameStarted)
            {
                PauseCautionWindow.ApplyDel applyDel = new(QuitGameDel);
                __instance.cw = Misc.UIInst(__instance.Cautionwindow, __instance.transform);
                __instance.cw.GetComponent<PauseCautionWindow>().init(ScriptLocalization.UI_Pause.Quit_Msg, applyDel);
                return false;
            }
            QuitGameDel();
            return false;
        }
        static void QuitGameDel()
        {
            SaveManager.savemanager.Save();
            if (!(SceneManager.GetActiveScene().name == "Main") && !(SceneManager.GetActiveScene().name == "MainOption") && (!(SceneManager.GetActiveScene().name == "Battle") || !(BattleSystem.instance != null) || BattleSystem.instance.ActWindow.On))
                Core.OnEndGame();
            Thread.Sleep(100);
            Application.Quit();
        }
        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ClearUnlock))]
        [HarmonyPostfix]
        static void ClearUnlock()
        {
            Core.OnWinGame();
        }
        [HarmonyPatch(typeof(SelectSkillList), nameof(SelectSkillList.NewSelectSkillList))]
        [HarmonyPostfix]
        static void NewSelectSkillList(bool back, string Desc, List<Skill> Skills, SkillButton.SkillClickDel Delegate, bool ManaView, bool HideButtonView, bool IsShowMySkill, bool ShowFace)
        {
            foreach (var skill in Skills)
                Core.OnShowCrad(skill);
        }
        [HarmonyPatch(typeof(SelectSkillList), nameof(SelectSkillList.AddCollectionDel))]
        [HarmonyPostfix]
        static void AddCollectionDel(SkillButton Mybutton)
        {
            Core.OnAddCrad(Mybutton);
        }
        [HarmonyPatch(typeof(CharStatV4), nameof(CharStatV4.ForgetSkill), [typeof(SkillButton)])]
        [HarmonyPostfix]
        static void ForgetSkill(SkillButton Mybutton)
        {
            Core.OnRemoveCrad(Mybutton);
        }
        [HarmonyPatch(typeof(CharSelectMainUIV2), nameof(CharSelectMainUIV2.Apply))]
        [HarmonyPostfix]
        static void NewGame()
        {
            foreach (var skill in PlayData.TSavedata.LucySkills)
            {
                var Evaluation_Data = Core.GetOrCreateEvaluationData(skill);
                Evaluation_Data.出现次数++;
                Evaluation_Data.获得次数++;
            }
        }
    }
}