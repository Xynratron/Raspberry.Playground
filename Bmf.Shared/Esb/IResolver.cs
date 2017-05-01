namespace Bmf.Shared.Esb
{
    /// <summary>
    /// IResolver encapsulates a Dependecy Injection Container or any other Factory, which is able to create instances for a given Contract.
    /// Normally the resolver needs to be configured with the known contracts.
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Resolves a given Contract (e.g Interface or Class) to an instance of an object
        /// </summary>
        /// <typeparam name="TContract">the Type where an object is needed</typeparam>
        /// <returns>A generated instance of an object of Type TContract</returns>
        TContract Resolve<TContract>();
    }
}