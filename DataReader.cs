using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace Data.Util.Extensions
{
    public static class DataReaderExtension
    {
        /// <summary>
        /// Converte para uma lista tipada do tipo passado em comparação com o IDataReader.
        /// </summary>
        /// <note>
        /// Essa extensão não esta 100% completa... não conseguir tempo para pensar nos casos de Propriedade que é classe, 
        /// relacionamento de 1 para 1 (1:1) ou 1 para muitos (1:n). Ex.: Model.Aluno tem a classe Pessoa como atributo.
        /// </note>
        /// <author>Charles Mendes de Macedo</author>
        /// <typeparam name="T">Tipo para retorna</typeparam>
        /// <param name="dr">IDataReader com os dados</param>
        /// <returns>Retorna a lista tipada</returns>
        public static List<T> ToList<T>(this IDataReader dr)
        {
            List<T> lista = new List<T>();
            Type tipo = typeof(T);
            List<PropertyInfo> propriedades = tipo.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            while (dr.Read())
            {
                object entidade = Assembly.GetAssembly(tipo).CreateInstance(tipo.FullName);

                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string nomeCampo = dr.GetName(i);

                    //PropertyInfo campo = entidade.GetType().GetProperty(nomeCampo.ToLower());
                    PropertyInfo campo = propriedades.FirstOrDefault(x => x.Name.ToLower().Equals(nomeCampo.ToLower()));

                    if (campo != null)
                    {
                        //Type tipoCampo = campo.PropertyType;
                        //object valor = dr.GetDataValue < get(tipoCampo) > (nomeCampo);
                        object valor = dr[nomeCampo];

                        if (dr[nomeCampo] == DBNull.Value)
                        {
                            valor = null;
                        }

                        if (campo.PropertyType.BaseType != null && campo.PropertyType.BaseType == typeof(Enum))
                        {
                            if (valor.ToString().Trim().Equals(string.Empty))
                            {
                                valor = null;
                            }

                            Type tipoEnum = campo.PropertyType;
                            try { campo.SetValue(entidade, Enum.Parse(tipoEnum, valor.ToString()), null); }
                            catch (Exception) { }
                        }
                        else if (campo.PropertyType == typeof(char))
                        {
                            campo.SetValue(entidade, Convert.ToChar(valor), null);
                        }
                        else if (campo.PropertyType == typeof(int?))
                        {
                            campo.SetValue(entidade, (int?)valor, null);
                        }
                        else if (campo.PropertyType == typeof(decimal?))
                        {
                            campo.SetValue(entidade, (decimal?)valor, null);
                        }
                        else if (campo.PropertyType == typeof(Boolean))
                        {
                            campo.SetValue(entidade, Convert.ToBoolean(valor), null);
                        }
                        else
                        {
                            campo.SetValue(entidade, valor, null);
                        }
                    }
                }

                lista.Add((T)entidade);
            }

            return lista;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static T GetDataValue<T>(this IDataReader dr, string columnName)
        {
            // NOTE: GetOrdinal() is used to automatically determine where the column
            //       is physically located in the database table. This allows the
            //       schema to be changed without affecting this piece of code.
            //       This of course sacrifices a little performance for maintainability.
            int i = dr.GetOrdinal(columnName);

            if (!dr.IsDBNull(i))
                return (T)dr.GetValue(i);
            else
                return default(T);
        }
    }
}
