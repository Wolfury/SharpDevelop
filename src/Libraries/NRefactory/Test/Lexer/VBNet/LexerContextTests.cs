﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Siegfried Pammer" email="siegfriedpammer@gmail.com" />
//     <version>$Revision$</version>
// </file>

using System;
using System.IO;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.NRefactory.Parser.VB;
using NUnit.Framework;
using VBParser = ICSharpCode.NRefactory.Parser.VB;

namespace ICSharpCode.NRefactory.Tests.Lexer.VB
{
	[TestFixture]
	public class LexerContextTests
	{
		[Test]
		public void SimpleGlobal()
		{
			RunTest(
				@"Option Explicit",
				@"enter Global
exit Global
"
			);
		}
		
		[Test]
		public void VariableWithXmlLiteral()
		{
			RunTest(
				@"Class Test
	Public Sub New()
		Dim x = <a />
	End Sub
End Class
",
				@"enter Global
	enter IdentifierExpected
	exit IdentifierExpected
	enter Type
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
			enter Body
				enter IdentifierExpected
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
							enter Xml
							exit Xml
						exit Expression
					exit Expression
				exit Expression
			exit Body
		exit Member
	exit Type
exit Global
"
			);
		}
		
		[Test]
		public void MemberWithXmlLiteral()
		{
			RunTest(
				@"Class Test
	Private xml As XElement = <b />
	
	Public Sub New()
		Dim x = <a />
	End Sub
End Class
",
				@"enter Global
	enter IdentifierExpected
	exit IdentifierExpected
	enter Type
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
			enter Expression
				enter Expression
					enter Expression
						enter Xml
						exit Xml
					exit Expression
				exit Expression
			exit Expression
		exit Member
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
			enter Body
				enter IdentifierExpected
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
							enter Xml
							exit Xml
						exit Expression
					exit Expression
				exit Expression
			exit Body
		exit Member
	exit Type
exit Global
"
			);
		}
		
		[Test]
		public void GlobalAttributeTest()
		{
			RunTest(
				@"<assembly: CLSCompliant(True)>
Class Test
	Public Sub New()
		Dim x = 5
	End Sub
End Class
",
				@"enter Global
	enter Attribute
	exit Attribute
	enter IdentifierExpected
	exit IdentifierExpected
	enter Type
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
			enter Body
				enter IdentifierExpected
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
			exit Body
		exit Member
	exit Type
exit Global
"
			);
		}
		
		[Test]
		public void ClassAttributeTest()
		{
			RunTest(
				@"<Serializable>
Class Test
	Public Sub New()
		Dim x = 5
	End Sub
End Class
",
				@"enter Global
	enter Attribute
	exit Attribute
	enter IdentifierExpected
	exit IdentifierExpected
	enter Type
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
			enter Body
				enter IdentifierExpected
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
			exit Body
		exit Member
	exit Type
exit Global
"
			);
		}
		
		[Test]
		public void MethodAttributeTest()
		{
			RunTest(
				@"Class Test
	<Test>
	Public Sub New()
		Dim x = 5
	End Sub
End Class
",
				@"enter Global
	enter IdentifierExpected
	exit IdentifierExpected
	enter Type
		enter Member
			enter Attribute
			exit Attribute
			enter IdentifierExpected
			exit IdentifierExpected
			enter Body
				enter IdentifierExpected
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
			exit Body
		exit Member
	exit Type
exit Global
"
			);
		}
		
		[Test]
		public void WithBlockTest()
		{
			RunTest(
				@"Class Test
	Public Sub New()
		With x
			
		End With
	End Sub
End Class
",
				@"enter Global
	enter IdentifierExpected
	exit IdentifierExpected
	enter Type
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
			enter Body
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
				enter Body
				exit Body
			exit Body
		exit Member
	exit Type
exit Global
"
			);
		}
		
