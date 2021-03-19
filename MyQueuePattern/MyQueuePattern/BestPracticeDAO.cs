using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQueuePattern
{
    public class BestPracticeDAO
    {
        private NpgsqlParameter[] GetParametersFromDataHolder(object dataObject)
        {
            List<NpgsqlParameter> paraResult = new List<NpgsqlParameter>();
            var props_in_dataObject = dataObject.GetType().GetProperties();
            foreach (var prop in props_in_dataObject)
            {
                paraResult.Add(new NpgsqlParameter(prop.Name, prop.GetValue(dataObject)));
            }
            return paraResult.ToArray();
        }

        public List<Dictionary<string, object>> Run_sp(string sp_name, object dataHolder)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            NpgsqlConnection conn = null;

            NpgsqlParameter[] para = null;
            try
            {
                conn = PostgresConnectionPool.Instance.GetConnection();

                NpgsqlCommand command = new NpgsqlCommand(sp_name, conn);
                command.CommandType = System.Data.CommandType.StoredProcedure; // this is default

                para = GetParametersFromDataHolder(dataHolder);

                command.Parameters.AddRange(para);

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> one_row = new Dictionary<string, object>();
                    foreach (var item in reader.GetColumnSchema())
                    {
                        object one_column_value = reader[item.ColumnName];
                        one_row[item.ColumnName] = one_column_value;
                    }
                    result.Add(one_row);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                string params_string = "";
                foreach (var item in para)
                {
                    if (params_string != "")
                        params_string += ", ";
                    params_string += $"Name : {item.ParameterName} value: {item.Value}";
                }
                Console.WriteLine($"Function {sp_name} failed. parameters: {params_string}");
            }
            finally
            {
                PostgresConnectionPool.Instance.ReturnConnection(conn);
            }

            return result;
        }

        public List<T> Run_sp<T>(string sp_name, object dataHolder) where T : new()
        {
            List<T> result = new List<T>();
            NpgsqlParameter[] para = null;

            NpgsqlConnection conn = null;
            try
            {
                conn = PostgresConnectionPool.Instance.GetConnection();
                {
                    NpgsqlCommand command = new NpgsqlCommand(sp_name, conn);
                    command.CommandType = System.Data.CommandType.StoredProcedure; // this is default

                    para = GetParametersFromDataHolder(dataHolder);

                    command.Parameters.AddRange(para);

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        T one_row = new T();
                        Type type_of_t = typeof(T);
                        foreach (var prop in type_of_t.GetProperties())
                        {
                            string column_name = prop.Name;

                            var custom_attr_column_name =
                                (ColumnAttribute[])prop.GetCustomAttributes(typeof(ColumnAttribute), true);
                            if (custom_attr_column_name.Length > 0)
                                column_name = custom_attr_column_name[0].Name;

                            var value = reader[column_name];

                            prop.SetValue(one_row, value);
                        }

                        result.Add(one_row);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                string params_string = "";
                foreach (var item in para)
                {
                    if (params_string != "")
                        params_string += ", ";
                    params_string += $"Name : {item.ParameterName} value: {item.Value}";
                }
                Console.WriteLine($"Function {sp_name} failed. parameters: {params_string}");
            }
            finally
            {
                PostgresConnectionPool.Instance.ReturnConnection(conn);
            }

            return result;
        }
    }
}
