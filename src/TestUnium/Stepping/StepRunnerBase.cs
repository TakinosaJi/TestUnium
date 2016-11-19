﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Ninject;
using TestUnium.Internal.Bootstrapping;
using TestUnium.Internal.Validation.Step;
using TestUnium.Internal.Validation.StepModules;
using TestUnium.Stepping.Pipeline;
using TestUnium.Stepping.Steps;

namespace TestUnium.Stepping
{
    /// <summary>
    /// In default implementation of StepDrivenTest instance of IStepRunner 
    /// will be recreated by DI container for running each separate step.
    /// </summary>
    public class StepRunnerBase : IStepRunner
    {
        private readonly IKernel _kernel;

        private IEnumerable<IStepModule> _modules;

        private readonly IEnumerable<IStepModuleValidator> _moduleValidators;
        private readonly IEnumerable<IStepValidator> _stepValidators;

        public StepRunnerBase(IKernel kernel, String sessionId)
        {
            _kernel = kernel;

            _modules = String.IsNullOrEmpty(sessionId)
                ? kernel.GetAll<IStepModule>()
                : kernel.GetAll<IStepModule>(sessionId);

            _moduleValidators = Container.Instance.Current.GetAll<IStepModuleValidator>();
            _stepValidators = Container.Instance.Current.GetAll<IStepValidator>();
        }

        private List<IStepModule> GetValidatedModulesForStep(IStep step)
        {
            var validatedModules = new List<IStepModule>();

            foreach (var module in _modules)
            {
                var isValid = true;
                var moduleType = module.GetType();
                foreach (var stepModuleValidator in _moduleValidators)
                {
                    if (!stepModuleValidator.Validate(moduleType, step))
                    {
                        isValid = false;
                    }
                }
                if (isValid)
                {
                    validatedModules.Add(module);
                }
            }

            return validatedModules;
        }

        private List<IStepModule> GetAbsendStepDefinedModules(IStep step, IEnumerable<IStepModule> contextualStepModules)
        {
            var modules = new List<IStepModule>();
            var attributes = step.GetType().GetCustomAttributes<UseWithStepModuleAttribute>().ToArray();
            var stepModuleTypes = attributes.SelectMany(a => a.GetStepModules()).Where(s => contextualStepModules.All(sm => sm.GetType() != s));
            foreach (var stepModuleType in stepModuleTypes)
            {
                _kernel.Bind(stepModuleType).ToSelf();
                modules.Add(_kernel.Get(stepModuleType) as IStepModule);
                _kernel.Unbind(stepModuleType);
            }

            return modules;
        }

        private void BeforeExecution(IStep step, IEnumerable<IStepModule> stepModules)
        {
            foreach (var module in stepModules)
            {
                module.BeforeExecution(step);
            }
        }

        private void AfterExecution(IStep step, StepState state, IEnumerable<IStepModule> stepModules)
        {
            foreach (var module in stepModules)
            {
                module.AfterExecution(step, state);
            }
        }

        public void Run<TStep>(IStepExecutor executor, String callingMethodName, TStep step,
            Action<TStep> stepSetUpAction, StepExceptionHandlingMode exceptionHandlingMode, Boolean validateStep)
            where TStep : IExecutableStep
        {
            step.Executor = executor;
            step.CallingMethodName = callingMethodName;
            step.ExceptionHandlingMode = exceptionHandlingMode;

            try
            {
                stepSetUpAction?.Invoke(step);
            }
            catch (Exception excp)
            {
                throw new StepSetUpException(
                    $"Unexpected error during setting up of step: {step.GetType().Name} has occured.", excp);
            }

            foreach (var stepValidator in _stepValidators)
            {
                var validator = stepValidator.Validate(step);
                Contract.Assert(validator.IsValid, validator.Message);
            }         

            step.PreExecute();

            var modules = GetValidatedModulesForStep(step);
            modules.AddRange(GetAbsendStepDefinedModules(step, modules));

            BeforeExecution(step, modules);
            try
            {
                step.Execute();
                step.State = StepState.Executed;
            }
            catch (Exception excp)
            {
                step.LastException = excp;
                step.State = StepState.Failed;
                AfterExecution(step, StepState.Failed, modules);
                if (step.ExceptionHandlingMode != StepExceptionHandlingMode.Rethrow) return;
                throw step.LastException;
            }

            AfterExecution(step, StepState.Executed, modules);
        }

        public TResult RunWithReturnValue<TStep, TResult>(IStepExecutor executor, String callingMethodName, TStep step,
            Action<TStep> stepSetUpAction, StepExceptionHandlingMode exceptionHandlingMode, Boolean validateStep)
            where TStep : IExecutableStep<TResult>
        {
            step.Executor = executor;
            step.CallingMethodName = callingMethodName;
            step.ExceptionHandlingMode = exceptionHandlingMode;

            try
            {
                stepSetUpAction?.Invoke(step);
            }
            catch (Exception excp)
            {
                throw new StepSetUpException($"Unexpected error during setting up of step: {step} has occured.", excp);
            }

            foreach (var stepValidator in _stepValidators)
            {
                var validator = stepValidator.Validate(step);
                Contract.Assert(validator.IsValid, validator.Message);
            }

            step.PreExecute();

            var modules = GetValidatedModulesForStep(step);
            modules.AddRange(GetAbsendStepDefinedModules(step, modules));

            var value = default(TResult);
            BeforeExecution(step, modules);
            try
            {
                value = step.Execute();
                step.State = StepState.Executed;
            }
            catch (Exception excp)
            {
                step.LastException = excp;
                step.State = StepState.Failed;
                AfterExecution(step, StepState.Failed, modules);
                if (step.ExceptionHandlingMode != StepExceptionHandlingMode.Rethrow) return value;
                throw step.LastException;
            }
            AfterExecution(step, StepState.Executed, modules);
            return value;
        }

        public void AddModules(params IStepModule[] modules)
        {
            if (modules == null || modules.Length <= 0) return;
            var modulesList = _modules.ToList();
            modulesList.AddRange(modules);
            _modules = modulesList.ToArray();
        }

        public void RemoveModules(params IStepModule[] modules)
        {
            if (modules == null || modules.Length <= 0) return;
            var modulesList = _modules.ToList();
            foreach (var module in modules)
            {
                modulesList.Remove(module);
            }
            _modules = modulesList.ToArray();
        }
    }
}
