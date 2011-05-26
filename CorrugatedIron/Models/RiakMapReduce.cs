// Copyright (c) 2010 - OJ Reeves & Jeremiah Peschka
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

using System;
using System.Collections.Generic;
using System.Text;
using CorrugatedIron.Extensions;
using CorrugatedIron.KeyFilters;
using CorrugatedIron.Messages;
using CorrugatedIron.Util;

namespace CorrugatedIron.Models
{
    public class RiakMapReduce
    {
        public List<IRiakKeyFilterToken> Filters { get; set; }
        public Dictionary<string, RiakMapReducePhase> MapReducePhases { get; set; }

        public string Bucket { get; set; }
        public string Request { get; set; }
        // TODO push this out to the client for a given request.
        // This could be exposed via the client interface as part of a configuration object
        public string ContentType { get; set; }

        public RiakMapReduce()
        {
            MapReducePhases = new Dictionary<string, RiakMapReducePhase>();
        }

        // TODO implement inputs as a single bucket 
        // TODO implement inputs as a list of <Bucket, Key> 
        // TODO implement inputs as a list of <Bucket, Key, KeyData>
        // TODO implement inputs as a RiakSearch query

        // TODO create and implement Bucket class
        public RiakMapReduce(string request, string contentType = Constants.ContentTypes.ApplicationJson) : this()
        {
            Request = request;
            ContentType = contentType;
        }

        public RiakMapReduce(string bucket, string request = "", string contentType = Constants.ContentTypes.ApplicationJson) : this()
        {
            Bucket = bucket;
            ContentType = contentType;
            Request = request;
        }

        // TODO implement tests for transforming MR jobs into Json strings
        public RiakMapReduce Filter(IRiakKeyFilterToken filter)
        {
            Filters.Add(filter);

            return this;
        }

        // TODO add support for passing arguments
        public RiakMapReduce Map(string module, string function, object[] args = null)
        {
            throw new NotImplementedException();
        }

        public RiakMapReduce Map(bool keep, string language, string source = "", string name = "")
        {
            AddMapReducePhase(keep, language, source, name, Constants.MapReducePhaseType.Map);

            return this;
        }

        public RiakMapReduce Reduce(bool keep, string language, string source = "", string name = "")
        {
            AddMapReducePhase(keep, language, source, name, Constants.MapReducePhaseType.Reduce);

            return this;
        }

        public RiakMapReduce Link()
        {
            throw new NotImplementedException();
        }

        public RpbMapRedReq ToMessage()
        {
            if (string.IsNullOrEmpty(Request))
            {
                var sb = new StringBuilder();
                MapReducePhases.ForEach(mr => sb.Append(mr.Value.ToString()));
                Request = sb.ToString();
            }

            var message = new RpbMapRedReq
                              {
                                  Request = Request.ToRiakString(),
                                  ContentType = ContentType.ToRiakString()
                              };

            return message;
        }

        private void AddMapReducePhase(bool keep, string language, string source, string name, string mapReducePhaseType)
        {
            var phase = new RiakMapReducePhase(mapReducePhaseType, language, keep) {Source = source, Name = name};
            MapReducePhases.Add(phase.MapReducePhaseType, phase);
        }
    }
}