// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace FiltersWebSite
{
    public class FormatFilterController : Controller
    {   
        public IActionResult GetProduct(int id)
        {
            return new ObjectResult("In GetProduct: " + id);
        }
    }
}