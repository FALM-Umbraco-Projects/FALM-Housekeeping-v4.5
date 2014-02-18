using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Configuration;
using System.Web.UI.HtmlControls;
using System.Data;

using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.propertytype;
using umbraco.DataLayer;
using umbraco.IO;

namespace FALMHousekeepingTools.logs
{
	public partial class showLogs : System.Web.UI.Page
	{
		// Initialize an SQL Helper Interface
		protected static ISqlHelper SqlHelper
		{
			get { return umbraco.BusinessLogic.Application.SqlHelper; }
		}

		// Current Logged User
		protected User userCurrent = umbraco.BusinessLogic.User.GetCurrent();

		private int iLastDays = 19;

		private DataView dgCache = new DataView();

		string sqlReadLog = string.Empty;

		protected void Page_Load(object sender,EventArgs e)
		{
			Server.ScriptTimeout = 100000;

			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(ui.Culture(umbraco.BasePages.UmbracoEnsuredPage.CurrentUser));
			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;

			if (!IsPostBack)
			{
				string[] strLogTypes = Enum.GetNames(typeof(LogTypes));

				Array.Sort(strLogTypes);

				ddlLogTypes.DataSource = strLogTypes;
				ddlLogTypes.DataBind();
			}
		}

		protected void btnShowLogs_Click(object sender,EventArgs e)
		{
			// Reset cache
			Cache.Remove("dgCache");

			// Reset viewstate sort order
			ViewState["SortOrder"] = null;

			// Reset GridView Page to the top
			gvLogTypesList.PageIndex = 0;

			// Assign default column sort order
			GetLogs("DateStamp desc");
		}

