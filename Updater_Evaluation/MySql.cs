using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

namespace Updater_Evaluation
{
    public class MySql
    {
        public static DataSet KeyValue(string Table)
        {
            using MySqlConnection conn = new(MySqlCon);
            conn.Open();
            MySqlDataAdapter mySqlDataAdapter = new(new("SELECT * FROM " + Table, conn));
            mySqlDataAdapter.SelectCommand.ExecuteNonQuery();
            DataSet dataSet = new();
            mySqlDataAdapter.Fill(dataSet);
            return dataSet;
        }
        public static void FixData(Skill_Evaluation_Data_List skill_Evaluation_Data_List)
        {
            using MySqlConnection conn = new(MySqlCon);
            conn.Open();
            new MySqlCommand("DROP TABLE skill_evaluation", conn).ExecuteNonQuery();
            string text = "CREATE TABLE skill_evaluation(技能名 TEXT, 评价等级 TEXT, 评价 TEXT, 序号 INT AUTO_INCREMENT PRIMARY KEY, 出现次数 INT DEFAULT 0, 获得次数 INT DEFAULT 0, 删除次数 INT DEFAULT 0, 尝试次数 INT DEFAULT 0, 通关次数 INT DEFAULT 0)";
            new MySqlCommand(text, conn).ExecuteNonQuery();
            StringBuilder sb = new("INSERT INTO skill_evaluation (技能名, 评价等级, 评价, 序号, 出现次数, 获得次数, 删除次数, 尝试次数, 通关次数) VALUES ");
            foreach (var skill in skill_Evaluation_Data_List.RECORDS)
            {
                sb.Append($"('{skill.技能名}', '{skill.评价等级}', '{skill.评价}', {skill.序号}, {skill.出现次数:F0}, 获得次数 = {skill.获得次数:F0}, 删除次数 = {skill.删除次数:F0}, 尝试次数 = {skill.尝试次数:F0}, 通关次数 = {skill.通关次数:F0}),");
            }
            sb.Length--;
            new MySqlCommand(sb.ToString(), conn).ExecuteNonQuery();
        }
        public static void UpdateDatabase(Skill_Evaluation_Data_List skill_Evaluation_Data_List, Version_Data_List version_Data_List)
        {
            if (skill_Evaluation_Data_List.RECORDS.Count < 1)
                return;

            using MySqlConnection conn = new(MySqlCon);
            conn.Open();
            string updateSql = "SET SQL_SAFE_UPDATES = 0;";
            foreach (var skill in skill_Evaluation_Data_List.RECORDS)
            {
                updateSql += $"UPDATE skill_evaluation SET 出现次数 = {skill.出现次数:F0}, 获得次数 = {skill.获得次数:F0}, 删除次数 = {skill.删除次数:F0}, 尝试次数 = {skill.尝试次数:F0}, 通关次数 = {skill.通关次数:F0} WHERE 序号 = {skill.序号};";
            }
            new MySqlCommand(updateSql, conn).ExecuteNonQuery();

            string NowTime = DateTime.Now.ToString("yyyy/%M/%d %H:%m:%s");
            string text2 = $"SET SQL_SAFE_UPDATES = 0;UPDATE versions SET 时间 = '{NowTime}' WHERE 版本 = '{version_Data_List.RECORDS[0].版本}'";
            new MySqlCommand(text2, conn).ExecuteNonQuery();
        }

        public static string MySqlCon = "${{ secrets.MySqlCon }}";
    }
}
