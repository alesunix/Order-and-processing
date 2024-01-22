using DocumentFormat.OpenXml.EMMA;
using GisalSpareParts.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;

namespace GisalSpareParts.Models
{
    public class OrderModel : BaseModel
    {
        public Int64 Id { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [StringLength(50, ErrorMessage = "Количество символов не больше 50")]
        public string Name { get; set; }
        public DateTime Dateorder { get; set; }
        public DateTime Datedelivery { get; set; }// Доступ: Поставщик
        public string Codegisal { get; set; }// Доступ: Поставщик
        public decimal Status { get; set; }// Доступ: Заказчик и Поставщик
        public string Overdue { get; set; }
        public string Statusname { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [StringLength(20, ErrorMessage = "Количество символов не больше 20")]
        public string Codemachine { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [StringLength(20, ErrorMessage = "Количество символов не больше 20")]
        public string Codeline { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [StringLength(20, ErrorMessage = "Количество символов не больше 20")]
        public string Module { get; set; }
        [Range(1, 9999, ErrorMessage = "Минимум 1, максимум 9999")]
        public decimal Amount { get; set; }
        public string Rem { get; set; }
        [Range(1, 100, ErrorMessage = "Поле не должно быть пустым!")]
        public decimal Urgency { get; set; }
        public string UrgencyName { get; set; }
        public decimal Ordernumber { get; set; }
        public string Deleted { get; set; }
        public decimal FileId { get; set; }
        public List<Chat> Chat { get; set; }
        public string Sms { get; set; }
        public Int64 Countsms { get; set; }
        public string Ischeck { get; set; }
        public string Nam { get; set; }
        public string FieldDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        LogsModel logModel = new();
        public string Action { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public string RoleName { get; set; }
        public int LoginID { get; set; }
        public decimal Divisionid { get; set; }
        public string Division { get; set; }
        public List<OrderModel> GetDataOrders()
        {
            Name ??= string.Empty;
            Codegisal ??= string.Empty;
            Codemachine ??= string.Empty;
            string prop = string.Empty;
            string field = "a.name";
            if (Name != "") { prop = Name; field = "a.name"; }
            else if (Codegisal != "") { prop = Codegisal; field = "a.codegisal"; }
            else if (Codemachine != "") { prop = Codemachine; field = "a.Codemachine"; }

            string dateWhere = string.Empty;
            string statusWhere = string.Empty;
            string overdueWhere = string.Empty;
            string urgencyWhere = string.Empty;
            string orderNumWhere = string.Empty;
            string divisionWhere = string.Empty;

            if (!string.IsNullOrEmpty(FieldDate)) dateWhere = $"AND {FieldDate} between '{StartDate:yyyy-MM-dd}' AND '{EndDate:yyyy-MM-dd}'";
            if (Status != 0 && Status != 7) statusWhere = $"AND a.status = {Status}";
            if (Status == 7) overdueWhere = $"AND a.overdue = '{Overdue}'";
            if (Urgency != 0) urgencyWhere = $"AND a.urgency = {Urgency}";
            if (Ordernumber != 0) orderNumWhere = $"AND a.ordernumber = {Ordernumber}";
            if (Divisionid != 0) divisionWhere = $"AND c.divisionid = {Divisionid}";

            var table = GetListT<OrderModel>($@"SELECT a.id, a.name, a.dateorder, a.datedelivery, a.codegisal, b.name AS statusname, a.status,
                                              a.Codemachine, a.Codeline, a.Module, a.amount, a.rem, a.urgency, a.ordernumber, a.deleted, c.divisionid, a.overdue
                                              FROM orders a
                                              LEFT JOIN sprstatus b ON a.status = b.cod
                                              LEFT JOIN userid c ON c.id = a.loginid
                                              WHERE LOWER({field}) LIKE '%{prop.ToLower()}%' {dateWhere} {statusWhere} {overdueWhere} {urgencyWhere} {orderNumWhere} {divisionWhere}
                                              ORDER BY a.ordernumber DESC");

            Division = Divisionid == 0 ? "Все фирмы" : (string)GetSingleResult($"SELECT note FROM sprdivision WHERE id = {Divisionid}");/// Название фирмы заказчика
            // Присоединяю чат к общей таблице 
            var listCountSMS = GetListT<OrderModel>($@"SELECT Count(*) as Countsms, pkey as id, MAX(nam), MAX(ischeck) FROM chat group by pkey");
            var listUnreadSMS = GetListT<OrderModel>($@"SELECT pkey AS id, ischeck, nam FROM chat WHERE ischeck = 'F'");
            foreach (var item in table)
            {
                var countSMSItem = listCountSMS.FirstOrDefault(x => x.Id == item.Id);
                if (countSMSItem != null)
                {
                    item.Countsms = countSMSItem.Countsms;
                    item.Ischeck = countSMSItem.Ischeck;
                    item.Nam = countSMSItem.Nam;
                }
                var unreadSMSItem = listUnreadSMS.FirstOrDefault(x => x.Id == item.Id);
                if (unreadSMSItem != null)
                {
                    item.Ischeck = unreadSMSItem.Ischeck;
                    item.Nam = unreadSMSItem.Nam;
                }
            }
            // Просрочка проверка и изменение
            decimal[] statuses = { 1, 2, 3, 4 };
            foreach (var item in table)
            {
                if (item.Deleted == "T") continue;
                if (DateTime.Now > item.Datedelivery && item.Overdue != "T" && statuses.Contains(item.Status))
                {
                    OverdueOrder(item.Id, "T");
                }
                else if (DateTime.Now < item.Datedelivery && item.Overdue == "T")
                {
                    OverdueOrder(item.Id, "F");
                }
                else if (item.Overdue == "T" && !statuses.Contains(item.Status))
                {
                    OverdueOrder(item.Id, "F");
                }
            }
            return table;
        }
        private void OverdueOrder(decimal id, string overdue) => SetQuery($@"UPDATE orders SET overdue = '{overdue}' WHERE Id = {id}");
        public Dictionary<Int64, string> GetListDevisions() => GetDictionary<Int64, string>("SELECT id, name FROM sprdivision ORDER BY name");
        public void GetDivisionID()
        {
            //var divisionId = GetSingleResult($@"WITH usertable AS (
            //                                        SELECT divisionid FROM userid WHERE id = {LoginID}
            //                                    ),
            //                                    orderstable AS (
            //                                        SELECT b.divisionid 
            //                                        FROM orders a
            //                                        LEFT JOIN userid b ON b.id = a.loginid
            //                                        WHERE a.id = {Id}
            //                                    )
            //                                    SELECT 
            //                                        CASE
            //                                            WHEN EXISTS (SELECT divisionid FROM usertable) THEN (SELECT divisionid FROM orderstable)
            //                                        END AS divisionid");
            //Divisionid = (divisionId != DBNull.Value) ? Convert.ToDecimal(divisionId) : 0;

            var obj = GetSingleResult($"SELECT divisionid FROM userid WHERE id = {LoginID}");
            Divisionid = (obj != DBNull.Value) ? (decimal)obj : 0;
        }
        public Int64 GetCountLogs() => (Int64)GetSingleResult("SELECT COUNT(dt) FROM logs");
        public List<Chat> GetDataChat(decimal id)
        {
            var list = new List<Chat>();
            var table = GetListT<Chat>($@"SELECT nam, dt, sms FROM chat WHERE pkey = {id} ORDER BY dt");
            foreach (var item in table)
            {
                list.Add(item);
            }
            return list;
        }
        public string GetStatusName() => GetSingleResult($"SELECT name FROM sprstatus WHERE cod = '{Status}'").ToString();
        public Dictionary<decimal, string> UrgencyList()
        {
            Dictionary<decimal, string> list = new();
            list.Add(1, "Обычный");
            list.Add(2, "Срочный");
            return list;
        }
        public Dictionary<decimal, string> StatusList() => GetDictionary<decimal, string>($"SELECT cod, name FROM sprstatus ORDER BY cod");
        public List<File> GetFiles() => GetListT<File>($"SELECT id, nam, files FROM Files WHERE pkey = {Id}");
        public void InsertFiles(byte[] bytes, string fileName, Int64 idOrder)
        {
            SetQuery($"INSERT INTO Files (PKEY, NAM, FILES) VALUES ({idOrder}, '{fileName}', :BLOB)", bytes);
            logModel.InsertLog(Login, $"{RoleName} {Login} добавил файл в заказе {Ordernumber} \"{Name}\"", Divisionid);
        }
        public void DeleteFile()
        {
            SetQuery($"DELETE FROM files WHERE id = {FileId}");
            MyMessage = "Удаление файла выполнено успешно";
            logModel.InsertLog(Login, $"{RoleName} {Login} Удалил файл в заказе {Ordernumber} \"{Name}\"", Divisionid);
        }
        public void InsertSMS()
        {
            if (Sms != null)
                SetQuery($"INSERT INTO chat (PKEY, NAM, DT, SMS, ISCHECK) VALUES ({Id}, '{Login}', datetime('now','localtime'), '{Sms}', 'F')");
            logModel.InsertLog(Login, $"{RoleName} {Login} написал SMS в заказе {Ordernumber} \"{Name}\"", Divisionid);
        }
        public void CheckSMS()
        {
            var count = (Int64)GetSingleResult($"SELECT COUNT(*) FROM chat WHERE ischeck = 'F' AND nam <> '{Login}' AND pkey = {Id}");
            if (count > 0)
            {
                SetQuery($"UPDATE chat SET ISCHECK = 'T' WHERE pkey = {Id} AND nam <> '{Login}'");
                logModel.InsertLog(Login, $"{RoleName} {Login} прочитал SMS в заказе {Ordernumber} \"{Name}\"", Divisionid);
            }
        }
        public void UpdateProvider()
        {
            if (!string.IsNullOrEmpty(Action))
            {
                if (CounterOperations(Id) < 2)
                {
                    SetQuery($"UPDATE ORDERS SET Datedelivery = '{Datedelivery:yyyy-MM-dd}', Codegisal = '{Codegisal}', Status = '{Status}' WHERE Id = {Id}");
                    MyMessage = "Заказ успешно изменен";
                    logModel.InsertLog(Login, $"{RoleName} {Login} изменил {Action} в заказе {Ordernumber} \"{Name}\"", Divisionid);
                }
            }
        }
        public decimal CounterOperations(long id, bool reset = false)
        {
            if (id == 0) return 0;
            Overdue = GetSingleResult($"SELECT overdue FROM orders WHERE Id = {id}").ToString();
            object obj = GetSingleResult($"SELECT count FROM counter WHERE pkey = {id}");
            if (Action == "дата поставки" && Overdue == "T")
            {
                if (obj == null)
                    SetQuery($"INSERT INTO counter (pkey, count) VALUES ('{id}', '1')");
                else if (obj != null && (decimal)obj < 2)
                    SetQuery($"UPDATE counter SET count = '{(decimal)obj + 1}' WHERE pkey = {id}");
            }
            if (reset == true)
                SetQuery($"UPDATE counter SET count = '0' WHERE pkey = {id}");
            return Convert.ToDecimal(obj);
        }
        public Int64 InsertUpdateOrder()
        {
            if (Mode.Button == ModeButton.Add || Mode.Button == ModeButton.Copy)
            {
                var id = SetQuery($@"INSERT INTO orders (NAME, ORDERNUMBER, Dateorder, Datedelivery, Status, Codemachine, Codeline, Module, AMOUNT, REM, URGENCY, LOGINID, DELETED) 
                                VALUES ('{Name}', {CreateOrderNumber()}, '{Dateorder:yyyy-MM-dd}', '{Datedelivery:yyyy-MM-dd}', '{Status}', '{Codemachine}', '{Codeline}','{Module}','{Amount}', '{Rem}', '{Urgency}', {LoginID}, 'F'); select last_insert_rowid();");
                MyMessage = "Заказ успешно создан";
                logModel.InsertLog(Login, $"{RoleName} {Login} создал заказ {Ordernumber} \"{Name}\"", Divisionid);
                return id;
            }
            else
            {
                if (!string.IsNullOrEmpty(Action))
                {
                    SetQuery($@"UPDATE orders SET Name = '{Name}', Dateorder = '{Dateorder:yyyy-MM-dd}', STATUS = '{Status}', Codemachine = '{Codemachine}', Codeline = '{Codeline}',Module = '{Module}',Amount = {Amount}, Rem = '{Rem}', Urgency = '{Urgency}' WHERE Id = {Id}");
                    MyMessage = "Заказ успешно изменен";
                    logModel.InsertLog(Login, $"{RoleName} {Login} изменил {Action} в заказе {Ordernumber} \"{Name}\"", Divisionid);
                }
                else MyMessage = "Нет изменений в карточке";
                return Id;
            }
        }
        public void DeleteOrRecovery()
        {
            string action = string.Empty;
            if (Mode.Button == ModeButton.MarkDelRec)
            {
                action = Deleted == "T" ? "удалил" : "восстановил";
                SetQuery($"UPDATE orders SET Deleted = '{Deleted}' WHERE Id = {Id}");
                MyMessage = Deleted == "T" ? "Заказ успешно удален" : "Заказ успешно восстановлен";
            }
            else if (Mode.Button == ModeButton.Delete)
            {
                action = "удалил из базы";
                SetQuery($"DELETE FROM orders WHERE Id = {Id}");
                SetQuery($"DELETE FROM chat WHERE pkey = {Id}");
                SetQuery($"DELETE FROM files WHERE pkey = {Id}");
                MyMessage = "Заказ успешно удален из базы";
            }
            logModel.InsertLog(Login, $"{RoleName} {Login} {action} заказ {Ordernumber} \"{Name}\"", Divisionid);
        }
        /// <summary>
        /// Добавляет уникальный номер заказа по шаблону ГГММ№№№. Т.е. третий заказ в апреле 23 года должен писаться как заказ от 2304003. 
        /// В каждом следующем месяце первый заказ должен начинаться с единицы (2305001)
        /// </summary>
        /// <returns>Возвращает decimal номер заказа</returns>
        private decimal CreateOrderNumber()
        {
            var ordernumber = Convert.ToString(GetSingleResult($@"SELECT a.ordernumber FROM orders a 
                                                                    LEFT JOIN userid c ON c.id = a.loginid 
                                                                    WHERE a.deleted == 'F' AND c.divisionid = {Divisionid} 
                                                                    ORDER BY ordernumber DESC"));
            var year = DateTime.Now.ToString("yy");
            if (!string.IsNullOrEmpty(ordernumber))
            {
                var month = ordernumber.Substring(2, 2).Split()[0];
                decimal count = Convert.ToDecimal(ordernumber.Substring(4));
                if (month == DateTime.Now.ToString("MM"))/// Если месяцы равны, инкрементируем
                {
                    count++;
                    return Ordernumber = Convert.ToDecimal(year + month + count.ToString().PadLeft(3, '0'));
                }
                else
                    return Ordernumber = Convert.ToDecimal(year + DateTime.Now.ToString("MM") + 001.ToString().PadLeft(3, '0'));
            }
            else
                return Ordernumber = Convert.ToDecimal(year + DateTime.Now.ToString("MM") + 001.ToString().PadLeft(3, '0'));
        }

    }
}
