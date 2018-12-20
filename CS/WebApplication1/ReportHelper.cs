using System;
using System.Collections.Generic;
using System.Web;
using DevExpress.XtraReports.UI;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.Web;
using System.Drawing;
using DevExpress.Data;
using DevExpress.XtraPrinting;
using System.Collections.ObjectModel;
using System.Collections;
using System.Net.Mime;
using System.IO;

namespace WebApplication1 {

    public delegate void CustomizeColumnsCollectionEventHandler(object source, ColumnsCreationEventArgs e);
    public delegate void CustomizeColumnEventHandler(object source, ControlCustomizationEventArgs e);

    public class ReportGeneratonHelper {
        XtraReport report;
        const int initialGroupOffset = 0;
        const int subGroupOffset = 10;
        const int bandHeight = 20;
        const bool shouldRepeatGroupHeadersOnEveryPage = false;
        Hashtable detailsInfo = new Hashtable();

        public event CustomizeColumnsCollectionEventHandler CustomizeColumnsCollection;
        public event CustomizeColumnEventHandler CustomizeColumn;


        public XtraReport GenerateReport(ASPxGridView dataGrid, object dataSource) {
            report = new XtraReport();
            report.Landscape = true;
            report.PaperKind = System.Drawing.Printing.PaperKind.Letter;

            IDataSource webDataSource = dataSource as IDataSource;
            IList listDataSource = dataSource as IList;
            if(webDataSource != null)
                InitDataSource(webDataSource, dataGrid.DataMember);
            else if(listDataSource != null)
                InitDataSource(listDataSource);
            else throw new ArgumentException("dataSource");
            InitDetailsAndPageHeader(dataGrid);
            InitSortings(dataGrid);
            InitGroupHeaders(dataGrid);
            InitGroupSummaries(dataGrid);
            InitFilters(dataGrid);
            InitTotalSummaries(dataGrid);
            return report;
        }

        void InitGroupSummaries(ASPxGridView aspxGridView1) {
            if(aspxGridView1.GroupSummary.Count > 0) {
                ReadOnlyCollection<GridViewDataColumn> groupedColumns = aspxGridView1.GetGroupedColumns();
                for(int i = groupedColumns.Count - 1; i >= 0; i--) {
                    GroupFooterBand footerBand = new GroupFooterBand();
                    footerBand.Height = bandHeight;
                    report.Bands.Add(footerBand);
                    footerBand.BackColor = Color.LightGray;
                    foreach(ASPxSummaryItem item in aspxGridView1.GroupSummary) {
                        GridViewColumn col = aspxGridView1.Columns[item.FieldName];
                        if(col != null) {
                            if(detailsInfo.Contains(col)) {
                                XRLabel label = new XRLabel();
                                label.LocationF = ((XRTableCell)detailsInfo[col]).LocationF;
                                label.SizeF = ((XRTableCell)detailsInfo[col]).SizeF;
                                label.DataBindings.Add("Text", null, ((GridViewDataColumn)col).FieldName);
                                label.Summary = new XRSummary() { Running = SummaryRunning.Group };
                                label.Summary.FormatString = item.DisplayFormat;
                                label.Summary.Func = GetSummaryFunc(item.SummaryType);
                                footerBand.Controls.Add(label);
                            }
                        }
                    }

                }
            }
        }
        void InitTotalSummaries(ASPxGridView aspxGridView1) {
            if(aspxGridView1.TotalSummary.Count > 0) {
                report.Bands.Add(new ReportFooterBand() { HeightF = bandHeight });
                foreach(ASPxSummaryItem item in aspxGridView1.TotalSummary) {
                    GridViewColumn col = aspxGridView1.Columns[item.ShowInColumn == string.Empty ? item.FieldName : item.ShowInColumn];
                    if(col != null) {
                        if(detailsInfo.Contains(col)) {
                            XRLabel label = new XRLabel();
                            label.LocationF = ((XRTableCell)detailsInfo[col]).LocationF;
                            label.SizeF = ((XRTableCell)detailsInfo[col]).SizeF;
                            label.DataBindings.Add("Text", null, ((GridViewDataColumn)col).FieldName);
                            label.Summary = new XRSummary() { Running = SummaryRunning.Report };
                            label.Summary.FormatString = item.DisplayFormat;
                            label.Summary.Func = GetSummaryFunc(item.SummaryType);
                            report.Bands[BandKind.ReportFooter].Controls.Add(label);
                        }
                    }
                }
            }
        }

        void InitDataSource(IDataSource dataSource, string dataMember) {
            DataSourceView view = dataSource.GetView(dataMember);
            view.Select(DataSourceSelectArguments.Empty, data => report.DataSource = data);
        }

