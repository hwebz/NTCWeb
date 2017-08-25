using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;
using Niteco.ContentTypes.DynamicData;
using EPiServer.Personalization;
using EPiServer.PlugIn;
using EPiServer.Security;
using EPiServer.Util.PlugIns;
using System.Web.UI;
using EPiServer.Web.Hosting;


namespace Niteco.Web.Plugins.ContactForm
{
      [GuiPlugIn(DisplayName = "Export Form Data Tool", Description = "Export Form Data Tool", Area = PlugInArea.AdminMenu, Url = "~/Plugins/ContactForm/ExportFormDataTool.aspx")]
    public partial class ExportFormDataTool : System.Web.UI.Page
    {
        private string[] contactFormColumn = { "Full Name", "Phone", "Email", "Country", "Message" };
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            btnExport.Click += btnExport_Click;
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            // ExportDataToExcel();
            DataTable dataTable = DataForContactForm();
            var fileName = "Contact-form-" + DateTime.Now.ToString("ddMMyyyy") + ".xls";
            ExportToExcel(fileName, dataTable);
        }
        private void ExportDataToExcel()
        {
            DataTable dataTable = DataForContactForm(); 
            var fileName = "ContactForm";
            var dataSet = new DataSet("DataSetTable");
            dataSet.Tables.Add(dataTable);

            var dataGrid = new DataGrid();
            dataGrid.DataSource = dataSet.Tables[0];
            dataGrid.DataBind();

            //export to excel
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.ms-excel";

            Response.AddHeader("Content-Disposition", string.Format("inline;filename={0}-{1}.xls", fileName, DateTime.Now.ToString("yyyyMMdd")));
            Response.ContentEncoding = Encoding.Default;
            Response.Charset = "";

            var oStringWriter = new StringWriter();
            var oHtmlTextWriter = new HtmlTextWriter(oStringWriter);
            dataGrid.RenderControl(oHtmlTextWriter);
            Response.Write(oStringWriter.ToString());
            Response.End();

        }
        private DataTable DataForContactForm()
        {
            var dataTable = new DataTable();
            var CSV_ROW = "{0}*{1}*{2}*{3}*{4}";
            foreach (string column in contactFormColumn)
            {
                var dcol = new DataColumn(column, typeof(String));
                dataTable.Columns.Add(dcol);
            }
            var contacts = ContactFormData.GetAll();
            foreach (var contact in contacts)
            {
                DataRow dr = dataTable.NewRow();
                int i = 0;

                try
                {
                    string data = string.Format(CSV_ROW,  contact.FullName,
                         contact.Phone, contact.Email, contact.Country, contact.Message);
                    char[] delimiter = { '*' };
                    string[] rows = data.Split(delimiter);
                    while (i < contactFormColumn.Count())
                    {
                        dr[contactFormColumn[i]] = rows[i];
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
        protected void Page_Load(object sender, EventArgs e)
        {
            DataTable dataTable = DataForContactForm();
            grdMedia.DataSource = dataTable;
            grdMedia.DataBind();
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