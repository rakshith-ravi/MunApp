using System;
using System.IO;
using System.Data;
using System.Data.OleDb;
using MetroFramework.Forms;
using System.Windows.Forms;
using System.Collections.Generic;
//using Excel = Microsoft.Office.Interop.Excel;

namespace MunApp.Win32
{
    public partial class ExcelImporter : MetroForm
    {
        private string fileName;

        public ExcelImporter(string fileName)
        {
            this.fileName = fileName;
            InitializeComponent();
            //InitializeDataGrid();
        }

        public string[] SelectedCountries { get; set; }

        /*
        private void InitializeDataGrid()
        {
            Excel.Application application = new Excel.Application();
            // See if the Excel Application Object was successfully constructed
            if (application == null)
            {
                MessageBox.Show("The Excel file could not be opened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else
            {
                // Here is the call to Open a Workbook in Excel 
                // It uses most of the default values (except for the read-only which we set to true)
                Excel.Workbook theWorkbook = application.Workbooks.Open(fileName, 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true);
                // get the collection of sheets in the workbook
                Excel.Sheets sheets = theWorkbook.Worksheets;
                // get the first and only worksheet from the collection of worksheets
                Excel.Worksheet worksheet = (Excel.Worksheet)sheets.get_Item(1);
                Excel.Range range = worksheet.UsedRange;
                DataTable table = new DataTable();
                for (int i = 0; i < range.Columns.Count; i++)
                {
                    Excel.Range column = (Excel.Range)range.Columns[i + 1];
                    table.Columns.Add("Column " + i);
                }
                foreach (Excel.Range row in range.Rows)
                {
                    List<object> rowList = new List<object>();
                    foreach(Excel.Range column in row.Columns)
                    {
                        Excel.Range cell = (Excel.Range)column.Cells[1, 1];
                        if (cell.Value2 != null)
                            rowList.Add(cell.Value2);
                        else
                            rowList.Add("");
                    }
                    DataRow dr = table.NewRow();
                    dr.ItemArray = rowList.ToArray();
                    table.Rows.Add(dr);

                    try
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                        worksheet = null;
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(theWorkbook);
                        theWorkbook = null;
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(application);
                        application = null;
                    }
                    catch (Exception ex)
                    {
                        worksheet = null;
                        theWorkbook = null;
                        application = null;
                        Common.Debug.Log("Unable to release the Object " + ex.ToString());
                    }
                    finally
                    {
                        GC.Collect();
                    }
                }
                dataGridView1.DataSource = table;
            }
        }
        */

        private void metroButton1_Click(object sender, EventArgs e)
        {
            //ok clicked
            List<string> items = new List<string>();
            DialogResult = DialogResult.OK;
            foreach (DataGridViewCell item in dataGridView1.SelectedCells)
            {
                items.Add(item.Value.ToString());
            }
            SelectedCountries = items.ToArray();
            Close();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            //cancel clicked
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
