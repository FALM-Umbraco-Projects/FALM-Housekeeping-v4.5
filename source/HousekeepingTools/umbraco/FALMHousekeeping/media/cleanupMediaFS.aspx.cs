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

namespace FALMHousekeeping.media
{
	public partial class cleanupMediaFS : System.Web.UI.Page
	{
		// Initialize an SQL Helper Interface
		protected static ISqlHelper SqlHelper
		{
			get
			{
				return umbraco.BusinessLogic.Application.SqlHelper;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			
		}

		protected void btnCheckOrphan_Click(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 100000;

			ltrlDeletable.Text = string.Empty;
			ltrlWarning.Text = string.Empty;

			string strWarnings = string.Empty;
			string strMediaDeletable = string.Empty;

			string strSQLGetMedia;

			string _filePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/");

			// Check if the files are stored in the /media folder root with a unique ID prefixed to the filename
			if (umbraco.UmbracoSettings.UploadAllowDirectories)
			{
				// Create an array with the list of media directories
				DirectoryInfo dir = new DirectoryInfo(_filePath);
				DirectoryInfo[] subDirs = dir.GetDirectories();

				// Sort Directories by name
				Array.Sort<DirectoryInfo>(subDirs, new Comparison<DirectoryInfo>(delegate(DirectoryInfo d1, DirectoryInfo d2)
				{
					int n1, n2;

					if (int.TryParse(d1.Name, out n1) && int.TryParse(d2.Name, out n2))
					{
						return n1 - n2;
					}
					else
					{
						return string.Compare(d1.Name, d2.Name);
					}
				}));

				strSQLGetMedia = "SELECT DISTINCT CONVERT(INT, SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) AS pId, cmsPropertyData.dataNvarchar ";
				strSQLGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
				strSQLGetMedia += "WHERE     (cmsPropertyType.dataTypeId = - 90) ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar LIKE '/media%') ";
				strSQLGetMedia += "      AND (ISNUMERIC(SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) = 1) ";
				strSQLGetMedia += "ORDER BY pId";
				
				int iDirectoryName = 0;
				bool bEOF = false;
				bool bThereAreOrphans = false;

				// Show orphan directories
				using (IRecordsReader dr = SqlHelper.ExecuteReader(strSQLGetMedia))
				{
					if (!dr.Read())
					{
						bEOF = true;
					}

					foreach (DirectoryInfo subDir in subDirs)
					{
						// Do check only if the folder have a number as a name
						if (int.TryParse(subDir.Name, out iDirectoryName))
						{
							while (!bEOF && iDirectoryName > dr.GetInt("pId"))
							{
								strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>/media/" + dr.GetInt("pId") + "/</td><td style='width: 0px; white-space: nowrap;'>Folder name is referred in the DB but doesn't exist in the file system</td></tr>";

								if (!dr.Read())
								{
									bEOF = true;
								}
							}

							if (bEOF || iDirectoryName < dr.GetInt("pId"))
							{
								strMediaDeletable += "<tr style='background-color: rgb(247, 247, 222);'><td style='width: 0px; white-space: nowrap;'>'/media/" + subDir.Name + "/' contains " + subDir.GetFileSystemInfos().Length + " item(s)</td></tr>";
								bThereAreOrphans = true;
							}
							else
							{
								if (!dr.Read())
								{
									bEOF = true;
								}
							}
						}
						else
						{
							strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>/media/" + subDir.Name + "/</td><td style='width: 0px; white-space: nowrap;'>Folder name is not a number</td></tr>";
						}
					}
				}

				// Show all non standard folder existing into DB
				strSQLGetMedia = "SELECT DISTINCT cmsPropertyData.dataNvarchar ";
				strSQLGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
				strSQLGetMedia += "WHERE     (cmsPropertyType.dataTypeId = - 90) ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar LIKE '/media%') ";
				strSQLGetMedia += "      AND (ISNUMERIC(SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) = 0) ";
				strSQLGetMedia += "      OR  (cmsPropertyType.dataTypeId = - 90) ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar <> '') ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar NOT LIKE '/media%') ";
				strSQLGetMedia += "ORDER BY cmsPropertyData.dataNvarchar";

				using (IRecordsReader drNSM = SqlHelper.ExecuteReader(strSQLGetMedia))
				{
					if (drNSM.Read())
					{
						while (drNSM.Read())
						{
							strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>" + drNSM.GetString("dataNvarchar") + "</td><td style='width: 0px; white-space: nowrap;'>DB entry doesn't match the format '/media/&lt;propertyid&gt;/'</td></tr>";
						}
					}
				}

				// Show Warnings
				if (strWarnings != "")
				{
					string strMediaTableWarnings = string.Empty;

					strMediaTableWarnings = "<p><strong>WARNING: The following entries will be ignored on deletion</strong></p>";
					strMediaTableWarnings += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); width: 100%; border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
					strMediaTableWarnings += "<tbody>";
					strMediaTableWarnings += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
					strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>Entry</th>";
					strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>Warning Message</th>";
					strMediaTableWarnings += "</tr>";
					strMediaTableWarnings += strWarnings;
					strMediaTableWarnings += "</tbody>";
					strMediaTableWarnings += "</table>";

					ltrlWarning.Text += strMediaTableWarnings;
				}

				// Show Deletable Folders
				if (bThereAreOrphans)
				{
					string strMediaTableDeletable = string.Empty;

					strMediaTableDeletable = "<p><strong>The following folders will be deleted with their contents:</strong></p>";
					strMediaTableDeletable += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
					strMediaTableDeletable += "<tbody>";
					strMediaTableDeletable += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
					strMediaTableDeletable += "<th scope='col' style='width: 0px; white-space: nowrap;'>Folder</th>";
					strMediaTableDeletable += "</tr>";
					strMediaTableDeletable += strMediaDeletable;
					strMediaTableDeletable += "</tbody>";
					strMediaTableDeletable += "</table>";

					ltrlDeletable.Text += strMediaTableDeletable;

					btnDeleteOrphan.Visible = true;
				}
				else
				{
					ltrlDeletable.Text = "<p><strong>No orphan media folder to delete</strong></p>";
				}
			}
			else
			{
				//
			}
		}

