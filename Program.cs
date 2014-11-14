using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace dbdump {
    class Program {
        static SqlConnection Connection = null;
        static string Destination = "";
        static string Destination2 = "";

        static void Main(string[] args) {
            if (args == null || args.Length < 2) {
                Console.Write("usage: dbdump database destination destination2\n"
                    + "  database:     The name of the database to export.\n"
                    + "  destination:  The name of the folder to export all of the files to on the sql server machine.\n"
                    + "  destination2: The name of the folder that you'll copy the export to on the mysql machine.");
                return;
            }

            string dbname = args[0];
            Destination = args[1].Replace('\\', '/').TrimEnd('/');
            if (args.Length > 2)
                Destination2 = args[2].Replace('\\', '/').TrimEnd('/');
            else
                Destination2 = Destination;

            SqlConnection connection = null;

            try {
                string cstr = System.Configuration.ConfigurationManager.ConnectionStrings["default"].ConnectionString;
                //Console.WriteLine(cstr);
                //return;

                connection = new SqlConnection(cstr);
                connection.Open();
                connection.ChangeDatabase(dbname);

                Connection = connection;


                ExportTableDefs();
                ExportTables(dbname);
            } finally {
                if (connection != null)
                    connection.Close();
            }
        }

        static void ExportTables(string dbName) {
            string commandFormat = "[{0}].[{1}].[{2}] out " + Destination + "\\{2}.txt -c -C RAW -T -t\"|||COL|||\" -r\"|||ROW|||\" -S{3}";

            String sql = String.Format(@"select * from INFORMATION_SCHEMA.TABLES
				where TABLE_TYPE = 'BASE TABLE'
					and TABLE_CATALOG = '{0}'", dbName);
            DataTable table = Execute(sql);

            foreach (DataRow row in table.Rows) {
                string tableName = row["TABLE_NAME"].ToString();
                string schema = row["TABLE_SCHEMA"].ToString();

                string command = String.Format(commandFormat, dbName, schema, tableName, Connection.DataSource);
                Console.WriteLine("bcp " + command);

                Process.Start("bcp.exe", command);
            }
        }

        static void ExportTableDefs() {
            String sql = "select * from sysobjects where xtype = 'U' order by name";
            DataTable table = Execute(sql);

            if (!Directory.Exists(Destination))
                Directory.CreateDirectory(Destination);

            FileStream fs = new FileStream(Destination + "/import.sql", FileMode.Create);
            TextWriter tw = new StreamWriter(fs);

            foreach (DataRow row in table.Rows) {
                StringBuilder str = new StringBuilder();
                string tableName = row["name"].ToString();

                // Grab the columns for the table.
                string colSql = String.Format(@"select c.*, t.name as typename
					from syscolumns c
					join systypes t
						on c.xtype = t.xtype and c.xusertype = t.xusertype
					where id = {0}
					order by colid", row["id"]);

                DataTable colTable = Execute(colSql);
                foreach (DataRow colRow in colTable.Rows) {
                    if (str.Length > 0)
                        str.Append(",\n");

                    str.AppendFormat("`{0}` {1}", colRow["name"], ColumnType(colRow));
                }

                // Build the final string.
                String createSql = String.Format("create table `{0}` (\n{1});\n", tableName, str);
                tw.WriteLine(createSql);
                string loadSql = LoadDataSql(tableName);
                tw.WriteLine(loadSql);
            }
            tw.Flush();
            tw.Close();
        }

        static string ColumnType(DataRow row) {
            //if (row["name"].ToString() == "Body") {
            //    int foo = 123;
            //}

            string type = row["typename"].ToString().ToLower();
            if (type == "bigint")
                return "bigint";
            if (type == "binary")
                if (Convert.ToInt32(row["length"]) > 255)
                    return "blob";
                else
                    return String.Format("binary({0})", row["length"]);
            if (type == "bit")
                return "tinyint";
            if (type == "char")
                return String.Format("char({0})", row["length"]);
            if (type == "datetime" || type == "smalldatetime")
                return "datetime";
            if (type == "decimal")
                return "double";
            if (type == "float")
                return "float";
            if (type == "image")
                return "mediumtext";
            if (type == "int")
                return "int";
            if (type == "nchar")
                return String.Format("char({0})", row["length"]);
            //if (type == "nvarchar")
            //        return String.Format("varchar({0})", row["length"]);
            if (type == "text" || type == "ntext")
                return "text";
            if (type == "real")
                return "double";
            if (type == "smallint")
                return "smallint";
            if (type == "sql_variant")
                return "text";
            if (type == "sysname")
                return "varchar(255)";
            if (type == "text")
                return "text";
            if (type == "timestamp")
                return "int";
            if (type == "tinyint")
                return "tinyint";
            if (type == "varbinary") {
                if (row["length"].ToString() == "-1")
                    return "blob";
                return String.Format("varbinary({0})", row["length"]);
            }
            if (type == "varchar" || type == "nvarchar") {
                if (row["length"].ToString() == "-1")
                    return "text";
                return String.Format("varchar({0})", row["length"]);
            }

            Console.WriteLine("Unrecognized type {0}.", type);

            return "varchar(255)";
        }

        static DataTable Execute(SqlCommand command) {
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        static DataTable Execute(string sql) {
            SqlCommand command = new SqlCommand(sql, Connection);
            return Execute(command);
        }

        static string LoadDataSql(string tableName) {
            string sql = String.Format(@"load data infile '{0}/{1}.txt' into table `{1}`
				character set utf8
				columns terminated by '|||COL|||'
				lines terminated by '|||ROW|||'
				ignore 0 lines;", Destination2, tableName);

            return sql;
        }
    }
}