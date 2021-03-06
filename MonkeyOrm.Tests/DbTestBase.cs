﻿// <copyright file="DbTestBase.cs" company="Sinbadsoft">
// Copyright (c) Chaker Nakhli 2012
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at http://www.apache.org/licenses/LICENSE-2.0 Unless required by 
// applicable law or agreed to in writing, software distributed under the License
// is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// </copyright>
// <author>Chaker Nakhli</author>
// <email>chaker.nakhli@sinbadsoft.com</email>
// <date>2012/02/19</date>
using System;
using System.Collections.Generic;
using System.Data;

using MySql.Data.MySqlClient;
using NUnit.Framework;

namespace MonkeyOrm.Tests
{
    public class DbTestBase
    {
        private const string ConnectionString = "server=localhost;user id=developer;password=etOile03;port=3306;";
        private string connectionString;

        public DbTestBase(bool createTestTable = true)
        {
            this.CreateTestTable = createTestTable;
        }

        protected string DatabaseName { get; set; }

        protected bool CreateTestTable { get; set; }


        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.DatabaseName = this.GetType().Name + DateTime.UtcNow.ToString("yyyy_MM_dd__HH_mm_ss");
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                connection.Execute(string.Format("CREATE DATABASE IF NOT EXISTS `{0}`", this.DatabaseName));
            }

            var connectionStringBuilder = new MySqlConnectionStringBuilder(ConnectionString) { Database = this.DatabaseName };
            this.connectionString = connectionStringBuilder.ConnectionString;
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            this.ConnectionFactory().Execute(string.Format("DROP DATABASE IF EXISTS `{0}`", this.DatabaseName));
        }

        [SetUp]
        public void SetUp()
        {
            if (this.CreateTestTable)
            {
                this.ConnectionFactory().Execute(
                   @"CREATE TABLE `Test` (
                    `Id` INT NOT NULL AUTO_INCREMENT,
                    `DataInt` INT,
                    `DataLong` BIGINT,
                    `DataString` VARCHAR(50) DEFAULT 'A Default Value',
                    PRIMARY KEY (`Id`)) AUTO_INCREMENT=0 ENGINE=InnoDB");
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (this.CreateTestTable)
            {
                this.ConnectionFactory().Execute(@"DROP TABLE IF EXISTS `Test`");
            }
        }

        protected static void CheckTestObject(dynamic expected, dynamic actual, bool defaultValueForString = false)
        {
            Assert.AreEqual(expected.DataInt, actual.DataInt);
            Assert.AreEqual(expected.DataLong, actual.DataLong);
            Assert.AreEqual(defaultValueForString ? "A Default Value" : expected.DataString, actual.DataString);
        }

        /// <summary>
        /// Generates a batch of objects all of them having the same property set.
        /// </summary>
        protected static IEnumerable<TestData> GenerateBatch(int size)
        {
            for (int i = 0; i < size; i++)
            {
                yield return new TestData { DataInt = 5 * i, DataLong = 3000000000L + (1000 * i), DataString = "hello world" + i };
            }
        }

        protected IConnectionFactory ConnectionFactory()
        {
            return new ConnectionFactory<MySqlConnection>(this.connectionString);
        }

        protected IDbConnection CreateAndOpen()
        {
            var connection = this.ConnectionFactory().Create();
            connection.Open();
            return connection;
        }

        public class TestData
        {
            public int DataInt { get; set; }

            public long DataLong { get; set; }

            public string DataString { get; set; }
        }
    }
}
