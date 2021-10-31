using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Text;

namespace Application.Runtime
{
    public class SqlData
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        private SqliteConnection    m_SqlConnection;
        /// <summary>
        /// 数据库命令
        /// </summary>
        private SqliteCommand       m_SqlCommand;
        /// <summary>
        /// 数据库读取
        /// </summary>
        private SqliteDataReader    m_SqlDataReader;

        public SqlData(string databasePath)
        {
            try
            {
                m_SqlConnection = new SqliteConnection(GetDataPath(databasePath));
                m_SqlConnection.Open();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 关闭数据库
        /// </summary>
        public void Close()
        {
            if (m_SqlCommand != null)
            {
                m_SqlCommand.Dispose();
                m_SqlCommand = null;
            }

            if (m_SqlDataReader != null)
            {
                m_SqlDataReader.Close();
                m_SqlDataReader = null;
            }

            if (m_SqlConnection != null)
            {
                m_SqlConnection.Dispose();
                m_SqlConnection.Close();
                m_SqlConnection = null;
            }
        }

#pragma warning disable CS0162
        private string GetDataPath(string databasePath)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return "data source=" + databasePath;
#endif
#if UNITY_ANDROID
            return "URI=file:" + databasePath;
#endif
#if UNITY_IOS
            return "data source=" + databasePath;
#endif
        }
#pragma warning restore CS0162        

        private SqliteDataReader ExecuteQuery(string command)
        {
            m_SqlCommand = m_SqlConnection.CreateCommand();
            m_SqlCommand.CommandText = command;
            m_SqlDataReader = m_SqlCommand.ExecuteReader();
            return m_SqlDataReader;
        }

        public void ExecuteNonQuery(string command)
        {
            m_SqlCommand = m_SqlConnection.CreateCommand();
            m_SqlCommand.CommandText = command;
            m_SqlCommand.ExecuteNonQuery();
        }

        private int ExecuteScalar(string command)
        {
            m_SqlCommand = m_SqlConnection.CreateCommand();
            m_SqlCommand.CommandText = command;
            return System.Convert.ToInt32(m_SqlCommand.ExecuteScalar());
        }

        /// <summary>
        /// 表格是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool ExistTable(string tableName)
        {
            // SELECT COUNT(*) FROM sqlite_master;
            StringBuilder stringBuilder = StringUtil.GetShareStringBuilder();
            stringBuilder.Append("SELECT COUNT(*) FROM sqlite_master where type='table' and name='");
            stringBuilder.Append(tableName);
            stringBuilder.Append("';");
            return ExecuteScalar(stringBuilder.ToString()) > 0;
        }

        public int GetNumberOfRow(string tableName)
        {
            string querystring = "SELECT COUNT(*) FROM " + tableName;
            return ExecuteScalar(querystring);
        }

        /// <summary>
        /// 创建数据表
        /// </summary>
        /// <returns>The table.</returns>
        /// <param name="tableName">数据表名</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colTypes">字段名类型</param>
        public void CreateTable(string tableName, string[] colNames, string[] colTypes)
        {
            // CREATE TABLE table_name(column1 type1, column2 type2, column3 type3,...);
            // value type: NULL, INTEGER, REAL, TEXT, BLOB, NUMERIC, BOOLEAN
            if(colNames.Length != colTypes.Length)
                throw new System.Exception("colNames.length != colTypes.length");

            string queryString = "CREATE TABLE IF NOT EXISTS " + tableName + "( " + colNames[0] + " " + colTypes[0] + " PRIMARY KEY";
            for (int i = 1; i < colNames.Length; i++)
            {
                queryString += ", " + colNames[i] + " " + colTypes[i];
            }
            queryString += "  ) ";
            ExecuteNonQuery(queryString);
        }

        public void DeleteTable(string tableName)
        {
            string querystring = "DROP TABLE IF EXISTS " + tableName;
            ExecuteNonQuery(querystring);
        }

        /// <summary>
        /// 读取整张数据表
        /// </summary>
        /// <returns>The full table.</returns>
        /// <param name="tableName">数据表名称</param>
        public SqliteDataReader ReadFullTable(string tableName)
        {
            string queryString = "SELECT * FROM " + tableName;
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// Reads the table.
        /// </summary>
        /// <returns>The table.</returns>
        /// <param name="tableName">Table name.</param>
        /// <param name="items">Items.</param>
        /// <param name="colNames">Col names.</param>
        /// <param name="operations">Operations.</param>
        /// <param name="colValues">Col values.</param>
        public SqliteDataReader ReadTable(string tableName, string[] items, string[] colNames, string[] operations, string[] colValues)
        {
            string queryString = "SELECT " + items[0];
            for (int i = 1; i < items.Length; i++)
            {
                queryString += ", " + items[i];
            }
            queryString += " FROM " + tableName + " WHERE " + colNames[0] + " " + operations[0] + " " + colValues[0];
            for (int i = 0; i < colNames.Length; i++)
            {
                queryString += " AND " + colNames[i] + " " + operations[i] + " " + colValues[0] + " ";
            }
            return ExecuteQuery(queryString);
        }

        public SqliteDataReader ReadTable(string tableName, string colName, string op, string colValue)
        {
            string queryString = "SELECT * FROM " + tableName + " WHERE " + colName + " " + op + " " + colValue;
            return ExecuteQuery(queryString);
        }

        /// <summary>
        /// 向指定数据表中插入数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="values">插入的数值</param>
        public void InsertValues(string tableName, string[] values)
        {
            // INSERT INTO table_name(column1, column2, column3,...) VALUES(value1, value2, value3,...);

            string queryString = "INSERT INTO " + tableName + " VALUES (" + values[0];
            for (int i = 1; i < values.Length; i++)
            {
                queryString += ", " + values[i];
            }
            queryString += " )";
            try
            {
                ExecuteNonQuery(queryString);
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(string.Format($"{tableName}:  {queryString}"));
            }
        }

        public void InsertValues(string tableName, string[] colNames, string[] values)
        {
            // INSERT INTO table_name(column1, column2, column3,...) VALUES(value1, value2, value3,...);
            if(colNames.Length != values.Length)
            {
                throw new System.Exception("InsertValues: colNames.length != values.length");
            }

            string queryString = "INSERT INTO " + tableName + " (" + colNames[0];
            for(int i = 1; i < colNames.Length; ++i)
            {
                queryString += ", " + colNames[i];
            }
            queryString += " ) VALUES (" + values[0];
            for(int i = 1; i < values.Length; ++i)
            {
                queryString += ", " + values[i];
            }
            queryString += " )";

            try
            {
                ExecuteNonQuery(queryString);
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(string.Format($"{tableName}:  {queryString}"));
            }
        }

        /// <summary>
        /// 删除指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        public void DeleteValuesOR(string tableName, string[] colNames, string[] operations, string[] colValues)
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length)
            {
                throw new SqliteException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
            }

            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += "OR " + colNames[i] + operations[i] + colValues[i];
            }
            ExecuteNonQuery(queryString);
        }

        /// <summary>
        /// 删除指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        public void DeleteValuesAND(string tableName, string[] colNames, string[] operations, string[] colValues)
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length)
            {
                throw new SqliteException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
            }

            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += " AND " + colNames[i] + operations[i] + colValues[i];
            }
            ExecuteNonQuery(queryString);
        }

        /// <summary>
        /// 更新指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        /// <param name="key">关键字</param>
        /// <param name="value">关键字对应的值</param>
        public void UpdateValues(string tableName, string[] colNames, string[] colValues, string key, string operation, string value)
        {
            // UPDATE table_name SET column1 = value1, column2 = value2,... WHERE some_column = some_value;

            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length)
            {
                throw new SqliteException("colNames.Length!=colValues.Length");
            }

            string queryString = "UPDATE " + tableName + " SET " + colNames[0] + "=" + colValues[0];
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += ", " + colNames[i] + "=" + colValues[i];
            }
            queryString += " WHERE " + key + operation + value;
            ExecuteNonQuery(queryString);
        }
    }
}