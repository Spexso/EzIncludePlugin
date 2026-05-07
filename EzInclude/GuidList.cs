using System;

namespace EzInclude
{
    internal static class GuidList
    {
        public const string PackageGuidString = EzIncludePackage.PackageGuidString;
        public const string CmdSetGuidString  = "c442b7f7-ead6-4088-b821-e6501615f9bd";

        public static readonly Guid PackageGuid = new Guid(PackageGuidString);
        public static readonly Guid CmdSetGuid  = new Guid(CmdSetGuidString);
    }

    internal static class PkgCmdIDList
    {
        public const uint CopyIncludeCmd    = 0x0100;
        public const uint CopyIncludeCmdTab = 0x0101;
    }
}
