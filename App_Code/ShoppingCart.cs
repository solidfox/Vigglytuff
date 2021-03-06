﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

/*
 * The ShoppingCart class
 * It holds the items that are in the cart and provides methods for their manipulation. 
 */
public class ShoppingCart
{

#region Properties
    // A shopping cart is a List of CartItem.
    public List<CartItem> Items {get; private set;}
    public static string connectionString = "AsiaWebShopDBConnectionString";
    public string userName = null;
    public string msg = "";
#endregion

#region Singleton Implementation of ShoppingCart
    // Readonly properties can only be set in initialization or in a constructor.
    public static readonly ShoppingCart Instance;

    // The static constructor is called as soon as the class is loaded into memory.
    public static ShoppingCart GetShoppingCart(string UserName)
    {
        // If the cart is not in the session, create one and put it there.
        if (HttpContext.Current.Session["MyShoppingCart"] == null)
        {
            ShoppingCart cart = new ShoppingCart();
            cart.Items = new List<CartItem>();
            cart.RetrieveFromDB (connectionString, UserName);
            /******
             * TODO: Load any previously saved items into the shopping cart from the database.
             */
            // Save the shopping cart in the Session variable "MyShoppingCart".
            HttpContext.Current.Session["MyShoppingCart"] = cart;
        }

        ShoppingCart temp = (ShoppingCart)HttpContext.Current.Session["MyShoppingCart"];
        if (temp.Items != null) temp.Items.Clear();           
        temp.RetrieveFromDB (connectionString, UserName);            
        HttpContext.Current.Session["MyShoppingCart"] = temp;
        return (ShoppingCart)HttpContext.Current.Session["MyShoppingCart"];
    }

    // A protected constructor ensures that an object cannot be created from outside.
    protected ShoppingCart() { }
#endregion

#region Shopping Cart Item Modification Methods
    /*
     * AddItem() adds an item to the shopping cart.
     */
    public void AddItem(string upc, string name, decimal discountPrice, int _quantity, bool reserved)
    {
        // Create a new item to add to the cart.
        CartItem newItem = new CartItem(upc);
        // If this item already exists in the list of items, increase the quantity.
        // Otherwise, add the new item to the list of items with quantity 1;
        if (Items.Contains(newItem))
        {
            foreach (CartItem item in Items)
            {
                if (item.Equals(newItem))
                {
                    if (GenericQuery.CheckItemStock(connectionString, newItem, _quantity - 1))
                    {
                        if (!reserved)
                            item.Quantity++;
                        GenericQuery.UpdateDBItem(connectionString, upc, -_quantity);
                        return;
                    }
                }
            }
        }

        else if (reserved )
        {
            newItem.UPC = upc;
            newItem.ItemName = name;
            newItem.DiscountPrice = discountPrice;
            newItem.Quantity = _quantity;
            Items.Add(newItem);
        }

        else if (GenericQuery.CheckItemStock(connectionString, newItem, _quantity - 1))
        {
            newItem.UPC = upc;
            newItem.ItemName = name;
            newItem.DiscountPrice = discountPrice;
            newItem.Quantity = _quantity;
            Items.Add(newItem);
            GenericQuery.UpdateDBItem(connectionString, upc, -_quantity);
            return;
        }

        else
        {
            // Inform the user the quantity is alreadt exceed the stock
            //msg += "The item " + name + " is deleted from your shopping cart.";
            //UserNotify test = new UserNotify(msg);
            UserNotify.outstock(name.Trim());
            GenericQuery.RemoveFromDBOrderItem(connectionString, upc, GenericQuery.GetOrderNumber(connectionString, userName));
            return;
        }
    }

    /*
     * SetItemQuantity() changes the quantity of an item in the shopping cart.
     */
    public void SetItemQuantity(string upc, int quantity)
    {
        // If the quantity is set to 0, remove the item entirely.
        int OrderNum = GenericQuery.GetOrderNumber(connectionString, this.userName);
        int _quantity = 0;
        CartItem updatedItem = new CartItem(upc);
        if (quantity == 0)
        {
            foreach (CartItem item in Items)
                if (item.Equals(updatedItem))
                    _quantity = item.Quantity;
            GenericQuery.UpdateDBItem(connectionString, upc, _quantity);
            GenericQuery.RemoveFromDBOrderItem(connectionString, upc, OrderNum);
            RemoveItem(upc);
            return;
        }

        // Find the item and update the quantity.
        foreach (CartItem item in Items)
        {
            int test = quantity - item.Quantity;
            if (item.Equals(updatedItem) && GenericQuery.CheckItemStock(connectionString, updatedItem, test - 1))
            {
                GenericQuery.SubUpdateOrderItem(connectionString, OrderNum, upc, quantity, item.DiscountPrice);
                int difference = item.Quantity - quantity;
                item.Quantity = quantity;
                GenericQuery.UpdateDBItem(connectionString, upc, difference);
                return;
            }

            else if (item.Equals(updatedItem) && !GenericQuery.CheckItemStock(connectionString, updatedItem, test - 1))
            {
                UserNotify.outstock(item.ItemName.Trim());
            }
        }
            // Inform the user the quantity is alreadt exceed the stock
            return;
    }

    // need to debug
    public int GetItemQuantity (string upc)
    {
        CartItem updatedItem = new CartItem(upc);
        foreach (CartItem item in Items)
        {
            if (item.Equals(updatedItem))
            {
                return item.Quantity;
            }
        }
        return 0;
    }

    /*
     * RemoveItem() removes an item from the shopping cart.
     */
    public void RemoveItem(string upc)
    {
        CartItem removedItem = new CartItem(upc);
        Items.Remove(removedItem);
    }

