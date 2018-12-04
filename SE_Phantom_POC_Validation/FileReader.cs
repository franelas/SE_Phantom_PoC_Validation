/*************************************************************************************************************************************\
|                                                             LIBRARIES                                                               |
\*************************************************************************************************************************************/
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE_Phantom_PoC_Validation
{
    public static class FileReader
    {
        /*****************************************************************************************************************************\
        |                                                      PUBLIC FUNCTIONS                                                       |
        \*****************************************************************************************************************************/
        public static void ReadExcelFile()
        {
            //using OfficeOpenXml;
            using (ExcelPackage xlPackage = new ExcelPackage(new FileInfo(@"C:\test\test.xlsx")))
            {
                ExcelWorksheet myWorksheet = xlPackage.Workbook.Worksheets["Sheet2"]; //select sheet here
                int totalRows = myWorksheet.Dimension.End.Row;
                int totalColumns = myWorksheet.Dimension.End.Column;
                String s = myWorksheet.Cells[2, 2].Value.ToString();

                var sb = new StringBuilder(); //this is your your data
                for (int rowNum = 1; rowNum <= totalRows; rowNum++) //selet starting row here
                {
                    var row = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                    sb.AppendLine(string.Join(",", row));
                }
                int i = 0;
            }
        }

        public static DataTable ConvertExcelToDataTable(string FileName, bool hasHeader, String sheetName)
        {

            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(FileName))
                {
                    pck.Load(stream);
                }
                var ws = pck.Workbook.Worksheets[sheetName];
                DataTable tbl = new DataTable();    
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                  
                    tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                }
                var startRow = hasHeader ? 2 : 1;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    DataRow row = tbl.NewRow();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;

                    }
                    tbl.Rows.Add(row);
                }
                return tbl;
            }
        }

        public static bool WriteDataTableToExcel(DataTable dt, String path)
        {
            try
            {
                using (ExcelPackage pck = new ExcelPackage(new FileInfo(path)))
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Results");
                    ws.Cells["A1"].LoadFromDataTable(dt, true);
                    pck.Save();
                    return true;
                }
            }catch (Exception ex)
            {

                return false;
            }
        }

        public static void CopyRows (DataRow source, ref DataRow dest)
        {
            //It's assumed that the rows have the same structure
            for (int i=0; i< source.ItemArray.Length; i++)
            {
                dest[i] = source[i];
            }
        }

        public static Dictionary<String, String> FindCompany(String companyNumber, String FileName)
        {
            Dictionary<String, String> comapnyInfo = new Dictionary<string, string>();

            companyNumber = "'" + companyNumber.Replace("'", "").Trim() + "'";
            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                try
                {
                    //Load the file
                    using (var stream = File.OpenRead(FileName))
                    {
                        pck.Load(stream);
                    }
                    var ws = pck.Workbook.Worksheets.First();

                    var searchCell = from cell in ws.Cells[1, 1, ws.Dimension.End.Row, 1]
                                     where "'" + cell.Value.ToString().Replace("'", "").Trim() + "'" == companyNumber
                                     select cell.Start.Row;
                    int rowNum = searchCell.First();

                    String clientId = ws.Cells[rowNum, ws.Dimension.End.Column - 2].Value.ToString();  // Client Id is situated 2 columns before the ending of the file
                    String businessName = ws.Cells[rowNum, 8].Value.ToString();
                    comapnyInfo.Add(clientId, businessName);

                    return comapnyInfo;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }


    }
}
