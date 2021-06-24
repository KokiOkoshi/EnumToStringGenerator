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

                // MEMO: 紐づけ用配列
                builder.AppendLine($"\t\tstatic readonly string[] _strings = new string[]");
                builder.AppendLine($"\t\t{{");
                foreach (var enamMemberName in enumMemberNames)
                {
                    builder.AppendLine($"\t\t\t{enamMemberName},");
                }
                builder.AppendLine($"\t\t}};");
                builder.AppendLine();

                // MEMO: 変換関数
                builder.AppendLine($"\t\tpublic static string ToConstString(this {enumName} self) => _strings[(int)self];");
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