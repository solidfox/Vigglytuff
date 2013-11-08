﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class MemberOnly_ShoppingCart : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            PopulateShoppingCart();
        }
    }
    protected void btnUpdateCart_Click(object sender, EventArgs e)
    {
        // Update the items in the shopping cart with the new quantities.
        // This code updates all items even if their quantity has not changed.

        for (int i = 0; i < gvShoppingCart.Rows.Count; i++)
        {
            // Get the upc and the quantity of the item.
            string upc = ((Label)gvShoppingCart.Rows[i].FindControl("lblUPC")).Text;
            TextBox txtQuantity = (TextBox)gvShoppingCart.Rows[i].Cells[2].FindControl("txtQuantity");
            int quantity = Convert.ToInt16(txtQuantity.Text);

            // Update the items in the shopping cart.
            if (upc != "")
            {
                ShoppingCart cart = ShoppingCart.GetShoppingCart();
                cart.SetItemQuantity(upc, quantity);
                HttpContext.Current.Session["MyShoppingCart"] = cart;
            }
            else
            {
                Debug.Fail("ERROR : We should never get to UpdateCart.aspx without a UPC.");
                throw new Exception("ERROR : It is illegal to load UpdateCart.aspx without setting a UPC.");
            }
        }

        // Display the updated quantities.
        PopulateShoppingCart();

    }

    protected void PopulateShoppingCart()
    {
        DataTable viewCart = new DataTable();

        // Define the columns of the DataTable.
        viewCart.Columns.Add("upc", typeof(String));
        viewCart.Columns.Add("name", typeof(String));
        viewCart.Columns.Add("quantity", typeof(Int16));
        viewCart.Columns.Add("discountPrice", typeof(Int16));
        viewCart.Columns.Add("totalPrice", typeof(Int16));

        // Populate the DataTable from the shopping cart.
        ShoppingCart cart = ShoppingCart.GetShoppingCart();
        for (int i = 0; i <= cart.Items.Count - 1; i++)
        {
            viewCart.Rows.Add(
                cart.Items[i].UPC,
                cart.Items[i].ItemName,
                cart.Items[i].Quantity,
                cart.Items[i].DiscountPrice,
                cart.Items[i].TotalPrice);
        }

        // Set the DataTable as the datasource for the shopping cart Gridview.
        gvShoppingCart.DataSource = viewCart;
        gvShoppingCart.DataBind();
    }


    // ----------
    // GetTotalPrice procedure
    // ----------

    public decimal GetTotalPrice()
    {
        // Calculate the total price of all items.
        ShoppingCart cart = ShoppingCart.GetShoppingCart();
        return (cart.GetCartTotal());
    }

}