using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.IO;

namespace FALMHousekeepingTools.users
{
	public partial class deleteUsersBySelection : System.Web.UI.Page
	{
		// Initialize an SQL Helper Interface
		protected static ISqlHelper SqlHelper
		{
			get { return umbraco.BusinessLogic.Application.SqlHelper; }
		}

		// Current Logged User
		protected User userCurrent = umbraco.BusinessLogic.User.GetCurrent();

		protected int iErr;

		protected void Page_Load(object sender,EventArgs e)
		{

		}

		// Check if there is almost one user
		protected void SqlDSUsers_Selected(object sender,SqlDataSourceStatusEventArgs e)
		{
			int iRows = e.AffectedRows;

			if (iRows <= 0)
			{
				ltrlUsers.Text = "There are no users to delete.";
				btnDeleteUsers.Visible = false;
			}
		}

		protected void btnDeleteUsers_Click(object sender,EventArgs e)
		{
			string gvIDs = "";
			bool chkBox = false;
			//'Navigate through each row in the GridView for checkbox items
			foreach (GridViewRow gv in gvUsers.Rows)
			{
				CheckBox deleteChkBxItem = (CheckBox)gv.FindControl("chkbUser");
				if (deleteChkBxItem.Checked)
				{
					chkBox = true;
					// Concatenate GridView items with comma for SQL Delete
					gvIDs += ((Label)gv.FindControl("lblUserId")).Text.ToString() + ",";
				}
			}

			if (chkBox)
			{
				// All documents related to selected user(s) will change to the administrator
				string sqlDelChangeUmbracoNodeUser = "UPDATE umbracoNode SET nodeUser = 0 WHERE nodeUser IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelChangeUmbracoNodeUser);

				string sqlDelChangeCmsDocumentUser = "UPDATE cmsDocument SET documentUser = 0 WHERE documentUser IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelChangeCmsDocumentUser);

				// Clear all selected user(s) informations from the log (sessions and workflow's tasks)
				string sqlDelLogUmbracoUserLogins = "DELETE FROM umbracoUserLogins WHERE [User] IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelLogUmbracoUserLogins);

				string sqlDelLogCmsTask = "DELETE FROM cmsTask WHERE UserId IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ") OR parentUserID IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelLogCmsTask);

				string sqlDelLogUmbracoLog = "DELETE FROM umbracoLog WHERE userId IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelLogUmbracoLog);

				// Delete all selected user(s) references
				string sqlDelUserUmbracoUser2app = "DELETE FROM umbracoUser2app WHERE [user] IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser2app);

				string sqlDelUserUmbracoUser2NodeNotify = "DELETE FROM umbracoUser2NodeNotify WHERE userId IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser2NodeNotify);

				string sqlDelUserUmbracoUser2NodePermission = "DELETE FROM umbracoUser2NodePermission WHERE userId IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser2NodePermission);

				string sqlDelUserUmbracoUser2UserGroup = "DELETE FROM umbracoUser2UserGroup WHERE [user] IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser2UserGroup);

				string sqlDelUserUmbracoUser = "DELETE FROM umbracoUser WHERE id IN (" + gvIDs.Substring(0,gvIDs.LastIndexOf(",")) + ")";
				sqlExecuteCleanup(sqlDelUserUmbracoUser);

				if (iErr == 0)
				{
					ltrlUsers.Text = "<span style='color: Red; font-weight: bold;'>All selected users have been deleted.</span>";
				}

				// Refresh gridview
				gvUsers.DataBind();
			}
		}

		private void sqlExecuteCleanup(string strSQL)
		{
			SqlConnection cn = new SqlConnection(SqlDSUsers.ConnectionString);

			// Execute SQL Query only if checkboxes are checked to avoid any error with initial null string
			try
			{
				SqlCommand cmd = new SqlCommand(strSQL,cn);
				cn.Open();
				cmd.ExecuteNonQuery();

				iErr = 0;
			}
			catch (SqlException err)
			{
				iErr = 1;

				ltrlUsers.Text += "<span style='color: Red; font-weight: bold;'>" + err.Message.ToString() + "<br />" + strSQL + "</span><br />";
			}
			finally
			{
				cn.Close();
			}

		}

		protected void gvUsers_RowDataBound(object sender,GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				Label lblUserDisabled = (Label)e.Row.FindControl("lblUserDisabled");

				if (lblUserDisabled.Text == "True")
				{
					e.Row.ForeColor = System.Drawing.Color.Gray;
				}
			}
		}
	}
}