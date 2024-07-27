namespace Updater_Evaluation
{
    public class Skill_Evaluation_Data
    {
        public string 技能名;
#if DEBUG
        public double GrabProb;
        public double DelProb;
        public double WinProb;
#endif
        public string 评价等级;
        public string 评价;
        public string 序号;

        public double 出现次数;
        public double 获得次数;
        public double 删除次数;
        public double 尝试次数;
        public double 通关次数;
    }
    public class Skill_Evaluation_Data_List
    {
        public List<Skill_Evaluation_Data> RECORDS;
    }
}
