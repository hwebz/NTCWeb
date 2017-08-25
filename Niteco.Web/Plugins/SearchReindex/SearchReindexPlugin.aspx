<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchReindexPlugin.aspx.cs" Inherits="Niteco.Web.App.Plugins.SearchReindex.SearchReindexPlugin" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style>
        #progressBar {
            margin-top: 10px;
        width: 400px;
        height: 22px;
        border: 1px solid #111;
        background-color: #D0CFCF;
        }

        #progressBar div {
        height: 100%;
        color: #fff;
        text-align: right;
        line-height: 22px; /* same as #progressBar height if we want text middle aligned */
        width: 0;
        background-color: #171717;
        }

    </style>
    <script src="/Scripts/jquery-1.10.2.js"></script>
    <script>

         var timer;
         var complete = false;
         var initialIndexRequestCount = 0;
         function GetIndexingStatus() {
             if (complete == true) {
                 clearTimeout(timer);
                 return;
             }
             $.ajax({
                 type: "GET",
                 url: "GetIndexingStatus.aspx",
                 success: function (data) {
                     if (data != "Done") {
                         var percentDone = Math.round(((initialIndexRequestCount - data) / initialIndexRequestCount) * 100);
                         
                         progress(percentDone, $('#progressBar'));
                         complete = false;
                     } else {
                         $("#<%=lblStatus.ClientID%>").text("Done!");
                         progress(100, $('#progressBar'));
                         complete = true;
                     }
                 }
             });

             timer = setTimeout('GetIndexingStatus()', 1000);
         };
        function progress(percent, $element) {
            var progressBarWidth = percent * $element.width() / 100;
            $element.find('div').animate({ width: progressBarWidth }, 500).html(percent + "%&nbsp;");
        }

    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <label>Click to start building index:</label><br/><br/>
        <asp:Button runat="server" ID="ReIndex" Text="Start Indexing" OnClick="ReIndex_Click" />
        <asp:Button runat="server" ID="ClearIndexDataStore" Text="Clear Index Queues" style="margin-left: 10px;" OnClick="ClearIndexDataStoreClick"/>
        
        <div id="progressBar"><div></div></div>
        <div><asp:Literal runat="server" ID="TotalItems"></asp:Literal></div>
        <div><asp:Label runat="server" ID="lblStatus"></asp:Label></div>
    </div>
    </form>
   
</body>
</html>

