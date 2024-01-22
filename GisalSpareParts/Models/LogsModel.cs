
using GisalSpareParts.Shared;

namespace GisalSpareParts.Models
{
    public class LogsModel : BaseModel
    {
        public static List<Chat> Chat { get; set; }
        public static Int64 CountLogs { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public decimal Divisionid { get; set; }
        public string Division { get; set; }
        public List<LogsModel> GetDataLogs()
        {
            string usernameWhere = string.Empty;
            string divisionWhere = string.Empty;
            if (!string.IsNullOrEmpty(Username)) usernameWhere = $"AND a.username = '{Username}'";
            if (Divisionid != 0) divisionWhere = $"AND a.divisionid = {Divisionid}";

            var table = GetListT<LogsModel>(@$"SELECT a.dt, a.username, a.message
                                                FROM logs a
                                                WHERE STRFTIME('%Y-%m-%d', a.dt) between '{StartDate:yyyy-MM-dd}' AND '{EndDate:yyyy-MM-dd}' {usernameWhere} {divisionWhere}
                                                ORDER BY a.dt DESC");

            Division = Divisionid == 0 ? "Все фирмы" : (string)GetSingleResult($"SELECT note FROM sprdivision WHERE id = {Divisionid}");/// Название фирмы заказчика
            return table;
        }
        public void InsertLog(string userName, string message, decimal divisionId)
        {
            SetQuery($"INSERT INTO logs (dt, username, message, divisionid) VALUES (datetime('now','localtime'), '{userName}', STRFTIME('%d-%m-%Y, %H:%M', datetime('now','localtime')) || ' {message}', {divisionId})");
        }
        public List<string> GetUsers()
        {
            if (Divisionid != 0)
                return GetList($"SELECT username FROM userid WHERE divisionid = {Divisionid} ORDER BY username");
            else return GetList($"SELECT username FROM userid ORDER BY username");
        }
        public void GetDivisionID(int loginId)
        {
            var obj = GetSingleResult($"SELECT divisionid FROM userid WHERE id = {loginId}");
            Divisionid = (obj != DBNull.Value) ? (decimal)obj : 0;
        }
        public Dictionary<Int64, string> GetListDevisions() => GetDictionary<Int64, string>("SELECT id, name FROM sprdivision ORDER BY name");
    }
}
