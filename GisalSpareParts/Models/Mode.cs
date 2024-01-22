
namespace GisalSpareParts.Models
{
    public enum ModeButton
    {
        Add,
        Copy,
        View,
        Edit,
        MarkDelRec,
        Delete,
        
    }
    public static class Mode
    {
        public static ModeButton Button { get; set; }
    }
}
