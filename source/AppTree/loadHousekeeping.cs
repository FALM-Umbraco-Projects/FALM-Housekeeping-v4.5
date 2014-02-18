using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;

namespace FALMHousekeepingAppTree
{
	/// <summary>
	/// Handles loading of the cache application into the developer application tree
	/// </summary>
	public class loadHousekeeping : BaseTree
	{
		public loadHousekeeping(string application)
			: base(application)
		{
		}

		protected override void CreateRootNode(ref XmlTreeNode rootNode)
		{
			rootNode.Icon = FolderIcon;
			rootNode.OpenIcon = FolderIconOpen;
			rootNode.NodeType = "init" + TreeAlias;
			rootNode.NodeID = "init";
		}

		protected override void CreateRootNodeActions(ref List<IAction> actions)
		{
			actions.Clear();
			actions.Add(ActionRefresh.Instance);
		}

		protected override void CreateAllowedActions(ref List<IAction> actions)
		{
			actions.Clear();
			actions.Add(ActionRefresh.Instance);
		}

		/// <summary>
		/// Renders the javascript.
		/// </summary>
		/// <param name="Javascript">The javascript.</param>
		public override void RenderJS(ref StringBuilder Javascript)
		{
			Javascript.Append(
				@"
function openHouseKeeping(id) {
	parent.right.document.location.href = '/umbraco/FALMHousekeeping/' + id;
}
");
		}

		public override void Render(ref XmlTree tree)
		{
			switch (this.NodeKey)
			{
				case "":
				default:
					XmlTreeNode xNodeLogs = XmlTreeNode.Create(this);
					xNodeLogs.NodeID = "Logs";
					xNodeLogs.Text = "Logs";
					xNodeLogs.Source = this.GetTreeServiceUrl("Logs");
					xNodeLogs.Icon = FolderIcon;
					xNodeLogs.OpenIcon = FolderIconOpen;
					tree.Add(xNodeLogs);

					XmlTreeNode xNodeMedia = XmlTreeNode.Create(this);
					xNodeMedia.NodeID = "Media";
					xNodeMedia.Text = "Media";
					xNodeMedia.Source = this.GetTreeServiceUrl("Media");
					xNodeMedia.Icon = FolderIcon;
					xNodeMedia.OpenIcon = FolderIconOpen;
					tree.Add(xNodeMedia);

					XmlTreeNode xNodeUsr = XmlTreeNode.Create(this);
					xNodeUsr.NodeID = "Users";
					xNodeUsr.Text = "Users";
					xNodeUsr.Source = this.GetTreeServiceUrl("Users");
					xNodeUsr.Icon = FolderIcon;
					xNodeUsr.OpenIcon = FolderIconOpen;
					tree.Add(xNodeUsr);

					XmlTreeNode xNodeVer = XmlTreeNode.Create(this);
					xNodeVer.NodeID = "Versions";
					xNodeVer.Text = "Versions";
					xNodeVer.Source = this.GetTreeServiceUrl("Versions");
					xNodeVer.Icon = FolderIcon;
					xNodeVer.OpenIcon = FolderIconOpen;
					tree.Add(xNodeVer);
					break;

				case "Logs":
					XmlTreeNode xNodeLogsItem1 = XmlTreeNode.Create(this);
					xNodeLogsItem1.NodeID = "Show Logs";
					xNodeLogsItem1.Text = "Show Logs";
					xNodeLogsItem1.Action = "javascript:openHouseKeeping('logs/showLogs.aspx?action=showlogs');";
					xNodeLogsItem1.Icon = "../../FALMHousekeeping/images/logs_viewer.gif";
					xNodeLogsItem1.OpenIcon = "../../FALMHousekeeping/images/logs_viewer.gif";
					tree.Add(xNodeLogsItem1);

					XmlTreeNode xNodeLogItem2 = XmlTreeNode.Create(this);
					xNodeLogItem2.NodeID = "Cleanup Logs";
					xNodeLogItem2.Text = "Cleanup Logs";
					xNodeLogItem2.Action = "javascript:openHouseKeeping('logs/cleanupLogs.aspx?action=cleanuplogs');";
					xNodeLogItem2.Icon = "../../FALMHousekeeping/images/logs_cleanup.gif";
					xNodeLogItem2.OpenIcon = "../../FALMHousekeeping/images/logs_cleanup.gif";
					tree.Add(xNodeLogItem2);
					break;

				case "Media":
					XmlTreeNode xNodeMediaItem1 = XmlTreeNode.Create(this);
					xNodeMediaItem1.NodeID = "Cleanup File System";
					xNodeMediaItem1.Text = "Cleanup File System";
					xNodeMediaItem1.Action = "javascript:openHouseKeeping('media/cleanupMediaFS.aspx?action=media');";
					xNodeMediaItem1.Icon = "../../FALMHousekeeping/images/media_folder_cleanup.gif";
					xNodeMediaItem1.OpenIcon = "../../FALMHousekeeping/images/media_folder_cleanup.gif";
					tree.Add(xNodeMediaItem1);
					break;

				case "Users":
					XmlTreeNode xNodeUsersItem1 = XmlTreeNode.Create(this);
					xNodeUsersItem1.NodeID = "Delete Users";
					xNodeUsersItem1.Text = "Delete Users";
					xNodeUsersItem1.Action = "javascript:openHouseKeeping('users/deleteUsersBySelection.aspx?action=users');";
					xNodeUsersItem1.Icon = "../../FALMHousekeeping/images/users_delete.gif";
					xNodeUsersItem1.OpenIcon = "../../FALMHousekeeping/images/users_delete.gif";
					tree.Add(xNodeUsersItem1);
					break;

				case "Versions":
					XmlTreeNode xNodeVersionsItem1 = XmlTreeNode.Create(this);
					xNodeVersionsItem1.NodeID = "Show Versions";
					xNodeVersionsItem1.Text = "Show Versions";
					xNodeVersionsItem1.Action = "javascript:openHouseKeeping('versions/showVersions.aspx?action=showversions');";
					xNodeVersionsItem1.Icon = "../../FALMHousekeeping/images/versions_view.gif";
					xNodeVersionsItem1.OpenIcon = "../../FALMHousekeeping/images/versions_view.gif";
					tree.Add(xNodeVersionsItem1);

					XmlTreeNode xNodeVersionsItem2 = XmlTreeNode.Create(this);
					xNodeVersionsItem2.NodeID = "Cleanup by Count";
					xNodeVersionsItem2.Text = "Cleanup by Count";
					xNodeVersionsItem2.Action = "javascript:openHouseKeeping('versions/cleanupVersionsByCount.aspx?action=cleanupversionsbycount');";
					xNodeVersionsItem2.Icon = "../../FALMHousekeeping/images/versions_cleanup.gif";
					xNodeVersionsItem2.OpenIcon = "../../FALMHousekeeping/images/versions_cleanup.gif";
					tree.Add(xNodeVersionsItem2);

					XmlTreeNode xNodeVersionsItem3 = XmlTreeNode.Create(this);
					xNodeVersionsItem3.NodeID = "Cleanup by Date";
					xNodeVersionsItem3.Text = "Cleanup by Date";
					xNodeVersionsItem3.Action = "javascript:openHouseKeeping('versions/cleanupVersionsByDate.aspx?action=cleanupversionsbydate');";
					xNodeVersionsItem3.Icon = "../../FALMHousekeeping/images/versions_cleanup.gif";
					xNodeVersionsItem3.OpenIcon = "../../FALMHousekeeping/images/versions_cleanup.gif";
					tree.Add(xNodeVersionsItem3);
					break;
			}
		}
	}
}
