// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class GearsOfWarQuerySqlServerFixture : GearsOfWarQueryRelationalFixture<SqlServerTestStore>
    {
        public static readonly string DatabaseName = "GearsOfWarQueryTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public GearsOfWarQuerySqlServerFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder();
                    optionsBuilder.UseSqlServer(_connectionString);

                    using (var context = new GearsOfWarContext(_serviceProvider, optionsBuilder.Options))
                    {
                        // TODO: Delete DB if model changed

                        if (context.Database.EnsureCreated())
                        {
                            GearsOfWarModelInitializer.Seed(context);
                        }

                        TestSqlLoggerFactory.SqlStatements.Clear();
                    }
                });
        }

        public override GearsOfWarContext CreateContext(SqlServerTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(testStore.Connection);

            var context = new GearsOfWarContext(_serviceProvider, optionsBuilder.Options);
            context.Database.AsRelational().Connection.UseTransaction(testStore.Transaction);
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ForSqlServer().UseSequence();
        }
    }
}
