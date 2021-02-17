using System;
using System.Collections.Generic;
using System.Linq;

namespace FunkyChat.Server.Services
{
    public interface INameGenerationService
    {
        string Generate();
    }

    public class NameGenerationService : INameGenerationService
    {
        private static readonly Random _random = new Random();
        private readonly List<string> _adjectives, _nouns;

        public NameGenerationService(List<string> adjectives, List<string> nouns)
        {
            _adjectives = adjectives;
            _nouns = nouns;
        }

        public string Generate()
        {
            var adjective = _adjectives[_random.Next(_adjectives.Count)];
            var noun = _nouns[_random.Next(_nouns.Count)];

            return $"{adjective}{noun.First().ToString().ToUpper()}{noun[1..]}";
        }
    }
}
