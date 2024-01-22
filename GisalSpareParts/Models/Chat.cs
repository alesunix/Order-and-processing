namespace GisalSpareParts.Models
{
    public class Chat
    {
        public Int64 Id { get; set; }
        public Int64 Pkey { get; set; }
        public string Nam { get; set; }
        public DateTime Dt { get; set; }
        public string Sms { get; set; }
        public bool IsIncoming { get; set; }
    }
}
