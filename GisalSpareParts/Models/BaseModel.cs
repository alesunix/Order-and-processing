using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Components;
using System;
using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace GisalSpareParts.Models
{
    public class BaseModel
    {
        public static string MyMessage { get; set; }
        private string ConnString { get; set; }
        public BaseModel()
        {
            GenerateConn();
        }
        private void GenerateConn()
        {
            //string path = @$"{Environment.CurrentDirectory}\GisalDB.db";
            string path = "C:\\inetpub\\blazor\\GisalDB.db";
            SQLiteConnectionStringBuilder connString = new()
            {
                DataSource = path,
            };
            ConnString = connString.ToString();
        }
        public List<UserAccount> GetUsers(string query)
        {
            List<UserAccount> list = new();
            using (SQLiteConnection con = new(ConnString))
            {
                con.Open();
                var cmd = new SQLiteCommand(query, con);
                SQLiteDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    UserAccount user = new()
                    {
                        Id = dr.GetInt32("Id"),
                        UserName = dr.GetString("Username"),
                        Password = Convert.ToString(dr.GetValue("Password")),
                        Role = dr.GetString("Cod"),
                        Deleted = dr.GetString("Deleted")
                    };
                    list.Add(user);
                }
                con.Close();
                con.Dispose();
                return list;
            }
        }
        public List<T> GetListT<T>(string query) where T : new()
        {
            List<T> list = new();
            using SQLiteConnection con = new(ConnString);
            con.Open();
            var cmd = new SQLiteCommand(query, con);
            SQLiteDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (dr.Read())
            {
                T t = new();
                Type type = typeof(T);
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string column = dr.GetName(i).ToLower();
                    string propName = column.First().ToString().ToUpper() + column.Substring(1);
                    PropertyInfo info = type.GetProperty(propName);

                    if (info != null && !dr.IsDBNull(i))
                    {
                        object obj = dr.GetValue(i);
                        info.SetValue(t, obj);
                    }
                }
                list.Add(t);
            }
            dr.Close();
            con.Close();
            con.Dispose();
            return list;
        }
        public List<string> GetList(string query)
        {
            List<string> list = new();
            using (SQLiteConnection con = new(ConnString))
            {
                con.Open();
                var cmd = new SQLiteCommand(query, con);
                SQLiteDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    list.Add(dr.GetValue(0).ToString());
                }
                con.Close();
                con.Dispose();
                return list;
            }
        }
        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(string query)
        {
            Dictionary<TKey, TValue> list = new();
            using (SQLiteConnection con = new(ConnString))
            {
                con.Open();
                var cmd = new SQLiteCommand(query, con);
                SQLiteDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    TKey key = dr.GetFieldValue<TKey>(0);
                    TValue value = dr.GetFieldValue<TValue>(1);
                    list.Add(key, value);
                }
            }
            return list;
        }
        public object GetSingleResult(string query)
        {
            using (SQLiteConnection con = new(ConnString))
            {
                con.Open();
                var cmd = new SQLiteCommand(query, con).ExecuteScalar();
                con.Close();
                con.Dispose();
                return cmd;
            }
        }
        public long SetQuery(string query, byte[] bytes = null)
        {
            using (SQLiteConnection con = new(ConnString))
            {
                con.Open();
                SQLiteCommand cmd = new(query, con);
                if (bytes != null)
                {
                    var blob = cmd.Parameters.Add("BLOB", DbType.Binary);
                    blob.Direction = ParameterDirection.Input;
                    blob.Value = bytes;
                }
                cmd.ExecuteNonQuery();
                cmd.CommandText = "select last_insert_rowid()";
                long id = (long)cmd.ExecuteScalar();
                con.Close();
                con.Dispose();
                return id;
            }
        }
    }
}
