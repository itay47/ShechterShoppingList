using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Display(Name = "Grocery Name")]
        [DynamoDBProperty]
        public string GroceyName { get; set; }

        [DynamoDBProperty]
        public int Ammount { get; set; }

        [DynamoDBIgnore]
        public UnitOfMeasure MeasureSelectList { get; set; }

        [DynamoDBProperty]
        public string Measure { get; set; }

        [Display(Name = "Date Modified")]
        [DynamoDBProperty]
        public DateTime DateModified { get; set; }

        public enum UnitOfMeasure
        {
            Gr,
            Kg,
            Unit,
            Box,
        }
    }
}