		[Test]
		public void StatementsTest()
		{
			RunTest(
				@"Class Test
	Public Sub New()
		For i As Integer = 0 To 10
		
		Next
	
		For Each x As Integer In list
		
		Next
		
		Try
		
		Catch e As Exception
		
		End Try
	End Sub
End Class
",
				@"enter Global
	enter IdentifierExpected
	exit IdentifierExpected
	enter Type
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
			enter Body
				enter IdentifierExpected
					enter Expression
					exit Expression
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
				enter Body
				exit Body
				enter IdentifierExpected
					enter Expression
					exit Expression
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
				enter Body
				exit Body
				enter Body
				exit Body
				enter IdentifierExpected
				exit IdentifierExpected
				enter Body
				exit Body
			exit Body
		exit Member
	exit Type
exit Global
"
			);
		}
		
		[Test]
		public void ClassTest()
		{
			RunTest(
				@"Class MainClass ' a comment
	Dim under_score_field As Integer
	Sub SomeMethod()
		simple += 1
		For Each loopVarName In collection
		Next
	End Sub
End Class",
				@"enter Global
	enter IdentifierExpected
	exit IdentifierExpected
	enter Type
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
		exit Member
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
			enter Body
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
				enter IdentifierExpected
					enter Expression
					exit Expression
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
				enter Body
				exit Body
			exit Body
		exit Member
	exit Type
exit Global
");
		}
		
		[Test]
		public void CollectionInitializer()
		{
			RunTest(@"'
' Created by SharpDevelop.
' User: Siegfried
' Date: 22.06.2010
' Time: 21:29
'
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'

Option Infer On

Imports System.Linq
Imports System.Xml.Linq

Module Program
	Sub Main()
		Console.WriteLine(""Hello World!"")
		
		Dim name = ""Test""
		Dim content = { 4, 5, New XAttribute(""a"", 3) }
		
		Dim xml = <<%= name %> <%= content %> />
		
		Console.ReadKey()
	End Sub
End Module",
			        @"enter Global
	enter IdentifierExpected
	exit IdentifierExpected
	enter Type
		enter Member
			enter IdentifierExpected
			exit IdentifierExpected
			enter Body
				enter Expression
					enter Expression
						enter Expression
						exit Expression
						enter Expression
							enter Expression
								enter Expression
								exit Expression
							exit Expression
						exit Expression
					exit Expression
				exit Expression
				enter IdentifierExpected
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
				enter IdentifierExpected
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
							enter Expression
								enter Expression
								exit Expression
							exit Expression
						exit Expression
						enter Expression
							enter Expression
								enter Expression
								exit Expression
							exit Expression
						exit Expression
						enter Expression
							enter Expression
								enter Expression
									enter Expression
										enter Expression
										exit Expression
									exit Expression
								exit Expression
								enter Expression
									enter Expression
										enter Expression
										exit Expression
									exit Expression
								exit Expression
							exit Expression
						exit Expression
					exit Expression
				exit Expression
				enter IdentifierExpected
				exit IdentifierExpected
				enter Expression
					enter Expression
						enter Expression
							enter Xml
								enter Expression
									enter Expression
										enter Expression
										exit Expression
									exit Expression
								exit Expression
								enter Expression
									enter Expression
										enter Expression
										exit Expression
									exit Expression
								exit Expression
							exit Xml
						exit Expression
					exit Expression
				exit Expression
				enter Expression
					enter Expression
						enter Expression
						exit Expression
					exit Expression
				exit Expression
			exit Body
		exit Member
	exit Type
exit Global
");
		}
		
		[Test]
		public void Imports()
		{
			RunTest(@"Imports System
Imports System.Linq
Imports System.Collections.Generic",
			@"enter Global
exit Global
");
		}
		
		void RunTest(string code, string expectedOutput)
		{
			ExpressionFinder p = new ExpressionFinder();
			ILexer lexer = ParserFactory.CreateLexer(SupportedLanguage.VBNet, new StringReader(code));
			Token t;
			
			do {
				t = lexer.NextToken();
				p.InformToken(t);
			} while (t.Kind != VBParser.Tokens.EOF);
			
			Console.WriteLine(p.Output);
			
			Assert.IsEmpty(p.Errors);
			
			Assert.AreEqual(expectedOutput, p.Output);
		}
	}
}
