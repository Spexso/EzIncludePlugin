using System;
using System.ComponentModel.Design;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace EzInclude
{
    internal sealed class CopyIncludeCommand
    {
        private readonly AsyncPackage _package;

        private CopyIncludeCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package;
            commandService.AddCommand(new OleMenuCommand(Execute, new CommandID(GuidList.CmdSetGuid, (int)PkgCmdIDList.CopyIncludeCmd)));
            commandService.AddCommand(new OleMenuCommand(Execute, new CommandID(GuidList.CmdSetGuid, (int)PkgCmdIDList.CopyIncludeCmdTab)));
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            if (await package.GetServiceAsync(typeof(IMenuCommandService)) is not OleMenuCommandService commandService)
                return;

            new CopyIncludeCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            var fullPath = dte?.ActiveDocument?.FullName;
            if (fullPath == null || fullPath.Length == 0)
                return;

            var include = BuildIncludePath(fullPath);
            if (include == null)
                return;

            Clipboard.SetText(include);

            var statusBar = (IVsStatusbar)Package.GetGlobalService(typeof(SVsStatusbar));
            statusBar?.SetText($"Copied: {include}");
        }

        internal static string? BuildIncludePath(string fullPath)
        {
            var parts = fullPath.Replace('\\', '/').Split('/');
            int sourceIdx = -1;
            // Search backwards (skip filename) to find the deepest "Source" dir
            // that still has at least 2 segments after it (module name + file).
            for (int i = parts.Length - 2; i >= 0; i--)
            {
                if (string.Equals(parts[i], "Source", StringComparison.OrdinalIgnoreCase)
                    && parts.Length - i >= 3
                    && !string.Equals(parts[i + 1], "repos", StringComparison.OrdinalIgnoreCase))
                {
                    sourceIdx = i;
                    break;
                }
            }

            if (sourceIdx < 0 || sourceIdx + 2 >= parts.Length)
                return null;

            int start = sourceIdx + 2; // skip Source + module name
            if (start < parts.Length &&
                (string.Equals(parts[start], "Public", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(parts[start], "Private", StringComparison.OrdinalIgnoreCase)))
                start++;

            if (start >= parts.Length)
                return null;

            var relative = string.Join("/", parts, start, parts.Length - start);
            return $"#include \"{relative}\"";
        }
    }
}
