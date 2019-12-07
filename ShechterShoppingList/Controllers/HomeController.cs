using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using ShechterShoppingList.Models;
using ShechterShoppingList.ViewModels;

namespace ShechterShoppingList.Controllers
{
    public class HomeController : Controller
    {
        //IAmazonDynamoDB AWSDBclient { get; set; }
        //private const string tableName = "ShechterShoppingList";
        //public DynamoDBContext DBContext { get; set; }

        public async Task<IActionResult> Index (Grocery grocery)
        {
            try
            {
                if (string.IsNullOrEmpty (grocery.GroceyName))
                {
                    var results = await DynamoDbCRUDOperations.GetDataAsync ();
                    var groceries = results;
                    GroceryViewModel viewModel = new GroceryViewModel { Groceries = groceries };

                    return View ("Index", viewModel);
                }
                return View ();
            }
            catch (Exception e)
            {
                return RedirectToAction("Error","Home",new {message = e.Message});
            }
        }

        [Route ("/Home/Error",Name = "Error")]
        public async Task<IActionResult> ErrorPage(string message)
        {
                ErrorViewModel evm = new ErrorViewModel();
                await Task.Delay(100);
                evm.ErrorMessage = message;
                
                return View("ErrorPage",evm);        
        }

        [Route ("/Home/Edit", Name = "Edit")]
        public async Task<IActionResult> Edit (string Id)
        {
            var guid = Guid.Parse (Id);
            var results = await DynamoDbCRUDOperations.GetItemsById (guid);
            Grocery grocery = results;
            return View (grocery);
        }

        public async Task<IActionResult> UpdateDataAsync (string gId, string gName, int gAmmount, Grocery gModel)
        {
            if (string.IsNullOrEmpty (gId) || string.IsNullOrEmpty (gName) || string.IsNullOrEmpty (gModel.Measure))
            {
                return BadRequest ();
            }

            var guid = Guid.Parse (gId);
            var lst = new List<Grocery> ();
            try
            {
                Grocery grocery = DynamoDbCRUDOperations.GetItemsById (guid).Result;
                lst.Add (grocery);

                Grocery updatedData = new Grocery
                {
                    Ammount = gAmmount,
                    DateModified = DateTime.Parse (DateTime.Now.ToShortDateString () + " " + DateTime.Now.ToShortTimeString ()),
                    GroceyName = gName,
                    Id = guid,
                    Measure = gModel.Measure,
                    Done = grocery.Done,
                };

                await DynamoDbCRUDOperations.UpdateItemAsync (lst, updatedData);
                return RedirectToAction ("Index", "Home", null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<IActionResult> AddNewGroceryAsync (string gName, int gAmmount, GroceryViewModel gModel)
        {
            if (string.IsNullOrEmpty (gName) || string.IsNullOrEmpty (gModel.NewGrocery.Measure))
            {
                return BadRequest ();
            }

            var guid = Guid.NewGuid ();
            try
            {
                Grocery newGrocery = new Grocery
                {
                    Ammount = gAmmount,
                    DateModified = DateTime.Parse (DateTime.Now.ToShortDateString () + " " + DateTime.Now.ToShortTimeString ()),
                    GroceyName = gName,
                    Id = guid,
                    Measure = gModel.NewGrocery.Measure,
                    Done = false
                };

                await DynamoDbCRUDOperations.AddItemAsync (newGrocery);
                return RedirectToAction ("Index", "Home", null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<IActionResult> DeleteItemAsync (Grocery gId)
        {
            if (gId.Id == null)
            {
                return BadRequest ();
            }

            var guid = gId.Id;

            try
            {
                Grocery grocery = DynamoDbCRUDOperations.GetItemsById (guid).Result;
                if (grocery != null)
                {
                    await DynamoDbCRUDOperations.DeleteItemAsync (guid);

                    return RedirectToAction ("Index", "Home", null);
                }
                else
                    return BadRequest ();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<IActionResult> ToggleDoneAsync (Grocery gId)
        {
            if (gId.Id == null)
            {
                return BadRequest ();
            }

            var guid = gId.Id;
            var lst = new List<Grocery> ();
            try
            {
                Grocery grocery = DynamoDbCRUDOperations.GetItemsById (guid).Result;
                lst.Add (grocery);

                Grocery updatedData = new Grocery
                {
                    Ammount = grocery.Ammount,
                    DateModified = DateTime.Parse (DateTime.Now.ToShortDateString () + " " + DateTime.Now.ToShortTimeString ()),
                    GroceyName = grocery.GroceyName,
                    Id = guid,
                    Measure = grocery.Measure,
                    Done = !grocery.Done,
                };

                await DynamoDbCRUDOperations.UpdateItemAsync (lst, updatedData);
                return RedirectToAction ("Index", "Home", null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public HomeController (IAmazonDynamoDB amazonDBService)
        {
            DynamoDbCRUDOperations operations = new DynamoDbCRUDOperations (amazonDBService);
            //var groceries = DynamoDbCRUDOperations.GetDataAsync().Result;

        }

        [ResponseCache (Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error ()
        {
            return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}