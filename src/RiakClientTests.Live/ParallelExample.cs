// <copyright file="ParallelExample.cs" company="Basho Technologies, Inc.">
// Copyright 2011 - OJ Reeves & Jeremiah Peschka
// Copyright 2014 - Basho Technologies, Inc.
//
// This file is provided to you under the Apache License,
// Version 2.0 (the "License"); you may not use this file
// except in compliance with the License.  You may obtain
// a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>

namespace RiakClientTests.Live.GeneralIntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using RiakClient;
    using RiakClient.Config;
    using RiakClient.Models;

    [TestFixture, Example]
    public class ParallelExample : LiveRiakConnectionTestBase
    {
        [Test, Ignore("Example")]
        public void Parallel_ForEach_Can_Be_Used_To_Put_And_Get_Objects()
        {
            const int numNodes = 4;
            const int poolSize = 8;
            const int totalConnectionCount = poolSize * numNodes;
            const ushort portInterval = 10;
            const ushort startingPort = 10017;
            const ushort endingPort = startingPort + ((numNodes - 1) * portInterval);
            const int totalObjects = 65536;

            byte[] data = new byte[65536];
            Random.NextBytes(data);

            int batchSize = totalConnectionCount;
            int totalBatches = totalObjects / batchSize;
            Assert.AreEqual(0, totalObjects % batchSize);
            Debug.WriteLine("batchSize: {0}, totalBatches: {1}", batchSize, totalBatches);

            string riakHost = Environment.GetEnvironmentVariable("RIAK_HOST");
            if (String.IsNullOrWhiteSpace(riakHost))
            {
                riakHost = "riak-test";
            }

            Debug.WriteLine("Riak host: {0}", riakHost);

            Assert.AreEqual(10047, endingPort);

            var objs = new List<RiakObject>();
            for (int i = 0; i < totalObjects; i++)
            {
                var key = String.Format("{0}_{1}", TestKey, i);
                var id = new RiakObjectId(TestBucket, key);
                var obj = new RiakObject(id, data, RiakConstants.ContentTypes.ApplicationOctetStream, null);
                objs.Add(obj);
            }

            IRiakClusterConfiguration clusterConfig = new RiakClusterConfiguration();

            for (ushort port = startingPort; port <= endingPort; port += portInterval)
            {
                IRiakNodeConfiguration nc = new RiakNodeConfiguration();
                nc.PoolSize = poolSize;
                nc.HostAddress = riakHost;
                nc.PbcPort = port;
                nc.Name = String.Format("dev_{0}", port);
                clusterConfig.AddNode(nc);
            }

            var batchObjs = new RiakObject[batchSize];
            var p = new int[] { 1, batchSize };

            foreach (int parallelism in p)
            {
                var parallelOptions = new ParallelOptions();
                parallelOptions.MaxDegreeOfParallelism = parallelism;

                using (var cluster = new RiakCluster(clusterConfig))
                {
                    var client = cluster.CreateClient();

                    var sw = new Stopwatch();
                    sw.Start();

                    for (int i = 0; i < totalObjects; i += batchSize)
                    {
                        objs.CopyTo(i, batchObjs, 0, batchSize);
                        Parallel.ForEach(batchObjs, parallelOptions, (obj) =>
                        {
                            try
                            {
                                client.Put(obj);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[ERROR] put exception: {0}", e.ToString());
                            }
                        });
                    }

                    sw.Stop();
                    Debug.WriteLine("parallelism: {0} - put {1} objects in {2}", parallelism, totalObjects, sw.Elapsed);

                    sw.Reset();

                    sw.Start();
                    for (int i = 0; i < totalObjects; i += batchSize)
                    {
                        objs.CopyTo(i, batchObjs, 0, batchSize);
                        Parallel.ForEach(batchObjs, parallelOptions, (obj) =>
                        {
                            try
                            {
                                var id = new RiakObjectId(obj.Bucket, obj.Key);
                                client.Get(id);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("[ERROR] put exception: {0}", e.ToString());
                            }
                        });
                    }

                    sw.Stop();
                    Debug.WriteLine("parallelism: {0} - fetched {1} objects in {2}", parallelism, totalObjects, sw.Elapsed);
                }
            }
        }
    }
}