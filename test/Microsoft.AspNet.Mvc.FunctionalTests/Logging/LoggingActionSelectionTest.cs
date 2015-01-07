// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50  // Since Json.net serialization fails in CoreCLR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LoggingWebSite;
using LoggingWebSite.Models;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class LoggingActionSelectionTest
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices(nameof(LoggingWebSite));
        private readonly Action<IApplicationBuilder> _app = new LoggingWebSite.Startup().Configure;

        [Fact]
        public async Task Successful_MvcRouteMatching_Logged()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var requestTraceId = Guid.NewGuid().ToString();

            // Act
            var response = await client.GetAsync(string.Format(
                                                        "http://localhost/home/index?{0}={1}",
                                                        LoggingExtensions.RequestTraceIdQueryKey,
                                                        requestTraceId));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Assert.Equal("Home.Index", responseData);

            var logs = await GetLogsAsync(client, requestTraceId);
            var scopeNode = logs.FindScope(nameof(MvcRouteHandler) + ".RouteAsync");

            Assert.NotNull(scopeNode);
            var logInfo = scopeNode.Messages.OfDataType<MvcRouteHandlerRouteAsyncValues>()
                                            .FirstOrDefault();

            Assert.NotNull(logInfo);
            Assert.NotNull(logInfo.State);

            dynamic actionSelection = logInfo.State;
            Assert.True((bool)actionSelection.ActionSelected);
            Assert.True((bool)actionSelection.ActionInvoked);
            Assert.True((bool)actionSelection.Handled);
        }

        [Fact]
        public async Task Failed_MvcRouteMatching_Logged()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var requestTraceId = Guid.NewGuid().ToString();

            // Act
            var response = await client.GetAsync(string.Format(
                                                        "http://localhost/InvalidController/InvalidAction?{0}={1}",
                                                        LoggingExtensions.RequestTraceIdQueryKey,
                                                        requestTraceId));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var logs = await GetLogsAsync(client, requestTraceId);
            var scopeNode = logs.FindScope(nameof(MvcRouteHandler) + ".RouteAsync");

            Assert.NotNull(scopeNode);
            var logInfo = scopeNode.Messages.OfDataType<MvcRouteHandlerRouteAsyncValues>()
                                            .FirstOrDefault();
            Assert.NotNull(logInfo);

            dynamic actionSelection = logInfo.State;
            Assert.False((bool)actionSelection.ActionSelected);
            Assert.False((bool)actionSelection.ActionInvoked);
            Assert.False((bool)actionSelection.Handled);
        }

        [Fact]
        public async Task ActionSelectionInformation_Logged()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var requestTraceId = Guid.NewGuid().ToString();

            // Act
            var response = await client.GetAsync(string.Format(
                                                        "http://localhost/home/index?{0}={1}",
                                                        LoggingExtensions.RequestTraceIdQueryKey,
                                                        requestTraceId));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Assert.Equal("Home.Index", responseData);

            var logs = await GetLogsAsync(client, requestTraceId);
            var scopeNode = logs.FindScope(nameof(DefaultActionSelector) + ".SelectAsync");

            Assert.NotNull(scopeNode);
            var logInfo = scopeNode.Messages.OfDataType<DefaultActionSelectorSelectAsyncValues>()
                                            .FirstOrDefault();

            Assert.NotNull(logInfo);
            Assert.NotNull(logInfo.State);

            var jo = logInfo.State as JObject;



            dynamic actionSelectionResult = logInfo.State;
            Assert.NotNull(actionSelectionResult);

            //dynamic selectedAction = actionSelectionResult.SelectedAction;
            //Assert.Equal(
            //            typeof(LoggingWebSite.Controllers.HomeController).FullName + ".Default",
            //            selectedAction.DisplayName.ToString());
            //Assert.Equal("Index", selectedAction.Name.ToString());
            //Assert.Equal(
            //            typeof(LoggingWebSite.Controllers.HomeController),
            //            (Type)selectedAction.ControllerTypeInfo);
            //Assert.Equal(0, selectedAction.Parameters.Count); // JArray
            //Assert.False(selectedAction.HttpMethods.HasValues); // HttpMethods is a List<string> and here its JValue
            //Assert.Equal(0, selectedAction.FilterDescriptors.Count); // JArray
        }

        [Fact]
        public async Task ActionSelectionScopes_Logged()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var requestTraceId = Guid.NewGuid().ToString();

            // Act
            var response = await client.GetAsync(string.Format(
                                                        "http://localhost/home/index?{0}={1}",
                                                        LoggingExtensions.RequestTraceIdQueryKey,
                                                        requestTraceId));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Assert.Equal("Home.Index", responseData);

            var logs = await GetLogsAsync(client, requestTraceId);
            logs = logs.FilterByRequestTraceId(requestTraceId);

            ScopeNodeDto rootScopeNode = logs.First().Root;

            LoggingAssert.Subset(
                    new[] {
                    "MvcRouteHandler.RouteAsync",
                    "DefaultActionSelector.SelectAsync"
                    },
                     rootScopeNode.FlattenScopeTree().ToArray());
        }
        
        [Fact]
        public async Task MethodConstraints_Filters_Logged()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var requestTraceId = Guid.NewGuid().ToString();
            var kvps = new List<KeyValuePair<string, string>>();
            kvps.Add(new KeyValuePair<string, string>("Id", "10"));
            kvps.Add(new KeyValuePair<string, string>("Name", "Mike"));

            // Act
            var response = await client.PostAsync(string.Format(
                                                        "http://localhost/employees/create?{0}={1}",
                                                        LoggingExtensions.RequestTraceIdQueryKey,
                                                        requestTraceId),
                                                  new FormUrlEncodedContent(kvps));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadAsStringAsync();
            Assert.Equal("Id:10,Name:Mike", responseData);

            var logs = await GetLogsAsync(client, requestTraceId);
            logs = logs.FilterByRequestTraceId(requestTraceId);

            var scopeNode = logs.FindScope(nameof(DefaultActionSelector) + ".SelectAsync");

            Assert.NotNull(scopeNode);
            var logInfo = scopeNode.Messages.OfDataType<DefaultActionSelectorSelectAsyncValues>()
                                            .FirstOrDefault();

            Assert.NotNull(logInfo);
            Assert.NotNull(logInfo.State);

            dynamic actionSelectionResult = logInfo.State;
            Assert.NotNull(actionSelectionResult);

            //dynamic selectedAction = actionSelectionResult.SelectedAction;
            //Assert.Equal("Create", selectedAction.Name.ToString());
            //Assert.Equal(
            //            typeof(LoggingWebSite.Controllers.EmployeesController),
            //            (Type)selectedAction.ControllerTypeInfo);

            //Assert.Equal(1, selectedAction.Parameters.Count); // JArray
            //Assert.Equal("employee", selectedAction.Parameters[0].ParameterName); // JArray
            //Assert.Equal(typeof(Employee), (Type)selectedAction.Parameters[0].ParameterType);
            //Assert.True(selectedAction.HttpMethods.HasValues); // HttpMethods is a List<string> and here its JValue
            //Assert.Equal("POST", selectedAction.HttpMethods[0]);
            //Assert.Equal(1, selectedAction.FilterDescriptors.Count); // JArray
            //Assert.Equal(typeof(DummyActionFilterAttribute), selectedAction.FilterDescriptors[0].FilterType);
        }
        private async Task<IEnumerable<ActivityContextDto>> GetLogsAsync(HttpClient client,
                                                                    string requestTraceId)
        {
            var responseData = await client.GetStringAsync("http://localhost/logs");
            var logActivities = JsonConvert.DeserializeObject<List<ActivityContextDto>>(responseData);
            return logActivities.FilterByRequestTraceId(requestTraceId);
        }

    }
}
#endif