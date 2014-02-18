﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="showLogs.aspx.cs" Inherits="FALMHousekeepingTools.logs.showLogs"
	MasterPageFile="~/umbraco/masterpages/umbracoPage.Master" Title="F.A.L.M. Housekeeping - Show logs" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols.DatePicker" Assembly="controls" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="cphBody" ContentPlaceHolderID="body" runat="server">

	<cc1:UmbracoPanel ID="umbPanelShowLogs" Text="F.A.L.M. Housekeeping" runat="server" Width="612px" Height="600px" hasMenu="false">
		
		<cc1:Pane ID="PanelShowLogs" runat="server" Text="Show logs">
			
			<asp:Panel ID="ShowLogsPanel" runat="server">
				
				<table>
					<tr>
						<td>
							<small>Filter by nodeId</small>
						</td>
						<td>
							<small>Filter by user</small>
						</td>
						<td>
							<small>Filter by log type</small>
						</td>
						<td>
							<small>Filter by date range (from)</small>
						</td>
						<td>
							<small>Filter by date range (to)</small>
						</td>
					</tr>
					<tr>
						<td>
							<asp:TextBox ID="txtbNodeID" runat="server" />
						<//td>
						<td>
							<asp:DropDownList ID="ddlUsers" AppendDataBoundItems="true" runat="server" DataSourceID="SqlDSUsers" DataTextField="userName" DataValueField="Id">
								<asp:ListItem Text="-- Any --" Value="any" />
							</asp:DropDownList>
							<asp:SqlDataSource ID="SqlDSUsers" runat="server" ConnectionString="<%$ appSettings:umbracoDbDSN %>" SelectCommand="SELECT [Id], [userName] FROM [umbracoUser] ORDER BY [userName]" />
						</td>
						<td>
							<asp:DropDownList ID="ddlLogTypes" AppendDataBoundItems="true" runat="server">
								<asp:ListItem Text="-- Any --" Value="any" />
							</asp:DropDownList>
						</td>
						<td>
							<cc2:DateTimePicker ID="dtpckrDateFrom" runat="server" ShowTime="false" />
						</td>
						<td>
							<cc2:DateTimePicker ID="dtpckrDateTo" runat="server" ShowTime="false" />
						</td>
					</tr>
					<tr>
						<td colspan="4">
							<asp:Button ID="btnShowLog" runat="server" Text="Show Logs"  OnClick="btnShowLogs_Click" />
						</td>
					</tr>
				</table>

				<p><asp:Literal ID="ltrlLogInfo" runat="server" Text="" Visible="false" /></p>

				<asp:GridView ID="gvLogTypesList" runat="server" AutoGenerateColumns="False" AllowPaging="True" AllowSorting="True" PageSize="20" BackColor="#FFFFFF" BorderColor="#DEDFDE" BorderWidth="1" GridLines="Both" CellPadding="4" ForeColor="#000000" OnSorting="gvLogTypesList_Sorting" OnPageIndexChanging="gvLogTypesList_PageIndexChanging">
					<RowStyle BackColor="#F7F7DE" />
					<HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" HorizontalAlign="Center" Wrap="false" />
					<AlternatingRowStyle BackColor="White" />
					<PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" />
					<PagerStyle BackColor="#6B696B" ForeColor="#FFFFFF" HorizontalAlign="Right" />
					<FooterStyle BackColor="#CCCC99" />
					<Columns>
						<asp:TemplateField SortExpression="UserName asc" HeaderText="User Name">
							<HeaderStyle Width="10%" />
							<ItemTemplate>
								<%# Eval("UserName", "{0}")%>
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField SortExpression="NodeId asc" HeaderText="Node Id">
							<HeaderStyle Width="5%" />
							<ItemTemplate>
								<%# Eval("NodeId", "{0}")%>
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField SortExpression="NodeName asc" HeaderText="Node Name">
							<HeaderStyle Width="26%" />
							<ItemTemplate>
								<%# Eval("NodeName", "{0}").Replace("SYSTEM DATA: umbraco master root", "")%>
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField SortExpression="DateStamp asc" HeaderText="Log Date">
							<HeaderStyle Width="9%" />
							<ItemTemplate>
								<%# convertToShortDateTime(Eval("DateStamp","{0}"))%>
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField SortExpression="logHeader asc" HeaderText="Log Type">
							<HeaderStyle Width="10%" />
							<ItemTemplate>
								<%# Eval("logHeader", "{0}")%>
							</ItemTemplate>
						</asp:TemplateField>
						<asp:TemplateField SortExpression="logComment asc" HeaderText="Log Detail">
							<HeaderStyle Width="40%" />
							<ItemTemplate>
								<%# Eval("logComment", "{0}")%>
							</ItemTemplate>
						</asp:TemplateField>
					</Columns>
					<EmptyDataTemplate>
						<strong>There are no logs to show.</strong></EmptyDataTemplate>
				</asp:GridView>
				
			</asp:Panel>
		
		</cc1:Pane>
	
	</cc1:UmbracoPanel>

</asp:Content>
