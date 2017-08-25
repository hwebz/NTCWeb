using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer.PlugIn;
using Niteco.ContentTypes.DynamicData;

namespace Niteco.Web.Plugins.NewLetters
{
    [GuiPlugIn(DisplayName = "Export Form News Letter", Description = "Export Form News Letter", Area = PlugInArea.AdminMenu, Url = "~/Plugins/NewLetters/NewLetterFromCareerPage.aspx")]
    public partial class NewLetterFromCareerPage : System.Web.UI.Page
    {
        private readonly string[] _newsletterFormColumn = { "Name","Email"};
        protected void Page_Load(object sender, EventArgs e)
        {
            DataTable dataTable = DataForNewsLetter();
            grdData.DataSource = dataTable;
            grdData.DataBind();
        }

        protected void btnExport_OnClick(object sender, EventArgs e)
        {
            DataTable dataTable = DataForNewsLetter();
            var fileName = "newletter-career-page-" + DateTime.Now.ToString("ddMMyyyy-HHmm") + ".xls";
            ExportToExcel(fileName, dataTable);
        }

        private DataTable DataForNewsLetter()
        {
            var dataTable = new DataTable();
            var CSV_ROW = "{0}*{1}";
            foreach (string column in _newsletterFormColumn)
            {
                var dcol = new DataColumn(column, typeof(String));
                dataTable.Columns.Add(dcol);
            }
            var contacts = StayUpToDateData.GetAll();
            foreach (var contact in contacts)
            {
                DataRow dr = dataTable.NewRow();
                int i = 0;

                try
                {
                    string data = string.Format(CSV_ROW, contact.Name,
                         contact.Email);
                    char[] delimiter = { '*' };
                    string[] rows = data.Split(delimiter);
                    while (i < _newsletterFormColumn.Count())
                    {
                        dr[_newsletterFormColumn[i]] = rows[i];
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    string what = "ever";
                }

                dataTable.Rows.Add(dr);
            }
            return dataTable;

        }

        private void ExportToExcel(string fileName, DataTable dtb)
        {
            Response.ContentType = "application/xls";
            Response.Charset = "";
            Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            Response.ContentEncoding = Encoding.Unicode;
            Response.BinaryWrite(Encoding.Unicode.GetPreamble());
            try
            {
                StringBuilder sb = new StringBuilder();
                //Tạo dòng tiêu để cho bảng tính
                for (int count = 0; count < dtb.Columns.Count; count++)
                {
                    if (dtb.Columns[count].ColumnName != null)
                        sb.Append(dtb.Columns[count].ColumnName);
                    if (count < dtb.Columns.Count - 1)
                    {
                        sb.Append("\t");
                    }
                }
                Response.Write(sb.ToString() + "\n");
                Response.Flush();
                //Duyệt từng bản ghi 
                int soDem = 0;
                while (dtb.Rows.Count >= soDem + 1)
                {
                    sb = new StringBuilder();

                    for (int col = 0; col < dtb.Columns.Count - 1; col++)
                    {
                        if (dtb.Rows[soDem][col] != null)
                            sb.Append(dtb.Rows[soDem][col].ToString().Replace(",", " "));
                        sb.Append("\t");
                    }
                    if (dtb.Rows[soDem][dtb.Columns.Count - 1] != null)
                        sb.Append(dtb.Rows[soDem][dtb.Columns.Count - 1].ToString().Replace(",", " "));

                    Response.Write(sb.ToString() + "\n");
                    Response.Flush();
                    soDem = soDem + 1;
                }

            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            dtb.Dispose();
            Response.End();
        }
    }
}