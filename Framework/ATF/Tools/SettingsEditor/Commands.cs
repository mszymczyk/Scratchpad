using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace SettingsEditor
{
    /// <summary>
    /// Command component that defines Timeline-specific commands. All of these are
    /// popup commands.</summary>
    [Export(typeof(IInitializable))]
    [Export( typeof( IContextMenuCommandProvider ) )]
    [Export(typeof(Commands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Commands : ICommandClient, IInitializable, IContextMenuCommandProvider
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
		public Commands( ICommandService commandService, IContextRegistry contextRegistry, IDocumentService documentService )
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
			m_documentService = documentService;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering timeline commands</summary>
        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(                
                Command.ReloadDocument,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Reload Document",
                "Reloads Document",
                Keys.None,
                null,
                CommandVisibility.ApplicationMenu | CommandVisibility.Toolbar,
                this);

            m_commandService.RegisterCommand(
                Command.CreatePreset,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Create Preset",
                "Creates preset",
                Keys.None,
                null,
                CommandVisibility.ContextMenu | CommandVisibility.ApplicationMenu | CommandVisibility.Toolbar,
                this );

            m_commandService.RegisterCommand(
                Command.UnselectPreset,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Unselect Preset",
                "Unselects selected preset",
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this );
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            return DoCommand(commandTag, false);
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            DoCommand(commandTag, true);
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, CommandState commandState)
        {
			//Document document = m_contextRegistry.GetActiveContext<Document>();
			//if (document == null)
			//	return;

			//if (commandTag is Command)
			//{
			//	switch ((Command)commandTag)
			//	{
			//		case Command.ReloadDocument:
			//			commandState.Check = document.SplitManipulator != null ? document.SplitManipulator.Active : false;
			//			break;
			//	}
			//}
        }

        #endregion

        #region IContextMenuCommandProvider Members

        IEnumerable<object> IContextMenuCommandProvider.GetCommands( object context, object target )
        {
            ICommandClient cmdclient = (ICommandClient)this;

            foreach ( Command command in Enum.GetValues( typeof( Command ) ) )
            {
                if ( cmdclient.CanDoCommand( command ) )
                {
                    yield return command;
                }
            }
        }

        #endregion

        private bool DoCommand(object commandTag, bool doing)
        {
			DocumentEditingContext context = m_contextRegistry.GetActiveContext<DocumentEditingContext>();
            if (context == null)
                return false;

			Document document = context.As<Document>();
            if (document == null)
                return false;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.ReloadDocument:

                        if (doing)
                        {
							m_editor.Reload( document );
                        }
                        return true;

                    case Command.CreatePreset:
                        if ( doing )
                        {
                            System.Diagnostics.Debug.WriteLine( "CreatePreset called" );

                            Group group = document.Control.GetLastHit().As<Group>();

                            context.As<ITransactionContext>().DoTransaction( delegate
                            {
                                Preset.CreatePreset( group );
                            }, string.Format( "Create Preset: {0}".Localize(), group.Name ) );
                            return true;
                        }
                        else
                        {
                            object lastHit = context.LastSelected;
                            Group group = lastHit.As<Group>();
                            if ( group != null && !lastHit.Is<Preset>() && group.SettingGroup.HasPresets )
                                return true;
                            else
                                return false;
                        }
                    case Command.UnselectPreset:
                        if ( doing )
                        {
                            System.Diagnostics.Debug.WriteLine( "Unselect preset called" );

                            Group group = context.LastSelected.Cast<Group>();
                            string selectedPresetName = group.SelectedPresetRef.PresetName;

                            context.As<ITransactionContext>().DoTransaction( delegate
                            {
                                group.SelectedPresetRef = null;
                            }, string.Format( "Unselect preset: {0}, {1}".Localize(), group.Name, selectedPresetName ) );
                            return true;
                        }
                        else
                        {
                            object lastHit = context.LastSelected;
                            Group group = lastHit.As<Group>();
                            if ( group != null && !lastHit.Is<Preset>() && group.SelectedPresetRef != null )
                                return true;
                            else
                                return false;
                        }
                }
            }

            return false;
        }

        private enum Command
        {
            ReloadDocument,
            CreatePreset,
            UnselectPreset,
        }

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
		private IDocumentService m_documentService;

		[Import( AllowDefault = false )]
		private Editor m_editor = null;
	}
}
