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

using umbraco;
using umbraco.cms;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.propertytype;
using umbraco.DataLayer;

namespace FALMHousekeepingTools.versions
{
	public partial class cleanupVersionsByDate : System.Web.UI.Page
	{
		// Initialize an SQL Helper Interface
		protected static ISqlHelper SqlHelper
		{
			get
			{
				return umbraco.BusinessLogic.Application.SqlHelper;
			}
		}

		// Retrieve all umbraco's unique id nodes
		private int[] nodeIds = Document.getAllUniqueNodeIdsFromObjectType(new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"));

		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected void btnClearVersions_Click(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 100000;

			string strSQLGetVersions = string.Empty;
			ltrlVersions.Text = string.Empty;
			int versionsGlobalCount = 0;

			if (dtpckrDate.Text == string.Empty)
			{
				dtpckrDate.DateTime = DateTime.Now;
			}

			Document currentDoc;

			strSQLGetVersions = "SELECT cmsDocument.nodeId, cmsDocument.versionId, cmsContentVersion.versionDate ";
			strSQLGetVersions += "FROM cmsDocument INNER JOIN cmsContentVersion ON cmsDocument.versionId = cmsContentVersion.VersionId ";
			strSQLGetVersions += "WHERE (nodeId = @NodeId AND published = 0 AND newest = 0) ";
			strSQLGetVersions += "ORDER BY cmsContentVersion.versionDate DESC ";

			foreach (int nodeId in nodeIds)
			{
				// Retrieve the document
				currentDoc = new Document(nodeId);

				// Retrieve all versions of current node, excluding "published" and "newest" versions
				using (IRecordsReader dr = SqlHelper.ExecuteReader(strSQLGetVersions, SqlHelper.CreateParameter("@nodeId", nodeId.ToString())))
				{
					while (dr.Read())
					{
						versionsGlobalCount++;

						// Start deleting versions
						if (dr.GetDateTime("versionDate") < DateTime.Parse(dtpckrDate.DateTime.ToString("yyyy-MM-dd") + " 00:00:00"))
						{
							// Delete node versions
							try
							{
								// Delete all data associated with this content
								SqlHelper.ExecuteNonQuery("Delete from cmsPropertyData where versionId = @versionId", SqlHelper.CreateParameter("@versionId", dr.GetGuid("versionId").ToString()));

								// Delete version history
								SqlHelper.ExecuteNonQuery("Delete from cmsContentVersion where versionId = @versionId", SqlHelper.CreateParameter("@versionId", dr.GetGuid("versionId").ToString()));

								//Delete documents versions
								SqlHelper.ExecuteNonQuery("Delete from cmsDocument where versionId = @versionId", SqlHelper.CreateParameter("@versionId", dr.GetGuid("versionId").ToString()));

							}
							catch (Exception ex)
							{
								ltrlVersions.Text = ex.Message;
							}
						}
					}
				}
			}

			// Print result
			ltrlVersions.Text = versionsGlobalCount + " version(s) have been deleted";
		}
	}
}
