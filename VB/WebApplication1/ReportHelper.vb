Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports DevExpress.XtraReports.UI
Imports System.Data
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports DevExpress.Web
Imports System.Drawing
Imports DevExpress.Data
Imports DevExpress.XtraPrinting
Imports System.Collections.ObjectModel
Imports System.Collections
Imports System.Net.Mime
Imports System.IO

Namespace WebApplication1

    Public Delegate Sub CustomizeColumnsCollectionEventHandler(ByVal source As Object, ByVal e As ColumnsCreationEventArgs)
    Public Delegate Sub CustomizeColumnEventHandler(ByVal source As Object, ByVal e As ControlCustomizationEventArgs)

    Public Class ReportGeneratonHelper
        Private report As XtraReport
        Private Const initialGroupOffset As Integer = 0
        Private Const subGroupOffset As Integer = 10
        Private Const bandHeight As Integer = 20
        Private Const shouldRepeatGroupHeadersOnEveryPage As Boolean = False
        Private detailsInfo As New Hashtable()

        Public Event CustomizeColumnsCollection As CustomizeColumnsCollectionEventHandler
        Public Event CustomizeColumn As CustomizeColumnEventHandler


        Public Function GenerateReport(ByVal dataGrid As ASPxGridView, ByVal dataSource As Object) As XtraReport
            report = New XtraReport()
            report.Landscape = True
            report.PaperKind = System.Drawing.Printing.PaperKind.Letter

            Dim webDataSource As IDataSource = TryCast(dataSource, IDataSource)
            Dim listDataSource As IList = TryCast(dataSource, IList)
            If webDataSource IsNot Nothing Then
                InitDataSource(webDataSource, dataGrid.DataMember)
            ElseIf listDataSource IsNot Nothing Then
                InitDataSource(listDataSource)
            Else
                Throw New ArgumentException("dataSource")
            End If
            InitDetailsAndPageHeader(dataGrid)
            InitSortings(dataGrid)
            InitGroupHeaders(dataGrid)
            InitGroupSummaries(dataGrid)
            InitFilters(dataGrid)
            InitTotalSummaries(dataGrid)
            Return report
        End Function
        Private Sub InitGroupSummaries(ByVal aspxGridView1 As ASPxGridView)
            If aspxGridView1.GroupSummary.Count > 0 Then
                Dim groupedColumns As ReadOnlyCollection(Of GridViewDataColumn) = aspxGridView1.GetGroupedColumns()
                For i As Integer = groupedColumns.Count - 1 To 0 Step -1
                    Dim footerBand As New GroupFooterBand()
                    footerBand.Height = bandHeight
                    report.Bands.Add(footerBand)
                    footerBand.BackColor = Color.LightGray
                    For Each item As ASPxSummaryItem In aspxGridView1.GroupSummary
                        Dim col As GridViewColumn = aspxGridView1.Columns(item.FieldName)
                        If col IsNot Nothing Then
                            If detailsInfo.Contains(col) Then
                                Dim label As New XRLabel()
                                label.LocationF = DirectCast(detailsInfo(col), XRTableCell).LocationF
                                label.SizeF = DirectCast(detailsInfo(col), XRTableCell).SizeF
                                label.DataBindings.Add("Text", Nothing, CType(col, GridViewDataColumn).FieldName)
                                label.Summary = New XRSummary() With {.Running = SummaryRunning.Group}
                                label.Summary.FormatString = item.DisplayFormat
                                label.Summary.Func = GetSummaryFunc(item.SummaryType)
                                footerBand.Controls.Add(label)
                            End If
                        End If
                    Next item

                Next i
            End If
        End Sub
        Private Sub InitTotalSummaries(ByVal aspxGridView1 As ASPxGridView)
            If aspxGridView1.TotalSummary.Count > 0 Then
                report.Bands.Add(New ReportFooterBand() With {.HeightF = bandHeight})
                For Each item As ASPxSummaryItem In aspxGridView1.TotalSummary
                    Dim col As GridViewColumn = aspxGridView1.Columns(If(item.ShowInColumn = String.Empty, item.FieldName, item.ShowInColumn))
                    If col IsNot Nothing Then
                        If detailsInfo.Contains(col) Then
                            Dim label As New XRLabel()
                            label.LocationF = DirectCast(detailsInfo(col), XRTableCell).LocationF
                            label.SizeF = DirectCast(detailsInfo(col), XRTableCell).SizeF
                            label.DataBindings.Add("Text", Nothing, CType(col, GridViewDataColumn).FieldName)
                            label.Summary = New XRSummary() With {.Running = SummaryRunning.Report}
                            label.Summary.FormatString = item.DisplayFormat
                            label.Summary.Func = GetSummaryFunc(item.SummaryType)
                            report.Bands(BandKind.ReportFooter).Controls.Add(label)
                        End If
                    End If
                Next item
            End If
        End Sub

        Private Sub InitDataSource(ByVal dataSource As IDataSource, ByVal dataMember As String)
            Dim view As DataSourceView = dataSource.GetView(dataMember)
            view.Select(DataSourceSelectArguments.Empty, Sub(data) report.DataSource = data)
        End Sub

        Private Sub InitDataSource(ByVal dataSource As IList)
            report.DataSource = dataSource
        End Sub
        Private Sub InitGroupHeaders(ByVal aspxGridView1 As ASPxGridView)
            Dim groupedColumns As ReadOnlyCollection(Of GridViewDataColumn) = aspxGridView1.GetGroupedColumns()
            For i As Integer = groupedColumns.Count - 1 To 0 Step -1
                If True Then
                    Dim groupedColumn As GridViewDataColumn = groupedColumns(i)
                    Dim gb As New GroupHeaderBand()
                    gb.Height = bandHeight
                    Dim l As New XRLabel()
                    l.Text = groupedColumn.FieldName & ": [" & groupedColumn.FieldName & "]"
                    l.LocationF = New PointF(initialGroupOffset + i * 10, 0)
                    l.BackColor = Color.Beige
                    l.SizeF = New SizeF((report.PageWidth - (report.Margins.Left + report.Margins.Right)) - (initialGroupOffset + i * subGroupOffset), bandHeight)
                    gb.Controls.Add(l)
                    gb.RepeatEveryPage = shouldRepeatGroupHeadersOnEveryPage
                    Dim gf As New GroupField(groupedColumn.FieldName,If(groupedColumn.SortOrder = ColumnSortOrder.Ascending, XRColumnSortOrder.Ascending, XRColumnSortOrder.Descending))
                    gb.GroupFields.Add(gf)
                    report.Bands.Add(gb)
                End If
            Next i
        End Sub
        Private Sub InitSortings(ByVal aspxGridView1 As ASPxGridView)
            Dim columns As List(Of GridViewDataColumn) = GetVisibleDataColumns(aspxGridView1)
            Dim groupedColumns As ReadOnlyCollection(Of GridViewDataColumn) = aspxGridView1.GetGroupedColumns()
            For i As Integer = 0 To columns.Count - 1
                If Not groupedColumns.Contains(columns(i)) Then
                    If columns(i).SortOrder <> ColumnSortOrder.None Then
                        CType(report.Bands(BandKind.Detail), DetailBand).SortFields.Add(New GroupField(columns(i).FieldName,If(columns(i).SortOrder = ColumnSortOrder.Ascending, XRColumnSortOrder.Ascending, XRColumnSortOrder.Descending)))
                    End If
                End If
            Next i
        End Sub
        Private Sub InitFilters(ByVal aspxGridView1 As ASPxGridView)
            report.FilterString = aspxGridView1.FilterExpression
        End Sub
 Private Sub InitDetailsAndPageHeader(ByVal aspxGridView1 As ASPxGridView)
            Dim groupedColumns As ReadOnlyCollection(Of GridViewDataColumn) = aspxGridView1.GetGroupedColumns()

            Dim pagewidth As Integer = (report.PageWidth - (report.Margins.Left + report.Margins.Right)) - groupedColumns.Count * subGroupOffset
            Dim columns As List(Of ColumnInfo) = GetColumnsInfo(aspxGridView1, pagewidth)
            RaiseEvent CustomizeColumnsCollection(report, New ColumnsCreationEventArgs(pagewidth) With {.ColumnsInfo = columns})

            report.Bands.Add(New DetailBand() With {.HeightF = bandHeight})
            report.Bands.Add(New PageHeaderBand() With {.HeightF = bandHeight})

            Dim headerTable As New XRTable()
            Dim row As New XRTableRow()
            Dim detailTable As New XRTable()
            Dim row2 As New XRTableRow()

            For i As Integer = 0 To columns.Count - 1
                If columns(i).IsVisible Then
                    Dim cell As New XRTableCell()
                    cell.Width = columns(i).ColumnWidth
                    cell.Text = columns(i).FieldName
                    row.Cells.Add(cell)

                    Dim cell2 As New XRTableCell()
                    cell2.Width = columns(i).ColumnWidth
                    Dim cc As New ControlCustomizationEventArgs() With { _
                        .FieldName = columns(i).FieldName, _
                        .IsModified = False, _
                        .Owner = cell2 _
                    }
                    RaiseEvent CustomizeColumn(report, cc)
                    If cc.IsModified = False Then
                        cell2.DataBindings.Add("Text", Nothing, columns(i).FieldName)
                    End If
                    detailsInfo.Add(columns(i).GridViewColumn, cell2)
                    row2.Cells.Add(cell2)
                End If
            Next i
            headerTable.Rows.Add(row)
            headerTable.Width = pagewidth
            headerTable.LocationF = New PointF(groupedColumns.Count * subGroupOffset,0)
            headerTable.Borders = BorderSide.Bottom

            detailTable.Rows.Add(row2)
            detailTable.LocationF = New PointF(groupedColumns.Count * subGroupOffset,0)
            detailTable.Width = pagewidth

            report.Bands(BandKind.PageHeader).Controls.Add(headerTable)
            report.Bands(BandKind.Detail).Controls.Add(detailTable)
 End Sub

        Private Function GetColumnsInfo(ByVal aspxGridView1 As ASPxGridView, ByVal pagewidth As Integer) As List(Of ColumnInfo)
            Dim columns As New List(Of ColumnInfo)()
            Dim visibleColumns As List(Of GridViewDataColumn) = GetVisibleDataColumns(aspxGridView1)
            For Each dataColumn As GridViewDataColumn In visibleColumns
                Dim column As New ColumnInfo(dataColumn) With { _
                    .ColumnCaption = If(String.IsNullOrEmpty(dataColumn.Caption), dataColumn.FieldName, dataColumn.Caption), _
                    .ColumnWidth = (CInt(pagewidth) \ visibleColumns.Count), _
                    .FieldName = dataColumn.FieldName, _
                    .IsVisible = True _
                }
                columns.Add(column)
            Next dataColumn
            Return columns

        End Function
        Private Function GetVisibleDataColumns(ByVal aspxGridView1 As ASPxGridView) As List(Of GridViewDataColumn)
            Dim columns As New List(Of GridViewDataColumn)()
            For Each column As GridViewColumn In aspxGridView1.VisibleColumns
                If TypeOf column Is GridViewDataColumn Then
                    columns.Add(TryCast(column, GridViewDataColumn))
                End If
            Next column
            Return columns
        End Function
        Private Function GetSummaryFunc(ByVal summaryItemType As SummaryItemType) As SummaryFunc
            Select Case summaryItemType
                Case SummaryItemType.Sum
                    Return SummaryFunc.Sum
                Case SummaryItemType.Average
                    Return SummaryFunc.Avg
                Case SummaryItemType.Max
                    Return SummaryFunc.Max
                Case SummaryItemType.Min
                    Return SummaryFunc.Min
                Case Else
                    Return SummaryFunc.Custom
            End Select
        End Function


        Friend Sub WritePdfToResponse(ByVal Response As HttpResponse, ByVal fileName As String, ByVal type As String)
            report.CreateDocument(False)
            Using ms As New MemoryStream()
                report.ExportToPdf(ms)
                ms.Seek(0, SeekOrigin.Begin)
                WriteResponse(Response, ms.ToArray(), type, fileName)
            End Using
        End Sub
        Public Shared Sub WriteResponse(ByVal response As HttpResponse, ByVal filearray() As Byte, ByVal type As String, ByVal fileName As String)
            response.ClearContent()
            response.Buffer = True
            response.Cache.SetCacheability(HttpCacheability.Private)
            response.ContentType = "application/pdf"
            Dim contentDisposition As New ContentDisposition()
            contentDisposition.FileName = fileName
            contentDisposition.DispositionType = type
            response.AddHeader("Content-Disposition", contentDisposition.ToString())
            response.BinaryWrite(filearray)
            HttpContext.Current.ApplicationInstance.CompleteRequest()
            Try
                response.End()
            Catch e1 As System.Threading.ThreadAbortException
            End Try

        End Sub
    End Class
    Public Class ControlCustomizationEventArgs
        Inherits EventArgs


        Private owner_Renamed As XRControl

        Public Property Owner() As XRControl
            Get
                Return owner_Renamed
            End Get
            Set(ByVal value As XRControl)
                owner_Renamed = value
            End Set
        End Property

        Private isModified_Renamed As Boolean

        Public Property IsModified() As Boolean
            Get
                Return isModified_Renamed
            End Get
            Set(ByVal value As Boolean)
                isModified_Renamed = value
            End Set
        End Property

        Private fieldName_Renamed As String

        Public Property FieldName() As String
            Get
                Return fieldName_Renamed
            End Get
            Set(ByVal value As String)
                fieldName_Renamed = value
            End Set
        End Property

    End Class
    Public Class ColumnsCreationEventArgs
        Inherits EventArgs


        Private pageWidth_Renamed As Integer
        Public ReadOnly Property PageWidth() As Integer
            Get
                Return pageWidth_Renamed
            End Get
        End Property
        Public Sub New(ByVal pageWidth As Integer)
            Me.pageWidth_Renamed = pageWidth
        End Sub

        Private columnsInfo_Renamed As List(Of ColumnInfo)

        Public Property ColumnsInfo() As List(Of ColumnInfo)
            Get
                Return columnsInfo_Renamed
            End Get
            Set(ByVal value As List(Of ColumnInfo))
                columnsInfo_Renamed = value
            End Set
        End Property
    End Class
    Public Class ColumnInfo
        Public Sub New(ByVal gridViewColumn As GridViewDataColumn)
            Me.gridViewColumn_Renamed = gridViewColumn
        End Sub

        Private gridViewColumn_Renamed As GridViewDataColumn

        Public ReadOnly Property GridViewColumn() As GridViewDataColumn
            Get
                Return gridViewColumn_Renamed
            End Get
        End Property



        Private columnCaption_Renamed As String
        Public Property ColumnCaption() As String
            Get
                Return columnCaption_Renamed
            End Get
            Set(ByVal value As String)
                columnCaption_Renamed = value
            End Set
        End Property

        Private fieldName_Renamed As String

        Public Property FieldName() As String
            Get
                Return fieldName_Renamed
            End Get
            Set(ByVal value As String)
                fieldName_Renamed = value
            End Set
        End Property

        Private columnWidth_Renamed As Integer

        Public Property ColumnWidth() As Integer
            Get
                Return columnWidth_Renamed
            End Get
            Set(ByVal value As Integer)
                columnWidth_Renamed = value
            End Set
        End Property

        Private isVisible_Renamed As Boolean

        Public Property IsVisible() As Boolean
            Get
                Return isVisible_Renamed
            End Get
            Set(ByVal value As Boolean)
                isVisible_Renamed = value
            End Set
        End Property


    End Class

End Namespace