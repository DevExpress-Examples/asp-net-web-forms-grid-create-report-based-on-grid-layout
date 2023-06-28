using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using DevExpress.XtraPrinting;
using System.Net.Mime;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting.Shape;
using System.Drawing;

namespace WebApplication1 {
    public partial class _Default : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {

        }
        protected void ASPxButton2_Click(object sender, EventArgs e) {
            ReportGeneratonHelper generator = new ReportGeneratonHelper();
            generator.CustomizeColumnsCollection += new CustomizeColumnsCollectionEventHandler(generator_CustomizeColumnsCollection);
            generator.CustomizeColumn += new CustomizeColumnEventHandler(generator_CustomizeColumn);
            XtraReport report = generator.GenerateReport(ASPxGridView1, AccessDataSource1);
            generator.WritePdfToResponse(Response, "test.pdf", System.Net.Mime.DispositionTypeNames.Attachment.ToString());

        }

        void generator_CustomizeColumn(object source, ControlCustomizationEventArgs e) {
            if(e.FieldName == "Discontinued") {
                XRShape control = new XRShape();
                control.SizeF = e.Owner.SizeF;
                control.LocationF = new System.Drawing.PointF(0, 0);
                e.Owner.Controls.Add(control);
                control.Shape = new ShapeStar() { StarPointCount = 5, Concavity = 30 };
                control.BeforePrint += new BeforePrintEventHandler(control_BeforePrint);
                e.IsModified = true;
            }
        }

        void control_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e) {
            if(Convert.ToBoolean(((XRShape)sender).Report.GetCurrentColumnValue("Discontinued")) == true)
                ((XRShape)sender).FillColor = Color.Yellow;
            else
                ((XRShape)sender).FillColor = Color.White;
        }

        void generator_CustomizeColumnsCollection(object source, ColumnsCreationEventArgs e) {
            e.ColumnsInfo[1].ColumnWidth *= 2;
            e.ColumnsInfo[4].ColumnWidth += 30;
            e.ColumnsInfo[3].ColumnWidth -= 30;
            e.ColumnsInfo[e.ColumnsInfo.Count - 1].IsVisible = false;
        }

    }
}
