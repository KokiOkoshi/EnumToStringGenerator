using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

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

            // MEMO: ファイルヘッダ部分
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Linq;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine();

            // MEMO: コンテンツ部分
            var synataxTree = CSharpSyntaxTree.ParseText(inputFileContent);
            var root = synataxTree.GetCompilationUnitRoot();
            var compilation = CSharpCompilation
                .Create(null, syntaxTrees: new[] { synataxTree });
            var semanticModel = compilation.GetSemanticModel(synataxTree);
            var enumNodes = root
                .DescendantNodes()
                .OfType<EnumDeclarationSyntax>();

            foreach (var enumNode in enumNodes)
            {
                var nameSpaceName = semanticModel.GetDeclaredSymbol(enumNode).ContainingNamespace;
                var enumName = enumNode.Identifier.ValueText;
                var enumMemberNames = enumNode.Members.OfType<EnumMemberDeclarationSyntax>().Select(x => x.Identifier.ValueText);

                // MEMO: 名前空間とクラス名
                builder.AppendLine($"namespace {nameSpaceName}");
                builder.AppendLine($"{{");
                builder.AppendLine($"\tstatic class {enumName}String");
                builder.AppendLine($"\t{{");

                // MEMO: 文字列定義
                foreach (var enamMemberName in enumMemberNames)
                {
                    builder.AppendLine($"\t\tpublic static readonly string {enamMemberName} = nameof({enumName}.{enamMemberName});");
                }
                builder.AppendLine();

                // MEMO: 変換関数
                builder.AppendLine($"\t\tpublic static string ToConstString(this {enumName} self)");
                builder.AppendLine($"\t\t{{");
                builder.AppendLine($"\t\t\tswitch(self)");
                builder.AppendLine($"\t\t\t{{");
                foreach (var enamMemberName in enumMemberNames)
                {
                    builder.AppendLine($"\t\t\t\tcase {enumName}.{enamMemberName}:");
                    builder.AppendLine($"\t\t\t\t\treturn {enamMemberName};");
                }

                builder.AppendLine($"\t\t\t\tdefault:");
                builder.AppendLine($"#if DEBUG");
                builder.AppendLine($"\t\t\t\t\tthrow new ArgumentOutOfRangeException(nameof(self));");
                builder.AppendLine($"#else");
                builder.AppendLine($"\t\t\t\t\treturn string.Empty;");
                builder.AppendLine($"#endif");
                builder.AppendLine($"\t\t\t}}");
                builder.AppendLine($"\t\t}}");
                builder.AppendLine();

                // MEMO: 名前空間とクラス名
                builder.AppendLine($"\t}}");
                builder.AppendLine($"}}");
                builder.AppendLine();
            }

            return Encoding.UTF8.GetBytes(builder.ToString());
        }
    }
}