        void InitDataSource(IList dataSource) {
            report.DataSource = dataSource;
        }

        void InitGroupHeaders(ASPxGridView aspxGridView1) {
            ReadOnlyCollection<GridViewDataColumn> groupedColumns = aspxGridView1.GetGroupedColumns();
            for(int i = groupedColumns.Count - 1; i >= 0; i--) {
                {
                    GridViewDataColumn groupedColumn = groupedColumns[i];
                    GroupHeaderBand gb = new GroupHeaderBand();
                    gb.Height = bandHeight;
                    XRLabel l = new XRLabel();
                    l.Text = groupedColumn.FieldName + ": [" + groupedColumn.FieldName + "]";
                    l.LocationF = new PointF(initialGroupOffset + i * 10, 0);
                    l.BackColor = Color.Beige;
                    l.SizeF = new SizeF((report.PageWidth - (report.Margins.Left + report.Margins.Right)) - (initialGroupOffset + i * subGroupOffset), bandHeight);
                    gb.Controls.Add(l);
                    gb.RepeatEveryPage = shouldRepeatGroupHeadersOnEveryPage;
                    GroupField gf = new GroupField(groupedColumn.FieldName, groupedColumn.SortOrder == ColumnSortOrder.Ascending ? XRColumnSortOrder.Ascending : XRColumnSortOrder.Descending);
                    gb.GroupFields.Add(gf);
                    report.Bands.Add(gb);
                }
            }
        }
        void InitSortings(ASPxGridView aspxGridView1) {
            List<GridViewDataColumn> columns = GetVisibleDataColumns(aspxGridView1);
            ReadOnlyCollection<GridViewDataColumn> groupedColumns = aspxGridView1.GetGroupedColumns();
            for(int i = 0; i < columns.Count; i++) {
                if(!groupedColumns.Contains(columns[i])) {
                    if(columns[i].SortOrder != ColumnSortOrder.None)
                        ((DetailBand)report.Bands[BandKind.Detail]).SortFields.Add(new GroupField(columns[i].FieldName, columns[i].SortOrder == ColumnSortOrder.Ascending ? XRColumnSortOrder.Ascending : XRColumnSortOrder.Descending));
                }
            }
        }
        void InitFilters(ASPxGridView aspxGridView1) {
            report.FilterString = aspxGridView1.FilterExpression;
        }
 void InitDetailsAndPageHeader(ASPxGridView aspxGridView1) {
            ReadOnlyCollection<GridViewDataColumn> groupedColumns = aspxGridView1.GetGroupedColumns();

            int pagewidth = (report.PageWidth - (report.Margins.Left + report.Margins.Right)) - groupedColumns.Count * subGroupOffset;
            List<ColumnInfo> columns = GetColumnsInfo(aspxGridView1, pagewidth);
            if (CustomizeColumnsCollection != null)
                CustomizeColumnsCollection(report, new ColumnsCreationEventArgs(pagewidth) { ColumnsInfo = columns });

            report.Bands.Add(new DetailBand() { HeightF = bandHeight });
            report.Bands.Add(new PageHeaderBand() { HeightF = bandHeight });

            XRTable headerTable = new XRTable();
            XRTableRow row = new XRTableRow();
            XRTable detailTable = new XRTable();
            XRTableRow row2 = new XRTableRow();

            for(int i = 0; i < columns.Count; i++) {
                if(columns[i].IsVisible) {
                    XRTableCell cell = new XRTableCell();
                    cell.Width = columns[i].ColumnWidth;
                    cell.Text = columns[i].FieldName;
                    row.Cells.Add(cell);

                    XRTableCell cell2 = new XRTableCell();
                    cell2.Width = columns[i].ColumnWidth;
                    if (CustomizeColumn != null) {
                        ControlCustomizationEventArgs cc = new ControlCustomizationEventArgs() { FieldName = columns[i].FieldName, IsModified = false, Owner = cell2 };
                        CustomizeColumn(report, cc);
                        if(cc.IsModified == false)
                            cell2.DataBindings.Add("Text", null, columns[i].FieldName);
                    }
                    detailsInfo.Add(columns[i].GridViewColumn, cell2);
                    row2.Cells.Add(cell2);
                }
            }
            headerTable.Rows.Add(row);
            headerTable.Width = pagewidth;
            headerTable.LocationF = new PointF(groupedColumns.Count * subGroupOffset,0);
            headerTable.Borders = BorderSide.Bottom;

            detailTable.Rows.Add(row2);
            detailTable.LocationF = new PointF(groupedColumns.Count * subGroupOffset,0);
            detailTable.Width = pagewidth;

            report.Bands[BandKind.PageHeader].Controls.Add(headerTable);
            report.Bands[BandKind.Detail].Controls.Add(detailTable);
        }

