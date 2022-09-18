using System;
using System.Collections.Generic;
using System.Linq;
using Wox.Plugin;

namespace ConsoleTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var main = new Wox.Plugin.DeepL.Main();

            var query = QueryBuilder.Build("dl en ", new Dictionary<string, PluginPair>());
            
            var results = main.Query(query);
            foreach (var result in results)
            {
                Console.WriteLine(result.Title);
            }

            Console.ReadKey();
        }
    }

    public static class QueryBuilder
    {
        public static Query Build(string text, Dictionary<string, PluginPair> nonGlobalPlugins)
        {
            var terms = text.Split(new[] { Query.TermSeperater }, StringSplitOptions.RemoveEmptyEntries);
            if (terms.Length == 0)
            { 
                return null;
            }

            var rawQuery = string.Join(Query.TermSeperater, terms);
            string actionKeyword, search;
            string possibleActionKeyword = terms[0];
            List<string> actionParameters;
            if (nonGlobalPlugins.TryGetValue(possibleActionKeyword, out var pluginPair) && !pluginPair.Metadata.Disabled)
            { 
                actionKeyword = possibleActionKeyword;
                actionParameters = terms.Skip(1).ToList();
                search = actionParameters.Count > 0 ? rawQuery.Substring(actionKeyword.Length + 1) : string.Empty;
            }
            else
            { 
                actionKeyword = string.Empty;
                actionParameters = terms.ToList();
                search = rawQuery;
            }

            var query = new Query();

            var type = query.GetType();
            type.GetProperty("Terms").SetValue(query, terms, null);
            type.GetProperty("RawQuery").SetValue(query, rawQuery, null);
            type.GetProperty("ActionKeyword").SetValue(query, actionKeyword, null);
            type.GetProperty("Search").SetValue(query, search, null);
            type.GetProperty("ActionName").SetValue(query, actionKeyword, null);
            type.GetProperty("ActionParameters").SetValue(query, actionParameters, null);

            return query;
        }
    }
}
