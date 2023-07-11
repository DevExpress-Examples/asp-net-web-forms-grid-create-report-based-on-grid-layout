Imports System
Imports System.Web
Imports System.Web.UI.WebControls
Imports System.Net.Mime
Imports DevExpress.XtraReports.UI
Imports DevExpress.XtraPrinting.Shape
Imports System.Drawing

Namespace WebApplication1

    Public Partial Class _Default
        Inherits UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
        End Sub

        Protected Sub ASPxButton2_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim generator As ReportGeneratonHelper = New ReportGeneratonHelper()
            AddHandler generator.CustomizeColumnsCollection, New CustomizeColumnsCollectionEventHandler(AddressOf generator_CustomizeColumnsCollection)
            AddHandler generator.CustomizeColumn, New CustomizeColumnEventHandler(AddressOf generator_CustomizeColumn)
            Dim report As XtraReport = generator.GenerateReport(ASPxGridView1, AccessDataSource1)
            generator.WritePdfToResponse(Response, "test.pdf", DispositionTypeNames.Attachment.ToString())
        End Sub

        Private Sub generator_CustomizeColumn(ByVal source As Object, ByVal e As ControlCustomizationEventArgs)
            If Equals(e.FieldName, "Discontinued") Then
                Dim control As XRShape = New XRShape()
                control.SizeF = e.Owner.SizeF
                control.LocationF = New System.Drawing.PointF(0, 0)
                e.Owner.Controls.Add(control)
                control.Shape = New ShapeStar() With {.StarPointCount = 5, .Concavity = 30}
                AddHandler control.BeforePrint, New System.Drawing.Printing.PrintEventHandler(AddressOf control_BeforePrint)
                e.IsModified = True
            End If
        End Sub

        Private Sub control_BeforePrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs)
            If Convert.ToBoolean(CType(sender, XRShape).Report.GetCurrentColumnValue("Discontinued")) = True Then
                CType(sender, XRShape).FillColor = Color.Yellow
            Else
                CType(sender, XRShape).FillColor = Color.White
            End If
        End Sub

        Private Sub generator_CustomizeColumnsCollection(ByVal source As Object, ByVal e As ColumnsCreationEventArgs)
            e.ColumnsInfo(1).ColumnWidth *= 2
            e.ColumnsInfo(4).ColumnWidth += 30
            e.ColumnsInfo(3).ColumnWidth -= 30
            e.ColumnsInfo(e.ColumnsInfo.Count - 1).IsVisible = False
        End Sub
    End Class
End Namespace
