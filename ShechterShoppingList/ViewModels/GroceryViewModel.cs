using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShechterShoppingList.Models;

namespace ShechterShoppingList.ViewModels
{
    public class GroceryViewModel
    {
        public List<Grocery> Groceries { get; set; }
        public Grocery _Grocery { get; set; }
    }
}
