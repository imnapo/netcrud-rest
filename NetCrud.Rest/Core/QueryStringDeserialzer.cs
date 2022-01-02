using NetCrud.Rest.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCrud.Rest.Core
{
    public static class QueryStringDeserialzer
    {
        static QueryFunction[] functions = new QueryFunction[] { new QueryFunction {
        Name = "equals",
        Args = 2,
        Format = "{0} == {1}"
        }, new QueryFunction {
        Name = "and",
        Args = 2,
        Format = "{0} && {1}"
        }, new QueryFunction {
        Name = "or",
        Args = 2,
        Format = "{0} || {1}"
        }, new QueryFunction {
        Name = "lessThan",
        Args = 2,
        Format = "{0} < {1}"
        } , new QueryFunction {
        Name = "lessOrEqual",
        Args = 2,
        Format = "{0} <= {1}"
        }, new QueryFunction {
        Name = "greaterThan",
        Args = 2,
        Format = "{0} > {1}"
        }, new QueryFunction {
        Name = "greaterOrEqual",
        Args = 2,
        Format = "{0} >= {1}"
        }, new QueryFunction {
        Name = "contains",
        Args = 2,
        Format = "{0}.Contains({1})"
        }, new QueryFunction {
        Name = "startsWith",
        Args = 2,
        Format = "{0}.StartsWith({1})"
        }, new QueryFunction {
        Name = "endsWith",
        Args = 2,
        Format = "{0}.EndsWith({1})"
        }, new QueryFunction {
        Name = "not",
        Args = 1,
        Format = "!{0}"
        }, new QueryFunction {
        Name = "has",
        Args = 1,
        Format = "{0}.Any()"
        }, new QueryFunction {
        Name = "any",
        Args = 2,
        AddParenthesis = false,
        Format = "{0}.Any(x=>x.{1})"
        }
        };

        public static string DeserializeFilter(string queries, bool addParenthesis = true)
        {
            int ptStartIndex = queries.IndexOf('(');
            if (ptStartIndex < 0) return queries.UppercaseFirst();
            if (ptStartIndex >= 1)
            {
                string funct = queries.Substring(0, ptStartIndex);
                //funcStack.Push(funct);

                int ptEndIndex = queries.LastIndexOf(')');

                string argsString = queries.Substring(ptStartIndex + 1, ptEndIndex - ptStartIndex - 1);
                var myFunc = functions.FirstOrDefault(x => x.Name == funct);
                if (myFunc == null) throw new Exception($"{funct} is not valid filter function");
                //if(myFunc.Args == 2)
                //{
                List<int> commaIndex = new List<int>();
                int parantCounter = 0;
                for (int i = 0; i < argsString.Length; i++)
                {
                    if (argsString[i] == '(')
                        parantCounter--;
                    else if (argsString[i] == ')')
                        parantCounter++;
                    else if (argsString[i] == ',' && parantCounter == 0)
                    { commaIndex.Add(i); }

                }
                //string[] args = argsString.Split(',');
                List<string> args = new List<string>();
                int prevIndex = 0;
                for (int i = 0; i < commaIndex.Count; i++)
                {
                    string arg1 = argsString.Substring(prevIndex, commaIndex[i]);
                    arg1 = arg1.StartsWith("'") && arg1.EndsWith("'") ? $"\"{arg1.Substring(1, arg1.Length - 2)}\"" : arg1;
                    args.Add(arg1);
                    prevIndex = commaIndex[i];
                }

                string arg2 = commaIndex.Count > 0 ? argsString.Substring(prevIndex + 1) : argsString;
                arg2 = arg2.StartsWith("'") && arg2.EndsWith("'") ? $"\"{arg2.Substring(1, arg2.Length - 2)}\"" : arg2;

                args.Add(arg2);

                List<string> results = new List<string>();
                foreach (var item in args)
                {
                    results.Add(DeserializeFilter(item, myFunc.AddParenthesis));

                }

                string format = $"{string.Format(myFunc.Format, results.ToArray())}";
                return addParenthesis ? $"({format})" : format;

                //} else
                //{
                //    return queries;
                //}


            }
            else
            {
                throw new Exception("no filter function found.");
            }
        }

        public static string DeserializeSort(string sortQuery)
        {
            string[] orders = sortQuery.Split(",").Select(s =>
            {
                return (s.StartsWith("-") ? $"{s.Substring(1).UppercaseFirst()} DESC" : s.UppercaseFirst());
            }).ToArray();
            if (orders != null && orders.Count() > 0)
                return string.Join(",", orders);
            else return "";
        }
    }

    public class QueryFunction
    {
        public string Name { get; set; }
        public string Format { get; set; }
        public int Args { get; set; }

        public bool AddParenthesis { get; set; } = true;
    }

}
