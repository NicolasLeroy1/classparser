using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class RoslynClassParserForm : Form
{
    private TextBox output;
    private Button selectDirectory;

    public RoslynClassParserForm()
    {
        output = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical
        };

        selectDirectory = new Button
        {
            Text = "Select Directory",
            Dock = DockStyle.Top
        };

        selectDirectory.Click += SelectDirectory_Click;

        this.Controls.Add(output);
        this.Controls.Add(selectDirectory);
    }

    private void SelectDirectory_Click(object sender, EventArgs e)
    {
        using (var fbd = new FolderBrowserDialog())
        {
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                ParseFolder(fbd.SelectedPath);
            }
        }
    }

    public void ParseFolder(string path)
    {
        var csFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
        foreach (var file in csFiles)
        {
            ParseFile(file);
        }
    }

    private void ParseFile(string filePath)
    {
        var code = File.ReadAllText(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (CompilationUnitSyntax)tree.GetRoot();

        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        foreach (var classDecl in classes)
        {
            AppendLine("Class: " + classDecl.Identifier.ValueText);

            var properties = classDecl.DescendantNodes().OfType<PropertyDeclarationSyntax>();
            foreach (var property in properties)
            {
                var type = property.Type.ToString();
                var name = property.Identifier.ValueText;

                AppendLine($"\tProperty: {name}, Type: {type}");
            }

            var methods = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                var returnType = method.ReturnType.ToString();
                var name = method.Identifier.ValueText;

                AppendLine($"\tMethod: {name}, Return Type: {returnType}");
            }
        }
    }

    private void AppendLine(string text)
    {
        output.AppendText(text + Environment.NewLine);
    }
}