		protected void btnDeleteOrphan_Click(object sender, EventArgs e)
		{
			Server.ScriptTimeout = 100000;
			Response.Buffer = false;

			ltrlDeletable.Text = "";
			ltrlWarning.Text = "";

			bool bThereAreOrphans = false;

			string strSQLGetMedia;
			string strWarnings = string.Empty;
			string strMediaDeletable = string.Empty;

			string _filePath = System.Web.HttpContext.Current.Server.MapPath(umbraco.GlobalSettings.Path + "/../media/");
			string _dirPathToDelete = string.Empty;

			if (umbraco.UmbracoSettings.UploadAllowDirectories)
			{
				// Create an array with the list of media directories
				DirectoryInfo dir = new DirectoryInfo(_filePath);
				DirectoryInfo[] subDirs = dir.GetDirectories();

				// Sort Directories by name
				Array.Sort<DirectoryInfo>(subDirs, new Comparison<DirectoryInfo>(delegate(DirectoryInfo d1, DirectoryInfo d2)
				{
					int n1, n2;

					if (int.TryParse(d1.Name, out n1) && int.TryParse(d2.Name, out n2))
					{
						return n1 - n2;
					}
					else
					{
						return string.Compare(d1.Name, d2.Name);
					}
				}));

				strSQLGetMedia = "SELECT DISTINCT CONVERT(INT, SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) AS pId, cmsPropertyData.dataNvarchar ";
				strSQLGetMedia += "FROM cmsPropertyData INNER JOIN cmsPropertyType ON cmsPropertyData.propertytypeid = cmsPropertyType.id ";
				strSQLGetMedia += "WHERE     (cmsPropertyType.dataTypeId = - 90) ";
				strSQLGetMedia += "      AND (cmsPropertyData.dataNvarchar LIKE '/media%') ";
				strSQLGetMedia += "      AND (ISNUMERIC(SUBSTRING(cmsPropertyData.dataNvarchar, 8, CHARINDEX('/', cmsPropertyData.dataNvarchar, 8) - 8)) = 1) ";
				strSQLGetMedia += "ORDER BY pId";

				int iDirectoryName = 0;
				bool bEOF = false;

				// Delete orphan directories
				using (IRecordsReader dr = SqlHelper.ExecuteReader(strSQLGetMedia))
				{
					if (!dr.Read())
					{
						bEOF = true;
					}

					foreach (DirectoryInfo subDir in subDirs)
					{
						// Do check only if the folder have a number as a name
						if (int.TryParse(subDir.Name, out iDirectoryName))
						{
							while (!bEOF && iDirectoryName > dr.GetInt("pId"))
							{
								if (!dr.Read())
								{
									bEOF = true;
								}
							}

							if (bEOF || iDirectoryName < dr.GetInt("pId"))
							{
								_dirPathToDelete = _filePath + subDir.Name + "\\";

								if (Directory.Exists(_dirPathToDelete))
								{
									Directory.Delete(_dirPathToDelete, true);

									strMediaDeletable += "<tr style='background-color: rgb(247, 247, 222);'><td style='width: 0px; white-space: nowrap;'>/media/" + subDir.Name + "/</td></tr>";

									bThereAreOrphans = true;
								}
								else
								{
									strWarnings += "<tr style='background-color: rgb(255, 255, 255);'><td style='width: 0px; white-space: nowrap;'>/media/" + subDir.Name + "/ was not found</td></tr>";
								}
							}
							else
							{
								if (!dr.Read())
								{
									bEOF = true;
								}
							}
						}
					}
				}

				if (strWarnings != "")
				{
					string strMediaTableWarnings = string.Empty;

					strMediaTableWarnings = "<p><strong>WARNING: The following have been ignored</strong></p>";
					strMediaTableWarnings += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); width: 100%; border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
					strMediaTableWarnings += "<tbody>";
					strMediaTableWarnings += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
					strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>Entry</th>";
					strMediaTableWarnings += "<th scope='col' style='width: 0px; white-space: nowrap;'>Warning Message</th>";
					strMediaTableWarnings += "</tr>";
					strMediaTableWarnings += strWarnings;
					strMediaTableWarnings += "</tbody>";
					strMediaTableWarnings += "</table>";

					ltrlWarning.Text += strMediaTableWarnings;
				}

				if (bThereAreOrphans)
				{
					string strMediaTableDeletable = string.Empty;

					strMediaTableDeletable = "<p><strong>The following folders have been deleted with their contents:</strong></p>";
					strMediaTableDeletable += "<table style='color: Black; background-color: White; border: 1px solid rgb(222, 223, 222); border-collapse: collapse;' border='1' cellpadding='4' cellspacing='0'";
					strMediaTableDeletable += "<tbody>";
					strMediaTableDeletable += "<tr style='color: White; background-color: rgb(107, 105, 107); font-weight: bold; white-space: nowrap;' align='center'>";
					strMediaTableDeletable += "<th scope='col' style='width: 0px; white-space: nowrap;'>Folder</th>";
					strMediaTableDeletable += "</tr>";
					strMediaTableDeletable += strMediaDeletable;
					strMediaTableDeletable += "</tbody>";
					strMediaTableDeletable += "</table>";

					ltrlDeletable.Text += strMediaTableDeletable;
					
				}
				else
				{
					ltrlDeletable.Text = "<p><strong>No orphan media folder deleted</strong></p>";
				}
			}
			else
			{

			}

			btnDeleteOrphan.Visible = false;
		}
	}
}
