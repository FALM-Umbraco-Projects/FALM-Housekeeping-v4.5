using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Profile;
using System.Web.SessionState;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;

using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.propertytype;
using umbraco.DataLayer;

namespace FALMHousekeepingTools.logs
{
	public partial class cleanupLogs : System.Web.UI.Page
	{
		// Initialize an SQL Helper Interface
		protected static ISqlHelper SqlHelper
		{
			get
			{
				return umbraco.BusinessLogic.Application.SqlHelper;
			}
		}

		private int iLastDays = 19;

		string sqlCheckLog = string.Empty;
		
		string sqlDeleteLog = string.Empty;

		protected void Page_Load(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 100000;

			if (!IsPostBack)
			{
				string[] strLogTypes = Enum.GetNames(typeof(LogTypes));

				Array.Sort(strLogTypes);

				ddlLogTypes.DataSource = strLogTypes;
				ddlLogTypes.DataBind();
			}
		}

		protected void btnShowLogs_Click(object sender, EventArgs e)
		{
			// Check logs
			CheckLogs();
		}

		protected void btnDelete_Click(object sender, EventArgs e)
		{
			sqlDeleteLog = "DELETE FROM umbracoLog WHERE (";

			if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				dtpckrDateFrom.DateTime = DateTime.Now.Subtract(new TimeSpan(iLastDays, 0, 0, 0));
				dtpckrDateTo.DateTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0));

				sqlDeleteLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (!dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlDeleteLog += "DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((!dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlDeleteLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) ";
			}
			else if (dtpckrDateFrom.DateTime <= dtpckrDateTo.DateTime)
			{
				sqlDeleteLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else
			{
				string strDateFrom = dtpckrDateFrom.Text;
				string strDateSince = dtpckrDateTo.Text;

				dtpckrDateFrom.DateTime = DateTime.Parse(strDateSince);
				dtpckrDateTo.DateTime = DateTime.Parse(strDateFrom);

				sqlDeleteLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";

				ltrlLogInfo.Text = "Warning: The dates have been reversed because start date (from) was greater than end date (since)";
				ltrlLogInfo.Visible = true;
			}

			if (!txtbNodeID.Text.Equals(string.Empty))
			{
				sqlDeleteLog += "AND NodeId = " + txtbNodeID.Text + " ";
			}

			if (ddlLogTypes.SelectedValue.ToString() != "any")
			{
				sqlDeleteLog += "AND logHeader LIKE '" + ddlLogTypes.SelectedValue.ToString() + "' ";
			}

			if (ddlUsers.SelectedValue.ToString() != "any")
			{
				sqlDeleteLog += "AND UserId = " + ddlUsers.SelectedValue + " ";
			}

			sqlDeleteLog += ")";

			// Loop through DataReader
			IRecordsReader irLogs = SqlHelper.ExecuteReader(sqlDeleteLog);

			ltrlLogTotal.Text = string.Empty;
			btnDelete.Visible = false;
			gvLogTypesList.Visible = false;
			ltrlLogInfo.Text = "<span style='color: #990000; font-weight: bold;'>Selected logs have been succesfully deleted</span>";
		}

		protected void CheckLogs()
		{
			sqlCheckLog = "SELECT logHeader, COUNT(logHeader) AS logCount FROM umbracoLog WHERE (";

			if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				dtpckrDateFrom.DateTime = DateTime.Now.Subtract(new TimeSpan(iLastDays, 0, 0, 0));
				dtpckrDateTo.DateTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0));

				sqlCheckLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (!dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlCheckLog += "DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((!dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlCheckLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) ";
			}
			else if (dtpckrDateFrom.DateTime <= dtpckrDateTo.DateTime)
			{
				sqlCheckLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else
			{
				string strDateFrom = dtpckrDateFrom.Text;
				string strDateSince = dtpckrDateTo.Text;

				dtpckrDateFrom.DateTime = DateTime.Parse(strDateSince);
				dtpckrDateTo.DateTime = DateTime.Parse(strDateFrom);

				sqlCheckLog += "DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";

				ltrlLogInfo.Text = "Warning: The dates have been reversed because start date (from) was greater than end date (since)";
				ltrlLogInfo.Visible = true;
			}

			if (!txtbNodeID.Text.Equals(string.Empty))
			{
				sqlCheckLog += "AND NodeId = " + int.Parse(txtbNodeID.Text) + " ";
			}

			if (ddlLogTypes.SelectedValue.ToString() != "any")
			{
				sqlCheckLog += "AND logHeader LIKE '" + ddlLogTypes.SelectedValue.ToString() + "' ";
			}

			if (ddlUsers.SelectedValue.ToString() != "any")
			{
				sqlCheckLog += "AND UserId = " + ddlUsers.SelectedValue + " ";
			}

			sqlCheckLog += ") GROUP BY logHeader ";

			sqlCheckLog += "ORDER BY logHeader";

			// Create DataTable
			DataTable DataReaderTable = new DataTable();
			DataColumn dc1 = new DataColumn("Log Type", typeof(string));
			DataColumn dc2 = new DataColumn("Log Count", typeof(int));

			DataReaderTable.Columns.Add(dc1);
			DataReaderTable.Columns.Add(dc2);

			// Loop through DataReader
			IRecordsReader irLogs = SqlHelper.ExecuteReader(sqlCheckLog);

			while (irLogs.Read())
			{
				// Set up DataRow object
				DataRow dr = DataReaderTable.NewRow();

				dr[0] = irLogs.GetString("logHeader");
				dr[1] = irLogs.GetInt("logCount");

				// Add rows to existing DataTable
				DataReaderTable.Rows.Add(dr);
			}

			// Create DataView to support our column sorting
			DataView Source = DataReaderTable.DefaultView;

			gvLogTypesList.DataSource = Source;
			gvLogTypesList.DataBind();
			gvLogTypesList.Visible = true;
			
			//Loop through DataReader
			if (Source.Table.Rows.Count != 0)
			{
				ltrlLogInfo.Text = "The table below summarizes the type and number of logs that will be deleted<br />";

				btnDelete.Visible = true;
			}
			else
			{
				ltrlLogTotal.Text = string.Empty;
				ltrlLogInfo.Text = "WARNING: no logs to delete up to '" + dtpckrDateFrom.DateTime.ToShortDateString() + "'";
				btnDelete.Visible = false;
			}
		}
	}
}
