﻿using System;
using Ninject;

namespace TestUnium.Services.Implementations
{
    public class NinjectionService : IInjectionService
    {
        public void Inject(Action<IKernel> injections, params IKernel[] kernels)
        {
            foreach (var kernel in kernels)
            {
                injections(kernel);
            }
        }

        public IKernel CreateKernel()
        {
            var kernel = new StandardKernel(new NinjectSettings
            {
                InjectNonPublic = true,
                InjectParentPrivateProperties = true
            });

            return kernel;
        }      
    }
}