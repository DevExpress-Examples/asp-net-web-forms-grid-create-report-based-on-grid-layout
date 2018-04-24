<%@ Page Title="Home Page" Language="vb" AutoEventWireup="true"
    CodeBehind="Default.aspx.vb" Inherits="WebApplication1._Default" %>

<%@ Register Assembly="DevExpress.Web.v13.1, Version=13.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a"
    Namespace="DevExpress.Web.ASPxGridView" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v13.1, Version=13.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a"
    Namespace="DevExpress.Web.ASPxEditors" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v13.1, Version=13.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a"
    Namespace="DevExpress.Web.ASPxGridLookup" TagPrefix="dx" %>

 <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
 <html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
<head id="Head1" runat="server">
    <title>Custom Export Sample</title>
</head>
<body>
    <form id="Form1" runat="server">

    <asp:AccessDataSource ID="AccessDataSource1" runat="server" DataFile="~/App_Data/nwind.mdb"
        SelectCommand="SELECT * FROM [Products]">
    </asp:AccessDataSource>
    <dx:ASPxButton ID="ASPxButton2" runat="server" OnClick="ASPxButton2_Click" 
        Text="Export to PDF via XtraReport">
    </dx:ASPxButton>
    <dx:ASPxGridView ID="ASPxGridView1" runat="server" AutoGenerateColumns="False" DataSourceID="AccessDataSource1"
        KeyFieldName="ProductID">
        <TotalSummary>
            <dx:ASPxSummaryItem FieldName="UnitsInStock"
                SummaryType="Sum" />
            <dx:ASPxSummaryItem FieldName="SupplierID" SummaryType="Sum" />
        </TotalSummary>
        <GroupSummary>
                    <dx:ASPxSummaryItem FieldName="UnitsInStock"
                SummaryType="Sum" 
                        ShowInGroupFooterColumn="Units In Stock" />
            <dx:ASPxSummaryItem FieldName="UnitsOnOrder" 
                ShowInGroupFooterColumn="Units On Order" SummaryType="Sum" />
        </GroupSummary>
        <Columns>
                    <dx:GridViewCommandColumn VisibleIndex="0">
                        <ClearFilterButton Visible="True">
                        </ClearFilterButton>
                    </dx:GridViewCommandColumn>
                    <dx:GridViewDataTextColumn FieldName="ProductID" ReadOnly="True" 
                        VisibleIndex="1"  >
                        <EditFormSettings Visible="False" />
                    </dx:GridViewDataTextColumn>
                    <dx:GridViewDataTextColumn FieldName="ProductName" VisibleIndex="2" />
                      <dx:GridViewDataTextColumn FieldName="SupplierID" VisibleIndex="3" />
                    <dx:GridViewDataTextColumn FieldName="CategoryID" VisibleIndex="4" />
                      <dx:GridViewDataTextColumn FieldName="QuantityPerUnit" VisibleIndex="5" />
                    <dx:GridViewDataTextColumn FieldName="UnitPrice" VisibleIndex="6" />
                      <dx:GridViewDataTextColumn FieldName="UnitsInStock" VisibleIndex="7"/>
                    <dx:GridViewDataTextColumn FieldName="UnitsOnOrder" VisibleIndex="8" />
                    <dx:GridViewDataTextColumn FieldName="ReorderLevel" VisibleIndex="9" />
                    <dx:GridViewDataCheckColumn FieldName="Discontinued" VisibleIndex="10">
                    </dx:GridViewDataCheckColumn>
                    <dx:GridViewDataTextColumn FieldName="EAN13" VisibleIndex="11"/>
                </columns>
        <Settings ShowFilterRow="True" ShowFooter="True" ShowGroupPanel="True" />
    </dx:ASPxGridView>
        </form>
</body>
</html>