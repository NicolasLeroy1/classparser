using System;
using System.ComponentModel;
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
    private CheckBox checkProperties;
    private CheckBox checkFields;
    private CheckBox checkMethods;
    private CheckBox checkStructs;

    public RoslynClassParserForm()
    {
        // Define the output textbox
        output = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
        };
        checkProperties = new CheckBox
        {
            Text = "Properties",
            Checked = true,
            AutoSize = true
        };
        checkFields = new CheckBox
        {
            Text = "Fields",
            Checked = true,
            AutoSize = true
        };
        checkMethods = new CheckBox
        {
            Text = "Methods",
            Checked = true,
            AutoSize = true
        };
        checkStructs = new CheckBox
        {
            Text = "Structs",
            Checked = true,
            AutoSize = true
        };
        var sidePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(5),
        };
        sidePanel.Controls.AddRange(
            new Control[] { checkProperties, checkFields, checkMethods, checkStructs }
        );

        selectDirectory = new Button { Text = "Select Directory", Dock = DockStyle.Top };
        selectDirectory.Click += SelectDirectory_Click;

        var mainPanel = new Panel { Dock = DockStyle.Fill, Controls = { output, selectDirectory } };

        this.Controls.AddRange(new Control[] { mainPanel, sidePanel });
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

            if (checkFields.Checked)
            {
                var fields = classDecl.DescendantNodes().OfType<FieldDeclarationSyntax>();
                foreach (var field in fields)
                {
                    var type = field.Declaration.Type.ToString();
                    var name = field.Declaration.Variables.First().Identifier.ValueText;
                    AppendLine($"\tField: {name}, Type: {type}");
                }
            }
            if (checkProperties.Checked)
            {
                var properties = classDecl.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                foreach (var property in properties)
                {
                    var type = property.Type.ToString();
                    var name = property.Identifier.ValueText;
                    AppendLine($"\tProperty: {name}, Type: {type}");
                }
            }
            if (checkMethods.Checked)
            {
                var methods = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>();
                foreach (var method in methods)
                {
                    var name = method.Identifier.ValueText;
                    var returnType = method.ReturnType.ToString();
                    AppendLine($"\tMethod: {name}, Return Type: {returnType}");
                }
            }
            if (checkStructs.Checked)
            {
                var structs = classDecl.DescendantNodes().OfType<StructDeclarationSyntax>();
                foreach (var structDecl in structs)
                {
                    var name = structDecl.Identifier.ValueText;
                    AppendLine($"\tStruct: {name}");
                }
            }
        }
    }

    private void InitializeComponent() { }

    private void AppendLine(string text)
    {
        output.AppendText(text + Environment.NewLine);
    }
}
