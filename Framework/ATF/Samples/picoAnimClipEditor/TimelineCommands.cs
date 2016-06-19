//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;
using ScrubberManipulator = Sce.Atf.Controls.Timelines.Direct2D.D2dScrubberManipulator;

using pico.Hub;
using picoAnimClipEditor.DomNodeAdapters;

namespace picoAnimClipEditor
{
    /// <summary>
    /// Command component that defines Timeline-specific commands. All of these are
    /// popup commands.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(TimelineCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TimelineCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public TimelineCommands(ICommandService commandService, IContextRegistry contextRegistry, HubService hubService, TimelineEditor editor )
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
			m_hubService = hubService;
			m_animClipEditor = editor;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering timeline commands</summary>
        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(                
                Command.RemoveGroup,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Remove/Group",
                "Removes the Group",
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(                
                Command.RemoveTrack,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Remove/Track",
                "Removes the track",
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(                
                Command.RemoveEmptyGroupsAndTracks,
                StandardMenu.Edit,
                StandardCommandGroup.EditGroup,
                "Remove/Empty Groups and Tracks",
                "Removes empty Groups and Tracks",
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(                
                Command.ToggleSplitMode,
                StandardMenu.Edit,
                null,
                "Interval Splitting Mode",
                "Toggles the interval splitting mode",
                Keys.S,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(StandardCommand.ViewZoomExtents, CommandVisibility.All, this);

			m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;

			m_commandService.RegisterMenu( TimelineMenu );

			m_autoPlay = new TimelineAutoPlay( m_contextRegistry );

			m_editMode = new ToolStripComboBox();
			m_editMode.DropDownStyle = ComboBoxStyle.DropDownList;
			m_editMode.Name = "Timeline Edit Mode";
			//m_editMode.ComboBox.Width = m_editMode.ComboBox.Width / 2;
			m_editMode.ComboBox.Items.Add( "Standalone" );
			m_editMode.ComboBox.Items.Add( "Editing" );
			m_editMode.ComboBox.Items.Add( "Preview" );
			m_editMode.ComboBox.SelectedIndex = 0;
			m_editMode.ToolTipText = "Selects editor operation mode".Localize();
			m_editMode.ComboBox.SelectedIndexChanged += (object sender, System.EventArgs e) =>
				{
					string editMode = m_editMode.SelectedItem as string;
					m_animClipEditor.changeEditMode( editMode );

					m_autoPlay.onEditModeChanged( m_animClipEditor.EditMode );	
				};

			TimelineMenu.GetToolStrip().Items.Add( m_editMode );

			//m_refreshSoundNames = new ToolStripButton();
			//m_refreshSoundNames.Enabled = true;
			//m_refreshSoundNames.Name = "RefreshSoundBanks";
			//m_refreshSoundNames.Text = "RefreshSoundBanks".Localize();
			//m_refreshSoundNames.Click += delegate
			//{
			//	pico.ScreamInterop.RefreshAllBanks();

			//	ISelectionContext selCtx = m_contextRegistry.GetActiveContext<ISelectionContext>();
			//	if ( selCtx != null )
			//	{
			//		// ui won't be automatically refreshed after bank refresh
			//		// this is a difficult to achieve because we don't know how many components there are using sound banks
			//		// changing current selection seems to be a good workaround
			//		// user is forced to select given element again (KeySound for instance), this time with refreshed sound names
			//		//
			//		TimelineDocument document = m_contextRegistry.GetActiveContext<TimelineDocument>();
			//		selCtx.Set( document );
			//	}
			//};

			//TimelineMenu.GetToolStrip().Items.Add( m_refreshSoundNames );
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
            TimelineDocument document = m_contextRegistry.GetActiveContext<TimelineDocument>();
            if (document == null)
                return;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.ToggleSplitMode:
                        commandState.Check = document.SplitManipulator != null ? document.SplitManipulator.Active : false;
                        break;
                }
            }
        }

        #endregion

        private bool DoCommand(object commandTag, bool doing)
        {
            TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
            if (context == null)
                return false;

            TimelineDocument document = context.As<TimelineDocument>();
            if (document == null)
                return false;

            if (commandTag is Command)
            {
                if ((Command)commandTag == Command.ToggleSplitMode)
                {
                    if (doing && document.SplitManipulator != null)
                        document.SplitManipulator.Active = !document.SplitManipulator.Active;
                    return true;
                }

                ITimelineObject target = m_contextRegistry.GetCommandTarget<ITimelineObject>();
                if (target == null)
                    return false;

                IInterval activeInterval = target as IInterval;
                ITrack activeTrack =
                    (activeInterval != null) ? activeInterval.Track : target.As<ITrack>();
                IGroup activeGroup =
                    (activeTrack != null) ? activeTrack.Group : target.As<IGroup>();
                ITimeline activeTimeline =
                    (activeGroup != null) ? activeGroup.Timeline : target.As<ITimeline>();
                ITransactionContext transactionContext = context.TimelineControl.TransactionContext;

                switch ((Command)commandTag)
                {
                    case Command.RemoveGroup:
                        if (activeGroup == null)
                            return false;

                        if (doing)
                        {
                            transactionContext.DoTransaction(delegate
                            {
                                activeGroup.Timeline.Groups.Remove(activeGroup);
                            },
                            "Remove Group");
                        }
                        return true;

                    case Command.RemoveTrack:
                        if (activeTrack == null)
                            return false;

                        if (doing)
                        {
                            transactionContext.DoTransaction(delegate
                            {
                                activeTrack.Group.Tracks.Remove(activeTrack);
                            },
                            "Remove Track");
                        }
                        return true;

                    case Command.RemoveEmptyGroupsAndTracks:
                        if (activeTimeline == null)
                            return false;

                        if (doing)
                        {
                            transactionContext.DoTransaction(delegate
                            {
                                IList<IGroup> groups = activeTimeline.Groups;
                                for (int i = 0; i < groups.Count; )
                                {
                                    IList<ITrack> tracks = groups[i].Tracks;
                                    for (int j = 0; j < tracks.Count; )
                                    {
                                        if (tracks[j].Intervals.Count == 0 && tracks[j].Keys.Count == 0)
                                            tracks.RemoveAt(j);
                                        else
                                            j++;
                                    }

                                    if (tracks.Count == 0)
                                        groups.RemoveAt(i);
                                    else
                                        i++;
                                }
                            },
                            "Remove Empty Groups and Tracks");
                        }
                        return true;
                }
            }
            else if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.ViewZoomExtents:
                        if (doing)
                        {
                            document.TimelineControl.Frame();
                        }
                        return true;
                }
            }

