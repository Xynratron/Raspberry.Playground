using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.IO;

namespace Bmf.Shared.Data
{
    /// <summary>
    /// Utility to export data from a SQL-Server Database table to a text file.
    /// If you can not use integrated security please provide user name and password. 
    /// Integrated security must then be set to false.
    /// An instance of this class is not guaranteed to be thread save.
    /// </summary>
    public class BulkExport
    {
        /// <summary>
        /// Default constructor. Sets some values to defaults.
        /// <code>
        /// FieldDelimiter = "\t";
        /// LineDelimiter = "\r\n";
        /// Encoding = Encoding.UTF8;
        /// IntegratedSecurity = true;
        /// </code>
        /// </summary>
        public BulkExport()
        {
            FieldDelimiter = "\t";
            LineDelimiter = "\r\n";
            Encoding = Encoding.UTF8;
            IntegratedSecurity = true;
        }
        /// <summary>
        /// Field delimiter in target text file, default is "\t" (Tabulator)
        /// </summary>
        public string FieldDelimiter { get; set; }
        /// <summary>
        /// Line delimiter used in text file, default is Windows-Style line endings "\r\n"
        /// </summary>
        public string LineDelimiter { get; set; }
        /// <summary>
        /// Encoding of the text file, default is UTF-8.
        /// </summary>
        public Encoding Encoding { get; set; }
        /// <summary>
        /// User name for connection to the database server. This will be ignored, when integrated security is set to true.
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// Password for connection to the database server. This will be ignored, when integrated security is set to true.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// When set to true, integrated security is used for connections to the database server. If set to false, 
        /// user name and password must be set.
        /// </summary>
        public bool IntegratedSecurity { get; set; }

        private string _outFile = "";
        private int _lineCount;
        private int _lineCounterStart;
        private bool _splitLines;
        private string _tablename;
        private string _servername;

        /// <summary>
        /// Export data from a given table on a Ms SQL-Database server <paramref name="serverName"/> to the provided
        /// text file.
        /// </summary>
        /// <param name="serverName">Name of the server and instance where the database is located</param>
        /// <param name="databaseAndTableName">this must be the full database, schema and table name of the SQL-Server table you want to export.Example: MyBigDatabase.dbo.Data_Table <example>MyBigDatabase.dbo.Data_Table</example>
        /// </param>
        /// <param name="outFile">The file name including directory where the text file should be written. If <paramref name="splitLines"/>
        /// is set to true, you can specify a format for the file, to have several file with a counter: C:\Temp\MyFile{0:00}.tsv
        /// </param>
        /// <param name="splitLines">Optional splits the file after <paramref name="lineCount"/> lines, default is false</param>
        /// <param name="lineCount">Number of lines in each file, if <paramref name="splitLines"/> is set to true.</param>
        /// <param name="lineCounterStart">Optional not start at 0 for the naming of <paramref name="outFile"/>, so you can have MyFile101.tsv for the first file if you set this to 101</param>
        public void ExportFromServerToFile(string serverName, string databaseAndTableName, string outFile, bool splitLines = false,
            int lineCount = 0, int lineCounterStart = 0)
        {
            if (string.IsNullOrWhiteSpace(outFile))
                throw new ArgumentException(nameof(outFile));

            outFile.ThrowIfArgumentIsNullOrWhitespace(nameof(outFile));
            serverName.ThrowIfArgumentIsNullOrWhitespace(nameof(serverName));
            databaseAndTableName.ThrowIfArgumentIsNullOrWhitespace(nameof(databaseAndTableName));

            _servername = serverName;
            _tablename = databaseAndTableName;
            _outFile = outFile;

            _splitLines = splitLines;
            _lineCount = lineCount;
            _lineCounterStart = lineCounterStart;

            if (!Directory.Exists(Path.GetDirectoryName(_outFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(_outFile));

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

            var sb = new StringBuilder();

            var currentLineCount = 0;
            using (var con = new SqlConnection(csb.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("select * from " + _tablename, con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            sb.Append(reader.GetName(i));
                            sb.Append(FieldDelimiter);
                        }
                        sb.Remove(sb.Length - 1, 1);
                        sb.Append(LineDelimiter);
                        Write(sb, ref currentLineCount);
                        sb = new StringBuilder();

                        int RowCounter = 0;

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    if (reader.IsDBNull(i))
                                    {
                                        sb.Append("");
                                    }
                                    else
                                    {
                                        var data = reader.GetValue(i) ?? "";
                                        var dataWithoutLimiter = data.ToString().Replace(LineDelimiter, "");
                                        sb.Append(dataWithoutLimiter);
                                    }
                                    sb.Append(FieldDelimiter);
                                }
                                sb.Remove(sb.Length - 1, 1);
                                sb.Append(LineDelimiter);
                                RowCounter++;
                                currentLineCount++;
                                if (RowCounter > 10000)
                                {
                                    RowCounter = 0;
                                    Write(sb, ref currentLineCount);
                                    sb = new StringBuilder();
                                }
                            }
                            //letzte Zeile
                            sb.Remove(sb.Length - 2, 2);
                            int writeAll = 0;
                            Write(sb, ref writeAll);
                        }
                    }
                }
            }
        }

        private void Write(StringBuilder sb, ref int currentLineCount)
        {
            if (_splitLines)
            {
                System.IO.File.AppendAllText(string.Format(_outFile, _lineCounterStart), sb.ToString(), Encoding);
                if (currentLineCount > _lineCount) //lineCounterStart > 0 && lineCount > 0))
                {
                    currentLineCount -= _lineCount;
                    _lineCounterStart++;
                }
            }
            else
            {
                System.IO.File.AppendAllText(_outFile, sb.ToString(), Encoding);
            }
        }

        private static void ShowArgs()
        {
            Console.WriteLine("Arguments: Server-name TableName OutFile Split-lines(true/false) lineCountToSplit FileNameIntToAddAfterNumOf");
            Console.WriteLine("eg: 192.168.10.20 prorad_v11.dbo.reifen c:\\temp\\MyFile.tsv");
            Console.WriteLine("eg: 192.168.10.20 prorad_v11.dbo.reifen c:\\temp\\MyFile{0:00}.tsv true 500000 7 -> c:\\temp\\MyFile07.tsv");
        }
    }
}
