using System.Collections.Generic;
using System.IO;

namespace UCS.Files.CSV
{
    internal class CSVTable
    {
        public CSVTable(string filePath)
        {
            m_vCSVRows = new List<CSVRow>();
            m_vColumnHeaders = new List<string>();
            m_vColumnTypes = new List<string>();
            m_vCSVColumns = new List<CSVColumn>();

            using (StreamReader sr = new StreamReader(filePath))
            {
                string[] columns = sr.ReadLine()?.Replace("\"", "").Replace(" ", "").Split(',');
                if (columns != null)
                    foreach (string column in columns)
                    {
                        m_vColumnHeaders.Add(column);
                        m_vCSVColumns.Add(new CSVColumn());
                    }

                string[] types = sr.ReadLine()?.Replace("\"", "").Split(',');
                if (types != null)
                    foreach (string type in types)
                        m_vColumnTypes.Add(type);

                while (!sr.EndOfStream)
                {
                    string[] values = sr.ReadLine()?.Replace("\"", "").Split(',');

                    if (values != null && values[0] != string.Empty)
                        CreateRow();

                    for (int i = 0; i < m_vColumnHeaders.Count; i++)
                        if (values != null) m_vCSVColumns[i].Add(values[i]);
                }
            }
        }

        readonly List<string> m_vColumnHeaders;
        readonly List<string> m_vColumnTypes;
        readonly List<CSVColumn> m_vCSVColumns;
        readonly List<CSVRow> m_vCSVRows;

        public void AddRow(CSVRow row)
        {
            m_vCSVRows.Add(row);
        }

        private void CreateRow()
        {
            new CSVRow(this);
        }

        public int GetArraySizeAt(CSVRow row, int columnIndex)
        {
            var rowIndex = m_vCSVRows.IndexOf(row);
            if (rowIndex == -1)
                return 0;
            var c = m_vCSVColumns[columnIndex];
            var nextOffset = 0;
            if (rowIndex + 1 >= m_vCSVRows.Count)
            {
                nextOffset = c.GetSize();
            }
            else
            {
                var nextRow = m_vCSVRows[rowIndex + 1];
                nextOffset = nextRow.GetRowOffset();
            }
            var currentOffset = row.GetRowOffset();
            return CSVColumn.GetArraySize(currentOffset, nextOffset);
        }

        public int GetColumnIndexByName(string name) => m_vColumnHeaders.IndexOf(name);

        public string GetColumnName(int index) => m_vColumnHeaders[index];

        public int GetColumnRowCount()
        {
            var result = 0;
            if (m_vCSVColumns.Count > 0)
                result = m_vCSVColumns[0].GetSize();
            return result;
        }

        public CSVRow GetRowAt(int index) => m_vCSVRows[index];

        public int GetRowCount() => m_vCSVRows.Count;

        public string GetValue(string name, int level)
        {
            var index = m_vColumnHeaders.IndexOf(name);
            return GetValueAt(index, level);
        }

        public string GetValueAt(int column, int row) => m_vCSVColumns[column].Get(row);
    }
}
