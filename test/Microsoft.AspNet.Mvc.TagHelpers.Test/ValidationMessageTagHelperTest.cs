﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.PipelineCore;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    public class ValidationMessageTagHelperTest
    {
        [Fact]
        public async Task ProcessAsync_GeneratesExpectedOutput()
        {
            // Arrange
            var expectedTagName = "not-span";
            var metadataProvider = new DataAnnotationsModelMetadataProvider();
            var modelExpression = CreateModelExpression("Name");
            var validationMessageTagHelper = new ValidationMessageTagHelper
            {
                For = modelExpression
            };

            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";

            var tagHelperContext = new TagHelperContext(
                allAttributes: new Dictionary<string, object>
                {
                    { "id", "myvalidationmessage" },
                    { "for", modelExpression },
                },
                uniqueId: "test",
                getChildContentAsync: () => Task.FromResult("Something"));
            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new Dictionary<string, string>
                {
                    { "id", "myvalidationmessage" }
                })
            {
                PreContent = expectedPreContent,
                Content = expectedContent,
                PostContent = expectedPostContent,
            };
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
            var viewContext = TestableHtmlGenerator.GetViewContext(model: null,
                                                                   htmlGenerator: htmlGenerator,
                                                                   metadataProvider: metadataProvider);
            validationMessageTagHelper.ViewContext = viewContext;
            validationMessageTagHelper.Generator = htmlGenerator;

            // Act
            await validationMessageTagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(4, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("id"));
            Assert.Equal("myvalidationmessage", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("class"));
            Assert.Equal("field-validation-valid", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-valmsg-for"));
            Assert.Equal("Name", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-valmsg-replace"));
            Assert.Equal("true", attribute.Value);
            Assert.Equal(expectedPreContent, output.PreContent);
            Assert.Equal(expectedContent, output.Content);
            Assert.Equal(expectedPostContent, output.PostContent);
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task ProcessAsync_CallsIntoGenerateValidationMessageWithExpectedParameters()
        {
            // Arrange
            var validationMessageTagHelper = new ValidationMessageTagHelper
            {
                For = CreateModelExpression("Hello")
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var context = new TagHelperContext(
                allAttributes: new Dictionary<string, object>(),
                uniqueId: "test",
                getChildContentAsync: () => Task.FromResult("Something"));
            var output = new TagHelperOutput(
                "span",
                attributes: new Dictionary<string, string>())
            {
                PreContent = expectedPreContent,
                Content = expectedContent,
                PostContent = expectedPostContent,
            };
            var expectedViewContext = CreateViewContext();
            var generator = new Mock<IHtmlGenerator>();
            generator
                .Setup(mock =>
                    mock.GenerateValidationMessage(expectedViewContext, "Hello", null, null, null))
                .Returns(new TagBuilder("span"))
                .Verifiable();
            validationMessageTagHelper.Generator = generator.Object;
            validationMessageTagHelper.ViewContext = expectedViewContext;

            // Act & Assert
            await validationMessageTagHelper.ProcessAsync(context, output: output);

            generator.Verify();
            Assert.Equal("span", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent);
            Assert.Equal(expectedContent, output.Content);
            Assert.Equal(expectedPostContent, output.PostContent);
        }

        [Theory]
        [InlineData("Content of validation message", null, "Content of validation message")]
        [InlineData("Content of validation message", "Some Content", "Some Content")]
        [InlineData("\r\n  \r\n", "", "New HTML")]
        [InlineData("\r\n  \r\n", "\r\n  \r\n", "New HTML")]
        [InlineData("\r\n  \r\n", "Some Content", "Some Content")]
        public async Task ProcessAsync_MergesTagBuilderFromGenerateValidationMessage(
            string childContent, string outputContent, string expectedOutputContent)
        {
            // Arrange
            var validationMessageTagHelper = new ValidationMessageTagHelper
            {
                For = CreateModelExpression("Hello")
            };
            var output = new TagHelperOutput(
                "span",
                attributes: new Dictionary<string, string>())
            {
                Content = outputContent
            };
            var context = new TagHelperContext(allAttributes: new Dictionary<string, object>(),
                                               uniqueId: "test",
                                               getChildContentAsync: () => Task.FromResult(childContent));
            var tagBuilder = new TagBuilder("span2")
            {
                InnerHtml = "New HTML"
            };
            tagBuilder.Attributes.Add("data-foo", "bar");
            tagBuilder.Attributes.Add("data-hello", "world");

            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            var setup = generator
                .Setup(mock => mock.GenerateValidationMessage(
                    It.IsAny<ViewContext>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(tagBuilder);
            var viewContext = CreateViewContext();
            validationMessageTagHelper.ViewContext = viewContext;
            validationMessageTagHelper.Generator = generator.Object;

            // Act
            await validationMessageTagHelper.ProcessAsync(context, output: output);

            // Assert
            Assert.Equal("span", output.TagName);
            Assert.Equal(2, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-foo"));
            Assert.Equal("bar", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-hello"));
            Assert.Equal("world", attribute.Value);
            Assert.Equal(expectedOutputContent, output.Content);
        }

        [Fact]
        public async Task ProcessAsync_DoesNothingIfNullFor()
        {
            // Arrange
            var validationMessageTagHelper = new ValidationMessageTagHelper();
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var output = new TagHelperOutput(
                "span",
                attributes: new Dictionary<string, string>())
            {
                PreContent = expectedPreContent,
                Content = expectedContent,
                PostContent = expectedPostContent,
            };
            var viewContext = CreateViewContext();
            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            validationMessageTagHelper.ViewContext = viewContext;
            validationMessageTagHelper.Generator = generator.Object;

            // Act
            await validationMessageTagHelper.ProcessAsync(context: null, output: output);

            // Assert
            Assert.Equal("span", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent);
            Assert.Equal(expectedContent, output.Content);
            Assert.Equal(expectedPostContent, output.PostContent);
        }

        private static ModelExpression CreateModelExpression(string name)
        {
            return new ModelExpression(
                name,
                new ModelMetadata(
                    new Mock<IModelMetadataProvider>().Object,
                    containerType: null,
                    modelAccessor: null,
                    modelType: typeof(object),
                    propertyName: string.Empty));
        }

        private static ViewContext CreateViewContext()
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor());

            return new ViewContext(
                actionContext,
                Mock.Of<IView>(),
                new ViewDataDictionary(
                    new DataAnnotationsModelMetadataProvider()),
                TextWriter.Null);
        }
    }
}