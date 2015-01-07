// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using LoggingWebSite;
using Xunit.Sdk;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public static class LoggingAssert
    {
        /// <summary>
        /// Compares two trees and verifies if the scope nodes are equal
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <returns></returns>
        public static bool ScopesEqual(ScopeNodeDto expected, ScopeNodeDto actual)
        {
            // To enable diagnosis, here a flat-list(pe-order traversal based) of 
            // these trees is provided.
            if (!AreScopesEqual(expected, actual))
            {
                throw new EqualException(
                    expected: string.Join(",", expected.FlattenScopeTree()),
                    actual: string.Join(",", actual.FlattenScopeTree()));
            }

            return true;
        }

        /// <summary>
        /// Checks if a list of items is a sub-set of another list.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <returns></returns>
        public static bool Subset(IEnumerable<string> expected, IEnumerable<string> actual)
        {
            bool isSubset = true;

            var actualItems = actual.ToArray();
            var expectedItems = expected.ToArray();

            var expectedFirstItem = expectedItems.First();
            int actualFirstItemIndex = -1;
            for (var i = 0; i < actualItems.Length; i++)
            {
                if (string.Equals(actualItems[i], expectedFirstItem, StringComparison.OrdinalIgnoreCase))
                {
                    actualFirstItemIndex = i;
                    break;
                }
            }

            if (actualFirstItemIndex >= 0
                && expectedItems.Length <= (actualItems.Length - actualFirstItemIndex))
            {
                for (int i = 0, j = actualFirstItemIndex; i < expectedItems.Length; i++, j++)
                {
                    if (!string.Equals(expectedItems[i], actualItems[j], StringComparison.OrdinalIgnoreCase))
                    {
                        isSubset = false;
                        break;
                    }
                }
            }
            else
            {
                isSubset = false;
            }

            if(!isSubset)
            {
                throw new EqualException(
                    expected: string.Join(",", expected),
                    actual: string.Join(",", actual));
            }

            return isSubset;
        }

        /// <summary>
        /// Compares two trees and verifies if the scope nodes are equal
        /// </summary>
        /// <param name="root1"></param>
        /// <param name="root2"></param>
        /// <returns></returns>
        private static bool AreScopesEqual(ScopeNodeDto root1, ScopeNodeDto root2)
        {
            if (root1 == null && root2 == null)
            {
                return true;
            }

            if (root1 == null || root2 == null)
            {
                return false;
            }

            if (!string.Equals(root1.State?.ToString(), root2.State?.ToString(), StringComparison.OrdinalIgnoreCase)
                || root1.Children.Count != root2.Children.Count)
            {
                return false;
            }

            bool isChildScopeEqual = true;
            for (int i = 0; i < root1.Children.Count; i++)
            {
                isChildScopeEqual = AreScopesEqual(root1.Children[i], root2.Children[i]);

                if (!isChildScopeEqual)
                {
                    break;
                }
            }

            return isChildScopeEqual;
        }
    }
}