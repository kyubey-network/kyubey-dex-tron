using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Pomelo.AspNetCore.TimedJob;
using Andoromeda.Framework.GitHub;
using Andoromeda.Framework.Logger;

namespace Andoromeda.Kyubey.Dex.Jobs
{
    public class GitHubJob : Job
    {
        [Invoke(Begin = "2018-11-01 0:00", Interval = 1000 * 60 * 5, SkipWhileExecuting = true)]
        public void SyncNewsRepository(IConfiguration config, ILogger logger)
        {
            try
            {
                GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
                "kyubey-network", "dex-news", "master",
                Path.Combine(config["RepositoryStore"], "dex-news")).Wait();
            }
            catch (AggregateException ex)
            {
                ex.Flatten().Handle((x) =>
                {
                    if (x is IOException)
                    {
                        logger.LogError(x.ToString());
                        return true;
                    }
                    return false;
                });
            }
        }

        [Invoke(Begin = "2018-11-01 0:01", Interval = 1000 * 60 * 5, SkipWhileExecuting = true)]
        public void SyncSlidesRepository(IConfiguration config, ILogger logger)
        {
            try
            {
                GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
                "kyubey-network", "dex-slides", "master",
                Path.Combine(config["RepositoryStore"], "dex-slides")).Wait();
            }
            catch (AggregateException ex)
            {
                ex.Flatten().Handle((x) =>
                {
                    if (x is IOException)
                    {
                        logger.LogError(x.ToString());
                        return true;
                    }
                    return false;
                });
            }
        }

        [Invoke(Begin = "2018-11-01 0:02", Interval = 1000 * 60 * 5, SkipWhileExecuting = true)]
        public void SyncTokensRepository(IConfiguration config, ILogger logger)
        {
            try
            {
                GitHubSynchronizer.CreateOrUpdateRepositoryAsync(
                "kyubey-network", "token-list", "master",
                Path.Combine(config["RepositoryStore"], "token-list")).Wait();
            }
            catch (AggregateException ex)
            {
                ex.Flatten().Handle((x) =>
                {
                    if (x is IOException)
                    {
                        logger.LogError(x.ToString());
                        return true;
                    }
                    return false;
                });
            }
        }
    }
}
