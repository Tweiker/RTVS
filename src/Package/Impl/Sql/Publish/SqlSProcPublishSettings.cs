﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Newtonsoft.Json;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Represents persistent settings for the SQL stored procedure publishing dialog.
    /// Serialized into SqlSProcSettings.json in the R project.
    /// </summary>
    [Serializable]
    internal class SqlSProcPublishSettings {
        /// <summary>
        /// List of entries
        /// </summary>
        public IList<SProcInfo> SProcInfoEntries { get; set; }

        /// <summary>
        /// Target SQL table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Target database project name
        /// </summary>
        public string TargetProject { get; set; }

        /// <summary>
        /// If true then generate stored procedures. If false, only
        /// table with variables is created.
        /// </summary>
        public bool GenerateStoredProcedures { get; set; }

        public SqlSProcPublishSettings() {
            SProcInfoEntries = new List<SProcInfo>();
            GenerateStoredProcedures = true;
        }

        /// <summary>
        /// Loads settings from JSON file
        /// </summary>
        public static SqlSProcPublishSettings LoadSettings(ICoreShell coreShell, IProjectSystemServices pss, IFileSystem fs, string folder) {
            SqlSProcPublishSettings settings = null;
            Exception exception = null;
            var settingsFile = GetSettingsFilePath(pss, folder);
            try {
                if (fs.FileExists(settingsFile)) {
                    using (var sr = new StreamReader(settingsFile)) {
                        var json = sr.ReadToEnd();
                        settings = JsonConvert.DeserializeObject<SqlSProcPublishSettings>(json);
                    }
                }
            } catch (IOException ioex) { exception = ioex; } catch (UnauthorizedAccessException uaex) { exception = uaex; } catch (JsonException jex) { exception = jex; }

            if (exception != null) {
                coreShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableToReadSqlPublishSettings, settingsFile, exception.Message));
            }

            settings = settings ?? new SqlSProcPublishSettings();
            settings.Initialize(pss, fs, folder);
            return settings;
        }

        /// <summary>
        /// Saves settings to JSON file
        /// </summary>
        public void Save(IProjectSystemServices pss, string folder) {
            try {
                var settingsFile = GetSettingsFilePath(pss, folder);
                using (var sw = new StreamWriter(settingsFile)) {
                    var json = JsonConvert.SerializeObject(this);
                    sw.Write(json);
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { } catch (JsonException) { }
        }

        private static string GetSettingsFilePath(IProjectSystemServices pss, string folder) {
            return Path.Combine(folder, "SqlSProcSettings.json");
        }

        /// <summary>
        /// Combines data from the saved settings and from the actual project and solition
        /// </summary>
        private void Initialize(IProjectSystemServices pss, IFileSystem fs, string folder) {
            // Fetch all R files in the folder
            folder = folder.EndsWithOrdinal("\\") ? folder : folder + "\\";
            var entries = fs.GetFileSystemEntries(folder, "*.r", SearchOption.TopDirectoryOnly);
            var combinedList = new List<SProcInfo>();
            foreach (var entry in entries) {
                string fileName = Path.GetFileName(entry);
                // Skip setting files
                if (!ProjectSettings.IsProjectSettingFile(fileName)) {
                    var spInfo = new SProcInfo() {
                        FileName = fileName,
                        FilePath = PathHelper.MakeRelative(folder, entry),
                        VariableName = GetVariableName(fileName),
                        SProcName = GetSProcName(fileName)
                    };
                    combinedList.Add(spInfo);
                }
            }
            SProcInfoEntries = combinedList;
            TableName = TableName ?? "RCodeTable";
        }

        private string GetVariableName(string fileName) {
            var name = SProcInfoEntries.FirstOrDefault(x => x.FileName.EqualsIgnoreCase(fileName))?.VariableName;
            if(name != null && name.StartsWithOrdinal("@")) {
                return name;
            }
            return "@" + (name ?? Path.GetFileNameWithoutExtension(fileName));
        }

        private string GetSProcName(string fileName) {
            var name = SProcInfoEntries.FirstOrDefault(x => x.FileName.EqualsIgnoreCase(fileName))?.SProcName;
            return name ?? Path.GetFileNameWithoutExtension(fileName);
        }

        private IReadOnlyCollection<string> GetDatabaseProjectsInSolution(IProjectSystemServices pss) {
            var solution = pss.GetSolution();
            var projects = new List<string>();
            foreach (EnvDTE.Project project in solution.Projects) {
                foreach (var prop in project.Properties) {
                }
                projects.Add(project.Name);
            }
            return projects;
        }
    }
}