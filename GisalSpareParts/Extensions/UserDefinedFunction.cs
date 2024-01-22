using System.Data.SQLite;

namespace GisalSpareParts.Extensions
{
    [SQLiteFunction(Name = "LOWER", FuncType = FunctionType.Scalar, Arguments = 1)]
    public class UserDefinedFunction : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            return args[0] != null ? ((string)args[0]).ToLower() : null;
        }
    }
}
