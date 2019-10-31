using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;

namespace ShechterShoppingList.Models
{
    [DynamoDBTable("ShechterShoppingList")]
    public class Grocery
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }
        public string GroceyName { get; set; }
        public int Ammount { get; set; }
        public string Measure { get; set; }

        public enum UnitOfMeasure
        {
            Gr,
            Kg,
            Unit,
            Box,
        }
    }
}
