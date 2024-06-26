<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/128538561/23.1.3%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/E4476)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->
# Grid View for ASP.NET Web Forms - How to create a report based on grid data
This example demonstrates how to dynamically create a [DevExpress report](https://docs.devexpress.com/XtraReports/DevExpress.XtraReports.UI.XtraReport) based on the [ASPxGridView](https://docs.devexpress.com/AspNet/5823/components/grid-view) control's data and layout. The resulting report maintains all sorting, grouping, and filtering settings applied to the grid. The example code exports this report to a PDF file, but you can also insert the resulting report into another report.

![Create a Report Based on Grid View Data](image.png)

Follow the steps below to create a report based on the Grid View control's data:

1. Create an instance of the [XtraReport](https://docs.devexpress.com/XtraReports/DevExpress.XtraReports.UI.XtraReport) class.
2. Bind the instance to the Grid View's data source.
3. Based on the Grid layout, create [bands](https://docs.devexpress.com/XtraReports/2587/detailed-guide-to-devexpress-reporting/introduction-to-banded-reports) and add them to the report.
4. Create [report controls](https://docs.devexpress.com/XtraReports/2605/detailed-guide-to-devexpress-reporting/use-report-controls) and add them to the bands.

## Files to Review

* [Default.aspx](./CS/WebApplication1/Default.aspx) (VB: [Default.aspx](./VB/WebApplication1/Default.aspx))
* [Default.aspx.cs](./CS/WebApplication1/Default.aspx.cs) (VB: [Default.aspx.vb](./VB/WebApplication1/Default.aspx.vb))
* [ReportHelper.cs](./CS/WebApplication1/ReportHelper.cs) (VB: [ReportHelper.vb](./VB/WebApplication1/ReportHelper.vb))

## Documentation

* [Create a Report in Code](https://docs.devexpress.com/XtraReports/115726/detailed-guide-to-devexpress-reporting/reporting-api/create-reports-in-code)

## More examples

* [Grid View for ASP.NET MVC - How to create a report based on grid data](https://github.com/DevExpress-Examples/how-to-convert-and-then-print-an-gridview-extension-by-using-the-xtrareport-e4755)
<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=asp-net-web-forms-grid-create-report-based-on-grid-layout&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=asp-net-web-forms-grid-create-report-based-on-grid-layout&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
