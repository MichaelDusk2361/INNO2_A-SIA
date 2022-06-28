using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.IO;

namespace A_SIA2WebAPI.DAL.Neo4J
{
    /// <summary>
    /// Database class for accessing the neo4j graphdb and running
    /// cypher queries on it
    /// </summary>
    public class Neo4JDatabase : IDisposable
    {
        private bool _disposed = false;
        private readonly IDriver _driver;
        private IConfiguration _configuration;

        ~Neo4JDatabase() => Dispose(false);

        public Neo4JDatabase()
        {
            string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (environment == null)
                environment = "Development";

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile($"neo4j.{environment}.json");
            _configuration = builder.Build();

            _driver = GraphDatabase.Driver(
                _configuration["uri"].ToString(),
                AuthTokens.Basic(
                    _configuration["username"].ToString(),
                    _configuration["password"].ToString()
                ));
        }

        /// <summary>
        /// Runs the given query on the database connection
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<IRecord> RunQuery(Query query)
        {
            var session = _driver.AsyncSession();

            var transaction = session.BeginTransactionAsync().Result;

            var result = transaction.RunAsync(query).Result.ToListAsync().Result;

            transaction.CommitAsync().Wait();

            transaction.Dispose();

            session.CloseAsync().Wait();

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _driver?.Dispose();
            }

            _disposed = true;
        }
    }
}
