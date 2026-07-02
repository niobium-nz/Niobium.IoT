namespace Cod.IoT
{
    public abstract class Constants
    {
        public const string BooleanStringTrue = "TRUE";
        public const string BooleanStringFalse = "FALSE";

        public const int ConfigurationProviderID = 1;
        public const int CommandServiceID = 2;
        public const int TaskServiceID = 3;
        public static string AppSettingFile { get; set; }
        public const int TaskActionInterval = 1000;

        public static string ExtensionFolder { get; set; }
        public const string ExtensionManifestFileName = "manifest.ini";
        public const string ExtensionManifestSignatureFileName = "manifest.sig";
        public const string ExtensionClassSuffix = "Extension";
    }
}
