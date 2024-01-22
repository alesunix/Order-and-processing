

namespace GisalSpareParts.Models
{
    public class DivisionModel : BaseModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string Deleted { get; set; }
        public List<DivisionModel> GetDataSubdevision()
        {
            Name ??= string.Empty;
            Note ??= string.Empty;
            return GetListT<DivisionModel>($@"SELECT a.id, a.name, a.note, a.deleted
                                                FROM sprdivision a
                                                WHERE LOWER(a.name) LIKE '%{Name.ToLower()}%' AND LOWER(a.note) LIKE '%{Note.ToLower()}%'
                                                ORDER BY a.name");
        }
        private bool LoginDublicate()
        {
            int count = Convert.ToInt32(GetSingleResult($"SELECT count(*) FROM sprdivision WHERE Name = '{Name}'"));
            if (count > 0)
            {
                MyMessage = "Подразделение с таким именем уже существует в базе";
                return false;
            }
            return true;
        }
        public bool InsertOrUpdate()
        {
            if (Mode.Button == ModeButton.Add)
            {
                if (LoginDublicate())
                {
                    SetQuery($@"INSERT INTO sprdivision (Name, Note, Deleted) VALUES ('{Name}','{Note}','F')");
                    MyMessage = "Подразделение успешно создано";
                    return true;
                }
                else return false;/// Если Подразделение с таким именем существует
            }
            else if (Mode.Button == ModeButton.Edit)
            {
                SetQuery($@"UPDATE sprdivision SET Name = '{Name}', Note = '{Note}' WHERE Id = {Id}");
                MyMessage = "Подразделение успешно изменено";
                return true;
            }
            else return false;
        }
        public void DeleteOrRecovery()
        {
            if (Mode.Button == ModeButton.MarkDelRec)
            {
                SetQuery($"UPDATE sprdivision SET Deleted = '{Deleted}' WHERE Id = {Id}");
                MyMessage = Deleted == "T" ? "Подразделение успешно удалено" : "Подразделение успешно восстановлено";
            }
            else if (Mode.Button == ModeButton.Delete)
            {
                SetQuery($"DELETE FROM sprdivision WHERE Id = {Id}");
                MyMessage = "Подразделение успешно удалено из базы";
            }
        }
    }
}
