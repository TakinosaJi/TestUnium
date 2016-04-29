﻿using System;
using TestUnium.Common;
using TestUnium.Customization;
using TestUnium.Instantiation.Prioritizing;
using TestUnium.Instantiation.WebDriving;

namespace TestUnium.Instantiation.Browsing
{
    [Priority((UInt16)CustomizationAttributePriorities.DefaultBrowser)]
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultBrowserAttribute : CustomizationBase, ICustomizationAttribute<WebDriverDrivenTest>
    {
        public Browser DefaultBrowser { get; set; }
        public DefaultBrowserAttribute(Browser defaultBrowser) : base(typeof(WebDriverDrivenTest))
        {
            DefaultBrowser = defaultBrowser;
        }
        public void Customize(WebDriverDrivenTest context)
        {
            context.Browser = DefaultBrowser;
        }
    }
}
