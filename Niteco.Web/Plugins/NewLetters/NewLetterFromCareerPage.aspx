<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="NewLetterFromCareerPage.aspx.cs" Inherits="Niteco.Web.Plugins.NewLetters.NewLetterFromCareerPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div class="epi-contentContainer epi-padding">
                <div class="epi-contentArea">
                    <h1>Export Form Data</h1>
                    <p class="EP-systemInfo">The tool is used to export all data from new letters from career page</p>
                </div>
                 <asp:DataGrid runat="server" ID="grdData" AutoGenerateColumns="true" Width="98%" Visible="False">
                    
                </asp:DataGrid>
                <span class="epi-cmsButton">
                    <asp:Button runat="server" ID="btnExport" Text="Export" OnClick="btnExport_OnClick"/>
                </span>
            </div>
    </form>
</body>
</html>
