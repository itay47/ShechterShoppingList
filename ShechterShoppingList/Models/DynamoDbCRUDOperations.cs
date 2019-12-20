using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShechterShoppingList.Models
{
    public class DynamoDbCRUDOperations
    {
        private static IAmazonDynamoDB AWSDBclient { get; set; }
        private const string tableName = "ShechterShoppingList";
        private static DynamoDBContext DBContext { get; set; }

        public DynamoDbCRUDOperations(IAmazonDynamoDB amazonDynamoDB)
        {
            AWSDBclient = amazonDynamoDB;
            DBContext = new DynamoDBContext(AWSDBclient);
        }

        private static Grocery GenerateData(string GroceryName, int Ammount, string measure)
        {
            return new Grocery
            {
                Id = Guid.NewGuid(),
                GroceyName = GroceryName,
                //Ammount = int.Parse(Ammount),
                Ammount = Ammount,
                Measure = measure,
                DateModified = DateTime.Parse(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()),
                Done = false,
            };
        }


        public static async Task AddItemAsync(Grocery grocery)
        {
            await ConnectToTableAsync();

            Grocery dBModel = GenerateData(grocery.GroceyName, grocery.Ammount, grocery.Measure);

            await DBContext.SaveAsync<Grocery>(dBModel);
        }

        private static async Task ConnectToTableAsync()
        {
            var tableResponse = await AWSDBclient.ListTablesAsync();

            //create new table if it's missing
            if (!tableResponse.TableNames.Contains(tableName))
            {
                await CreateTableAsync();
            }
        }

        public static async Task<Grocery> GetItemsById(Guid guid)
        {
            List<ScanCondition> conditions = new List<ScanCondition>
                {
                    new ScanCondition("Id", ScanOperator.Equal, guid)
                };

            var tableResponse = await AWSDBclient.ListTablesAsync();
            if (tableResponse.TableNames.Contains(tableName))
            {
                var allDocs = await DBContext.ScanAsync<Grocery>(conditions).GetRemainingAsync();
                var singleDoc = allDocs.SingleOrDefault();
                return singleDoc;
            }
            else
                return new Grocery();
        }

        public static async Task UpdateItemAsync(List<Grocery> allDocs, Grocery newData)
        {
            if (allDocs.Count > 0 && allDocs != null && newData != null)
            {
                var doc = allDocs.SingleOrDefault();
                //doc.Id = newData.Id;  <--- we don't want to create new item, we want to update existing!
                doc.GroceyName = newData.GroceyName;
                doc.Measure = newData.Measure;
                doc.Ammount = newData.Ammount;
                doc.DateModified = newData.DateModified; //DateTime.Parse(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                doc.Done = newData.Done;

                await DBContext.SaveAsync<Grocery>(doc);
            }
        }

        public static async Task<List<Grocery>> GetDataAsync()
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

        public static async Task DeleteItemAsync(Guid existingGuid)
        {
            if (!string.IsNullOrEmpty(existingGuid.ToString()))
            {
                try
                {
                    var DBItems = await GetItemsById(existingGuid);
                    var itemtoDelete = DBItems;

                    await DBContext.DeleteAsync<Grocery>(itemtoDelete);
                }
                catch (Exception ex)
                { throw ex; }
            }
        }

        private async static Task CreateTableAsync()
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
    }
}
