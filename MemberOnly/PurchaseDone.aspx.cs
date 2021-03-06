﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Text;
using System.IO;

public partial class MemberOnly_ViewMemberInformation : System.Web.UI.Page
{
    public static String username;
    private static String confirmationNumber;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack) {
            username = User.Identity.Name;
            confirmationNumber = ShoppingCart.GetShoppingCart(User.Identity.Name).checkOut();
            ConfirmationNumberLabel.Text = confirmationNumber;
            sendReceipt();
        }
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        //base.VerifyRenderingInServerForm(control);
    }
    private void sendReceipt()
    {
        var message = new StringBuilder();
        email.RenderControl(new HtmlTextWriter(new StringWriter(message)));
        
        string s = message.ToString();

        EmailAlert alerter = new EmailAlert();

        alerter.sendEmail(GenericQuery.getUserEmail(User.Identity.Name), "Web Shop Receipt", s);
    }
    protected void orderDataSource_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
    {
        e.Command.Parameters[0].Value = confirmationNumber;
    }
    protected void orderItemsDataSource_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
    {
        e.Command.Parameters[0].Value = confirmationNumber;
    }
    protected String total()
    {
        string userName = username;
        string connectionString = "AsiaWebShopDBConnectionString";
        string query = "SELECT  SUM(OrderItem.quantity * OrderItem.PriceWhenAdded) AS Total " +
                        "FROM   OrderItem INNER JOIN [Order] ON OrderItem.orderNum = [Order].orderNum " +
                        "WHERE  ([Order].confirmationNumber = N'" + confirmationNumber + "') ";

        SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
        SqlCommand command = new SqlCommand(query, connection);
        command.Connection.Open();
        String total = command.ExecuteScalar().ToString();
        command.Connection.Close();
        return total;
    }
}