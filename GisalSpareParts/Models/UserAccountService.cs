namespace GisalSpareParts.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Deleted { get; set; }
    }
    public class UserAccountService : BaseModel
    {
        private List<UserAccount> _users;
        public UserAccountService()
        {
            _users = new List<UserAccount>();
            _users = GetListUsers();
        }
        public List<UserAccount> GetListUsers()
        {
            return GetUsers($@"SELECT a.id, a.username, a.password, b.cod, a.Deleted
                                FROM userid a
                                left join sprroles b on a.RoleId = b.id
                                ORDER BY username");
        }
        public UserAccount? GetByUserName(string userName)
        {
            return _users.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower());
        }
    }
}