        private List<ColumnInfo> GetColumnsInfo(ASPxGridView aspxGridView1, int pagewidth) {
            List<ColumnInfo> columns = new List<ColumnInfo>();
            List<GridViewDataColumn> visibleColumns = GetVisibleDataColumns(aspxGridView1);
            foreach(GridViewDataColumn dataColumn in visibleColumns) {
                ColumnInfo column = new ColumnInfo(dataColumn) { ColumnCaption = string.IsNullOrEmpty(dataColumn.Caption) ? dataColumn.FieldName : dataColumn.Caption, ColumnWidth = ((int)pagewidth / visibleColumns.Count), FieldName = dataColumn.FieldName, IsVisible = true };
                columns.Add(column);
            }
            return columns;

        }
        List<GridViewDataColumn> GetVisibleDataColumns(ASPxGridView aspxGridView1) {
            List<GridViewDataColumn> columns = new List<GridViewDataColumn>();
            foreach(GridViewColumn column in aspxGridView1.VisibleColumns) {
                if(column is GridViewDataColumn)
                    columns.Add(column as GridViewDataColumn);
            }
            return columns;
        }
        private SummaryFunc GetSummaryFunc(SummaryItemType summaryItemType) {
            switch(summaryItemType) {
                case SummaryItemType.Sum:
                    return SummaryFunc.Sum;
                case SummaryItemType.Average:
                    return SummaryFunc.Avg;
                case SummaryItemType.Max:
                    return SummaryFunc.Max;
                case SummaryItemType.Min:
                    return SummaryFunc.Min;
                default:
                    return SummaryFunc.Custom;
            }
        }


        internal void WritePdfToResponse(HttpResponse Response, string fileName, string type) {
            report.CreateDocument(false);
            using(MemoryStream ms = new MemoryStream()) {
                report.ExportToPdf(ms);
                ms.Seek(0, SeekOrigin.Begin);
                WriteResponse(Response, ms.ToArray(), type, fileName);
            }
        }
        public static void WriteResponse(HttpResponse response, byte[] filearray, string type,string fileName) {
            response.ClearContent();
            response.Buffer = true;
            response.Cache.SetCacheability(HttpCacheability.Private);
            response.ContentType = "application/pdf";
            ContentDisposition contentDisposition = new ContentDisposition();
            contentDisposition.FileName = fileName;
            contentDisposition.DispositionType = type;
            response.AddHeader("Content-Disposition", contentDisposition.ToString());
            response.BinaryWrite(filearray);
            HttpContext.Current.ApplicationInstance.CompleteRequest();
            try {
                response.End();
            }
            catch(System.Threading.ThreadAbortException) {
            }

        }
    }
    public class ControlCustomizationEventArgs : EventArgs {
        XRControl owner;

        public XRControl Owner {
            get { return owner; }
            set { owner = value; }
        }
        bool isModified;

        public bool IsModified {
            get { return isModified; }
            set { isModified = value; }
        }
        string fieldName;

        public string FieldName {
            get { return fieldName; }
            set { fieldName = value; }
        }

    }
    public class ColumnsCreationEventArgs : EventArgs {
        int pageWidth;
        public int PageWidth {
            get { return pageWidth; }
        }
        public ColumnsCreationEventArgs(int pageWidth) {
            this.pageWidth = pageWidth;
        }
        List<ColumnInfo> columnsInfo;

        public List<ColumnInfo> ColumnsInfo {
            get { return columnsInfo; }
            set { columnsInfo = value; }
        }
    }
    public class ColumnInfo {
        public ColumnInfo(GridViewDataColumn gridViewColumn) {
            this.gridViewColumn = gridViewColumn;
        }
        GridViewDataColumn gridViewColumn;

        public GridViewDataColumn GridViewColumn {
            get { return gridViewColumn; }
        }
        

        string columnCaption;
        public string ColumnCaption {
            get { return columnCaption; }
            set { columnCaption = value; }
        }
        string fieldName;

        public string FieldName {
            get { return fieldName; }
            set { fieldName = value; }
        }
        int columnWidth;

        public int ColumnWidth {
            get { return columnWidth; }
            set { columnWidth = value; }
        }
        bool isVisible;

        public bool IsVisible {
            get { return isVisible; }
            set { isVisible = value; }
        }


    }

}