﻿using System;
using Castle.Windsor;

namespace TestUnium.Stepping.Pipeline
{
    public interface IStepModuleRegistrationStrategy
    {
        void RegisterStepModule<TStepModule>(IWindsorContainer container, Boolean makeReusable) where TStepModule : IStepModule;
        void RegisterStepModules(IWindsorContainer container, params Type[] moduleTypes);
        void RegisterStepModules(IWindsorContainer container, Boolean makeReusable, params Type[] moduleTypes);
        //void UnregisterStepModule<TStepModule>(IWindsorContainer container) where TStepModule : IStepModule;
        //void UnregisterStepModules(IWindsorContainer container, params Type[] moduleTypes);
    }
}