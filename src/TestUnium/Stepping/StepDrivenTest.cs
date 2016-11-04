﻿using System;
using System.Runtime.CompilerServices;
using Ninject;
using TestUnium.Sessioning;
using TestUnium.Stepping.Modules;
using TestUnium.Stepping.Steps;

namespace TestUnium.Stepping
{
    public class StepDrivenTest : SessionDrivenTest, IStepExecutor, IStepModuleRegistrator
    {
        private readonly IStepModuleRegistrationStrategy _moduleRegistrationStrategy;
        public StepDrivenTest()
        {
            _moduleRegistrationStrategy = Kernel.Get<IStepModuleRegistrationStrategy>();
            Kernel.Bind<IStepExecutor>().ToConstant(this);
        }

        public void RegisterStepModule<TStepModule>(Boolean makeReusable) where TStepModule : IStepModule =>
            _moduleRegistrationStrategy.RegisterStepModules(Kernel, String.Empty, makeReusable, typeof(TStepModule));
       
        public void RegisterStepModule<TStepModule>() where TStepModule : IStepModule =>
            RegisterStepModule<TStepModule>(false);

        public void RegisterStepModules(params Type[] moduleTypes) =>
            _moduleRegistrationStrategy.RegisterStepModules(Kernel, String.Empty, false, moduleTypes);
        
        public void RegisterStepModules(Boolean makeReusable, params Type[] moduleTypes) =>
            _moduleRegistrationStrategy.RegisterStepModules(Kernel, String.Empty, makeReusable, moduleTypes);
      
        public void UnregisterStepModule<T>() where T : IStepModule =>
            UnregisterStepModules(typeof(T));

        public void UnregisterStepModules(params Type[] moduleTypes) =>
            _moduleRegistrationStrategy.UnregisterStepModules(Kernel, moduleTypes);
        
        public void Do<TStep>(Action<TStep> stepSetUpAction = null,
            StepExceptionHandlingMode exceptionHandlingMode = StepExceptionHandlingMode.Rethrow, Boolean validateStep = true, [CallerMemberName] String callingMethodName = "") 
            where TStep : IExecutableStep
        {
            Kernel.Get<IStepRunner>(GetKernelConstructorArg(), GetCurrentSessionIdConstructorArg())
                .Run(this, callingMethodName,
                Kernel.Get<TStep>(), 
                stepSetUpAction,
                exceptionHandlingMode, validateStep);
        }
        public void Do<TStep>(StepExceptionHandlingMode exceptionHandlingMode, Boolean validateStep = true, [CallerMemberName] String callingMethodName = "") 
            where TStep : IExecutableStep =>
            Do((Action<TStep>)null, exceptionHandlingMode, validateStep);
        public void Do<TStep>(Boolean validateStep, [CallerMemberName] String callingMethodName = "")
            where TStep : IExecutableStep =>
            Do((Action<TStep>)null, StepExceptionHandlingMode.Rethrow, validateStep);


        public TResult Do<TStep, TResult>(Action<TStep> stepSetUpAction = null,
            StepExceptionHandlingMode exceptionHandlingMode = StepExceptionHandlingMode.Rethrow, Boolean validateStep = true, [CallerMemberName] String callingMethodName = "") 
            where TStep : IExecutableStep<TResult>
        {           
            return Kernel.Get<IStepRunner>(GetKernelConstructorArg(), GetCurrentSessionIdConstructorArg())
                .RunWithReturnValue<TStep, TResult>(
                this, callingMethodName,
                Kernel.Get<TStep>(), 
                stepSetUpAction, 
                exceptionHandlingMode, validateStep);
        }
        public TResult Do<TStep, TResult>(StepExceptionHandlingMode exceptionHandlingMode, Boolean validateStep = true, [CallerMemberName] String callingMethodName = "")
            where TStep : IExecutableStep<TResult> =>
            Do<TStep, TResult>(null, exceptionHandlingMode, validateStep);
        public TResult Do<TStep, TResult>(Boolean validateStep, [CallerMemberName] String callingMethodName = "")
            where TStep : IExecutableStep<TResult> =>
            Do<TStep, TResult>(null, StepExceptionHandlingMode.Rethrow, validateStep);

        public void Do(Action outOfStepOperations, 
            StepExceptionHandlingMode exceptionHandlingMode = StepExceptionHandlingMode.Rethrow, [CallerMemberName] String callingMethodName = "")
        {
            var runner = Kernel.Get<IStepRunner>(GetKernelConstructorArg(), GetCurrentSessionIdConstructorArg());
            var step = Kernel.Get<FakeStep>();
            step.Operations = outOfStepOperations;
            runner.Run(this, callingMethodName, step, null, exceptionHandlingMode, false);
        }

        public TResult Do<TResult>(Func<TResult> outOfStepFuncWithReturnValue, 
            StepExceptionHandlingMode exceptionHandlingMode = StepExceptionHandlingMode.Rethrow, [CallerMemberName] String callingMethodName = "")
        {
            var runner = Kernel.Get<IStepRunner>(GetKernelConstructorArg(), GetCurrentSessionIdConstructorArg());
            var step = Kernel.Get<FakeStepWithReturnValue<TResult>>();
            step.OperationsWithReturnValue = outOfStepFuncWithReturnValue;
            return runner.RunWithReturnValue<FakeStepWithReturnValue<TResult>, TResult>(this, callingMethodName, step, null, exceptionHandlingMode, false);
        }

        public TStep GetStep<TStep>(Action<TStep> stepSetupAction = null) where TStep : IStep
        {
            var step = Kernel.Get<TStep>();
            stepSetupAction?.Invoke(step);
            return step;
        }
    }
}
