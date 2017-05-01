using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Data.SqlClient;
using Bmf.Shared.Annotations;


namespace Bmf.Shared.Data
{
    /// <summary>
    /// Utility to import data from a text file to a Sql-Server Database table.
    /// If you can not use integrated security please provide user name and password.
    /// Integrated security must then be set to false.
    /// </summary>
    public class BulkImport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkImport"/> class.
        /// Sets the following default Values:
        /// <code>
        /// FieldDelimiter = "\t";
        /// LineDelimiter = "\r\n";
        /// Encoding = Encoding.UTF8;
        /// IntegratedSecurity = true;
        /// </code>
        /// </summary>
        public BulkImport()
        {
            FieldDelimiter = "\t";
            LineDelimiter = "\r\n";
            Encoding = Encoding.UTF8;
            IntegratedSecurity = true;
            MaxLinesPerInsert = 1000;
            TruncateTable = true;
        }

        /// <summary>
        /// Gets or sets the field delimiter.
        /// Field delimiter in imported text file, default is "\t" (Tabulator)
        /// </summary>
        /// <value>
        /// The field delimiter.
        /// </value>
        public string FieldDelimiter { get; set; }
        /// <summary>
        /// Gets or sets the line delimiter.
        /// Line delimiter used in the text file, default is Windows-Style line endings "\r\n"
        /// </summary>
        /// <value>
        /// The line delimiter.
        /// </value>
        public string LineDelimiter { get; set; }
        /// <summary>
        /// Gets or sets the encoding of the import file. Default is set to UTF-8.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public Encoding Encoding { get; set; }
        /// <summary>
        /// Gets or sets the name of the user for the connection to the database server. This will be ignored, when integrated security is set to true.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }
        /// <summary>
        /// Gets or sets the password for the connection to the database server. This will be ignored, when integrated security is set to true.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [integrated security] is used for database connections.
        /// If set to false, user name and password must be provided.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [integrated security]; otherwise, <c>false</c>.
        /// </value>
        public bool IntegratedSecurity { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the target table should be truncated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [truncate table]; otherwise, <c>false</c>.
        /// </value>
        public bool TruncateTable { get; set; }
        /// <summary>
        /// Gets or sets the maximum lines per insert. This value will be used for bulk imports to the SQL-Server using the <see cref="SqlBulkCopy"/> feature.
        /// </summary>
        /// <value>
        /// The maximum lines per insert.
        /// </value>
        public int MaxLinesPerInsert { get; set; }
        
        private string _tablename;
        private string _servername;

        private string[] _fieldnames;
        private string _inFile;
        private DataTable _dataTable;
        private readonly Dictionary<int, int> _sourceTargetMapping = new Dictionary<int, int>();

        /// <summary>
        /// A little log, which can be used to check the import progress.
        /// </summary>
        public List<string> Log = new List<string>();

        /// <summary>
        /// Imports data from a file to an SqlServer table.
        /// </summary>
        /// <param name="inFile">The file where data is read from</param>
        /// <param name="serverName">The target SQL-Server including the instance if needed</param>
        /// <param name="tableName">Name of the target Database + Schema + Table name like MyDatabase.dbo.TargetTable</param>
        public void ImportFromFileToServer([NotNull] string inFile, [NotNull] string serverName, [NotNull] string tableName)
        {
            inFile.ThrowIfArgumentIsNullOrWhitespace(nameof(inFile));
            serverName.ThrowIfArgumentIsNullOrWhitespace(nameof(serverName));
            tableName.ThrowIfArgumentIsNullOrWhitespace(nameof(tableName));

            _inFile = inFile;
            _servername = serverName;
            _tablename = tableName;

            var csb = new SqlConnectionStringBuilder();
            csb.DataSource = _servername;
            if (IntegratedSecurity)
            {
                csb.IntegratedSecurity = true;
            }
            else
            {
                csb.UserID = UserName;
                csb.Password = Password;
            }

            ReadSchemaTable(csb);
            if (TruncateTable)
                DoTruncateTable(csb);

            var start = DateTime.Now;
            Log.Add($"Start Import {start}");

            var count = BulkInsertData(csb);

            var ende = DateTime.Now;
            Log.Add($"finished import {ende}");
            var performance = (int) (count/(ende - start).TotalSeconds);
            Log.Add($"Performance: {performance} Rows per Second");
        }
        private void DoTruncateTable(SqlConnectionStringBuilder csb)
        {
            Log.Add($"truncating target table {_tablename}");
            using (var con = new SqlConnection(csb.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("truncate table " + _tablename, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int BulkInsertData(DbConnectionStringBuilder csb)
        {
            var currentCount = 0;

            foreach (var line in System.IO.File.ReadLines(_inFile, Encoding))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (_fieldnames == null)
                {
                    _fieldnames = line.Split(new[] { FieldDelimiter }, StringSplitOptions.None);
                    //Check if every Source-Column is in target data table
                    for (var i = 0; i < _dataTable.Columns.Count; i++)
                    {
                        var fieldname = _fieldnames[i];
                        for (var j = 0; j < _dataTable.Columns.Count; j++)
                        {
                            var col = _dataTable.Columns[j];
                            if (col.ColumnName.Equals(fieldname, StringComparison.OrdinalIgnoreCase))
                            {
                                _sourceTargetMapping.Add(i, j);
                                break;
                            }
                        }
                        if (!_sourceTargetMapping.ContainsKey(i))
                            throw new Exception(string.Format("SourceColumn {0} not found in Target Table {1}", fieldname, _tablename));
                    }
                    continue;
                }

                var datas = line.Split(new[] { FieldDelimiter }, StringSplitOptions.None);

                var dataRow = _dataTable.NewRow();
                for (var i = 0; i < _fieldnames.Length; i++)
                {
                    var targetIndex = _sourceTargetMapping[i];
                    dataRow[targetIndex] = ConvertData(datas[i], _dataTable.Columns[targetIndex]);
                }
                _dataTable.Rows.Add(dataRow);

                currentCount++;
                if (currentCount % MaxLinesPerInsert != 0)
                    continue;

                Log.Add($"Line {currentCount}");
                WriteToDatabase(csb);
            }
            WriteToDatabase(csb);
            return currentCount;
        }

        private object ConvertData(string intputData, DataColumn dataCol)
        {
            var isEmpty = false;
            if (string.IsNullOrEmpty(intputData))
            {
                if (dataCol.AllowDBNull)
                    return DBNull.Value;
                isEmpty = true;
            }

            if (dataCol.DataType == typeof(Byte)) return isEmpty ? default(Byte) : Byte.Parse(intputData);
            if (dataCol.DataType == typeof(Int16)) return isEmpty ? default(Int16) : Int16.Parse(intputData);
            if (dataCol.DataType == typeof(Int32)) return isEmpty ? default(Int32) : Int32.Parse(intputData);
            if (dataCol.DataType == typeof(Boolean)) return isEmpty ? default(Boolean) : Boolean.Parse(intputData);
            if (dataCol.DataType == typeof(String)) return isEmpty ? "" : intputData;
            if (dataCol.DataType == typeof(DateTime)) return isEmpty ? default(DateTime) : DateTime.Parse(intputData);
            if (dataCol.DataType == typeof(Double)) return isEmpty ? default(Double) : Double.Parse(intputData);
            if (dataCol.DataType == typeof(Decimal)) return isEmpty ? default(Decimal) : Decimal.Parse(intputData);
            if (dataCol.DataType == typeof(Guid)) return isEmpty ? default(Guid) : Guid.Parse(intputData);

            throw new Exception("Could not Parse Type " + dataCol.DataType.FullName);
        }

        private void WriteToDatabase(DbConnectionStringBuilder csb)
        {
            using (var conn = new SqlConnection(csb.ConnectionString))
            {
                conn.Open();

                var bulkCopy = new SqlBulkCopy(conn,
                    SqlBulkCopyOptions.KeepIdentity |
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );
                bulkCopy.DestinationTableName = _tablename;
                bulkCopy.WriteToServer(_dataTable);
            }
            _dataTable.Clear();
        }

        private void ReadSchemaTable(DbConnectionStringBuilder csb)
        {
            _dataTable = new DataTable();
            using (var con = new SqlConnection(csb.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("select * from " + _tablename, con))
                {
                    using (var reader = cmd.ExecuteReader(CommandBehavior.KeyInfo))
                    {
                        var schemaTable = reader.GetSchemaTable();

                        if (schemaTable == null)
                            throw new Exception("Could not read Target Schema");

                        foreach (DataRow myField in schemaTable.Rows)
                        {
                            var col = _dataTable.Columns.Add(myField["ColumnName"].ToString(), (Type)myField["DataType"]);
                            col.AllowDBNull = (bool)myField["AllowDBNull"];
                        }
                    }
                }
            }
        }
    }
}
