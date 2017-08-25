<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportFormDataTool.aspx.cs" Inherits="Niteco.Web.Plugins.ContactForm.ExportFormDataTool" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head>
       
    </head>
    <body>
        <form runat="server">
            <div class="epi-contentContainer epi-padding">
                <div class="epi-contentArea">
                    <h1>Export Form Data</h1>
                    <p class="EP-systemInfo">The tool is used to export all data from contact forms</p>
                </div>
                 <asp:DataGrid runat="server" ID="grdMedia" AutoGenerateColumns="true" Width="98%" Visible="False">
                    
                </asp:DataGrid>
                <span class="epi-cmsButton">
                    <asp:Button runat="server" ID="btnExport" Text="Export"/>
                </span>
            </div>
        </form>
    </body>
</html>
