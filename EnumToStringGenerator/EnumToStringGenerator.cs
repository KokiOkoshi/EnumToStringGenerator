using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextTemplating.VSHost;

namespace EnumToStringGenerator
{
    [Guid("81C41393-B4FD-46E5-8691-C704CF5DEF51")]
    public sealed class EnumToStringGenerator : BaseCodeGeneratorWithSite
    {
        public const string Name = nameof(EnumToStringGenerator);
        public const string Description = "enumを文字列化するクラスファイルを出力します。";

        public override string GetDefaultExtension() => ".str.cs";

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            var builder = new StringBuilder();

            builder.Append("Test");

            return Encoding.UTF8.GetBytes(builder.ToString());
        }
    }
}
