using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Text;

namespace SQLite
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
                m_SqlCommand = m_SqlConnection.CreateCommand();
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
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

        private SqliteDataReader ExecuteReader(string command)
        {
            m_SqlCommand.CommandText = command;
            m_SqlDataReader = m_SqlCommand.ExecuteReader();
            return m_SqlDataReader;
        }

        private void ExecuteNonQuery(string command)
        {
            m_SqlCommand.CommandText = command;
            m_SqlCommand.ExecuteNonQuery();
        }

        private bool ExecuteScalar(string command)
        {
            m_SqlCommand.CommandText = command;
            int result = System.Convert.ToInt32(m_SqlCommand.ExecuteScalar());
            return (result > 0);
        }

        /// <summary>
        /// 创建数据表
        /// </summary> +
        /// <returns>The table.</returns>
        /// <param name="tableName">数据表名</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colTypes">字段名类型</param>
        public SqliteDataReader CreateTable(string tableName, string[] colNames, string[] colTypes)
        {
            if(colNames.Length != colTypes.Length)
                throw new System.Exception("colNames.length != colTypes.length");

            string queryString = "CREATE TABLE " + tableName + "( " + colNames[0] + " " + colTypes[0];
            for (int i = 1; i < colNames.Length; i++)
            {
                queryString += ", " + colNames[i] + " " + colTypes[i];
            }
            queryString += "  ) ";
            return ExecuteReader(queryString);
        }

        /// <summary>
        /// 读取整张数据表
        /// </summary>
        /// <returns>The full table.</returns>
        /// <param name="tableName">数据表名称</param>
        public SqliteDataReader ReadFullTable(string tableName)
        {
            string queryString = "SELECT * FROM " + tableName;
            return ExecuteReader(queryString);
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
            return ExecuteReader(queryString);
        }

        public SqliteDataReader ReadTable(string tableName, string colName, string op, string colValue)
        {
            string queryString = "SELECT * FROM " + tableName + " WHERE " + colName + " " + op + " " + colValue;
            return ExecuteReader(queryString);
        }
    }
}