using System.Data;
using Dapper;

namespace Helpers
{
    public class SqliteBooleanHandler : SqlMapper.TypeHandler<bool>
    {
        public override void SetValue(IDbDataParameter parameter, bool value)
        {
            parameter.Value = value ? 1 : 0;
        }

        public override bool Parse(object value)
        {
            if (value == null || value is DBNull)
                return false;
            return Convert.ToInt32(value) == 1;
        }
    }
}
