﻿<%@ Page Title="" Language="C#" MasterPageFile="~/AsiaWebShopSite.master" AutoEventWireup="true" CodeFile="WishListPage.aspx.cs" Inherits="MemberOnly_WishList_WishListPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
    <style type="text/css">
        .style4
        {
            font-size: large;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <span class="style4"><strong>Wish List</strong></span><br />
    <a href="../../ItemSearch.aspx"><-Back to Item Search</a><br />
    <br />
    <asp:GridView ID="gvWishList" runat="server" AutoGenerateColumns="False" 
        CellPadding="4" DataKeyNames="userName,upc" DataSourceID="SqlDataSource1" 
        ForeColor="#333333" GridLines="None" AutoGenerateEditButton="True">
        <AlternatingRowStyle BackColor="White" />
        <Columns>
            <asp:BoundField DataField="userName" HeaderText="userName" ReadOnly="True" 
                SortExpression="userName" Visible="False" />
            <asp:BoundField DataField="upc" HeaderText="upc" SortExpression="upc" 
                ReadOnly="True" Visible="False" />
            <asp:BoundField DataField="name" HeaderText="Product Name" 
                SortExpression="name" ReadOnly="True" />
            <asp:BoundField DataField="normalPrice" HeaderText="Normal Price" 
                SortExpression="normalPrice" ReadOnly="True" />
            <asp:BoundField DataField="discountPrice" HeaderText="Discount Price" 
                ReadOnly="True" SortExpression="discountPrice" />
            <asp:TemplateField HeaderText="Quantity" SortExpression="quantityAvailable">
                <ItemTemplate>
                    <asp:Label ID="ItemQuantity" runat="server" Text='<%# Eval("quantityAvailable") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="WishListed" HeaderText="Wish Listed" ReadOnly="True" />
            <asp:BoundField DataField="comment" HeaderText="Annotation" 
                SortExpression="comment" />
            <asp:TemplateField HeaderText="Stock Back Email Alert" SortExpression="emailSent">
                <EditItemTemplate>
                    <asp:CheckBox ID="CheckBox2" runat="server" Checked='<%# Bind("emailSent") %>' 
                         OnCheckedChanged="CheckBox2_CheckedChanged"/>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:CheckBox ID="CheckBox1" runat="server" Checked='<%# Bind("emailSent") %>' 
                        Enabled="false"/>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:HyperLink ID="AddItemLink" runat="server" NavigateUrl='<%# string.Format("~/MemberOnly/AddToCart.aspx?upc={0}&name={1}&discountPrice={2}&normalPrice={3}&quantityAvailable={4}", HttpUtility.UrlEncode(Eval("upc").ToString()), HttpUtility.UrlEncode(Eval("name").ToString()), HttpUtility.UrlEncode(Eval("discountPrice").ToString()), HttpUtility.UrlEncode(Eval("normalPrice").ToString()), HttpUtility.UrlEncode(Eval("quantityAvailable").ToString())) %>' Text="Add to cart" ForeColor="Black"></asp:HyperLink>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:CommandField ShowDeleteButton="True" DeleteText="Remove" />
        </Columns>
        <EditRowStyle BackColor="#EFF3FB" />
        <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#2461BF" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#EFF3FB" />
        <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#F5F7FB" />
        <SortedAscendingHeaderStyle BackColor="#6D95E1" />
        <SortedDescendingCellStyle BackColor="#E9EBEF" />
        <SortedDescendingHeaderStyle BackColor="#4870BE" />
    </asp:GridView>
    <br />
    <asp:SqlDataSource ID="SqlDataSource1" runat="server" 
        ConnectionString="<%$ ConnectionStrings:AsiaWebShopDBConnectionString %>" 
        SelectCommand="SELECT WishListItem.userName, WishListItem.upc, Item.name, Item.discountPrice, Item.normalPrice,Item.quantityAvailable, WishListItem.comment, WishListItem.emailSent,(select count(*) from WishListItem where [upc] = Item.upc) AS WishListed FROM WishListItem INNER JOIN Item ON WishListItem.upc = Item.upc WHERE (WishListItem.userName = @userName)"
        UpdateCommand="UPDATE [WishListItem] SET [comment] = @comment, [emailSent] = @emailSent WHERE [userName] = @userName AND [upc] = @upc" 
        DeleteCommand="DELETE FROM [WishListItem] WHERE [upc] = @upc AND [userName] = @userName"  >
        <SelectParameters>
            <asp:Parameter Name="userName" Type="String" />
        </SelectParameters>
        <UpdateParameters>
                <asp:Parameter Name="userName" Type="String" />
                <asp:Parameter Name="upc" Type="String" />
                <asp:Parameter Name="comment" Type="String" />
                <asp:Parameter Name="emailSent" Type="String" />
            </UpdateParameters>
    </asp:SqlDataSource>
</asp:Content>

