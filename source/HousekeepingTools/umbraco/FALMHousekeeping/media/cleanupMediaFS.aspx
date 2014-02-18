<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cleanupMediaFS.aspx.cs" Inherits="FALMHousekeeping.media.cleanupMediaFS" MasterPageFile="../../masterpages/umbracoPage.Master" Title="F.A.L.M. Housekeeping - Cleanup Media File System" %>

<%@ Register TagPrefix="cc1" Assembly="controls" Namespace="umbraco.uicontrols" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="head" runat="server" />

<asp:Content ID="cphBody" ContentPlaceHolderID="body" runat="server">
	<cc1:UmbracoPanel ID="umbPanelCleanupMediaFS" Text="F.A.L.M. Housekeeping" runat="server" Width="612px" Height="600px" hasMenu="false">
		<cc1:Pane ID="PanelCleanupMediaFS" Style="padding-right: 10px; padding-left: 10px; padding-bottom: 10px; padding-top: 10px;" runat="server" CssClass="innerContent" Text="Cleanup orphan media folders on file system">
			<p>With this tool you can delete file system folders under '/media' which have no entry in the DB (orphans).</p>
			<p>
				<asp:Button ID="btnCheckOrphan" runat="server" Text="Show orphans to delete" OnClick="btnCheckOrphan_Click" /></p>
			<p>
				<asp:Literal ID="ltrlDeletable" Text="" runat="server" /></p>
			<p>
				<asp:Literal ID="ltrlWarning" Text="" runat="server" /></p>
			<p>
				<asp:Button ID="btnDeleteOrphan" runat="server" Text="Confirm deletion" Visible="false" OnClick="btnDeleteOrphan_Click" /></p>
		</cc1:Pane>
	</cc1:UmbracoPanel>
</asp:Content>
