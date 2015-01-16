// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class FormatFilterTest
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("FiltersWebSite");
        private readonly Action<IApplicationBuilder> _app = new FiltersWebSite.Startup().Configure;

        [Fact]
        public async Task AllowsAnonymousUsersToAccessController()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/FormatFilter/GetProduct/5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("4", await response.Content.ReadAsStringAsync());
        }
    }
}