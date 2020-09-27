using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jelineksoft.Entity.TestApp
{
    /// <summary>
    /// Table definition
    /// </summary>
    public class TableOrder : Jelineksoft.Entity.TableBase
    {
        
        public TableOrder()
        : base("Order")
        {
            //Columns creating
            Id = new Column(this, "Id", "INT", true, true);
            Number = new Column(this, "Number", "NVARCHAR(50)", false);
            DateCreate = new Column(this, "DateCreate", "DATETIME2", false);
        }

        //Columns def.
        public Column Id { get; set; }
        public Column Number { get; set; }
        public Column DateCreate { get; set; }

        public override RowBase GetNewRow()
        {
            //Return new Row class
            return new TableOrderRow();
        }

        public override IList GetCollectionRows()
        {
            //Return new Rows collection
            return TypedRows;
        }

        public ObservableCollection<TableOrderRow> TypedRows { get; set; } = new ObservableCollection<TableOrderRow>();


        public class TableOrderRow : Jelineksoft.Entity.RowBase
        {
            public TableOrderRow()
                : base(new TableOrder())
            {
                //Default values
                Id = 0;
                DateCreate = DateTime.Now;
            }

            //Data properties
            public int Id { get; set; }
            public string Number { get; set; }
            public DateTime DateCreate { get; set; }

            //Other methods and properties
            public List<TableOrderItem.TableOrderItemRow> Items { get; set; } = new List<TableOrderItem.TableOrderItemRow>();


        }

    }
}