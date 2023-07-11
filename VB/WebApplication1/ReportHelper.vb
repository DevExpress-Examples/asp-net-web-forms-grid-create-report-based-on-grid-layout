Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports DevExpress.XtraReports.UI
Imports System.Data
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports DevExpress.Web.ASPxGridView
Imports System.Drawing
Imports DevExpress.Data
Imports DevExpress.XtraPrinting
Imports System.Collections.ObjectModel
Imports System.Collections
Imports System.Net.Mime
Imports System.IO

Namespace WebApplication1

    Public Delegate Sub CustomizeColumnsCollectionEventHandler(ByVal source As Object, ByVal e As WebApplication1.ColumnsCreationEventArgs)

    Public Delegate Sub CustomizeColumnEventHandler(ByVal source As Object, ByVal e As WebApplication1.ControlCustomizationEventArgs)

    Public Class ReportGeneratonHelper

        Private report As DevExpress.XtraReports.UI.XtraReport

        Const initialGroupOffset As Integer = 0

        Const subGroupOffset As Integer = 10

        Const bandHeight As Integer = 20

        Const shouldRepeatGroupHeadersOnEveryPage As Boolean = False

        Private detailsInfo As System.Collections.Hashtable = New System.Collections.Hashtable()

        Public Event CustomizeColumnsCollection As WebApplication1.CustomizeColumnsCollectionEventHandler

        Public Event CustomizeColumn As WebApplication1.CustomizeColumnEventHandler

        Public Function GenerateReport(ByVal aspxGridView1 As DevExpress.Web.ASPxGridView.ASPxGridView, ByVal aspDataSource As System.Web.UI.WebControls.SqlDataSource) As XtraReport
            Me.report = New DevExpress.XtraReports.UI.XtraReport()
            Me.report.Landscape = True
            Me.report.PaperKind = System.Drawing.Printing.PaperKind.Letter
            Me.InitDataSource(aspDataSource)
            Me.InitDetailsAndPageHeader(aspxGridView1)
            Me.InitSortings(aspxGridView1)
            Me.InitGroupHeaders(aspxGridView1)
            Me.InitGroupSummaries(aspxGridView1)
            Me.InitFilters(aspxGridView1)
            Me.InitTotalSummaries(aspxGridView1)
            Return Me.report
        End Function

        Private Sub InitGroupSummaries(ByVal aspxGridView1 As DevExpress.Web.ASPxGridView.ASPxGridView)
            If aspxGridView1.GroupSummary.Count > 0 Then
                Dim groupedColumns As System.Collections.ObjectModel.ReadOnlyCollection(Of DevExpress.Web.ASPxGridView.GridViewDataColumn) = aspxGridView1.GetGroupedColumns()
                For i As Integer = groupedColumns.Count - 1 To 0 Step -1
                    Dim footerBand As DevExpress.XtraReports.UI.GroupFooterBand = New DevExpress.XtraReports.UI.GroupFooterBand()
                    footerBand.Height = WebApplication1.ReportGeneratonHelper.bandHeight
                    Me.report.Bands.Add(footerBand)
                    footerBand.BackColor = System.Drawing.Color.LightGray
                    For Each item As DevExpress.Web.ASPxGridView.ASPxSummaryItem In aspxGridView1.GroupSummary
                        Dim col As DevExpress.Web.ASPxGridView.GridViewColumn = aspxGridView1.Columns(item.FieldName)
                        If col IsNot Nothing Then
                            If Me.detailsInfo.Contains(col) Then
                                Dim label As DevExpress.XtraReports.UI.XRLabel = New DevExpress.XtraReports.UI.XRLabel()
                                label.LocationF = CType(Me.detailsInfo(CObj((col))), DevExpress.XtraReports.UI.XRTableCell).LocationF
                                label.SizeF = CType(Me.detailsInfo(CObj((col))), DevExpress.XtraReports.UI.XRTableCell).SizeF
                                label.DataBindings.Add("Text", Nothing, CType(col, DevExpress.Web.ASPxGridView.GridViewDataColumn).FieldName)
                                label.Summary = New DevExpress.XtraReports.UI.XRSummary() With {.Running = DevExpress.XtraReports.UI.SummaryRunning.Group}
                                label.Summary.FormatString = item.DisplayFormat
                                label.Summary.Func = Me.GetSummaryFunc(item.SummaryType)
                                footerBand.Controls.Add(label)
                            End If
                        End If
                    Next
                Next
            End If
        End Sub

        Private Sub InitTotalSummaries(ByVal aspxGridView1 As DevExpress.Web.ASPxGridView.ASPxGridView)
            If aspxGridView1.TotalSummary.Count > 0 Then
                Me.report.Bands.Add(New DevExpress.XtraReports.UI.ReportFooterBand() With {.HeightF = WebApplication1.ReportGeneratonHelper.bandHeight})
                For Each item As DevExpress.Web.ASPxGridView.ASPxSummaryItem In aspxGridView1.TotalSummary
                    Dim col As DevExpress.Web.ASPxGridView.GridViewColumn = aspxGridView1.Columns(If(Equals(item.ShowInColumn, String.Empty), item.FieldName, item.ShowInColumn))
                    If col IsNot Nothing Then
                        If Me.detailsInfo.Contains(col) Then
                            Dim label As DevExpress.XtraReports.UI.XRLabel = New DevExpress.XtraReports.UI.XRLabel()
                            label.LocationF = CType(Me.detailsInfo(CObj((col))), DevExpress.XtraReports.UI.XRTableCell).LocationF
                            label.SizeF = CType(Me.detailsInfo(CObj((col))), DevExpress.XtraReports.UI.XRTableCell).SizeF
                            label.DataBindings.Add("Text", Nothing, CType(col, DevExpress.Web.ASPxGridView.GridViewDataColumn).FieldName)
                            label.Summary = New DevExpress.XtraReports.UI.XRSummary() With {.Running = DevExpress.XtraReports.UI.SummaryRunning.Report}
                            label.Summary.FormatString = item.DisplayFormat
                            label.Summary.Func = Me.GetSummaryFunc(item.SummaryType)
                            Me.report.Bands(CType((DevExpress.XtraReports.UI.BandKind.ReportFooter), DevExpress.XtraReports.UI.BandKind)).Controls.Add(label)
                        End If
                    End If
                Next
            End If
        End Sub

        Private Sub InitDataSource(ByVal aspDataSource As System.Web.UI.WebControls.SqlDataSource)
            Dim dv As System.Data.DataView = New System.Data.DataView()
            Dim dt As System.Data.DataTable = New System.Data.DataTable()
            dv = TryCast(aspDataSource.[Select](System.Web.UI.DataSourceSelectArguments.Empty), System.Data.DataView)
            dt = dv.ToTable()
            Me.report.DataSource = dt
        End Sub

        Private Sub InitGroupHeaders(ByVal aspxGridView1 As DevExpress.Web.ASPxGridView.ASPxGridView)
            Dim groupedColumns As System.Collections.ObjectModel.ReadOnlyCollection(Of DevExpress.Web.ASPxGridView.GridViewDataColumn) = aspxGridView1.GetGroupedColumns()
            For i As Integer = groupedColumns.Count - 1 To 0 Step -1
                If True Then
                    Dim groupedColumn As DevExpress.Web.ASPxGridView.GridViewDataColumn = groupedColumns(i)
                    Dim gb As DevExpress.XtraReports.UI.GroupHeaderBand = New DevExpress.XtraReports.UI.GroupHeaderBand()
                    gb.Height = WebApplication1.ReportGeneratonHelper.bandHeight
                    Dim l As DevExpress.XtraReports.UI.XRLabel = New DevExpress.XtraReports.UI.XRLabel()
                    l.Text = groupedColumn.FieldName & ": [" & groupedColumn.FieldName & "]"
                    l.LocationF = New System.Drawing.PointF(WebApplication1.ReportGeneratonHelper.initialGroupOffset + i * 10, 0)
                    l.BackColor = System.Drawing.Color.Beige
                    l.SizeF = New System.Drawing.SizeF((Me.report.PageWidth - (Me.report.Margins.Left + Me.report.Margins.Right)) - (WebApplication1.ReportGeneratonHelper.initialGroupOffset + i * WebApplication1.ReportGeneratonHelper.subGroupOffset), WebApplication1.ReportGeneratonHelper.bandHeight)
                    gb.Controls.Add(l)
                    gb.RepeatEveryPage = WebApplication1.ReportGeneratonHelper.shouldRepeatGroupHeadersOnEveryPage
                    Dim gf As DevExpress.XtraReports.UI.GroupField = New DevExpress.XtraReports.UI.GroupField(groupedColumn.FieldName, If(groupedColumn.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending, DevExpress.XtraReports.UI.XRColumnSortOrder.Ascending, DevExpress.XtraReports.UI.XRColumnSortOrder.Descending))
                    gb.GroupFields.Add(gf)
                    Me.report.Bands.Add(gb)
                End If
            Next
        End Sub

        Private Sub InitSortings(ByVal aspxGridView1 As DevExpress.Web.ASPxGridView.ASPxGridView)
            Dim columns As System.Collections.Generic.List(Of DevExpress.Web.ASPxGridView.GridViewDataColumn) = Me.GetVisibleDataColumns(aspxGridView1)
            Dim groupedColumns As System.Collections.ObjectModel.ReadOnlyCollection(Of DevExpress.Web.ASPxGridView.GridViewDataColumn) = aspxGridView1.GetGroupedColumns()
            For i As Integer = 0 To columns.Count - 1
                If Not groupedColumns.Contains(columns(i)) Then
                    If columns(CInt((i))).SortOrder <> DevExpress.Data.ColumnSortOrder.None Then CType(Me.report.Bands(CType((DevExpress.XtraReports.UI.BandKind.Detail), DevExpress.XtraReports.UI.BandKind)), DevExpress.XtraReports.UI.DetailBand).SortFields.Add(New DevExpress.XtraReports.UI.GroupField(columns(CInt((i))).FieldName, If(columns(CInt((i))).SortOrder = DevExpress.Data.ColumnSortOrder.Ascending, DevExpress.XtraReports.UI.XRColumnSortOrder.Ascending, DevExpress.XtraReports.UI.XRColumnSortOrder.Descending)))
                End If
            Next
        End Sub

        Private Sub InitFilters(ByVal aspxGridView1 As DevExpress.Web.ASPxGridView.ASPxGridView)
            Me.report.FilterString = aspxGridView1.FilterExpression
        End Sub

        Private Sub InitDetailsAndPageHeader(ByVal aspxGridView1 As DevExpress.Web.ASPxGridView.ASPxGridView)
            Dim groupedColumns As System.Collections.ObjectModel.ReadOnlyCollection(Of DevExpress.Web.ASPxGridView.GridViewDataColumn) = aspxGridView1.GetGroupedColumns()
            Dim pagewidth As Integer =(Me.report.PageWidth - (Me.report.Margins.Left + Me.report.Margins.Right)) - groupedColumns.Count * WebApplication1.ReportGeneratonHelper.subGroupOffset
            Dim columns As System.Collections.Generic.List(Of WebApplication1.ColumnInfo) = Me.GetColumnsInfo(aspxGridView1, pagewidth)
            RaiseEvent CustomizeColumnsCollection(Me.report, New WebApplication1.ColumnsCreationEventArgs(pagewidth) With {.ColumnsInfo = columns})
            Me.report.Bands.Add(New DevExpress.XtraReports.UI.DetailBand() With {.HeightF = WebApplication1.ReportGeneratonHelper.bandHeight})
            Me.report.Bands.Add(New DevExpress.XtraReports.UI.PageHeaderBand() With {.HeightF = WebApplication1.ReportGeneratonHelper.bandHeight})
            Dim headerTable As DevExpress.XtraReports.UI.XRTable = New DevExpress.XtraReports.UI.XRTable()
            Dim row As DevExpress.XtraReports.UI.XRTableRow = New DevExpress.XtraReports.UI.XRTableRow()
            Dim detailTable As DevExpress.XtraReports.UI.XRTable = New DevExpress.XtraReports.UI.XRTable()
            Dim row2 As DevExpress.XtraReports.UI.XRTableRow = New DevExpress.XtraReports.UI.XRTableRow()
            For i As Integer = 0 To columns.Count - 1
                If columns(CInt((i))).IsVisible Then
                    Dim cell As DevExpress.XtraReports.UI.XRTableCell = New DevExpress.XtraReports.UI.XRTableCell()
                    cell.Width = columns(CInt((i))).ColumnWidth
                    cell.Text = columns(CInt((i))).FieldName
                    row.Cells.Add(cell)
                    Dim cell2 As DevExpress.XtraReports.UI.XRTableCell = New DevExpress.XtraReports.UI.XRTableCell()
                    cell2.Width = columns(CInt((i))).ColumnWidth
                    Dim cc As WebApplication1.ControlCustomizationEventArgs = New WebApplication1.ControlCustomizationEventArgs() With {.FieldName = columns(CInt((i))).FieldName, .IsModified = False, .Owner = cell2}
                    RaiseEvent CustomizeColumn(Me.report, cc)
                    If cc.IsModified = False Then cell2.DataBindings.Add("Text", Nothing, columns(CInt((i))).FieldName)
                    Me.detailsInfo.Add(columns(CInt((i))).GridViewColumn, cell2)
                    row2.Cells.Add(cell2)
                End If
            Next

            headerTable.Rows.Add(row)
            headerTable.Width = pagewidth
            headerTable.LocationF = New System.Drawing.PointF(groupedColumns.Count * WebApplication1.ReportGeneratonHelper.subGroupOffset, 0)
            headerTable.Borders = DevExpress.XtraPrinting.BorderSide.Bottom
            detailTable.Rows.Add(row2)
            detailTable.LocationF = New System.Drawing.PointF(groupedColumns.Count * WebApplication1.ReportGeneratonHelper.subGroupOffset, 0)
            detailTable.Width = pagewidth
            Me.report.Bands(CType((DevExpress.XtraReports.UI.BandKind.PageHeader), DevExpress.XtraReports.UI.BandKind)).Controls.Add(headerTable)
            Me.report.Bands(CType((DevExpress.XtraReports.UI.BandKind.Detail), DevExpress.XtraReports.UI.BandKind)).Controls.Add(detailTable)
        End Sub

        Private Function GetColumnsInfo(ByVal aspxGridView1 As DevExpress.Web.ASPxGridView.ASPxGridView, ByVal pagewidth As Integer) As List(Of WebApplication1.ColumnInfo)
            Dim columns As System.Collections.Generic.List(Of WebApplication1.ColumnInfo) = New System.Collections.Generic.List(Of WebApplication1.ColumnInfo)()
            Dim visibleColumns As System.Collections.Generic.List(Of DevExpress.Web.ASPxGridView.GridViewDataColumn) = Me.GetVisibleDataColumns(aspxGridView1)
            For Each dataColumn As DevExpress.Web.ASPxGridView.GridViewDataColumn In visibleColumns
                Dim column As WebApplication1.ColumnInfo = New WebApplication1.ColumnInfo(dataColumn) With {.ColumnCaption = If(String.IsNullOrEmpty(dataColumn.Caption), dataColumn.FieldName, dataColumn.Caption), .ColumnWidth =(CInt(pagewidth) \ visibleColumns.Count), .FieldName = dataColumn.FieldName, .IsVisible = True}
                columns.Add(column)
            Next

            Return columns
        End Function

        Private Function GetVisibleDataColumns(ByVal aspxGridView1 As DevExpress.Web.ASPxGridView.ASPxGridView) As List(Of DevExpress.Web.ASPxGridView.GridViewDataColumn)
            Dim columns As System.Collections.Generic.List(Of DevExpress.Web.ASPxGridView.GridViewDataColumn) = New System.Collections.Generic.List(Of DevExpress.Web.ASPxGridView.GridViewDataColumn)()
            For Each column As DevExpress.Web.ASPxGridView.GridViewColumn In aspxGridView1.VisibleColumns
                If TypeOf column Is DevExpress.Web.ASPxGridView.GridViewDataColumn Then columns.Add(TryCast(column, DevExpress.Web.ASPxGridView.GridViewDataColumn))
            Next

            Return columns
        End Function

        Private Function GetSummaryFunc(ByVal summaryItemType As DevExpress.Data.SummaryItemType) As SummaryFunc
            Select Case summaryItemType
                Case DevExpress.Data.SummaryItemType.Sum
                    Return DevExpress.XtraReports.UI.SummaryFunc.Sum
                Case DevExpress.Data.SummaryItemType.Average
                    Return DevExpress.XtraReports.UI.SummaryFunc.Avg
                Case DevExpress.Data.SummaryItemType.Max
                    Return DevExpress.XtraReports.UI.SummaryFunc.Max
                Case DevExpress.Data.SummaryItemType.Min
                    Return DevExpress.XtraReports.UI.SummaryFunc.Min
                Case Else
                    Return DevExpress.XtraReports.UI.SummaryFunc.Custom
            End Select
        End Function

        Friend Sub WritePdfToResponse(ByVal Response As System.Web.HttpResponse, ByVal fileName As String, ByVal type As String)
            Me.report.CreateDocument(False)
            Using ms As System.IO.MemoryStream = New System.IO.MemoryStream()
                Me.report.ExportToPdf(ms)
                ms.Seek(0, System.IO.SeekOrigin.Begin)
                Call WebApplication1.ReportGeneratonHelper.WriteResponse(Response, ms.ToArray(), type, fileName)
            End Using
        End Sub

        Public Shared Sub WriteResponse(ByVal response As System.Web.HttpResponse, ByVal filearray As Byte(), ByVal type As String, ByVal fileName As String)
            response.ClearContent()
            response.Buffer = True
            response.Cache.SetCacheability(System.Web.HttpCacheability.[Private])
            response.ContentType = "application/pdf"
            Dim contentDisposition As System.Net.Mime.ContentDisposition = New System.Net.Mime.ContentDisposition()
            contentDisposition.FileName = fileName
            contentDisposition.DispositionType = type
            response.AddHeader("Content-Disposition", contentDisposition.ToString())
            response.BinaryWrite(filearray)
            Call System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest()
            Try
                response.[End]()
            Catch __unusedThreadAbortException1__ As System.Threading.ThreadAbortException
            End Try
        End Sub
    End Class

    Public Class ControlCustomizationEventArgs
        Inherits System.EventArgs

        Private ownerField As DevExpress.XtraReports.UI.XRControl

        Public Property Owner As XRControl
            Get
                Return Me.ownerField
            End Get

            Set(ByVal value As XRControl)
                Me.ownerField = value
            End Set
        End Property

        Private isModifiedField As Boolean

        Public Property IsModified As Boolean
            Get
                Return Me.isModifiedField
            End Get

            Set(ByVal value As Boolean)
                Me.isModifiedField = value
            End Set
        End Property

        Private fieldNameField As String

        Public Property FieldName As String
            Get
                Return Me.fieldNameField
            End Get

            Set(ByVal value As String)
                Me.fieldNameField = value
            End Set
        End Property
    End Class

    Public Class ColumnsCreationEventArgs
        Inherits System.EventArgs

        Private pageWidthField As Integer

        Public ReadOnly Property PageWidth As Integer
            Get
                Return Me.pageWidthField
            End Get
        End Property

        Public Sub New(ByVal pageWidth As Integer)
            Me.pageWidthField = pageWidth
        End Sub

        Private columnsInfoField As System.Collections.Generic.List(Of WebApplication1.ColumnInfo)

        Public Property ColumnsInfo As List(Of WebApplication1.ColumnInfo)
            Get
                Return Me.columnsInfoField
            End Get

            Set(ByVal value As List(Of WebApplication1.ColumnInfo))
                Me.columnsInfoField = value
            End Set
        End Property
    End Class

    Public Class ColumnInfo

        Public Sub New(ByVal gridViewColumn As DevExpress.Web.ASPxGridView.GridViewDataColumn)
            Me.gridViewColumnField = gridViewColumn
        End Sub

        Private gridViewColumnField As DevExpress.Web.ASPxGridView.GridViewDataColumn

        Public ReadOnly Property GridViewColumn As GridViewDataColumn
            Get
                Return Me.gridViewColumnField
            End Get
        End Property

        Private columnCaptionField As String

        Public Property ColumnCaption As String
            Get
                Return Me.columnCaptionField
            End Get

            Set(ByVal value As String)
                Me.columnCaptionField = value
            End Set
        End Property

        Private fieldNameField As String

        Public Property FieldName As String
            Get
                Return Me.fieldNameField
            End Get

            Set(ByVal value As String)
                Me.fieldNameField = value
            End Set
        End Property

        Private columnWidthField As Integer

        Public Property ColumnWidth As Integer
            Get
                Return Me.columnWidthField
            End Get

            Set(ByVal value As Integer)
                Me.columnWidthField = value
            End Set
        End Property

        Private isVisibleField As Boolean

        Public Property IsVisible As Boolean
            Get
                Return Me.isVisibleField
            End Get

            Set(ByVal value As Boolean)
                Me.isVisibleField = value
            End Set
        End Property
    End Class
End Namespace
