using Org.BouncyCastle.Asn1.Mozilla;
using System;

namespace Jelineksoft.Entity.TestApp
{
    public class DbContext: Jelineksoft.Entity.DatabaseBase
    {
        public DbContext()
        :base("TestovaciDB")
        {
            Jelineksoft.Entity.Settings.ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=P:\Jelineksoft\JelinekPetr.Entities\Jelineksoft.Entity.TestApp\TestDB.mdf;Integrated Security=True;Connect Timeout=30";
            Jelineksoft.Entity.Settings.DefaultProvider = new Jelineksoft.Entity.Providers.MSSQLProvider( );
            Jelineksoft.Entity.Settings.SetDBNullToDefaultClassValue = false; //When in DB table is null value, setter in class property set default column value
            Jelineksoft.Entity.Settings.Log.LogSQLToConsole = true;

            TableOrder = new TableOrder();
            TableOrderItem = new TableOrderItem();
        }
        
        public TableOrder TableOrder { get; set; }
        public TableOrderItem TableOrderItem { get; set; }
        
    }
}