using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jelineksoft.Entity.TestApp
{
    public class TableOrderItem: Jelineksoft.Entity.TableBase
    {
        public TableOrderItem()
      : base("OrderItem")
        {
            //Columns creating
            Id = new Column(this, "Id", "INT", true, true);
            Description = new Column(this, "Description", "NVARCHAR(50)", false);
            Quantity= new Column(this, "Quantity", "DECIMAL", false);
            OrderId = new Column(this, "OrderId", "INT", false);
        }

        //Columns def.
        public Column Id { get; set; }
        public Column Description { get; set; }
        public Column Quantity { get; set; }
        public Column OrderId { get; set; }

        public override RowBase GetNewRow()
        {
            //Return new Row class
            return new TableOrderItemRow();
        }

        public override IList GetCollectionRows()
        {
            //Return new Rows collection
            return TypedRows;
        }

        public ObservableCollection<TableOrderItemRow> TypedRows { get; set; } = new ObservableCollection<TableOrderItemRow>();


        public class TableOrderItemRow : Jelineksoft.Entity.RowBase
        {
            public TableOrderItemRow()
                : base(new TableOrderItem())
            {
                //Default values
                Id = 0;
                Quantity = 0;
            }

            //Data properties
            public int Id { get; set; }
            public string Description { get; set; }
            public decimal Quantity { get; set; }
            public int OrderId { get; set; }

            //Custom properties
            public TableOrder.TableOrderRow OrderRef { get; set; }

        }

    }
}