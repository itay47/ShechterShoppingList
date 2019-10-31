using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShechterShoppingList.Models;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.DynamoDBv2.Model;
using System.Threading;

namespace ShechterShoppingList.Controllers
{
    public class HomeController : Controller
    {
        IAmazonDynamoDB AWSDBclient { get; set; }
        private const string tableName = "ShechterShoppingList";
        public DynamoDBContext DBContext { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        public HomeController(IAmazonDynamoDB DynamoDBClient)
        {
            //get credentials from app.config
            this.AWSDBclient = DynamoDBClient;
            
            //Set a local DB context
            DBContext = new DynamoDBContext(AWSDBclient);
        }

        private async Task ConnectToTableAsync()
        {
            var tableResponse = await AWSDBclient.ListTablesAsync();

            //create new table if it's missing
            if (!tableResponse.TableNames.Contains(tableName))
            {
                await CreateTableAsync();
            }
        }

        private static Grocery GenerateData(string GroceryName, string Ammount, Grocery.UnitOfMeasure measure)
        {
            return new Grocery
            {
                Id = Guid.NewGuid(),
                GroceyName = GroceryName,
                Ammount = int.Parse(Ammount),
                Measure = measure.ToString()
            };
        }

        private async Task<List<Grocery>> GetItemsById(Guid guid)
        {
            List<ScanCondition> conditions = new List<ScanCondition>
                {
                    new ScanCondition("Id", ScanOperator.Equal, guid)
                };

            var tableResponse = await AWSDBclient.ListTablesAsync();
            if (tableResponse.TableNames.Contains(tableName))
            {
                var allDocs = await DBContext.ScanAsync<Grocery>(conditions).GetRemainingAsync();
                return allDocs;
            }
            else
                return new List<Grocery>();
        }

        private async Task UpdateItemAsync(List<Grocery> allDocs, Grocery newData)
        {
            if (allDocs.Count > 0 && allDocs != null && newData != null)
            {
                var doc = allDocs.SingleOrDefault();
                //doc.Id = newData.Id;  <--- we don't want to create new item, we want to update existing!
                doc.GroceyName = newData.GroceyName;
                doc.Measure = newData.Measure;
                doc.Ammount = 3;

                await DBContext.SaveAsync<Grocery>(doc);
            }
        }

        private async Task CreateTableAsync()
        {
            //Table not found, creating table
            await AWSDBclient.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                },
                KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "Id",
                            KeyType = KeyType.HASH
                        }
                    },
                AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition { AttributeName = "Id", AttributeType=ScalarAttributeType.S }
                    },
            });

            bool isTableAvailable = false;
            while (!isTableAvailable)
            {
                //Waiting for table to be active...
                Thread.Sleep(5000);
                var tableStatus = await AWSDBclient.DescribeTableAsync(tableName);
                isTableAvailable = tableStatus.Table.TableStatus == TableStatus.ACTIVE;
            }
        }

        private async Task<List<Grocery>> GetDataAsync()
        {
            try
            {
                await ConnectToTableAsync();

                List<ScanCondition> conditions = new List<ScanCondition>();
                var allDocs = await DBContext.ScanAsync<Grocery>(conditions).GetRemainingAsync();
                if (allDocs != null)
                {
                    return allDocs;
                }
                else
                { return new List<Grocery>(); }
            }
            catch (AmazonDynamoDBException ex)
            { throw ex; }
            catch (AmazonServiceException ex)
            { throw ex; }
            catch (Exception ex)
            { throw ex; }
        }

        private async Task DeleteItemAsync(Guid existingGuid)
        {
            if (!string.IsNullOrEmpty(existingGuid.ToString()))
            {
                try
                {
                    var DBItems = await GetItemsById(existingGuid);
                    var itemtoDelete = DBItems.SingleOrDefault();

                    await DBContext.DeleteAsync<Grocery>(itemtoDelete);
                }
                catch (Exception ex)
                { throw ex; }
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
