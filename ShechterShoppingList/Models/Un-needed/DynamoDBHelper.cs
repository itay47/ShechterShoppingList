using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using log4net;


namespace ShechterShoppingList.Models
{
    public class DynamoDBHelper
    {
        AmazonDynamoDBClient client;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DynamoDBHelper));
        private readonly string accessKeyId, secretKey, serviceUrl;

        public DynamoDBHelper(IAmazonDynamoDB dynamoDBService)
        {
            client = GetClient();
        }

        public DynamoDBHelper(string accessKeyId, string secretKey, string serviceUrl)
        {
            this.accessKeyId = accessKeyId;
            this.secretKey = secretKey;
            this.serviceUrl = serviceUrl;
            client = GetClient();
        }

        /// <summary>
        /// Initializes and returns the DynamoDBClient object
        /// </summary>
        /// <returns></returns>
        private AmazonDynamoDBClient GetClient()
        {
            if (client == null)
            {
                try
                {
                    // DynamoDB config object
                    AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig
                    {
                        // Set the endpoint URL
                        ServiceURL = serviceUrl
                    };
                    client = new AmazonDynamoDBClient(); //<-- Get from appsettings.json
                    //client = new AmazonDynamoDBClient(accessKeyId, secretKey, clientConfig);
                }
                catch (AmazonDynamoDBException ex)
                { _logger.Error($"Error (AmazonDynamoDBException) creating dynamodb client", ex); }
                catch (AmazonServiceException ex)
                { _logger.Error($"Error (AmazonServiceException) creating dynamodb client", ex); }
                catch (Exception ex)
                { _logger.Error($"Error creating dynamodb client", ex); }
            }
            return client;
        }

        /// <summary>
        /// Creates new table in DynamoDB
        /// </summary>
        /// <param name="tableName">name of the table to create</param>
        /// <param name="hashKey">Hash key name</param>
        /// <param name="haskKeyType">Hask key type</param>
        /// <param name="rangeKey">range key name</param>
        /// <param name="rangeKeyType">range key type</param>
        public async Task CreateTable(string tableName, string hashKey, ScalarAttributeType haskKeyType, string rangeKey = null, ScalarAttributeType rangeKeyType = null)
        {
            // Build a 'CreateTableRequest' for the new table
            CreateTableRequest createRequest = new CreateTableRequest
            {
                TableName = tableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 1,
                    WriteCapacityUnits = 1
                }
            };
            List<KeySchemaElement> schemaElements = new List<KeySchemaElement>();
            List<AttributeDefinition> attributeDefinitions = new List<AttributeDefinition>();

            schemaElements.Add(new KeySchemaElement
            {
                AttributeName = hashKey,
                KeyType = KeyType.HASH
            });

            attributeDefinitions.Add(new AttributeDefinition
            {
                AttributeName = hashKey,
                AttributeType = haskKeyType
            }
            );

            if (!string.IsNullOrEmpty(rangeKey) && !string.IsNullOrEmpty(rangeKeyType))
            {
                schemaElements.Add(new KeySchemaElement
                {
                    AttributeName = rangeKey,
                    KeyType = KeyType.RANGE
                });
                attributeDefinitions.Add(new AttributeDefinition
                {
                    AttributeName = rangeKey,
                    AttributeType = rangeKeyType
                }
               );
            }

            try
            {
                var client = GetClient();
                await client.CreateTableAsync(createRequest);
                bool isTableAvailable = false;
                while (!isTableAvailable)
                {
                    Thread.Sleep(2000);
                    var tableStatus = await client.DescribeTableAsync(tableName);
                    isTableAvailable = tableStatus.Table.TableStatus == "ACTIVE";
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Error: failed to create the new table:{ex.Message}");
                return;
            }
        }

        /// <summary>
        /// Get all the records from the given table
        /// </summary>
        /// <typeparam name="T">Table object</typeparam>
        /// <returns></returns>
        public async Task<IList<T>> GetAll<T>()
        {
            var context = new DynamoDBContext(GetClient());
            // Here we are passing the ScanCoditions as empty to get all the rows
            List<ScanCondition> conditions = new List<ScanCondition>();
            return await context.QueryAsync<T>(conditions).GetRemainingAsync();
        }

        /// <summary>
        /// Get the rows from the given table which maches the given key and conditions 
        /// </summary>
        /// <typeparam name="T">Table object</typeparam>
        /// <param name="keyValue">hash key value</param>
        /// <param name="scanConditions">any other scan conditions</param>
        /// <returns></returns>
        public async Task<IList<T>> GetRows<T>(object keyValue, List<ScanCondition> scanConditions = null)
        {
            var context = new DynamoDBContext(GetClient());
            DynamoDBOperationConfig config = null;

            if (scanConditions != null && scanConditions.Count > 0)
            {
                config = new DynamoDBOperationConfig()
                {
                    QueryFilter = scanConditions
                };
            }
            return await context.QueryAsync<T>(keyValue, config).GetRemainingAsync();
        }

        /// <summary>
        /// Get the rows from the given table which maches the given conditions 
        /// </summary>
        /// <typeparam name="T"> Table object</typeparam>
        /// <param name="scanConditions"></param>
        /// <returns></returns>
        public async Task<IList<T>> GetRows<T>(List<ScanCondition> scanConditions)
        {
            var context = new DynamoDBContext(GetClient());
            return await context.ScanAsync<T>(scanConditions).GetRemainingAsync();
        }

        /// <summary>
        /// Gets a record which matches the given key value
        /// </summary>
        /// <typeparam name="T">Table object</typeparam>
        /// <param name="keyValue">Hash key value</param>
        /// <returns></returns>
        public T Load<T>(object keyValue)
        {
            var context = new DynamoDBContext(GetClient());
            return context.LoadAsync<T>(keyValue).Result;
        }

        /// <summary>
        /// Saves the given record in the table
        /// </summary>
        /// <typeparam name="T">Table object</typeparam>
        /// <param name="document">Record to save in the table</param>
        /// <returns></returns>
        public async Task Save<T>(T document)
        {
            var context = new DynamoDBContext(GetClient());
            await context.SaveAsync(document);
        }

        /// <summary>
        /// Deletes the given record in the table
        /// </summary>
        /// <typeparam name="T">Table object</typeparam>
        /// <param name="document">Record to be removed from the table</param>
        /// <returns></returns>
        public async Task Delete<T>(T document)
        {
            var context = new DynamoDBContext(GetClient());
            await context.DeleteAsync(document);
        }

        /// <summary>
        /// Saves batch of records in the table
        /// </summary>
        /// <typeparam name="T">Table object</typeparam>
        /// <param name="documents">Records to be saved</param>
        /// <returns></returns>
        public async Task BatchSave<T>(IEnumerable<T> documents)
        {
            var context = new DynamoDBContext(GetClient());
            var batch = context.CreateBatchWrite<T>();
            batch.AddPutItems(documents);
            await batch.ExecuteAsync();
        }

        /// <summary>
        /// Deletes batch of records in the table
        /// </summary>
        /// <typeparam name="T">Table object</typeparam>
        /// <param name="documents">Records to be delete</param>
        /// <returns></returns>
        public async Task BatchDelete<T>(IEnumerable<T> documents)
        {
            var context = new DynamoDBContext(GetClient());
            var batch = context.CreateBatchWrite<T>();
            batch.AddDeleteItems(documents);
            await batch.ExecuteAsync();
        }
    }
}