    //retrieve from db
    public void RetrieveFromDB(string connectString, string UserName)
    {
        userName = UserName;
        string query = "SELECT [orderNum], [confirmationNumber] FROM [Order] WHERE ([username] =N'" + userName + "')";
        int OrderNum = 0;
        // Create the connection and the SQL command.
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            // Open the connection.
            command.Connection.Open();
            // Execute the SELECT query and place the result in a DataReader.
            SqlDataReader reader = command.ExecuteReader();
            // Check if a result was returned.
            if (reader.HasRows)
            {
                // Iterate through the table to get the retrieved values.
                while (reader.Read())
                {
                    // ToAsk: what happens when there are two rows ?
                    if (reader.IsDBNull(1))
                    {
                        OrderNum = int.Parse(reader["orderNum"].ToString());
                    }

                }
                command.Connection.Close(); // Close the connection and the DataReader.
                reader.Close();
                RetrieveFromDBOrderItem(connectionString, OrderNum);
            }

        }
        return;
    }

    //retrieve from order Item
    public void RetrieveFromDBOrderItem(string connectionString, int OrderNum)
    {
        string query = "SELECT [upc], [quantity],[PriceWhenAdded],[removed] FROM [OrderItem] WHERE ([orderNum] =N'" + OrderNum + "')";
        string UPC = null;
        int Quantity = 0;
        decimal price;
        // Create the connection and the SQL command.
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            // Open the connection.
            command.Connection.Open();
            // Execute the SELECT query and place the result in a DataReader.
            SqlDataReader reader = command.ExecuteReader();
            // Check if a result was returned.
            if (reader.HasRows)
            {
                // Iterate through the table to get the retrieved values.
                while (reader.Read())
                {
                    UPC = reader.GetString(0);
                    Quantity = reader.GetInt32(1);
                    price = reader.GetDecimal(2);
                    if (reader.GetBoolean(3))
                    {
                        AddItemFromDBItem(connectionString, UPC, Quantity, price, false);
                    }
                    else AddItemFromDBItem(connectionString, UPC, Quantity, price, true);

                }
            }
            command.Connection.Close();
            reader.Close();
        }
    }

    public void AddItemFromDBItem(string connectionString, string UPC, int Quantity, decimal price, bool removed)
    {
        // AddItem(string upc, string name, decimal discountPrice, quantity, 1)
        string query = "SELECT [name], [discountPrice] FROM [Item] WHERE ([upc] =N'" + UPC + "')";
        string name = null;
        decimal _price = price;
        // Create the connection and the SQL command.
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            // Open the connection.
            command.Connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            // Check if a result was returned.
            if (reader.HasRows)
            {
                // Iterate through the table to get the retrieved values.
                reader.Read();
                name = reader.GetString(0);
                
                if (reader.GetDecimal(1) < price)
                {
                    _price = reader.GetDecimal(1);
                    CartItem updatedItem = new CartItem(UPC);
                    foreach (CartItem item in Items)
                    {
                        if (item.Equals(updatedItem))
                        {
                            item.DiscountPrice = _price;
                        }
                    }

                }
                

            }
            command.Connection.Close();
            reader.Close();
            GenericQuery.SubUpdateOrderItem(connectionString, GenericQuery.GetOrderNumber(connectionString, userName), UPC, Quantity, _price);
            this.AddItem(UPC, name, _price, Quantity, removed);
        }
    }

    /*
     * 
     */
    public string checkOut()
    {
        String authenticationCode = CreditCardAuthorization.chargeCard(this.getCreditCard(), this.GetCartTotal());
        if (authenticationCode == null)
        {
            return null;
        }
        String confirmationString = this.generateConfirmationNumber();
        string query = "UPDATE [Order] SET [confirmationNumber] = '" + confirmationString + "', code = '" + authenticationCode + "' WHERE ([username] =N'" + this.userName + "' AND [confirmationNumber] IS NULL)";
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        return confirmationString;
    }

    private string getCreditCard()
    {
        string query = "SELECT [creditCardNumber] FROM [Order] WHERE ([username] =N'" + this.userName + "' AND [confirmationNumber] IS NULL)";

        SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
        SqlCommand command = new SqlCommand(query, connection);
        command.Connection.Open();
        String cardNumber = (String)command.ExecuteScalar();

        return cardNumber;
    }

    private string generateConfirmationNumber()
    {
        const int firstConfirmationNumber = 999999 * 16 * 16;

        Int32 confirmationNumber = firstConfirmationNumber - this.getOrderNum();

        char letter1 = (char)(((confirmationNumber >> 24) & 15) + 65);
        char letter2 = (char)(((confirmationNumber >> 20) & 15) + 65);
        Int32 sixDigits = (confirmationNumber << 12) >> 12;
        return letter1.ToString() + letter2.ToString() + sixDigits.ToString().PadLeft(6, '0');
    }

    public Int32 getOrderNum()
    {
        string query = "SELECT [orderNum] FROM [Order] WHERE ([username] = N'" + this.userName + "' AND [confirmationNumber] IS NULL)";
        
        // Create the connection and the SQL command.
        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            // Open the connection.
            command.Connection.Open();

            Object raw = command.ExecuteScalar();
            if (raw == null)
            {
                return -1;
            }
            else
            {
                return (Int32)raw;
            }
        }
    }
#endregion

#region Reporting Methods
    /*
     * GetCartTotal() returns the total price of all of the items in the shopping cart.
     */
    public decimal GetCartTotal()
    {
        decimal cartTotal = 0;
        foreach (CartItem item in Items)
            cartTotal += item.TotalPrice;
        return cartTotal;
    }

#endregion
}

