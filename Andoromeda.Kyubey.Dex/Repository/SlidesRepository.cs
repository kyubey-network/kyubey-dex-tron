using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Andoromeda.Framework.GitHub;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Dex.Repository;
using Microsoft.Extensions.Configuration;

namespace Andoromeda.Kyubey.Dex.Repository
{
    public class SlidesRepository : IRepository<Slides>
    {
        private string _path;
        private string _lang;
        private List<string> _dict;
        public SlidesRepository(string path, string lang, List<string> dict)
        {
            _path = path;
            _lang = lang;
            _dict = dict;
        }

        private IEnumerable<string> EnumerateSlidesFiles()
        {
            foreach (var el in _dict)
            {
                yield return el;
            }
        }

        public IEnumerable<Slides> EnumerateAll()
        {
            foreach (var x in EnumerateSlidesFiles())
            {
                yield return GetSingle(x);
            }
        }

        public Slides GetSingle(object id)
        {
            var backGroundPath = Path.Combine(id.ToString(), "bg.png");
            var foreGroundPath = Path.Combine(id.ToString(), $"front.{_lang}.png");
            return new Slides
            {
                Background = backGroundPath,
                Foreground = foreGroundPath
            };
        }
    }

    public class SlidesRepositoryFactory : IRepositoryFactory<Slides>
    {
        private IConfiguration _config;

        public SlidesRepositoryFactory(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IRepository<Slides>> CreateAsync(string lang)
        {
            var path = Path.Combine(_config["RepositoryStore"], "dex-slides");
            if (!Directory.Exists(path))
            {
                await GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
                    "kyubey-network", "dex-slides", "master",
                    Path.Combine(_config["RepositoryStore"], "dex-slides"));
            }
            return new SlidesRepository(path, lang, InitSlidesList());
        }

        public IRepository<Slides> Create(string lang)
        {
            return CreateAsync(lang).Result;
        }

        private List<string> InitSlidesList()
        {
            var path = Path.Combine(Path.Combine(_config["RepositoryStore"], "dex-slides"), "slides.json");
            var json = File.ReadAllText(path);
            var JsonObject = JsonConvert.DeserializeObject<List<string>>(json);
            return JsonObject;
        }
    }
}


namespace Microsoft.Extensions.DependencyInjection
{
    public static class SlidesRepositoryExtensions
    {
        public static IServiceCollection AddSlidesRepositoryFactory(this IServiceCollection self)
        {
            return self.AddSingleton<SlidesRepositoryFactory>();
        }
    }
}
