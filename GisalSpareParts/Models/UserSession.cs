using System.ComponentModel.DataAnnotations;

namespace GisalSpareParts.Models
{
    public class UserSession : BaseModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public string Pass1 { get; set; }
        [Required(ErrorMessage = "Поле не должно быть пустым!")]
        public string Pass2 { get; set; }
        public string Password { get; set; }
        UserModel uModel = new();
        public bool ChangePassword()
        {
            if (Id < 0)
                return false;
            if (Pass1 == Pass2)
            {
                if (uModel.PasswordComplexity(Pass1))
                {
                    Password = uModel.HashPassword(Pass2);
                    SetQuery($"UPDATE userid SET Password = '{Password}' WHERE Id = {Id}");
                    return true;
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
        public string GetLogin() => GetSingleResult($@"SELECT Username FROM userid WHERE id = {Id}").ToString();
    }
}
