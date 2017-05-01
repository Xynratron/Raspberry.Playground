using System;
using System.Collections.Generic;

namespace Bmf.Shared.Esb
{
    /// <summary>
    /// Interface to tell the plugin system, that here are some dependencies, which must be registered to the global 
    /// Dependency Injection Container. There should be a Method, which accepts a Contract-Type and an Implementation Type
    /// </summary>
    public interface IDependecyInjectionRegistration
    {
        /// <summary>
        /// Method which has the implementation of registering the types. The types are only known to the plugin assembly and 
        /// therefore it is not possible to register them in a global bootstrapper
        /// </summary>
        /// <param name="registration">An Action with takes 2 Parameters: First one is the Contract Type and the second one is the implementation type</param>
        void RegisterDependencies(Action<Type, Type> registration);
    }
}
