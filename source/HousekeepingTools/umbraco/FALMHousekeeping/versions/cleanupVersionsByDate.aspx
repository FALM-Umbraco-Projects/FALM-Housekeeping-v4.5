<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cleanupVersionsByDate.aspx.cs" Inherits="FALMHousekeepingTools.versions.cleanupVersionsByDate" MasterPageFile="../../masterpages/umbracoPage.Master" Title="F.A.L.M. Housekeeping - Cleanup Versions by Date" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols.DatePicker" Assembly="controls" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="cphBody" ContentPlaceHolderID="body" runat="server">

	<cc1:UmbracoPanel ID="umbPanelCleanupVersionsByDate" Text="F.A.L.M. Housekeeping" runat="server" Width="612px" Height="600px" hasMenu="false">
		
		<cc1:Pane ID="PanelCleanupVersionsByDate" runat="server" Text="Cleanup versions by Date">
			
			<asp:Panel ID="CleanupVersionsByDatePanel" runat="server">

				<p>With this tool you can cleanup the version history, deleting every version up to a given date.</p>
				
				<p><em>Please note that Umbraco requires each node to have at least 2 versions (the currently published and the latest). These versions will never be deleted.</em><br /><br /></p>
				
				<p>
					Delete all versions since&nbsp;&nbsp;&nbsp;
					<cc2:DateTimePicker ID="dtpckrDate" runat="server" ShowTime="false" />
					&nbsp;&nbsp;&nbsp;<asp:Button ID="btnClearVersions" runat="server" Text="Delete" OnClick="btnClearVersions_Click" />
				</p>
				
				<p>&nbsp;</p>
				
				<p><asp:Literal ID="ltrlVersions" runat="server" Text="" /></p>

			</asp:Panel>
		
		</cc1:Pane>
	
	</cc1:UmbracoPanel>

</asp:Content>