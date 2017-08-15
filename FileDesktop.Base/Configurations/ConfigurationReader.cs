using Hake.Extension.ValueRecord;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Base.Configurations
{
    public static class ConfigurationReader
    {
        private static ScanFilterOptions ReadFilterOptions(SetRecord set)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            string pattern;
            Regex tempRegex;
            RecordBase recordbase;
            ListRecord directories = null;
            ListRecord files = null;
            ListRecord common = null;
            ListRecord paths = null;
            if (set.TryGetValue("directory", out recordbase))
                directories = recordbase as ListRecord;
            if (set.TryGetValue("file", out recordbase))
                files = recordbase as ListRecord;
            if (set.TryGetValue("common", out recordbase))
                common = recordbase as ListRecord;
            if (set.TryGetValue("path", out recordbase))
                paths = recordbase as ListRecord;
            List<Regex> resultDirectories = new List<Regex>();
            List<Regex> resultFiles = new List<Regex>();
            List<Regex> resultCommon = new List<Regex>();
            List<Regex> resultPaths = new List<Regex>();
            if (directories != null)
            {
                foreach (RecordBase rec in directories)
                {
                    if (rec is ScalerRecord scaler && scaler.ScalerType == ScalerType.String)
                    {
                        pattern = scaler.ReadAs<string>();
                        try
                        {
                            tempRegex = new Regex(pattern);
                            resultDirectories.Add(tempRegex);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (files != null)
            {
                foreach (RecordBase rec in files)
                {
                    if (rec is ScalerRecord scaler && scaler.ScalerType == ScalerType.String)
                    {
                        pattern = scaler.ReadAs<string>();
                        try
                        {
                            tempRegex = new Regex(pattern);
                            resultFiles.Add(tempRegex);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (common != null)
            {
                foreach (RecordBase rec in common)
                {
                    if (rec is ScalerRecord scaler && scaler.ScalerType == ScalerType.String)
                    {
                        pattern = scaler.ReadAs<string>();
                        try
                        {
                            tempRegex = new Regex(pattern);
                            resultCommon.Add(tempRegex);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (paths != null)
            {
                foreach (RecordBase rec in paths)
                {
                    if (rec is ScalerRecord scaler && scaler.ScalerType == ScalerType.String)
                    {
                        pattern = scaler.ReadAs<string>();
                        try
                        {
                            tempRegex = new Regex(pattern);
                            resultPaths.Add(tempRegex);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return new ScanFilterOptions(resultFiles, resultDirectories, resultCommon, resultPaths);
        }
        private static DirectoryScanOptions ReadDirectoryScanOptions(ScanFilterOptions globalExcludes, SetRecord set)
        {
            if (set == null)
                return null;
            if (!set.TryGetValue("path", out RecordBase pathRecord) || !set.TryGetValue("depth", out RecordBase depthRecord))
                return null;
            if (!(pathRecord is ScalerRecord pathScaler) || !(depthRecord is ScalerRecord depthScaler))
                return null;
            if (!pathScaler.TryReadAs(out string path) || !depthScaler.TryReadAs(out int depth))
                return null;

            if (depth <= 0)
                depth = 1;

            ScanFilterOptions includeOptions;
            ScanFilterOptions excludeOptions;

            if (set.TryGetValue("include", out RecordBase includeRecordBase) && includeRecordBase is SetRecord includeRecord)
                includeOptions = ReadFilterOptions(includeRecord);
            else
                includeOptions = ScanFilterOptions.CreateIncludeDefault();

            if (set.TryGetValue("exclude", out RecordBase excludeRecordBase) && excludeRecordBase is SetRecord excludeRecord)
                excludeOptions = ReadFilterOptions(excludeRecord);
            else
                excludeOptions = ScanFilterOptions.CreateExcludeDefault();

            bool ignoreGlobalExclude = false;
            if (set.TryGetValue("ignoreGlobalExclude", out RecordBase ignoreGlobalExcludeBase) &&
                ignoreGlobalExcludeBase is ScalerRecord ignoreGlobalExcludeScaler &&
                ignoreGlobalExcludeScaler.TryReadAs<bool>(out bool ignoreGlobalExcludeValue))
                ignoreGlobalExclude = ignoreGlobalExcludeValue;
            if (!ignoreGlobalExclude)
                excludeOptions.Combine(globalExcludes);
            return new DirectoryScanOptions(path, depth, includeOptions, excludeOptions);
        }
        private static ItemIncludeOptions ReadItemIncludeOptions(SetRecord set)
        {
            List<string> resultDirectories = new List<string>();
            List<string> resultFiles = new List<string>();

            // return empty list if null
            if (set == null)
                return new ItemIncludeOptions(resultDirectories, resultFiles);

            RecordBase record;
            ListRecord directories = null;
            ListRecord files = null;
            if (set.TryGetValue("directories", out record))
                directories = record as ListRecord;
            if (set.TryGetValue("files", out record))
                files = record as ListRecord;

            if (directories != null)
            {
                foreach (RecordBase rec in directories)
                {
                    if (rec is ScalerRecord scaler && scaler.ScalerType == ScalerType.String)
                        resultDirectories.Add(scaler.ReadAs<string>());
                }
            }
            if (files != null)
            {
                foreach (RecordBase rec in files)
                {
                    if (rec is ScalerRecord scaler && scaler.ScalerType == ScalerType.String)
                        resultFiles.Add(scaler.ReadAs<string>());
                }
            }
            return new ItemIncludeOptions(resultDirectories, resultFiles);
        }

        public static IConfiguartion Read(SetRecord set)
        {
            if (set == null)
                throw new ArgumentNullException(nameof(set));

            RecordBase record;
            SetRecord setIncludeItems = null;
            SetRecord setGlobalExcludes = null;
            ListRecord setScanDirectories = null;
            if (set.TryGetValue("scan", out record))
                setScanDirectories = record as ListRecord;
            if (set.TryGetValue("include", out record))
                setIncludeItems = record as SetRecord;
            if (set.TryGetValue("exclude", out record))
                setGlobalExcludes = record as SetRecord;

            ItemIncludeOptions resultInclude = ReadItemIncludeOptions(setIncludeItems);
            ScanFilterOptions resultExclude = null;
            if (setGlobalExcludes == null)
                resultExclude = ScanFilterOptions.CreateExcludeDefault();
            else
                resultExclude = ReadFilterOptions(setGlobalExcludes);

            List<DirectoryScanOptions> resultScanDirectories = new List<DirectoryScanOptions>();
            if (setScanDirectories != null)
            {
                foreach (RecordBase scanOptions in setScanDirectories)
                {
                    if (scanOptions is SetRecord setScanOptions)
                        resultScanDirectories.Add(ReadDirectoryScanOptions(resultExclude, setScanOptions));
                }
            }

            return new Configuration(resultExclude, resultInclude, resultScanDirectories);
        }
    }
}
