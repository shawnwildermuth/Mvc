// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.PipelineCore;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ContentResultTest
    {
        [Fact]
        public async Task ContentResult_Response_SetsContentType()
        {
            // Arrange
            var contentResult = new ContentResult
            {
                Content = "Test Content",
                ContentType = "text/plain",
                ContentEncoding = null
            };
            var httpContext = GetHttpContext();
            var actionContext = GetActionContext(httpContext);

            // Act
            await contentResult.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal("text/plain", httpContext.Response.ContentType);
        }

        [Fact]
        public async Task ContentResult_Response_SetsContentTypeAndEncoding()
        {
            // Arrange
            var contentResult = new ContentResult
            {
                Content = "Test Content",
                ContentType = "text/plain",
                ContentEncoding = Encoding.UTF8
            };
            var httpContext = GetHttpContext();
            var actionContext = GetActionContext(httpContext);

            // Act
            await contentResult.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal("text/plain; charset=utf-8", httpContext.Response.ContentType);
        }

        [Fact]
        public async Task ContentResult_Response_EncodingNotSet_IfContentTypeNull()
        {
            // Arrange
            var contentResult = new ContentResult
            {
                Content = "Test Content",
                ContentType = null,
                ContentEncoding = Encoding.UTF8
            };
            var httpContext = GetHttpContext();
            var actionContext = GetActionContext(httpContext);

            // Act
            await contentResult.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Null(httpContext.Response.ContentType);
        }

        private static ActionContext GetActionContext(HttpContext httpContext)
        {
            var routeData = new RouteData();
            routeData.Routers.Add(Mock.Of<IRouter>());

            return new ActionContext(httpContext,
                                    routeData,
                                    new ActionDescriptor());
        }

        private static HttpContext GetHttpContext()
        {
            var httpContext = new Mock<HttpContext>();
            var realContext = new DefaultHttpContext();
            var response = realContext.Response;
            httpContext.Setup(o => o.Response)
                       .Returns(response);

            return httpContext.Object;
        }
    }
}