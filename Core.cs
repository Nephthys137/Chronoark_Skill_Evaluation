namespace ChronoArk_Evaluation
{
    public static class Core
    {
        public static Skill_Evaluation_Data GetOrCreateEvaluationData(string key)
        {
            if (!Mod_Init.User_Data_Dic.TryGetValue(key, out var Evaluation_Data))
                Mod_Init.User_Data_Dic[key] = Evaluation_Data = new();
            return Evaluation_Data;
        }
        public static void OnShowCrad(Skill skill)
        {
            GetOrCreateEvaluationData(skill.MySkill.KeyID).出现次数++;
        }
        public static void OnAddCrad(SkillButton skill)
        {
            GetOrCreateEvaluationData(skill.Myskill.MySkill.KeyID).获得次数++;
        }
        public static void OnRemoveCrad(SkillButton skill)
        {
            GetOrCreateEvaluationData(skill.Myskill.MySkill.KeyID).删除次数++;
        }
        public static void OnEndGame()
        {
            foreach (Character character in PlayData.TSavedata.Party)
                foreach (var skill in character.SkillDatas)
                    GetOrCreateEvaluationData(skill.SkillInfo.KeyID).尝试次数++;
            foreach (var skill in PlayData.TSavedata.LucySkills)
                GetOrCreateEvaluationData(skill).尝试次数++;
        }
        public static void OnWinGame()
        {
            foreach (Character character in PlayData.TSavedata.Party)
                foreach (var skill in character.SkillDatas)
                {
                    var Evaluation_Data = GetOrCreateEvaluationData(skill.SkillInfo.KeyID);
                    Evaluation_Data.通关次数++;
                    Evaluation_Data.尝试次数++;
                }
            foreach (var skill in PlayData.TSavedata.LucySkills)
            {
                var Evaluation_Data = GetOrCreateEvaluationData(skill);
                Evaluation_Data.通关次数++;
                Evaluation_Data.尝试次数++;
            }
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
