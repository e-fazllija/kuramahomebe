using ClosedXML.Excel;
using System.Data;
using System.Reflection;
using System.Text;

namespace BackEnd.Services
{
    public static class Export
    {
        //convert generic list to datatable.
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {

                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static byte[] GenerateExcelContent(DataTable dati)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Dati");

                // Aggiungi intestazioni
                for (int colonna = 1; colonna <= dati.Columns.Count; colonna++)
                {
                    worksheet.Cell(1, colonna).Value = dati.Columns[colonna - 1].ColumnName;
                }

                for (int riga = 0; riga < dati.Rows.Count; riga++)
                {
                    for (int colonna = 0; colonna < dati.Columns.Count; colonna++)
                    {
                        object valoreCella = dati.Rows[riga][colonna];

                        // Controlla se il valore è DBNull
                        if (Convert.IsDBNull(valoreCella))
                        {
                            worksheet.Cell(riga + 2, colonna + 1).SetValue(string.Empty); // o un altro valore di default
                        }
                        else
                        {
                            // Utilizza il valore della cella
                            worksheet.Cell(riga + 2, colonna + 1).SetValue(valoreCella.ToString());
                        }
                    }
                }


                // Converte il workbook in un array di byte
                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] GenerateCsvContent(DataTable dati)
        {
            StringBuilder csvContent = new StringBuilder();

            // Aggiungi intestazioni CSV
            csvContent.AppendLine(string.Join(",", dati.Columns.Cast<DataColumn>().Select(col => QuoteCsvValue(col.ColumnName))));

            // Aggiungi righe CSV
            foreach (DataRow row in dati.Rows)
            {
                var values = row.ItemArray.Select(val => QuoteCsvValue(val.ToString()));
                csvContent.AppendLine(string.Join(",", values));
            }

            // Converte il contenuto CSV in un array di byte
            return Encoding.UTF8.GetBytes(csvContent.ToString());
        }

        private static string QuoteCsvValue(string value)
        {
            // Se il valore contiene virgole o doppi apici, racchiudi il valore tra doppi apici
            if (value.Contains(",") || value.Contains("\""))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
