using Andoromeda.Kyubey.Dex.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Dex.Lib
{
    public static class GlobalizationFileFinder
    {
        public static IEnumerable<string> GetCultureFiles(string folderPath, string culture, string fileType = ".md", string fileIdFilter = null)
        {
            var files = Directory.EnumerateFiles(folderPath);
            return GetCultureFiles(files, culture, fileType, fileIdFilter);
        }

        public static IEnumerable<string> GetCultureFiles(IEnumerable<string> filesPath, string culture, string fileType = ".md", string fileIdFilter = null)
        {
            var fileIds = GetFileIds(filesPath).Where(x => fileIdFilter == null || x == fileIdFilter);

            var result = new List<string>();

            foreach (var fileId in fileIds)
            {
                switch (GetFileNameSuffixByCulture(culture))
                {
                    case FileCultureFileSuffix.ZHTW:
                        {
                            var currentFile = filesPath.FirstOrDefault(x => x.EndsWith(fileId + FileCultureFileSuffix.ZHTW + fileType));
                            if (!string.IsNullOrWhiteSpace(currentFile))
                            {
                                result.Add(currentFile);
                            }
                            else
                            {
                                goto case FileCultureFileSuffix.ZHCN;
                            }
                            break;
                        }
                    case FileCultureFileSuffix.ZHCN:
                        {
                            var currentFile = filesPath.FirstOrDefault(x => x.EndsWith(fileId + FileCultureFileSuffix.ZHCN + fileType));
                            if (!string.IsNullOrWhiteSpace(currentFile))
                            {
                                result.Add(currentFile);
                            }
                            else
                            {
                                goto case FileCultureFileSuffix.EN;
                            }
                            break;
                        }
                    case FileCultureFileSuffix.JP:
                        {
                            var currentFile = filesPath.FirstOrDefault(x => x.EndsWith(fileId + FileCultureFileSuffix.JP + fileType));
                            if (!string.IsNullOrWhiteSpace(currentFile))
                            {
                                result.Add(currentFile);
                            }
                            else
                            {
                                goto case FileCultureFileSuffix.EN;
                            }
                            break;
                        }
                    case FileCultureFileSuffix.EN:
                        {
                            var currentFile = filesPath.FirstOrDefault(x => x.EndsWith(fileId + FileCultureFileSuffix.EN + fileType));
                            if (!string.IsNullOrWhiteSpace(currentFile))
                            {
                                result.Add(currentFile);
                            }
                            break;
                        }
                }
            }

            return result;
        }

        public static class FileCultureFileSuffix
        {
            public const string ZHCN = ".zh";
            public const string ZHTW = ".zh-Hant";
            public const string EN = ".en";
            public const string JP = ".ja";
        }

        private static string GetFileNameSuffixByCulture(string cultureStr)
        {
            if (new string[] { "en" }.Contains(cultureStr))
                return FileCultureFileSuffix.EN;
            if (new string[] { "zh" }.Contains(cultureStr))
                return FileCultureFileSuffix.ZHCN;
            if (new string[] { "zh-Hant", "zh_tw" }.Contains(cultureStr))
                return FileCultureFileSuffix.ZHTW;
            if (new string[] { "ja" }.Contains(cultureStr))
                return FileCultureFileSuffix.JP;
            return "";
        }

        private static IEnumerable<string> GetFileIds(IEnumerable<string> filePaths)
        {
            return filePaths.Select(x => GetFileId(x))
                       .Where(x => !string.IsNullOrWhiteSpace(x))
                       .Distinct();
        }

        public static string GetFileId(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath)
                         .TrimEnd(FileCultureFileSuffix.EN)
                         .TrimEnd(FileCultureFileSuffix.JP)
                         .TrimEnd(FileCultureFileSuffix.ZHCN)
                         .TrimEnd(FileCultureFileSuffix.ZHTW);
        }

        public static string GetFilePathById(string filePath, string id, string cultureStr)
        {
            var files = Directory.EnumerateFiles(filePath);

            return GetCultureFiles(files, cultureStr, fileIdFilter: id).FirstOrDefault();
        }
    }
}
