using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//GitHubAction ��UML�𐶐�����N���X�Ȃ̂ŐG��Ȃ��ł��������B

public class UMLGenerator
{
    // �蓮�ݒ肷��ꍇ
    [MenuItem("Tools/UMLGenerate")]
    public static void Generate()
    {
        string path;
        string outputFile;
        path = "Assets/Project/Scripts"; // ��͑Ώۂ̃f�B���N�g��
        outputFile = "UML/Result/UML.md"; // �o�̓t�@�C��
        //�t�@�C���̃p�X���擾
        path = Path.GetFullPath(path);
        //�\�[�X�R�[�h�t�@�C���̎擾
        var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
        var syntaxTrees = files
            .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file)))
            .ToList();
        //���O����N���X
        HashSet<string> excludedTypes = new HashSet<string> { "Transform", "GameObject" };
        //�L�^����UML�N���X�}
        List<string> resultClass = new List<string>();
        foreach (var syntaxTree in syntaxTrees)
        {
            //�S�Ẵ��[�g���擾
            SyntaxNode root = syntaxTree.GetRoot();
            //�N���X�̃��[�g���擾
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            //interface�̃��[�g���擾
            var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

            foreach (var classDeclaration in classes)
            {
                string className = classDeclaration.Identifier.Text;
                //�N���X�̃��\�b�h���擾
                var mehods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
                //�ϐ����擾(int��string�Ȃǂ̈ˑ������O)
                var variables = classDeclaration.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Where(f =>
                        !(f.Declaration.Type is PredefinedTypeSyntax) && // �g�ݍ��݌^�̏��O
                        !(f.Declaration.Type is NullableTypeSyntax) &&     // Nullable�^ (CustomType?) �̏��O
                        !(f.Declaration.Type.ToString() is string typeName && excludedTypes.Contains(typeName)))
                    .ToList();

                //�v���p�e�B�̎擾
                var properties = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>();

                //�ˑ��̌p�����L�^
                if (classDeclaration.BaseList != null)
                {
                    foreach (var baseType in classDeclaration.BaseList.Types)
                    {
                        resultClass.Add($"{baseType.Type} <|-- {className}");
                    }
                }
                //�ϐ��̈ˑ������L�^
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
                //�v���p�e�B�̈ˑ������L�^
                foreach (var prop in properties)
                {
                    string typeName = prop.Type.ToString();
                    resultClass.Add($"{className} --> {typeName}");
                }
            }

        }

        // �o�̓f�B���N�g�������݂��Ȃ��ꍇ�͍쐬
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