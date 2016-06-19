using Sce.Atf.Dom;

namespace SettingsEditor
{
    /// <summary>
    /// See docs of ReferenceValidator
    /// This validator overrides only one method
    /// </summary>
    public class SettingsReferenceValidator : ReferenceValidator
    {
        /// <summary>
        /// Performs actions after a reference's target DOM node has been removed; the
        /// default behavior is to remove the node from its parent
        /// New behavior is not to delete owner but just nullify reference to target
        /// </summary>
        /// <param name="e">Event args describing reference</param>
        protected override void OnReferentRemoved(ReferenceEventArgs e)
        {
            // check if node has been removed already; this can happen if it
            //  has multiple referents that have been removed
            DomNode parent = e.Owner.Parent;
            if ( parent != null && parent.Type == Schema.groupType.Type && e.AttributeInfo.Equivalent(Schema.groupType.selectedPresetRefAttribute) )
            {
                // nullify reference attribute
                e.Owner.SetAttribute( e.AttributeInfo, null );
            }
        }
    }
}
