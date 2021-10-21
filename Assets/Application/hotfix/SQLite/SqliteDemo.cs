using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace SQLite
{
    public class SqliteDemo : MonoBehaviour
    {
        /// <summary>
        /// SQLite数据库辅助类
        /// </summary>
        private SqlData sql;

        void Start()
        {
            Test2();
        }

        private void Test1()
        {
            //创建名为sqlite4unity的数据库
            sql = new SqlData("Deployment/sqlite4unity.db");

            //创建名为table1的数据表
            sql.DeleteTable("table1");
            sql.CreateTable("table1", new string[] { "ID", "Name", "Age", "Email", "Male", "ABC" }, 
                                      new string[] { "INTEGER", "TEXT", "REAL", "TEXT", "BOOLEAN", "NUMERIC" });

            Debug.Log(sql.ExistTable("table1"));        // tableName大小写敏感

            //插入两条数据
            sql.InsertValues("table1", new string[] { "'1'", "'张三'", "'22'", "'Zhang3@163.com'", "FALSE", "'0.321'" });
            sql.InsertValues("table1", new string[] { "'2'", "'李四'", "25", "'Li4@163.com'", "3", "23.56" });
            // sql.InsertValues("table1", new string[] {"NAme", "age"}, new string[] {"'王二'", "99"});
            
            // 更新数据，将Name="张三"的记录中的Name改为"Zhang3"
            sql.UpdateValues("table1", new string[] { "Name" }, new string[] { "'Zhang3'" }, "Name", "=", "'张三'");
            

            // 插入3条数据
            sql.InsertValues("table1", new string[] { "3", "'王五'", "25", "'Wang5@163.com'", "TRUE", "3" });
            sql.InsertValues("table1", new string[] { "4", "'王五'", "26", "'Wang5@163.com'", "false", "4" });
            sql.InsertValues("table1", new string[] { "5", "'王五'", "27", "'Wang5@163.com'", "True", "5" });

            // 删除Name="王五"且Age=26的记录,DeleteValuesOR方法类似
            sql.DeleteValuesAND("table1", new string[] { "Name", "Age" }, new string[] { "=", "=" }, new string[] { "'王五'", "'26'" });

            //读取整张表
            SqliteDataReader reader = sql.ReadFullTable("table1");
            // while (reader.Read())
            // {
            //     Debug.Log(reader.GetInt32(reader.GetOrdinal("ID")));
            //     Debug.Log(reader.GetString(reader.GetOrdinal("Name")));
            //     Debug.Log(reader.GetFloat(reader.GetOrdinal("Age")));
            //     Debug.Log(reader.GetString(reader.GetOrdinal("Email")));
            //     Debug.Log(reader.GetBoolean(reader.GetOrdinal("Male")));
            //     Debug.Log(reader.GetDouble(reader.GetOrdinal("ABC")));
            // }

            //读取数据表中Age>=25的所有记录的ID和Name
            reader = sql.ReadTable("table1", new string[] { "ID", "Name" }, new string[] { "Age" }, new string[] { ">=" }, new string[] { "'25'" });
            while (reader.Read())
            {
                Debug.Log(reader.GetInt32(reader.GetOrdinal("ID")));
                Debug.Log(reader.GetString(reader.GetOrdinal("Name")));
            }

            reader = sql.ReadTable("table1", "Age", ">=", "26");
            while (reader.Read())
            {
                Debug.Log(reader.GetInt32(reader.GetOrdinal("ID")));
                Debug.Log(reader.GetString(reader.GetOrdinal("Name")));
                Debug.Log(reader.GetFloat(reader.GetOrdinal("Age")));
                Debug.Log(reader.GetString(reader.GetOrdinal("Email")));
                Debug.Log(reader.GetBoolean(reader.GetOrdinal("Male")));
                Debug.Log(reader.GetDouble(reader.GetOrdinal("ABC")));
            }

            //关闭数据库连接
            sql.Close();
        }

        private void Test2()
        {
            //创建名为sqlite4unity的数据库
            sql = new SqlData("Deployment/sqlite4unity.db");

            sql.DeleteTable("people");
            sql.DeleteTable("addresses");
            sql.DeleteTable("addresses2");

            string querystring = @"CREATE TABLE IF NOT EXISTS people (
   person_id INTEGER PRIMARY KEY,   
   first_name text,
   last_name TEXT,
   address_id INTEGER,
   address_id2 INTEGER,
   FOREIGN KEY (address_id) REFERENCES addresses (id),
   FOREIGN KEY (address_id2) REFERENCES addresses2 (id)
);

CREATE TABLE IF NOT EXISTS addresses (
   id1 INTEGER PRIMARY KEY,
   house_no TEXT,
   street TEXT,
   city TEXT,
   postal_code TEXT,
   country TEXT
);

CREATE TABLE IF NOT EXISTS addresses2 (
   address_id INTEGER PRIMARY KEY,
   house_no TEXT,
   street TEXT,
   city TEXT,
   postal_code TEXT,
   country TEXT
);
";
            

            sql.ExecuteNonQuery(querystring);

            sql.InsertValues("people", new string[] { "'1'", "'张'", "'三'", "1", "2" });
            sql.InsertValues("addresses", new string[] { "'1'", "'aaa'", "'bbb'", "'shanghai'", "'001'", "'China'"});
            sql.InsertValues("addresses", new string[] { "'2'", "'aaa'", "'bbb'", "'shanghai'", "'001'", "'China'"});
            sql.InsertValues("addresses2", new string[] { "'1'", "'aaa'", "'bbb'", "'shanghai'", "'001'", "'China'"});
            sql.InsertValues("addresses2", new string[] { "'2'", "'aaa'", "'bbb'", "'shanghai'", "'001'", "'China'"});

            Debug.Log(sql.GetNumberOfRow("addresses"));

            SqliteDataReader reader = sql.ReadFullTable("people");
            Debug.Log("------------: " + reader.Depth);
            while (reader.Read())
            {
                Debug.Log(reader.GetInt32(reader.GetOrdinal("person_id")));
                Debug.Log(reader.GetString(reader.GetOrdinal("first_name")));
                Debug.Log(reader.GetString(reader.GetOrdinal("last_name")));
                Debug.Log(reader.GetInt32(reader.GetOrdinal("address_id")));
                Debug.Log(reader.GetInt32(reader.GetOrdinal("address_id2")));
            }

            sql.Close();
        }
    }
}