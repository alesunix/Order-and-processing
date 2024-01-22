using GisalSpareParts.Extensions;
using GisalSpareParts.Shared;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace GisalSpareParts.Models
{
    public class UserModel : BaseModel
    {
        public bool IsNew { get; set; }
        public Int64 Id { get; set; }
        public string Password { get; set; }

        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [StringLength(12, ErrorMessage = "Количество символов не больше 12")]
        public string Username { get; set; }
        [RequiredIf(nameof(IsNew), true, ErrorMessage = "Поле не должно быть пустым!")]
        public string Pass1 { get; set; }
        [RequiredIf(nameof(IsNew), true, ErrorMessage = "Поле не должно быть пустым!")]
        public string Pass2 { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [Range(1, 9, ErrorMessage = "Поле не должно быть пустым!")]
        public decimal Roleid { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        [Range(1, 9, ErrorMessage = "Поле не должно быть пустым!")]
        public decimal Divisionid { get; set; }
        public string Role { get; set; }
        public string Cod { get; set; }
        public string Deleted { get; set; }
        public string DeletedFilter { get; set; }

        public List<UserModel> GetDataUsers()
        {
            Username ??= string.Empty;
            Role ??= string.Empty;
            DeletedFilter ??= string.Empty;
            string divisionWhere = string.Empty;
            if (Divisionid != 0) divisionWhere = $"AND a.Divisionid = {Divisionid}";

            return GetListT<UserModel>($@"SELECT a.id, a.username, c.name as role, a.Deleted, a.Roleid, c.Cod, a.Divisionid
                                        FROM userid a
                                        left join sprroles c on a.Roleid = c.id        
                                        WHERE LOWER(a.username) LIKE '%{Username.ToLower()}%' AND LOWER(c.name) LIKE '%{Role.ToLower()}%' AND LOWER(a.Deleted) LIKE '%{DeletedFilter.ToLower()}%'
                                        {divisionWhere}
                                        ORDER BY a.Roleid");
        }
        public void GetDivisionID(int loginId)
        {
            var obj = GetSingleResult($"SELECT divisionid FROM userid WHERE id = {loginId}");
            Divisionid = (obj != DBNull.Value) ? (decimal)obj : 0;
        }
        public string GetRoleName(string role) => GetSingleResult($"SELECT name FROM sprroles WHERE cod = '{role}'").ToString();
        public Dictionary<Int64, string> GetListRoles(string role)
        {
            if (role == "DEVEL")
                return GetDictionary<Int64, string>("SELECT id, name FROM sprroles ORDER BY name");
            else
                return GetDictionary<Int64, string>("SELECT id, name FROM sprroles WHERE name NOT IN ('Разработчик', 'Поставщик') ORDER BY name");/// Если не разработчик, то исключаем из списка роль разработчика и поставщика
        }
        public Dictionary<Int64, string> GetListDevisions() => GetDictionary<Int64, string>("SELECT id, name FROM sprdivision ORDER BY name");
        public bool PasswordComplexity(string password)/// Сложность пароля
        {
            string pattern = @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-.]).{8,}$";
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/FilePass.txt");
            string[] listPass = System.IO.File.ReadAllLines(path);
            if (!Regex.IsMatch(password, pattern))
            {
                MyMessage = "Пароль слишком прост";
                return false;
            }
            else if (listPass.Contains(password))
            {
                MyMessage = "Данный пароль нельзя использовать";
                return false;
            }
            return true;
        }
        public bool AddOrEdit()
        {
            if (!string.IsNullOrWhiteSpace(Pass1) & !string.IsNullOrWhiteSpace(Pass2))
            {
                if (Pass1 == Pass2)
                {
                    if (PasswordComplexity(Pass1))
                    {
                        Password = HashPassword(Pass2);
                        return InsertOrUpdate();
                    }
                    else
                        return false;
                }
                else
                {
                    MyMessage = "Пароли не совпадают";
                    return false;
                }
            }
            else
            {
                /// Если поля пустые то не обновляем пароль
                SetQuery($@"UPDATE userid SET Username = '{Username}', Roleid = {Roleid}, Divisionid = {Divisionid} WHERE Id = {Id}");
                MyMessage = "Пользователь успешно изменен";
                return true;
            }
        }
        private bool LoginDublicate()
        {
            int count = Convert.ToInt32(GetSingleResult($"SELECT count(*) FROM userid WHERE username = '{Username}'"));
            if (count > 0)
            {
                MyMessage = "Пользователь с таким именем уже существует в базе";
                return false;
            }
            return true;
        }
        private bool InsertOrUpdate()
        {
            if (Mode.Button == ModeButton.Add)
            {
                if (LoginDublicate())
                {
                    SetQuery($@"INSERT INTO userid (Username, Password, Deleted, Roleid, Divisionid) VALUES ('{Username}','{Password}','F',{Roleid}, {Divisionid})");
                    MyMessage = "Пользователь успешно создан";
                    return true;
                }
                else return false;/// Если пользователь с таким именем существует
            }
            else if (Mode.Button == ModeButton.Edit)
            {
                SetQuery($@"UPDATE userid SET Username = '{Username}', Password = '{Password}', Roleid = {Roleid}, Divisionid = {Divisionid} WHERE Id = {Id}");
                MyMessage = "Пользователь успешно изменен";
                return true;
            }
            else return false;
        }
        public string HashPassword(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToHexString(SHA256.HashData(bytes)).ToUpper();
        }
        public void DeleteOrRecovery()
        {
            if (Mode.Button == ModeButton.MarkDelRec)/// Метка на удаление или восстановление
            {
                SetQuery($"UPDATE userid SET Deleted = '{Deleted}' WHERE Id = {Id}");
                MyMessage = Deleted == "T" ? "Пользователь успешно удален" : "Пользователь успешно восстановлен";
            }
            else if (Mode.Button == ModeButton.Delete)
            {
                SetQuery($"DELETE FROM userid WHERE Id = {Id}");
                MyMessage = "Пользователь успешно удален из базы";
            }
        }
    }
}