            return false;
        }

        private enum Command
        {
            RemoveGroup,
            RemoveTrack,
            RemoveEmptyGroupsAndTracks,
            ToggleSplitMode,
        }

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
		private HubService m_hubService;
		private TimelineEditor m_animClipEditor;

		private void contextRegistry_ActiveContextChanged( object sender, System.EventArgs e )
		{
			//m_autoPlay.contextRegistry_ActiveContextChanged( sender, e );

			TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
			if ( context == null )
				return;

			if ( m_animClipEditor.EditMode != EditMode.Standalone && !m_hubService.BlockOutboundTraffic )
			{
				Timeline tim = context.Cast<Timeline>();
				m_animClipEditor.changePreview( tim.AnimCategory, tim.AnimUserName );
			}
		}

		class TimelineAutoPlay
		{
			public TimelineAutoPlay( IContextRegistry contextRegistry )
			{
				m_contextRegistry = contextRegistry;

				ScrubberManipulator.Moved += ( object sender, System.EventArgs e ) =>
				{
					ScrubberManipulator scrubber = sender as ScrubberManipulator;
					if ( scrubber == null )
						return;

					int inewPos = (int) scrubber.Position;
					m_scrubberPosTextBox.Text = inewPos.ToString();
				};

				//m_stopWatch = new System.Diagnostics.Stopwatch();

				//m_timer = new Timer();
				//m_timer.Interval = 16; // 10 secs
				//m_timer.Tick += ( object sender, System.EventArgs e ) =>
				//{
				//	long milis = m_stopWatch.ElapsedMilliseconds;
				//	float newPosition = setScrubberPosition( (float) milis, true );

				//	m_stopWatch.Restart();
				//};

				//m_playTimelineButton = new ToolStripButton();
				//m_playTimelineButton.Name = "PlayTimeline";
				//m_playTimelineButton.Text = "Play".Localize();
				//m_playTimelineButton.Click += delegate
				//{
				//	if ( m_playTimelineButton.Text == "Play" )
				//	{
				//		// start playing timeline
				//		//
				//		m_playTimelineButton.Text = "Pause";
				//		m_playTimelineButtonOrigColor = m_playTimelineButton.BackColor;
				//		m_playTimelineButton.BackColor = System.Drawing.Color.Red;
				//		m_stopWatch.Restart();
				//		m_timer.Start();

				//	}
				//	else
				//	{
				//		// stop playing timeline
				//		//
				//		m_playTimelineButton.Text = "Play";
				//		m_playTimelineButton.BackColor = m_playTimelineButtonOrigColor;
				//		m_timer.Stop();
				//	}
				//};

				MenuInfo menuInfo = TimelineMenu;
				//MenuInfo menuInfo = MenuInfo.Edit;
				//menuInfo.GetToolStrip().Items.Add( m_playTimelineButton );

				m_resetTimelineButton = new ToolStripButton();
				m_resetTimelineButton.Enabled = false;
				m_resetTimelineButton.Name = "ResetTimeline";
				m_resetTimelineButton.Text = "Reset".Localize();
				m_resetTimelineButton.Click += delegate
				{
					setScrubberPosition( 0, false );
				};

				menuInfo.GetToolStrip().Items.Add( m_resetTimelineButton );


				m_scrubberPosTextBox = new ToolStripTextBox();
				m_scrubberPosTextBox.Enabled = false;
				m_scrubberPosTextBox.Name = "m_scrubberPosTextBox";
				m_scrubberPosTextBox.Text = "0";
				m_scrubberPosTextBox.LostFocus += ( object sender, System.EventArgs e ) =>
				{
					int pos;
					if ( int.TryParse( m_scrubberPosTextBox.Text, out pos ) )
					{
						setScrubberPosition( (float) pos, false );
					}
				};
				m_scrubberPosTextBox.KeyPress += ( object sender, KeyPressEventArgs e ) =>
				{
					if ( e.KeyChar == 13 )
					{
						int pos;
						if ( int.TryParse( m_scrubberPosTextBox.Text, out pos ) )
						{
							setScrubberPosition( (float) pos, false );
						}
					}
				};

				menuInfo.GetToolStrip().Items.Add( m_scrubberPosTextBox );
			}

			float setScrubberPosition( float pos, bool add )
			{
				TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
				if ( context == null )
					return 0;

				TimelineDocument document = context.As<TimelineDocument>();
				if ( document == null )
					return 0;

				if ( add )
					document.ScrubberManipulator.Position += pos;
				else
					document.ScrubberManipulator.Position = pos;

				return document.ScrubberManipulator.Position;
			}

			//public void contextRegistry_ActiveContextChanged( object sender, System.EventArgs e )
			//{
			//	m_timer.Stop();
			//	m_stopWatch.Stop();
			//}

			public void onEditModeChanged( EditMode editMode )
			{
				if ( editMode == EditMode.Standalone )
				{
					m_resetTimelineButton.Enabled = false;
					m_scrubberPosTextBox.Enabled = false;
				}
				else if ( editMode == EditMode.Editing )
				{
					m_resetTimelineButton.Enabled = true;
					m_scrubberPosTextBox.Enabled = true;
				}
				else if ( editMode == EditMode.Preview )
				{
					m_resetTimelineButton.Enabled = false;
					m_scrubberPosTextBox.Enabled = false;
				}
			}

			private IContextRegistry m_contextRegistry;
			//private ToolStripButton m_playTimelineButton;
			private ToolStripButton m_resetTimelineButton;
			private ToolStripTextBox m_scrubberPosTextBox;
			//private System.Drawing.Color m_playTimelineButtonOrigColor;
			//private System.Windows.Forms.Timer m_timer;
			//private System.Diagnostics.Stopwatch m_stopWatch;
		};

		private TimelineAutoPlay m_autoPlay;
		private ToolStripComboBox m_editMode;
		//private ToolStripButton m_refreshSoundNames;

		public static MenuInfo TimelineMenu =
            new MenuInfo( "Timeline", "Timeline".Localize( "this is the name of a menu" ), "Timeline Commands".Localize() );
	}
}
