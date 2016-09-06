﻿using System;
using Ninject;
using TestUnium.Stepping.Modules;

namespace TestUnium.Stepping
{
    public interface IStepModuleRegistrationStrategy
    {
        void RegisterStepModule<TStepModule>(IKernel kernel, String context, Boolean makeReusable) where TStepModule : IStepModule;
        void RegisterStepModules(IKernel kernel, String context, params Type[] moduleTypes);
        void RegisterStepModules(IKernel kernel, String context, Boolean makeReusable, params Type[] moduleTypes);
        void UnregisterStepModule<TStepModule>(IKernel kernel) where TStepModule : IStepModule;
        void UnregisterStepModules(IKernel kernel, params Type[] moduleTypes);
    }
}