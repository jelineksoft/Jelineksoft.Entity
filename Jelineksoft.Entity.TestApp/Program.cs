using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Jelineksoft.Entity.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Writing to DB Orders...");
            var db = new DbContext();

            for (var i = 0; i<100; i++)
            {
                var o = new TableOrder.TableOrderRow();
                o.DbHelpers.IsNew = true;
                o.DateCreate = DateTime.Now;
                o.Number = "Nr. " + i.ToString();
                o.Save();
                for (var x = 0; x<10; x++)
                {
                    var oi = new TableOrderItem.TableOrderItemRow();
                    oi.Description = "Item nr. " + x.ToString();
                    oi.OrderId = o.Id;
                    oi.Quantity = x * 5;
                    oi.Save();
                }
            }
            Console.WriteLine("Writing to DB Orders... OK");
            Console.WriteLine("Selecting from DB Orders...");

            var xo = new TableOrder();
            xo.AddFrom(xo);
            xo.AddOrderBy(xo.DateCreate, OrderByEnum.Desc);
            xo.LoadData();

            foreach (var oo in xo.TypedRows)
            {
                Console.WriteLine($"Id:  {oo.Id}    Number: {oo.Number}");
                var xi = new TableOrderItem();
                xi.AddFrom(xi);
                xi.AddWhere(xi.OrderId, oo.Id);
                xi.LoadData();
                foreach (var ii in xi.TypedRows)
                {
                    ii.OrderRef = oo;
                    Console.WriteLine($"         ItemId:  {ii.Id}    Description: {ii.Description}   Quantity: {ii.Quantity}");
                }
            }
        }
    }
}
