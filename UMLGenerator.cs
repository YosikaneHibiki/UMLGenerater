using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//GitHubAction でUMLを生成するクラスなので触らないでください。

public class UMLGenerator
{
    // 手動設定する場合
    [MenuItem("Tools/UMLGenerate")]
    public static void Generate()
    {
        string path;
        string outputFile;
        path = "Assets/Project/Scripts"; // 解析対象のディレクトリ
        outputFile = "UML/Result/UML.md"; // 出力ファイル
        //ファイルのパスを取得
        path = Path.GetFullPath(path);
        //ソースコードファイルの取得
        var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
        var syntaxTrees = files
            .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file)))
            .ToList();
        //除外するクラス
        HashSet<string> excludedTypes = new HashSet<string> { "Transform", "GameObject" };
        //記録したUMLクラス図
        List<string> resultClass = new List<string>();
        foreach (var syntaxTree in syntaxTrees)
        {
            //全てのルートを取得
            SyntaxNode root = syntaxTree.GetRoot();
            //クラスのルートを取得
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            //interfaceのルートを取得
            var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

            foreach (var classDeclaration in classes)
            {
                string className = classDeclaration.Identifier.Text;
                //クラスのメソッドを取得
                var mehods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
                //変数を取得(intやstringなどの依存を除外)
                var variables = classDeclaration.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Where(f =>
                        !(f.Declaration.Type is PredefinedTypeSyntax) && // 組み込み型の除外
                        !(f.Declaration.Type is NullableTypeSyntax) &&     // Nullable型 (CustomType?) の除外
                        !(f.Declaration.Type.ToString() is string typeName && excludedTypes.Contains(typeName)))
                    .ToList();

                //プロパティの取得
                var properties = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>();

                //依存の継承を記録
                if (classDeclaration.BaseList != null)
                {
                    foreach (var baseType in classDeclaration.BaseList.Types)
                    {
                        resultClass.Add($"{baseType.Type} <|-- {className}");
                    }
                }
                //変数の依存性を記録
                foreach (var variable in variables)
                {
                    if (variable.Declaration.Type is ArrayTypeSyntax)
                    {
                        foreach (var array in variable.DescendantNodes().OfType<ArrayTypeSyntax>())
                        {
                            resultClass.Add($"{className} --> {array.ElementType}");
                        }
                    }
                    else if (variable.Declaration.Type is GenericNameSyntax)
                    {
                        foreach (var generic in variable.DescendantNodes().OfType<GenericNameSyntax>())
                        {
                            resultClass.Add($"{className} --> {generic.TypeArgumentList.Arguments}");
                            Debug.Log(generic.TypeArgumentList.Arguments.ToString());
                        }
                    }
                    else
                    {
                        string typeName = variable.Declaration.Type.ToString();
                        resultClass.Add($"{className} --> {typeName}");
                    }
                }
                //プロパティの依存性を記録
                foreach (var prop in properties)
                {
                    string typeName = prop.Type.ToString();
                    resultClass.Add($"{className} --> {typeName}");
                }
            }

        }

        // 出力ディレクトリが存在しない場合は作成
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);

        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            writer.WriteLine("```mermaid");
            writer.WriteLine("classDiagram");
            foreach (var relation in resultClass.Distinct())
            {
                writer.WriteLine($"  {relation}");
            }
            writer.WriteLine("```");
        }
    }

}

//