using Andoromeda.Framework.GitHub;
using Andoromeda.Kyubey.Dex.Lib;
using Andoromeda.Kyubey.Dex.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Andoromeda.Kyubey.Dex.Repository.TokenRespository;

namespace Andoromeda.Kyubey.Dex.Repository
{
    public class TokenRespository : IRepository<TokenManifest>
    {
        private string tokenFolderAbsolutePath;
        private const string manifestFileName = "manifest.json";
        private const string iconFileName = "icon.png";

        private string _lang;

        public TokenRespository(string path, string lang)
        {
            _lang = lang;
            tokenFolderAbsolutePath = path;
        }

        public IEnumerable<TokenManifest> EnumerateAll()
        {
            var tokenFolderDirectories = Directory.GetDirectories(tokenFolderAbsolutePath);
            var result = new List<TokenManifest>();
            foreach (var t in tokenFolderDirectories)
            {
                var item = GetSingle(t);
                if (item != null)
                    result.Add(item);
            }
            return result;
        }

        public TokenManifest GetSingle(object id)
        {
            var filePath = Path.Combine(tokenFolderAbsolutePath, id.ToString(), manifestFileName);
            if (File.Exists(filePath))
            {
                var fileContent = System.IO.File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<TokenManifest>(fileContent);
            }
            return null;
        }

        public string GetTokenIconPath(string tokenId)
        {
            var absolutePath = Path.Combine(tokenFolderAbsolutePath, tokenId, iconFileName);
            if (File.Exists(absolutePath))
            {
                return absolutePath;
            }
            return null;
        }

        public string GetPriceJsText(string tokenId)
        {
            var filePath = Path.Combine(tokenFolderAbsolutePath, tokenId, "contract_exchange", "price.js");
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
        }

        public string GetTokenDescription(string tokenId, string cultureStr)
        {
            var folderPath = Path.Combine(tokenFolderAbsolutePath, tokenId, "incubator");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            var availableFilePaths = GlobalizationFileFinder.GetCultureFiles(folderPath, cultureStr, ".txt").Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("description."));

            var availablePath = availableFilePaths.FirstOrDefault();
            if (availablePath != null)
                return File.ReadAllText(availablePath);
            return null;
        }

        public class TokenRepositoryFactory
        {
            private IConfiguration _config;
            private IHostingEnvironment _hostingEnv;

            public TokenRepositoryFactory(IConfiguration config, IHostingEnvironment hostingEnv)
            {
                _config = config;
                _hostingEnv = hostingEnv;
            }

            public async Task<TokenRespository> CreateAsync(string lang)
            {
                var path = Path.Combine(_hostingEnv.ContentRootPath, _config["RepositoryStore"], "token-list");
                if (!Directory.Exists(path))
                {
                    await GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
    "kyubey-network", "token-list", "master",
    Path.Combine(_config["RepositoryStore"], "token-list"));
                }
                return new TokenRespository(path, lang);
            }

            public TokenRespository Create(string lang)
            {
                return CreateAsync(lang).Result;
            }
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TokenRepositoryExtensions
    {
        public static IServiceCollection AddTokenRepositoryactory(this IServiceCollection self)
        {
            return self.AddSingleton<TokenRepositoryFactory>();
        }
    }
}