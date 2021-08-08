using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBookApp.Utility
{
    public static class SD
    {
        public const string Role_User_Indi = "Individual Customer";
        public const string Role_User_Admin = "Admin";
        public const string Role_User_Company = "Company Customer";
        public const string Role_User_Employee = "Employee";

        public const string ssShopingCart= "Shoping Cart Session";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCanceled = "Canceled";
        public const string StatusRefunded = "Refunded";


        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";


        public static double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if(quantity < 50)
            {
                return price;
            }
            else
            {
                if(quantity < 100)
                {
                    return price50;
                }
                return price100;
            }
        }

        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
}
