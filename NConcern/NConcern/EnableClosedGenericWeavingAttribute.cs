using System;

namespace NConcern
{
    [Flags]
    public enum GenericWeavingFlags
    {
        None = 0,
        // todo Jens EnableOpenGenericWeaving without EnableClosedGenericWeaving is unpredictable
        EnableOpenGenericWeaving = 1,
        EnableClosedGenericWeaving = 2,
        EnableAllGenericWeaving = 3
    }
    /// <summary>
    /// This setting is only intended for backward compatibility it is recommended to handle the decision in the aspect.
    /// See also <see cref="Aspect.DefaultGenericWeavingFlags"/>
    /// <para>Can be applied to Aspects.</para>
    /// <para>Enables Weaving of generic types (usual skipped).</para>
    /// Flags:
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="GenericWeavingFlags.EnableOpenGenericWeaving"/></term>
    /// <description>
    /// <see cref="IAspect.Advise"/> is called with the open generic type as argument.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="GenericWeavingFlags.EnableClosedGenericWeaving"/></term>
    /// <description>
    /// <see cref="IAspect.Advise"/> is called with the close generic type as argument.
    /// <para>Note: Advise is called as the first thing in the types static constructor (before any user code).</para>
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="GenericWeavingFlags.EnableAllGenericWeaving"/></term>
    /// <description>
    /// Enable all of the above
    /// </description>
    /// </item>
    /// </list>
    /// Requirements:
    /// <list type="table">
    /// <listheader>
    /// <term>When</term>
    /// <description>Condition</description>
    /// </listheader>
    /// <item>
    /// <term>Always</term>
    /// <description>
    /// <see cref="IAspect.Advise"/> must return an <see cref="IAdvice"/> where <see cref="IAdvice.Decorate"/> is re-entrant and thread-safe.
    /// </description>
    /// </item>
    /// <item>
    /// <term>When <see cref="Flags"/> includes <see cref="GenericWeavingFlags.EnableClosedGenericWeaving"/></term>
    /// <description>
    /// <see cref="IAspect.Advise"/> must be re-entrant and thread-safe.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Notes:
    /// <list type="bullet">
    /// <item><decripion>As usual struct are skipped.</decripion></item>
    /// <item><decripion>As usual only classes managed by CNeptune can be weaved.</decripion></item>
    /// <item><decripion>Weaving of non generic types are not influenced by this attribute.</decripion></item>
    /// <item><decripion>Weaving of open generic are implemented types are not influenced by this attribute.</decripion></item>
    /// <item><decripion>
    /// For generic type <see cref="IAdvice.Decorate"/> is called as the first thing in the types static constructor (before any user code).
    /// Unless the aspect is Weaved after the type is loaded in which case it will be called as part of the weaving call.
    /// </decripion></item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public class EnableGenericWeavingAttribute : Attribute
    {
        /// <summary>
        /// Default: <see cref="GenericWeavingFlags.EnableAllGenericWeaving"/>
        /// </summary>
        public GenericWeavingFlags Flags { get; set; } = GenericWeavingFlags.EnableAllGenericWeaving;
    }
}