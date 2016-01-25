﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.DataInspect {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]   // required for tests using R Host 
    public class EvaluationWrapperTest {
        // TODO: RStudio difference
        //    value.integer.1   RS 1L                    RTVS just 1
        //    value.numeric.big RS 98765432109876543210  RTVS 9.88e+19
        //    value.date        RS 2015-08-01            RTVS Date, format: "2015-08-01"
        // NOTE: value.date  HasChildren is true. really?
        object[,] valueTestData = new object[,] {
            { "value.na <- NA", new VariableExpectation() { Name = "value.na", Value = "NA", TypeName = "logical", Class = "logical", HasChildren = false, CanShowDetail = false } },
            { "value.null <- NULL", new VariableExpectation() { Name = "value.null", Value = "NULL", TypeName = "NULL", Class = "NULL", HasChildren = false, CanShowDetail = false } },
            { "value.NaN <- NaN", new VariableExpectation() { Name = "value.NaN", Value = "NaN", TypeName = "double", Class = "numeric", HasChildren = false, CanShowDetail = false } },
            { "value.character <- 'abcdefghijklmnopqrstuvwxyz'", new VariableExpectation() { Name = "value.character", Value = "\"abcdefghijklmnopqrstuvwxyz\"", TypeName = "character", Class = "character", HasChildren = false, CanShowDetail = false } },
            { "value.character.1 <- 'abcde\"fhi,kjl \"op,qr\" s @t#u$v%w^x&y*z*()./\\`-+_=!'", new VariableExpectation() { Name = "value.character.1", Value = "\"abcde\\\"fhi,kjl \\\"op,qr\\\" s @t#u$v%w^x&y*z*()./`-+_=!\"", TypeName = "character", Class = "character", HasChildren = false, CanShowDetail = false } },
            { "value.numeric.1 <- 1", new VariableExpectation() { Name = "value.numeric.1", Value = "1", TypeName = "double", Class = "numeric", HasChildren = false, CanShowDetail = false } },
            { "value.numeric.negative <- -123456", new VariableExpectation() { Name = "value.numeric.negative", Value = "-123456", TypeName = "double", Class = "numeric", HasChildren = false, CanShowDetail = false } },
            { "value.numeric.big <- 98765432109876543210.9876543210", new VariableExpectation() { Name = "value.numeric.big", Value = "9.88e+19", TypeName = "double", Class = "numeric", HasChildren = false, CanShowDetail = false } },
            { "value.integer.1 <- 1L", new VariableExpectation() { Name = "value.integer.1", Value = "1", TypeName = "integer", Class = "integer", HasChildren = false, CanShowDetail = false } },
            { "value.integer.negative <- -123456L", new VariableExpectation() { Name = "value.integer.negative", Value = "-123456", TypeName = "integer", Class = "integer", HasChildren = false, CanShowDetail = false } },
            { "value.complex <- complex(real=100, imaginary=100)", new VariableExpectation() { Name = "value.complex", Value = "100+100i", TypeName = "complex", Class = "complex", HasChildren = false, CanShowDetail = false } },
            { "value.complex.neg <- complex(real=-200, imaginary=-900)", new VariableExpectation() { Name = "value.complex.neg", Value = "-200-900i", TypeName = "complex", Class = "complex", HasChildren = false, CanShowDetail = false } },
            { "value.logical <- TRUE", new VariableExpectation() { Name = "value.logical", Value = "TRUE", TypeName = "logical", Class = "logical", HasChildren = false, CanShowDetail = false } },
            { "value.date <- as.Date('2015-08-01')", new VariableExpectation() { Name = "value.date", Value = "Date, format: \"2015-08-01\"", TypeName = "double", Class = "Date", HasChildren = true, CanShowDetail = false } },
        };

        object[,] factorTestData = new object[,] {
            { "factor.5 <- factor(1:5)", new VariableExpectation() { Name = "factor.5", Value = "Factor w/ 5 levels \"1\",\"2\",\"3\",\"4\",..: 1 2 3 4 5", TypeName = "integer", Class = "factor", HasChildren = true, CanShowDetail = false } },
            { "factor.ordered <- ordered(c('5','4','100','2','1'))", new VariableExpectation() { Name = "factor.ordered", Value = "Ord.factor w/ 5 levels \"1\"<\"100\"<\"2\"<..: 5 4 2 3 1", TypeName = "integer", Class = "ordered, factor", HasChildren = true, CanShowDetail = false } },
            { "factor.gender <- factor(c('male','female','male','male','female'))", new VariableExpectation() { Name = "factor.gender", Value = "Factor w/ 2 levels \"female\",\"male\": 2 1 2 2 1", TypeName = "integer", Class = "factor", HasChildren = true, CanShowDetail = false } },
        };

        object[,] formulaTestData = new object[,] {
            { "class(fo <- y~x1 * x2)", new VariableExpectation() { Name = "fo", Value = "Class 'formula' length 3 y ~ x1 * x2", TypeName = "language", Class = "formula", HasChildren = true, CanShowDetail = false } },
        };

        object[,] expressionTestData = new object[,] {
            { "expr <- expression('print(\"hello\")', '1+2', 'print(\"world\")', 'ls()')", new VariableExpectation() { Name = "expr", Value = "expression(\"print(\\\"hello\\\")\", \"1+2\", \"print(\\\"world\\\")\") ...", TypeName = "expression", Class = "expression", HasChildren = true, CanShowDetail = false } },
        };

        object[,] listTestData = new object[,] {
            { "list.length1 <- list(c(1, 2, 3))", new VariableExpectation() { Name = "list.length1", Value = "List of 1", TypeName = "list", Class = "list", HasChildren = true, CanShowDetail = false } },
        };

        [Test]
        [Category.Variable.Explorer]
        public Task ValuesTest() {
            return RunTest(valueTestData);
        }

        [Test]
        [Category.Variable.Explorer]
        public Task FactorTest() {
            return RunTest(factorTestData);
        }

        [Test]
        [Category.Variable.Explorer]
        public Task FormulaTest() {
            return RunTest(formulaTestData);
        }

        [Test]
        [Category.Variable.Explorer]
        public Task ExpressionTest() {
            return RunTest(expressionTestData);
        }

        [Test]
        [Category.Variable.Explorer]
        public Task ListTest() {
            return RunTest(listTestData);
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task TruncateGrandChildrenTest() {
            using (var hostScript = new VariableRHostScript()) {
                await hostScript.EvaluateAsync("x.truncate.children<-1:100");
                var children = await hostScript.GlobalEnvrionment.GetChildrenAsync();
                var child = children.First(c => c.Name == "x.truncate.children");

                var grandChildren = await child.GetChildrenAsync();

                grandChildren.Count.ShouldBeEquivalentTo(21);   // truncate 20 + ellipsis
                grandChildren[20].Value.ShouldBeEquivalentTo(Resources.VariableExplorer_Truncated);
            }
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task Matrix10x100Test() {
            var script = "matrix.10x100 <-matrix(1:1000, 10, 100)";
            var expectation = new VariableExpectation() { Name = "matrix.10x100", Value = "int [1:10, 1:100] 1 2 3 4 5 6 7 8 9 10 ...", TypeName = "integer", Class = "matrix", HasChildren = true, CanShowDetail = true };

            using (var hostScript = new VariableRHostScript()) {
                var evaluation = (EvaluationWrapper)await hostScript.EvaluateAndAssert(
                    script,
                    expectation,
                    VariableRHostScript.AssertEvaluationWrapper);

                Range rowRange = new Range(0, 2);
                Range columnRange = new Range(1, 3);
                var grid = await evaluation.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

                grid.ColumnHeader.Range.ShouldBeEquivalentTo(columnRange);
                grid.ColumnHeader[1].ShouldBeEquivalentTo("[,2]");
                grid.ColumnHeader[2].ShouldBeEquivalentTo("[,3]");
                grid.ColumnHeader[3].ShouldBeEquivalentTo("[,4]");

                grid.RowHeader.Range.ShouldBeEquivalentTo(rowRange);
                grid.RowHeader[0].ShouldBeEquivalentTo("[1,]");
                grid.RowHeader[1].ShouldBeEquivalentTo("[2,]");

                grid.Grid.Range.ShouldBeEquivalentTo(new GridRange(rowRange, columnRange));
                grid.Grid[0, 1].ShouldBeEquivalentTo("11");
                grid.Grid[0, 2].ShouldBeEquivalentTo("21");
                grid.Grid[0, 3].ShouldBeEquivalentTo("31");
                grid.Grid[1, 1].ShouldBeEquivalentTo("12");
                grid.Grid[1, 2].ShouldBeEquivalentTo("22");
                grid.Grid[1, 3].ShouldBeEquivalentTo("32");
            }
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task MatrixNamedTest() {
            var script = "matrix.named <- matrix(1:10, 2, 5, dimnames = list(r = c('r1', 'r2'), c = c('a', 'b', 'c', 'd', 'e')))";
            var expectation = new VariableExpectation() { Name = "matrix.named", Value = "int [1:2, 1:5] 1 2 3 4 5 6 7 8 9 10", TypeName = "integer", Class = "matrix", HasChildren = true, CanShowDetail = true };

            using (var hostScript = new VariableRHostScript()) {
                var evaluation = (EvaluationWrapper)await hostScript.EvaluateAndAssert(
                    script,
                    expectation,
                    VariableRHostScript.AssertEvaluationWrapper);

                Range rowRange = new Range(0, 2);
                Range columnRange = new Range(2, 3);
                var grid = await evaluation.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

                grid.ColumnHeader.Range.ShouldBeEquivalentTo(columnRange);
                grid.ColumnHeader[2].ShouldBeEquivalentTo("c");
                grid.ColumnHeader[3].ShouldBeEquivalentTo("d");
                grid.ColumnHeader[4].ShouldBeEquivalentTo("e");

                grid.RowHeader.Range.ShouldBeEquivalentTo(rowRange);
                grid.RowHeader[0].ShouldBeEquivalentTo("r1");
                grid.RowHeader[1].ShouldBeEquivalentTo("r2");

                grid.Grid.Range.ShouldBeEquivalentTo(new GridRange(rowRange, columnRange));
                grid.Grid[0, 2].ShouldBeEquivalentTo("5");
                grid.Grid[0, 3].ShouldBeEquivalentTo("7");
                grid.Grid[0, 4].ShouldBeEquivalentTo(" 9"); // TODO: R returns with space
                grid.Grid[1, 2].ShouldBeEquivalentTo("6");
                grid.Grid[1, 3].ShouldBeEquivalentTo("8");
                grid.Grid[1, 4].ShouldBeEquivalentTo("10");
            }
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task MatrixNATest() {
            var script = "matrix.na.header <- matrix(c(1, 2, 3, 4, NA, NaN, 7, 8, 9, 10), 2, 5, dimnames = list(r = c('r1', NA), c = c('a', 'b', NA, 'd', NA)))";
            var expectation = new VariableExpectation() { Name = "matrix.na.header", Value = "num [1:2, 1:5] 1 2 3 4 NA NaN 7 8 9 10", TypeName = "double", Class = "matrix", HasChildren = true, CanShowDetail = true };

            using (var hostScript = new VariableRHostScript()) {
                var evaluation = (EvaluationWrapper)await hostScript.EvaluateAndAssert(
                    script,
                    expectation,
                    VariableRHostScript.AssertEvaluationWrapper);

                Range rowRange = new Range(0, 2);
                Range columnRange = new Range(2, 3);
                var grid = await evaluation.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

                grid.ColumnHeader.Range.ShouldBeEquivalentTo(columnRange);
                grid.ColumnHeader[2].ShouldBeEquivalentTo("NA");
                grid.ColumnHeader[3].ShouldBeEquivalentTo("d");
                grid.ColumnHeader[4].ShouldBeEquivalentTo("NA");

                grid.RowHeader.Range.ShouldBeEquivalentTo(rowRange);
                grid.RowHeader[0].ShouldBeEquivalentTo("r1");
                grid.RowHeader[1].ShouldBeEquivalentTo("NA");

                grid.Grid.Range.ShouldBeEquivalentTo(new GridRange(rowRange, columnRange));
                grid.Grid[0, 2].ShouldBeEquivalentTo(" NA");    // TODO: R returns with space
                grid.Grid[0, 3].ShouldBeEquivalentTo("7");
                grid.Grid[0, 4].ShouldBeEquivalentTo(" 9");     // TODO: R returns with space
                grid.Grid[1, 2].ShouldBeEquivalentTo("NaN");
                grid.Grid[1, 3].ShouldBeEquivalentTo("8");
                grid.Grid[1, 4].ShouldBeEquivalentTo("10");
            }
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task MatrixOneRowColumnTest() {
            var script1 = "matrix.singlerow <- matrix(1:3, nrow =1);";
            var expectation1 = new VariableExpectation() { Name = "matrix.singlerow", Value = "int [1, 1:3] 1 2 3", TypeName = "integer", Class = "matrix", HasChildren = true, CanShowDetail = true };

            var script2 = "matrix.singlecolumn <- matrix(1:3, ncol=1);";
            var expectation2 = new VariableExpectation() { Name = "matrix.singlecolumn", Value = "int [1:3, 1] 1 2 3", TypeName = "integer", Class = "matrix", HasChildren = true, CanShowDetail = true };

            using (var hostScript = new VariableRHostScript()) {
                var evaluation = (EvaluationWrapper)await hostScript.EvaluateAndAssert(
                    script1,
                    expectation1,
                    VariableRHostScript.AssertEvaluationWrapper);

                Range rowRange = new Range(0, 1);
                Range columnRange = new Range(0, 3);
                var grid = await evaluation.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

                grid.ColumnHeader.Range.ShouldBeEquivalentTo(columnRange);
                grid.ColumnHeader[0].ShouldBeEquivalentTo("[,1]");
                grid.ColumnHeader[1].ShouldBeEquivalentTo("[,2]");
                grid.ColumnHeader[2].ShouldBeEquivalentTo("[,3]");

                grid.RowHeader.Range.ShouldBeEquivalentTo(rowRange);
                grid.RowHeader[0].ShouldBeEquivalentTo("[1,]");

                grid.Grid.Range.ShouldBeEquivalentTo(new GridRange(rowRange, columnRange));
                grid.Grid[0, 0].ShouldBeEquivalentTo("1");
                grid.Grid[0, 1].ShouldBeEquivalentTo("2");
                grid.Grid[0, 2].ShouldBeEquivalentTo("3");


                evaluation = (EvaluationWrapper)await hostScript.EvaluateAndAssert(
                    script2,
                    expectation2,
                    VariableRHostScript.AssertEvaluationWrapper);

                rowRange = new Range(0, 3);
                columnRange = new Range(0, 1);
                grid = await evaluation.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

                grid.ColumnHeader.Range.ShouldBeEquivalentTo(columnRange);
                grid.ColumnHeader[0].ShouldBeEquivalentTo("[,1]");

                grid.RowHeader.Range.ShouldBeEquivalentTo(rowRange);
                grid.RowHeader[0].ShouldBeEquivalentTo("[1,]");
                grid.RowHeader[1].ShouldBeEquivalentTo("[2,]");
                grid.RowHeader[2].ShouldBeEquivalentTo("[3,]");

                grid.Grid.Range.ShouldBeEquivalentTo(new GridRange(rowRange, columnRange));
                grid.Grid[0, 0].ShouldBeEquivalentTo("1");
                grid.Grid[1, 0].ShouldBeEquivalentTo("2");
                grid.Grid[2, 0].ShouldBeEquivalentTo("3");
            }
        }

        [Test]
        [Category.Variable.Explorer]
        public async Task DataFrameTest() {
            var script = "df.test <- data.frame(101:103, c('a', 'b', 'c'))";
            var expectation = new VariableExpectation() { Name = "df.test", Value = "3 obs. of  2 variables", TypeName = "list", Class = "data.frame", HasChildren = true, CanShowDetail = true };

            using (var hostScript = new VariableRHostScript()) {
                var evaluation = (EvaluationWrapper)await hostScript.EvaluateAndAssert(
                    script,
                    expectation,
                    VariableRHostScript.AssertEvaluationWrapper);

                Range rowRange = new Range(0, 3);
                Range columnRange = new Range(0, 2);
                var grid = await evaluation.GetGridDataAsync(evaluation.Expression, new GridRange(rowRange, columnRange));

                grid.ColumnHeader.Range.ShouldBeEquivalentTo(columnRange);
                grid.ColumnHeader[0].ShouldBeEquivalentTo("X101.103");
                grid.ColumnHeader[1].ShouldBeEquivalentTo("c..a....b....c..");

                grid.RowHeader.Range.ShouldBeEquivalentTo(rowRange);
                grid.RowHeader[0].ShouldBeEquivalentTo("1");
                grid.RowHeader[1].ShouldBeEquivalentTo("2");
                grid.RowHeader[2].ShouldBeEquivalentTo("3");

                grid.Grid.Range.ShouldBeEquivalentTo(new GridRange(rowRange, columnRange));
                grid.Grid[0, 0].ShouldBeEquivalentTo("101");
                grid.Grid[0, 1].ShouldBeEquivalentTo("a");
                grid.Grid[1, 0].ShouldBeEquivalentTo("102");
                grid.Grid[1, 1].ShouldBeEquivalentTo("b");
                grid.Grid[2, 0].ShouldBeEquivalentTo("103");
                grid.Grid[2, 1].ShouldBeEquivalentTo("c");
            }
        }

        private static async Task RunTest(object[,] testData) {
            using (var hostScript = new VariableRHostScript()) {
                int testCount = testData.GetLength(0);

                for (int i = 0; i < testCount; i++) {
                    await hostScript.EvaluateAndAssert(
                        (string)testData[i, 0],
                        (VariableExpectation)testData[i, 1],
                        VariableRHostScript.AssertEvaluationWrapper);
                }
            }
        }
    }
}