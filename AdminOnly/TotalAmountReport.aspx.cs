﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;



public partial class AdminOnly_MemberReport : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            StartCalendar.Visible = false;
            EndCalendar.Visible = false;
        }
    }
    protected void StartCalendarButton_Click(object sender, EventArgs e)
    {
        if (StartCalendar.Visible)
        {
            StartCalendar.Visible = false;
        }
        else
        {
            StartCalendar.Visible = true;
        }
    }
    protected void EndCalendarButton_Click(object sender, EventArgs e)
    {
        if (EndCalendar.Visible)
        {
            EndCalendar.Visible = false;
        }
        else
        {
            EndCalendar.Visible = true;
        }

    }
 

    protected void MemberCustomValidator_ServerValidate(object source, ServerValidateEventArgs args)
    {

        if (UserName.Text.Trim() == "")
        {
            args.IsValid = true;
           
        }
        else
        {
            MembershipUser user = Membership.GetUser(UserName.Text.Trim());
            if (user != null)
            {
                args.IsValid = true;
                
            }
            else
            {
                args.IsValid = false;
             
            }
        }
    }


    protected void btnGenerate_Click(object sender, EventArgs e)
    {
        string userNameQuery = "";
        string orderRuleQuery = "";
        string dateRangQuery = "";

        if (UserName.Text.Trim() != "")
        {
            userNameQuery = "AND (Member.userName = N'" + UserName.Text.Trim() + "')";
        }
        else
        {

            orderRuleQuery = " ORDER BY ";
            if (cbGroupByDistrict.Checked)
            {
                orderRuleQuery += " Result1.district ";
                TotalAmountGridView.Columns[0].Visible = true;
            }

            else
            {   
                switch (OrderRadioButtonList.SelectedIndex)
                {
                    case 0:
                        orderRuleQuery += " Result1.LastName ";
                        break;
                    case 1:
                        orderRuleQuery += " TotalAmount ";
                        break;
                }
                TotalAmountGridView.Columns[0].Visible = false;
            }

        }
        if (StartDayTextBox.Text != "")
        {
            dateRangQuery = "AND (([Order].orderDateTime >= CAST('" + StartDayTextBox.Text.Trim() + "' AS SMALLDATETIME)))";
           
        }
       
        if (EndDayTextBox.Text != "")
        {
            dateRangQuery += "AND (([Order].orderDateTime <= CAST('" + EndDayTextBox.Text.Trim() + "' AS SMALLDATETIME)))"; 
        }

        string SQLCmd = "SELECT Result1.userName, Result1.firstName, Result1.lastName, Result1.district, SUM(OrderItem.quantity * OrderItem.PriceWhenAdded) AS TotalAmount FROM OrderItem INNER JOIN (SELECT Result0.userName, Result0.firstName, Result0.lastName, [Order].orderNum, Result0.district FROM [Order] INNER JOIN (SELECT Member.userName, Member.firstName, Member.lastName, Address.district FROM Member INNER JOIN Address ON Member.userName = Address.userName WHERE (Address.nickname = N'Mailing') " +
              userNameQuery + ") AS Result0 ON Result0.userName = [Order].userName WHERE (([Order].confirmationNumber IS NOT NULL)" + dateRangQuery +")) AS Result1 ON OrderItem.orderNum = Result1.orderNum GROUP BY Result1.district, Result1.userName, Result1.firstName, Result1.lastName " + orderRuleQuery;

        MemberSqlDataSource.SelectCommand = SQLCmd;
        TotalAmountGridView.DataBind();
        MergeRows(TotalAmountGridView);

        MemberSqlDataSource.Select(DataSourceSelectArguments.Empty);
    }





    public static void MergeRows(GridView gridView)
    {
        for (int rowIndex = gridView.Rows.Count - 2; rowIndex >= 0; rowIndex--)
        {
            GridViewRow row = gridView.Rows[rowIndex];
            GridViewRow previousRow = gridView.Rows[rowIndex + 1];

            string rowString = ((Label)row.FindControl("DistrictLabel")).Text;
            string previousRowString = ((Label)previousRow.FindControl("DistrictLabel")).Text;

            if (rowString == previousRowString)
                {
                    row.Cells[0].RowSpan = previousRow.Cells[0].RowSpan < 2 ? 2 :
                                           previousRow.Cells[0].RowSpan + 1;
                    previousRow.Cells[0].Visible = false;
                }



        }
    }

    protected void StartCalendar_SelectionChanged(object sender, EventArgs e)
    {
        StartDayTextBox.Text = StartCalendar.SelectedDate.ToShortDateString();
        StartCalendar.Visible = false;
    }


    protected void EndCalendar_SelectionChanged(object sender, EventArgs e)
    {
        EndDayTextBox.Text = EndCalendar.SelectedDate.ToShortDateString();
        EndCalendar.Visible = false;
    }
    protected void ClearRangeButton_Click(object sender, EventArgs e)
    {
        StartDayTextBox.Text = null;
        EndDayTextBox.Text = null;
    }


}