		protected void GetLogs(string ColumnOrder)
		{
			sqlReadLog = "SELECT umbracoLog.userId, umbracoUser.userName, umbracoUser.userLogin, umbracoLog.NodeId, umbracoNode.text AS nodeName, umbracoLog.DateStamp, umbracoLog.logHeader, umbracoLog.logComment ";
			sqlReadLog += "FROM umbracoLog INNER JOIN umbracoUser ON umbracoLog.userId = umbracoUser.id LEFT OUTER JOIN umbracoNode ON umbracoLog.NodeId = umbracoNode.id ";
			sqlReadLog += "WHERE (";

			if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				dtpckrDateFrom.DateTime = DateTime.Now.Subtract(new TimeSpan(iLastDays,0,0,0));
				dtpckrDateTo.DateTime = DateTime.Now.Subtract(new TimeSpan(0,0,0,0));

				sqlReadLog += "umbracoLog.DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND umbracoLog.DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((dtpckrDateFrom.Text.Equals(string.Empty)) && (!dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlReadLog += "umbracoLog.DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else if ((!dtpckrDateFrom.Text.Equals(string.Empty)) && (dtpckrDateTo.Text.Equals(string.Empty)))
			{
				sqlReadLog += "umbracoLog.DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) ";
			}
			else if (dtpckrDateFrom.DateTime <= dtpckrDateTo.DateTime)
			{
				sqlReadLog += "umbracoLog.DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND umbracoLog.DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";
			}
			else
			{
				string strDateFrom = dtpckrDateFrom.Text;
				string strDateTo = dtpckrDateTo.Text;

				dtpckrDateFrom.DateTime = DateTime.Parse(strDateTo);
				dtpckrDateTo.DateTime = DateTime.Parse(strDateFrom);

				sqlReadLog += "umbracoLog.DateStamp >= CONVERT(DATETIME, '" + dtpckrDateFrom.DateTime.ToString("yyyy.MM.dd") + " 00:00:00', 102) AND umbracoLog.DateStamp <= CONVERT(DATETIME, '" + dtpckrDateTo.DateTime.ToString("yyyy.MM.dd") + " 23:59:00', 102) ";

				ltrlLogInfo.Text = "Warning: The dates have been reversed because start date was greater than end date";
				ltrlLogInfo.Visible = true;
			}

			if (!txtbNodeID.Text.Equals(string.Empty))
			{
				sqlReadLog += "AND umbracoLog.NodeId = " + int.Parse(txtbNodeID.Text) + " ";
			}

			if (ddlLogTypes.SelectedValue.ToString() != "any")
			{
				sqlReadLog += "AND logHeader LIKE '" + ddlLogTypes.SelectedValue.ToString() + "' ";
			}

			if (ddlUsers.SelectedValue.ToString() != "any")
			{
				sqlReadLog += "AND UserId = " + ddlUsers.SelectedValue;
			}

			sqlReadLog += ") ORDER BY umbracoLog.DateStamp DESC";

			//Set up Cache Object and determine if it exists
			dgCache = (DataView)Cache.Get("dgCache");

			//Assign ColumnOrder to ViewState
			ViewState["SortOrder"] = ColumnOrder;

			if (dgCache == null)
			{
				//Create DataTable
				DataTable DataReaderTable = new DataTable();
				DataColumn dc1 = new DataColumn("UserName",typeof(string));
				DataColumn dc2 = new DataColumn("NodeId",typeof(int));
				DataColumn dc3 = new DataColumn("NodeName",typeof(string));
				DataColumn dc4 = new DataColumn("DateStamp",typeof(DateTime));
				DataColumn dc5 = new DataColumn("logHeader",typeof(string));
				DataColumn dc6 = new DataColumn("logComment",typeof(string));

				DataReaderTable.Columns.Add(dc1);
				DataReaderTable.Columns.Add(dc2);
				DataReaderTable.Columns.Add(dc3);
				DataReaderTable.Columns.Add(dc4);
				DataReaderTable.Columns.Add(dc5);
				DataReaderTable.Columns.Add(dc6);

				//Loop through DataReader
				IRecordsReader irLogs = SqlHelper.ExecuteReader(sqlReadLog);

				while (irLogs.Read())
				{
					//Set up DataRow object
					DataRow dr = DataReaderTable.NewRow();

					dr[0] = irLogs.GetString("userName");
					dr[1] = irLogs.GetInt("NodeId");
					dr[2] = irLogs.GetString("NodeName");
					dr[3] = irLogs.GetDateTime("DateStamp");
					dr[4] = irLogs.GetString("logHeader");
					dr[5] = irLogs.GetString("logComment");

					//Add rows to existing DataTable
					DataReaderTable.Rows.Add(dr);
				}

				//Create DataView to support our column sorting
				DataView Source = DataReaderTable.DefaultView;

				//Assign column sort order for DataView
				Source.Sort = ColumnOrder;

				//Insert DataTable into Cache object
				Cache.Insert("dgCache",Source);

				//Bind DataGrid from DataView
				gvLogTypesList.DataSource = Source;
			}
			else
			{
				//Assign Cached DataView new sort order
				dgCache.Sort = ViewState["SortOrder"].ToString();

				//Bind DataGrid from Cached DataView
				gvLogTypesList.DataSource = dgCache;
			}

			//gvLogTypesList.Style.Add("table-layout","fixed");
			gvLogTypesList.Style.Add("width","100%");
			gvLogTypesList.DataBind();
		}

		protected string SortOrder(string strField)
		{
			string strSortOrder = string.Empty;

			if (strField == ViewState["SortOrder"].ToString())
			{
				strSortOrder = strField.Replace("asc","desc");
			}
			else
			{
				strSortOrder = strField.Replace("desc","asc");
			}

			return strSortOrder;
		}

		protected void gvLogTypesList_PageIndexChanging(object sender,GridViewPageEventArgs e)
		{
			gvLogTypesList.PageIndex = e.NewPageIndex;
			GetLogs(ViewState["SortOrder"].ToString());
		}

		protected void gvLogTypesList_Sorting(object sender,GridViewSortEventArgs e)
		{
			gvLogTypesList.PageIndex = 0;
			GetLogs(SortOrder(e.SortExpression.ToString()));
		}

		protected string convertToShortDateTime(string dDate)
		{
			string convertedDate = DateTime.Parse(dDate).ToShortDateString() + "<br />" + DateTime.Parse(dDate).ToLongTimeString();

			return convertedDate;
		}
	}
}