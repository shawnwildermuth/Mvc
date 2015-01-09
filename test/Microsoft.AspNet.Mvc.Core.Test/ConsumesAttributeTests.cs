// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.PipelineCore;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ConsumesAttributeTests
    {
        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json;Parameter1=12")]
        [InlineData("text/xml")]
        public void Accept_MatchesForMachingRequestContentType(string contentType)
        {
            // Arrange
            var action = new ActionDescriptor();
            var constraint = new ConsumesAttribute("application/json", "text/xml");

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(action, new [] { constraint }),
            };

            context.CurrentCandidate = context.Candidates[0];
            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            Assert.True(constraint.Accept(context));
        }

        [Fact]
        public void Accept_TheFirstCandidateReturnsFalse_IfALaterOneMatches()
        {
            // Arrange
            var action1 = new ActionDescriptor();
            var action2 = new ActionDescriptor();
            var constraint1 = new ConsumesAttribute("application/json", "text/xml");
            var constraint2 = new Mock<IConsumesActionConstraint>();
            constraint2.Setup(o => o.Accept(It.IsAny<ActionConstraintContext>()))
                       .Returns(true);

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(action1, new [] { constraint1 }),
                new ActionSelectorCandidate(action2, new [] { constraint2.Object }),
            };

            context.CurrentCandidate = context.Candidates[0];
            context.RouteContext = CreateRouteContext(contentType: "application/custom");

            // Act & Assert
            Assert.False(constraint1.Accept(context));
        }

        [Theory]
        [InlineData("application/custom")]
        [InlineData("")]
        [InlineData(null)]
        public void Accept_ForNoMatchingCandidates_SelectsTheFirstCandidate(string contentType)
        {
            // Arrange
            var action1 = new ActionDescriptor();
            var action2 = new ActionDescriptor();
            var constraint1 = new ConsumesAttribute("application/json", "text/xml");
            var constraint2 = new Mock<IConsumesActionConstraint>();
            constraint2.Setup(o => o.Accept(It.IsAny<ActionConstraintContext>()))
                       .Returns(false);

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(action1, new [] { constraint1 }),
                new ActionSelectorCandidate(action2, new [] { constraint2.Object }),
            };

            context.CurrentCandidate = context.Candidates[0];
            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            Assert.True(constraint1.Accept(context));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Accept_ForNoRequestType_SelectsTheCandidateWithoutConstraintIfPresent(string contentType)
        {
            // Arrange
            var actionWithConstraint = new ActionDescriptor();
            var actionWithConstraint2 = new ActionDescriptor();
            var actionWithoutConstraint = new ActionDescriptor();
            var constraint1 = new ConsumesAttribute("application/json");
            var constraint2 = new ConsumesAttribute("text/xml");

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(actionWithConstraint, new [] { constraint1 }),
                new ActionSelectorCandidate(actionWithConstraint2, new [] { constraint2 }),
                new ActionSelectorCandidate(actionWithoutConstraint, new List<IActionConstraint>()),
            };

            context.CurrentCandidate = context.Candidates[0];
            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            Assert.False(constraint1.Accept(context));
            Assert.False(constraint2.Accept(context));
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("application/custom")]
        [InlineData("invalid/invalid")]
        public void Accept_UnrecognizedMediaType_SelectsTheCandidateWithoutConstraintIfPresent(string contentType)
        {
            // Arrange
            var actionWithConstraint = new ActionDescriptor();
            var actionWithConstraint2 = new ActionDescriptor();
            var actionWithoutConstraint = new ActionDescriptor();
            var constraint1 = new ConsumesAttribute("application/json");
            var constraint2 = new ConsumesAttribute("text/xml");

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(actionWithConstraint, new [] { constraint1 }),
                new ActionSelectorCandidate(actionWithConstraint2, new [] { constraint2 }),
                new ActionSelectorCandidate(actionWithoutConstraint, new List<IActionConstraint>()),
            };

            context.CurrentCandidate = context.Candidates[0];
            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            Assert.False(constraint1.Accept(context));
            Assert.False(constraint2.Accept(context));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Accept_ForNoRequestType_ReturnsTrueForAllConstraints(string contentType)
        {
            // Arrange
            var actionWithConstraint = new ActionDescriptor();
            var actionWithConstraint2 = new ActionDescriptor();
            var actionWithoutConstraint = new ActionDescriptor();
            var constraint1 = new ConsumesAttribute("application/json");
            var constraint2 = new ConsumesAttribute("text/xml");

            var context = new ActionConstraintContext();
            context.Candidates = new List<ActionSelectorCandidate>()
            {
                new ActionSelectorCandidate(actionWithConstraint, new [] { constraint1 }),
                new ActionSelectorCandidate(actionWithConstraint2, new [] { constraint2 }),
            };

            context.CurrentCandidate = context.Candidates[0];
            context.RouteContext = CreateRouteContext(contentType: contentType);

            // Act & Assert
            Assert.True(constraint1.Accept(context));
            Assert.True(constraint2.Accept(context));
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("application/custom")]
        public void OnResourceExecuting_ForNoContentTypeMatch_SetsUnsupportedMediaTypeResult(string contentType)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = contentType;
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var actionWithConstraint = new ActionDescriptor();
            var consumesFilter = new ConsumesAttribute("application/json");

            var resourceExecutingContext = new ResourceExecutingContext(actionContext, new[] { consumesFilter });

            // Act
            consumesFilter.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.NotNull(resourceExecutingContext.Result);
            Assert.IsType<UnsupportedMediaTypeResult>(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void OnResourceExecuting_NullOrEmptyRequestContentType_IsNoOp(string contentType)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = contentType;
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var actionWithConstraint = new ActionDescriptor();
            var consumesFilter = new ConsumesAttribute("application/json");

            var resourceExecutingContext = new ResourceExecutingContext(actionContext, new[] { consumesFilter });

            // Act
            consumesFilter.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Null(resourceExecutingContext.Result);
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("application/json")]
        public void OnResourceExecuting_ForAContentTypeMatch_IsNoOp(string contentType)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = contentType;
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var actionWithConstraint = new ActionDescriptor();
            var consumesFilter = new ConsumesAttribute("application/json", "application/xml");

            var resourceExecutingContext = new ResourceExecutingContext(actionContext, new[] { consumesFilter });

            // Act
            consumesFilter.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Null(resourceExecutingContext.Result);
        }

        private static RouteContext CreateRouteContext(string contentType = null, object routeValues = null)
        {
            var httpContext = new DefaultHttpContext();
            if (contentType != null)
            {
                httpContext.Request.ContentType = contentType;
            }

            var routeContext = new RouteContext(httpContext);
            routeContext.RouteData = new RouteData();

            foreach (var kvp in new RouteValueDictionary(routeValues))
            {
                routeContext.RouteData.Values.Add(kvp.Key, kvp.Value);
            }

            return routeContext;
        }
    }
}