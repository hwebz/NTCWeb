<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CleanUpPagePlugin.aspx.cs" Inherits="Niteco.Web.App.Plugins.CleanUpPage.CleanUpPagePlugin" %>

<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.DataAbstraction" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>



    <style type="text/css">
        table.list-table {
            border-left: 1px solid #D0CFCA;
            border-collapse: collapse;
        }

            table.list-table thead {
                background: #D0CFCA;
            }

            table.list-table th {
                border: 1px solid #D0CFCA;
                margin: 0;
            }

            table.list-table td {
                border-right: 1px solid #D0CFCA;
                border-bottom: 1px solid #D0CFCA;
                margin: 0;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:DropDownList runat="server" ID="ddlChooseCleanUpActionType" AutoPostBack="True" OnSelectedIndexChanged="ddlChooseCleanUpActionType_SelectedIndexChanged" />
            <br />
            <asp:Panel runat="server" ID="plhCleanupPage" Visible="False">
                <label><strong>Unused page type list</strong></label>
                <br />
                <asp:Button runat="server" ID="btnCleanUpPageData" OnClick="btnCleanUpPageData_Click" Text="Clean up Page Data" OnClientClick="ConfirmUpdate();" />
                <br />
                <asp:Repeater runat="server" ID="repeaterUnusedPageTypes" OnItemDataBound="ItemDataBound">
                    <HeaderTemplate>
                        <table class="list-table">
                            <thead>
                                <tr>
                                    <th>
                                        <input type="checkbox" id="chkAll" onclick="ToggleCleanPageDataSelectAll(this);" class="checkbox-all" /></th>
                                    <th>Name</th>
                                    <th>Model Type</th>
                                    <th>Usage Pages</th>
                                </tr>
                            </thead>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <asp:CheckBox runat="server" CssClass="checkbox" ID="chkPage" />
                                <asp:HiddenField runat="server" Value='<%#Eval("Id")%>' ID="hiddenPageId" />
                            </td>
                            <td>
                                <%#Eval("Name")%>
                            </td>
                            <td>
                                <%#Eval("ModelType")%>
                            </td>
                            <td>
                                <asp:Repeater runat="server" ID="repeaterUsagePages">
                                    <ItemTemplate>
                                        Page Id: <%# ((IContent)Container.DataItem).ContentLink.ID%> - Page Name: <%# ((IContent)Container.DataItem).Name%>
                                        <br />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:PlaceHolder runat="server" ID="plhCleanupPageEmpty" Visible="False">
                    <div>No page types found</div>
                </asp:PlaceHolder>
            </asp:Panel>

            <asp:Panel runat="server" ID="plhCleanupProperties" Visible="True">
                <label>Unused page properties list</label>
                <br />
                <asp:Button runat="server" ID="btnCleanUpProperty" OnClick="btnCleanUpProperty_Click" Text="Clean up property" OnClientClick="ConfirmUpdate();" />
                <br />
                <asp:DropDownList runat="server" ID="ddlChooseActionType" AutoPostBack="True" OnSelectedIndexChanged="ddlChooseActionType_SelectedIndexChanged" />
                <br />
                <asp:Repeater runat="server" ID="repeaterUnusedProperties" OnItemDataBound="repeaterUnusedProperties_ItemDataBound">
                    <HeaderTemplate>
                        <table class="list-table">
                            <thead>
                                <tr>
                                    <th>
                                        <input type="checkbox" id="chkAll" onclick="ToggleCleanPropertySelectAll(this);" class="checkbox-all" /></th>
                                    <th>Name</th>
                                    <th>Content Type Name</th>
                                    <th>Description</th>
                                    <th>Model Description</th>
                                    <th>Tab Name</th>
                                    <th>Model Tab Name</th>
                                    <th>Usage Contents</th>
                                </tr>
                            </thead>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <asp:CheckBox runat="server" CssClass="checkbox" ID="chkProperties" />
                                <asp:HiddenField runat="server" Value='<%#Eval("Id")%>' ID="hiddenPropertyId" />
                            </td>
                            <td><%#Eval("Name")%></td>
                            <td><%#Eval("ContentTypeName")%></td>
                            <td><%#Eval("Description")%></td>
                            <td><%#Eval("ModelDescription")%></td>
                            <td><%#Eval("TabName")%></td>
                            <td><%#Eval("ModelTabName")%></td>
                            <td>
                                <asp:Repeater runat="server" ID="repeaterUsagePages">
                                    <ItemTemplate>
                                        Content Id: <%# ((ContentUsage)Container.DataItem).ContentLink.ID%> - Name: <%# ((ContentUsage)Container.DataItem).Name%>
                                        <br />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:PlaceHolder runat="server" ID="plhNoPropertiesFound" Visible="False">
                    <div>No properties found</div>
                </asp:PlaceHolder>
            </asp:Panel>
        </div>
        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js"></script>
        <script>window.jQuery || document.write('<script src="/Static/js/jquery-1.8.3.min.js"><\/script>')</script>
        <script type="text/javascript">
            function ToggleCleanPageDataSelectAll(checkbox) {
                var checkBoxes = $('#<%= plhCleanupPage.ClientID%>').find('input:checkbox');
                    for (i = 0; i < checkBoxes.length; i++) {
                        if ($(checkbox).prop('checked') == true) {
                            $(checkBoxes[i]).prop('checked', true);
                        }
                        else {
                            $(checkBoxes[i]).prop('checked',false);
                        }

                    }
                }
                function ToggleCleanPropertySelectAll(checkbox) {
                    var checkBoxes = $('#<%= plhCleanupProperties.ClientID%>').find('input:checkbox');
            for (i = 0; i < checkBoxes.length; i++) {
                if ($(checkbox).prop('checked') == true) {
                    $(checkBoxes[i]).prop('checked', true);
                }
                else {
                    $(checkBoxes[i]).prop('checked',false);
                }

            }
        }
        function ConfirmUpdate() {
            return confirm("Are you sure?");
        }
        $(function () {
            $('#<%= plhCleanupPage.ClientID%>').find('input:checkbox').each(function () {
                $(this).click(function () {
                    var checkBoxes = $('#<%= plhCleanupPage.ClientID%>').find('input:checkbox');
                    var selectAllCheckBox = $('#<%= plhCleanupPage.ClientID%>').find('#chkAll');
                    var length = checkBoxes.length - 1;
                    var checkedLength = 0;
                    if ($(this).prop('checked') == true) {
                        for (i = 0; i < checkBoxes.length; i++) {
                            if (($(checkBoxes[i]).prop('checked') == true) && ($(checkBoxes[i]).attr('id') != 'chkAll')) {
                                checkedLength++;
                            }
                        }
                        if (length == checkedLength) {
                            selectAllCheckBox.prop('checked', true);
                        }
                    } else {
                        selectAllCheckBox.prop('checked',false);
                    }

                });
            });
            $('#<%= plhCleanupProperties.ClientID%>').find('input:checkbox').each(function () {
                $(this).click(function () {
                    var checkBoxes = $('#<%= plhCleanupProperties.ClientID%>').find('input:checkbox');
                    var selectAllCheckBox = $('#<%= plhCleanupProperties.ClientID%>').find('#chkAll');
                    var length = checkBoxes.length - 1;
                    var checkedLength = 0;
                    if ($(this).prop('checked') == true) {
                        for (i = 0; i < checkBoxes.length; i++) {
                            if (($(checkBoxes[i]).prop('checked') == true) && ($(checkBoxes[i]).attr('id') != 'chkAll')) {
                                checkedLength++;
                            }
                        }
                        if (length == checkedLength) {
                            selectAllCheckBox.prop('checked', true);
                        }
                    } else {
                        selectAllCheckBox.prop('checked',false);
                    }

                });
            });
        });
        </script>
    </form>
</body>
</html>
