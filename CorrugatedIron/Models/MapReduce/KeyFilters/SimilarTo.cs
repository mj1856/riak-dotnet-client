// Copyright (c) 2011 - OJ Reeves & Jeremiah Peschka
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
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CorrugatedIron.Models.MapReduce.KeyFilters
{
    /// <summary>
    /// Tests that input is within the Levenshtein distance of the first argument given by the second argument.
    /// </summary>
    public class SimilarTo<T> : IRiakKeyFilterToken
    {
        private readonly Tuple<string, T, int> _kfDefintion;

        public string FunctionName
        {
            get { return _kfDefintion.Item1; }
        }

        public T Argument
        {
            get { return _kfDefintion.Item2; }
        }

        public int Distance
        {
            get { return _kfDefintion.Item3; }
        }

        public SimilarTo(T arg, int distance)
        {
            _kfDefintion = Tuple.Create("similar_to", arg, distance);
        }

        public override string ToString()
        {
            return ToJsonString();
        }

        public string ToJsonString()
        {
            var sb = new StringBuilder();

            using(var sw = new StringWriter(sb))
            using(JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.WriteStartArray();

                jw.WriteValue(FunctionName);
                jw.WriteValue(Argument);
                jw.WriteValue(Distance);

                jw.WriteEndArray();
            }

            return sb.ToString();
        }
    }
}