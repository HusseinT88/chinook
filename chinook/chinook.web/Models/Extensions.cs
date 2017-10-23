using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using Npgsql;

namespace chinook.web.Models
{
    public static class Extensions
    {
        public static dynamic ToSingle(this IDataReader reader)
        {
            var e = new ExpandoObject();
            var d = e as IDictionary<string, object>;
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var textInfo = new CultureInfo("en-US",false).TextInfo;
                var replacedName = reader.GetName(i).Replace("_", " ").ToLower();
                var name = textInfo.ToTitleCase(replacedName).Replace(" ",string.Empty);
                d.Add(name,DBNull.Value.Equals(reader.GetValue(i)) ? null : reader.GetValue(i));
            }
            return e;
        }
        public static T ToSingle<T>(this IDataReader reader) where T : new()
        {
            var item = new T();
            var properties = item.GetType().GetProperties();
            foreach (var property in properties)
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    if (property.Name.Equals(reader.GetName(i),StringComparison.CurrentCultureIgnoreCase))
                    {
                        property.SetValue(item,DBNull.Value.Equals(reader.GetValue(i)) ? null : reader.GetValue(i));
                    }
                }
            }
            return item;
        }
        public static void AddParameter(this NpgsqlCommand command, object arg)
        {
            var parameter = new NpgsqlParameter{ ParameterName = string.Format("@{0}",command.Parameters.Count)};
            if (arg == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                switch (arg)
                {
                    case string _:
                        parameter.Value = arg.ToString();
                        parameter.Size = arg.ToString().Length > 4000 ? -1 : 4000;
                        break;
                    case Guid _:
                        parameter.DbType = DbType.Guid;
                        parameter.Value = arg.ToString();
                        parameter.Size = 4000;
                        break;
                    default:
                        parameter.Value = arg;
                        break;
                }
            }
            command.Parameters.Add(parameter);
        }
        public static IEnumerable<dynamic> DynamicList(this IDataReader reader)
        {
            while (reader.Read())
            {
                yield return reader.ToSingle();
            }
        }
        public static IEnumerable<T> ToList<T>(this IDataReader reader) where T : new()
        {
            while (reader.Read())
            {
                yield return reader.ToSingle<T>();
            }
        }
    }
}