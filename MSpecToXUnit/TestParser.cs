using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MSpecToXUnit
{
    class TestParser
    {
        public static TestMetaData ParseClass(ClassDeclarationSyntax syntax)
        {
            var testMetaData = new TestMetaData();

            testMetaData.Syntax = syntax;
            testMetaData.ClassName = syntax.Identifier.Text;
            testMetaData.ClassModifiers = syntax.Modifiers.ToString();
            testMetaData.BaseClassName = ParseBaseClass(syntax);
            testMetaData.ClassAttributes = ParseClassAttributes(syntax);
            testMetaData.Ignore = ParseIgnoreAttribute(syntax);
            testMetaData.Tags = ParseTagsAttributes(syntax);
            testMetaData.Tags.AddRange(ParseSubjectAttributes(syntax).ToList());
            testMetaData.EstablishBody = ParseFieldDeclaration(syntax, "Establish");
            testMetaData.BecauseBody = ParseFieldDeclaration(syntax, "Because");
            testMetaData.CleanupBody = ParseFieldDeclaration(syntax, "Cleanup");
            testMetaData.Asserts = ParseAsserts(syntax, "It");
            testMetaData.Fields = ParseFields(syntax);
            testMetaData.Methods = ParseMethods(syntax);
            testMetaData.Classes = ParseInnerClasses(syntax);

            return testMetaData;
        }

        public static string ParseBaseClass(ClassDeclarationSyntax syntax)
        {
            if (syntax.BaseList == null)
            {
                return null;
            }

            return syntax.BaseList.Types.Count > 1 ? null : syntax.BaseList.Types.Single().Type.ToString();
        }

        public static DelegateMetaData ParseFieldDeclaration(ClassDeclarationSyntax syntax, string name)
        {
            var fields = syntax.Members.OfType<FieldDeclarationSyntax>().Where(x => x.Declaration.Type is IdentifierNameSyntax);
            var field = fields.SingleOrDefault(x => ((IdentifierNameSyntax)x.Declaration.Type).Identifier.Text == name);
            return field != null ? ParseDelegate(field) : new DelegateMetaData() { Body = "", Name = "" };
        }

        public static List<string> ParseClassAttributes(ClassDeclarationSyntax syntax)
        {
            var attributes = syntax.AttributeLists.Select(x => x.Attributes).SelectMany(x => x);
            
            return attributes
                .Where(x => x.Name.ToString() != "Subject" && x.Name.ToString() != "Tags" && x.Name.ToString() != "Ignore")
                .Select(x => x.ToString()).ToList();
        }

        public static List<string> ParseInnerClasses(ClassDeclarationSyntax syntax)
        {
            return syntax.Members.OfType<ClassDeclarationSyntax>().Select(x => x.ToString()).ToList();
        }

        public static IEnumerable<string> ParseSubjectAttributes(ClassDeclarationSyntax syntax)
        {
            var argumentLists = syntax.AttributeLists
                .Select(x => x.Attributes)
                .SelectMany(x => x)
                .Where(x => x.Name.ToString() == "Subject")
                .Select(x => x.ArgumentList);

            foreach (var argumentSyntax in argumentLists.SelectMany(attrList => attrList.Arguments)) 
            {
                if (argumentSyntax.Expression is TypeOfExpressionSyntax)
                {
                    yield return ((TypeOfExpressionSyntax)argumentSyntax.Expression).Type.ToString();
                }

                if (argumentSyntax.Expression is LiteralExpressionSyntax)
                {
                    var value = ((LiteralExpressionSyntax)argumentSyntax.Expression).Token.Value.ToString();
                    foreach(var parts in value.Split(',').Select(x => x.Trim()))
                    {
                        yield return parts;
                    }
                }
            }
        }

        public static List<string> ParseTagsAttributes(ClassDeclarationSyntax syntax)
        {
            return syntax.AttributeLists
                .Select(x => x.Attributes)
                .SelectMany(x => x)
                .Where(x => x.Name.ToString() == "Tags")
                .SelectMany(x=> x.ArgumentList.Arguments)
                .Select(x => ((LiteralExpressionSyntax)x.Expression).Token.Value.ToString())
                .SelectMany(expressionValue => expressionValue.Split(',').Select(x => x.Trim()))
                .ToList();
        }

        public static string ParseIgnoreAttribute(ClassDeclarationSyntax syntax)
        {
            var attributes = syntax.AttributeLists.Select(x => x.Attributes).SelectMany(x=> x).SingleOrDefault(x => x.Name.ToString() == "Ignore");
            
            return attributes == default(AttributeSyntax) ? 
                null : 
                ((LiteralExpressionSyntax)attributes.ArgumentList.Arguments.Single().Expression).Token.Value.ToString();
        }

        public static List<string> ParseMethods(ClassDeclarationSyntax syntax)
        {
            return syntax.Members.OfType<MethodDeclarationSyntax>()
                .Select(method => method.ToString())
                .ToList();
        }

        public static DelegateMetaData ParseDelegate(FieldDeclarationSyntax field)
        {
            var variable = field.Declaration.Variables.Single();
            var lambdaExpression = variable.Initializer.Value as ParenthesizedLambdaExpressionSyntax;

            if (lambdaExpression.Body is BlockSyntax)
            {
                return new DelegateMetaData
                {
                    Name = variable.Identifier.Text,
                    Body = lambdaExpression.Body.ToString()
                };
            }

            return new DelegateMetaData
            {
                Name = variable.Identifier.Text,
                Body = lambdaExpression.Body + ";"
            };
        }

        public static List<DelegateMetaData> ParseAsserts(ClassDeclarationSyntax syntax, string name)
        {
            var fields = syntax.Members.OfType<FieldDeclarationSyntax>().Where(x => x.Declaration.Type is IdentifierNameSyntax);
            return fields
                .Where(x => ((IdentifierNameSyntax)x.Declaration.Type).Identifier.Text == name)
                .Select(ParseDelegate)
                .ToList();
        }

        public static List<string> ParseFields(ClassDeclarationSyntax syntax)
        {
            var fields = syntax.Members.OfType<FieldDeclarationSyntax>()
                .Where(x => x.Declaration.Type is IdentifierNameSyntax)
                .Where(x => ((IdentifierNameSyntax)x.Declaration.Type).Identifier.ToString() != "Establish")
                .Where(x => ((IdentifierNameSyntax)x.Declaration.Type).Identifier.ToString() != "Because")
                .Where(x => ((IdentifierNameSyntax)x.Declaration.Type).Identifier.ToString() != "It")
                .Where(x => ((IdentifierNameSyntax)x.Declaration.Type).Identifier.ToString() != "Cleanup")
                .Select(x => x.ToString());
                
            var fieldsWithPredefinedType = syntax.Members.OfType<FieldDeclarationSyntax>()
                .Where(x => x.Declaration.Type is PredefinedTypeSyntax).Select(x=> x.ToString());

            return fields.Concat(fieldsWithPredefinedType).ToList();
        }
    }
}
