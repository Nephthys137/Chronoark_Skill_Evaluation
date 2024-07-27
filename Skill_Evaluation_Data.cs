namespace ChronoArk_Evaluation
{
    public class Skill_Evaluation_Data
    {
        public string 技能名;
        public string 评价等级;
        public string 评价;
        public double 出现次数;
        public double 获得次数;
        public double 删除次数;
        public double 尝试次数;
        public double 通关次数;
        private string GrabProb;
        private string DelProb;
        private string WinProb;
        public void UpdaterData()
        {
            GrabProb = 出现次数 < 100 ? "N/A" : (100 * (获得次数 / 出现次数)).ToString("F1") + "%";
            DelProb = 获得次数 < 50 ? "N/A" : (100 * (删除次数 / 获得次数)).ToString("F1") + "%";
            WinProb = 尝试次数 < 10 ? "N/A" : (100 * (通关次数 / 尝试次数)).ToString("F1") + "%";
        }
        public override string ToString()
        {
            return Mod_Init.IsChinese ? Mod_Init.Evaluation_Bool ? $"{评价等级} 抓{GrabProb}删{DelProb}胜率{WinProb}\n{评价}" : $"抓{GrabProb}删{DelProb}胜率{WinProb}" : $"GrabProb {GrabProb} DelProb {DelProb} WinProb {WinProb}";
        }
    }
    public class Skill_Evaluation_Data_List
    {
        public List<Skill_Evaluation_Data> RECORDS;
    }
}
