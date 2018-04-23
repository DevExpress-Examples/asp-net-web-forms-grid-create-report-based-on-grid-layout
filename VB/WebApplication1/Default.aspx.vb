Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.IO
Imports DevExpress.XtraPrinting
Imports System.Net.Mime
Imports DevExpress.XtraReports.UI
Imports DevExpress.XtraPrinting.Shape
Imports System.Drawing

Namespace WebApplication1
	Partial Public Class _Default
		Inherits System.Web.UI.Page
		Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

		End Sub
		Protected Sub ASPxButton2_Click(ByVal sender As Object, ByVal e As EventArgs)
			Dim generator As New ReportGeneratonHelper()
			AddHandler generator.CustomizeColumnsCollection, AddressOf generator_CustomizeColumnsCollection
			AddHandler generator.CustomizeColumn, AddressOf generator_CustomizeColumn
			Dim report As XtraReport = generator.GenerateReport(ASPxGridView1, AccessDataSource1)
			generator.WritePdfToResponse(Response, "test.pdf", System.Net.Mime.DispositionTypeNames.Attachment.ToString())

		End Sub

		Private Sub generator_CustomizeColumn(ByVal source As Object, ByVal e As ControlCustomizationEventArgs)
			If e.FieldName = "Discontinued" Then
				Dim control As New XRShape()
				control.SizeF = e.Owner.SizeF
				control.LocationF = New System.Drawing.PointF(0, 0)
				e.Owner.Controls.Add(control)
				control.Shape = New ShapeStar() With {.StarPointCount = 5, .Concavity = 30}
				AddHandler control.BeforePrint, AddressOf control_BeforePrint
				e.IsModified = True
			End If
		End Sub

		Private Sub control_BeforePrint(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintEventArgs)
			If Convert.ToBoolean((CType(sender, XRShape)).Report.GetCurrentColumnValue("Discontinued")) = True Then
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
