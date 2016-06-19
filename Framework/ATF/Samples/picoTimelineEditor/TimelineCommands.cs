//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Timelines;
using Sce.Atf.Controls.CurveEditing;
using ScrubberManipulator = Sce.Atf.Controls.Timelines.Direct2D.D2dScrubberManipulator;

using pico.Hub;

namespace picoTimelineEditor
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
        public TimelineCommands(ICommandService commandService, IContextRegistry contextRegistry, IDocumentRegistry documentRegistry, HubService hubService, TimelineEditor timelineEditor )
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
			m_documentRegistry = documentRegistry;
			m_hubService = hubService;
			//m_hubService.BlockOutboundTraffic = true;
			m_timelineEditor = timelineEditor;
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

			m_playPauseCommandInfo = m_commandService.RegisterCommand(
				Command.PlayPause,
				"Timeline",
				null,
				"Play",
				"Toggles Play/Pause",
				Keys.Space,
				pico.ResourcesRegistry.PlayImage,
				CommandVisibility.Default,
				this );

			m_commandService.RegisterCommand(
				Command.FlipCurveVertically,
				"Timeline",
				null,
				"Flip Curve Vertically",
				"Flips selected curve vertically",
				Keys.None,
				pico.ResourcesRegistry.FlipVerticallyImage,
				CommandVisibility.Default,
				this );

			m_commandService.RegisterCommand(
				Command.FlipCurveHorizontally,
				"Timeline",
				null,
				"Flip Curve Horizontally",
				"Flips selected curve horizontally",
				Keys.None,
				pico.ResourcesRegistry.FlipHorizontallyImage,
				CommandVisibility.Default,
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

					case Command.PlayPause:
						CommandService cs = (CommandService) m_commandService;

						if ( TimelineEditor.Playing && m_playPauseCommandInfo.ImageName != pico.ResourcesRegistry.PauseImage )
						{
							m_playPauseCommandInfo.ImageName = pico.ResourcesRegistry.PauseImage;
							commandState.Text = "Pause";

							if ( cs != null )
								cs.RefreshImage( m_playPauseCommandInfo );
						}
						else if ( !TimelineEditor.Playing && m_playPauseCommandInfo.ImageName != pico.ResourcesRegistry.PlayImage  )
						{
							m_playPauseCommandInfo.ImageName = pico.ResourcesRegistry.PlayImage;
							commandState.Text = "Play";

							if ( cs != null )
								cs.RefreshImage( m_playPauseCommandInfo );
						}

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
				Command command = (Command)commandTag;

				if (command == Command.PlayPause)
				{
					if ( doing )
					{
						togglePlayPause();
					}

					return true;
				}
				else
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

					case Command.FlipCurveVertically:
						{
							if ( activeInterval == null )
								return false;

							ICurveSet curveSet = activeInterval.As<ICurveSet>();
							if ( curveSet == null )
								return false;

							bool anyCurveHasEnoughControlPoints = false;
							foreach ( ICurve curve in curveSet.Curves )
							{
								if ( curve.ControlPoints.Count > 1 )
								{
									anyCurveHasEnoughControlPoints = true;
									break;
								}
							}

							if ( !anyCurveHasEnoughControlPoints )
								return false;

							if ( doing )
							{
								transactionContext.DoTransaction( delegate
								{
									foreach ( ICurve curve in curveSet.Curves )
									{
										if ( curve.ControlPoints.Count > 1 )
										{
											float maxY = curve.ControlPoints[0].Y;
											float minY = curve.ControlPoints[0].Y;

											foreach ( IControlPoint cp in curve.ControlPoints )
											{
												maxY = MathUtil.Max<float>( maxY, cp.Y );
												minY = MathUtil.Min<float>( minY, cp.Y );
											}

											float rangeY = maxY - minY;
											if ( rangeY > 0.0001f )
											{
												float rcpRangeY = 1.0f / rangeY;
												foreach ( IControlPoint cp in curve.ControlPoints )
												{
													float y = cp.Y - minY;
													y *= rcpRangeY;
													y = 1 - MathUtil.Clamp( y, 0, 1 );
													y *= rangeY;
													y += minY;
													cp.Y = y;
												}
												CurveUtils.ComputeTangent( curve );
											}
										}
									}
								},
								"Flip curves vertically" );
							}

							return true;
						}

					case Command.FlipCurveHorizontally:
						{
							if ( activeInterval == null )
								return false;

							ICurveSet curveSet = activeInterval.As<ICurveSet>();
							if ( curveSet == null )
								return false;

							bool anyCurveHasEnoughControlPoints = false;
							foreach ( ICurve curve in curveSet.Curves )
							{
								if ( curve.ControlPoints.Count > 1 )
								{
									anyCurveHasEnoughControlPoints = true;
									break;
								}
							}
							
							if ( ! anyCurveHasEnoughControlPoints )
								return false;

							if ( doing )
							{
								transactionContext.DoTransaction( delegate
								{
									foreach ( ICurve curve in curveSet.Curves )
									{
										if ( curve.ControlPoints.Count > 1 )
										{
											float maxX = curve.ControlPoints[0].X;
											float minX = curve.ControlPoints[0].X;

											// we need to make a copy of control points in reverse order
											// because after horizontal flip control points must have their X coordinate
											// in order, left to right
											//
											List<Sce.Atf.VectorMath.Vec2F> controlPoints = new List<Sce.Atf.VectorMath.Vec2F>();

											//for ( int i = curve.ControlPoints.Count-1; i >= 0; --i )
											for ( int i = curve.ControlPoints.Count; i-- > 0; )
											{
												IControlPoint cp = curve.ControlPoints[i];
												maxX = MathUtil.Max<float>( maxX, cp.X );
												minX = MathUtil.Min<float>( minX, cp.X );
												controlPoints.Add( new Sce.Atf.VectorMath.Vec2F( cp.X, cp.Y ) );
											}

											// it's not right to just change X coordinate of all control points
											// we would end with control points ordered by decreasing X coordinate
											// which atf curve editor doesn't accept
											// so:
											// delete all control points and recreate them in the right order
											//

											curve.Clear();

											float rangeX = maxX - minX;
											if ( rangeX > 0.0001f )
											{
												float rcpRangeX = 1.0f / rangeX;
												foreach ( Sce.Atf.VectorMath.Vec2F cp in controlPoints )
												{
													float x = cp.X - minX;
													x *= rcpRangeX;
													x = 1 - MathUtil.Clamp( x, 0, 1 );
													x *= rangeX;
													x += minX;
													IControlPoint newCP = curve.CreateControlPoint();
													newCP.X = x;
													newCP.Y = cp.Y;
													curve.AddControlPoint( newCP );
												}
												CurveUtils.ComputeTangent( curve );
											}
										}
									}
								},
								"Flip curves horizontally" );
							}

							return true;
						}
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
			PlayPause,
			FlipCurveVertically,
			FlipCurveHorizontally
        }

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
		private IDocumentRegistry m_documentRegistry;
		private HubService m_hubService;
		private TimelineEditor m_timelineEditor;

		private void togglePlayPause()
		{
			TimelineEditor.Playing = !TimelineEditor.Playing;
			setPlayPause( TimelineEditor.Playing );
		}

		private void setPlayPause( bool play )
		{
			TimelineEditor.Playing = play;

			m_timelineEditor.hubService_sendPlayPause();
		}

		private void contextRegistry_ActiveContextChanged( object sender, System.EventArgs e )
		{
			m_autoPlay.contextRegistry_ActiveContextChanged( sender, e );

			m_timelineEditor.hubService_sendSelectTimeline( true );
			m_timelineEditor.hubService_sendPlayPause();
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

					int inewPos = (int)scrubber.Position;
					m_scrubberPosTextBox.Text = inewPos.ToString();
				};

				MenuInfo menuInfo = TimelineMenu;

				m_resetTimelineButton = new ToolStripButton();
				m_resetTimelineButton.Name = "ResetTimeline";
				m_resetTimelineButton.Text = "Reset".Localize();
				m_resetTimelineButton.Click += delegate
				{
					setScrubberPosition( 0, false );
				};

				menuInfo.GetToolStrip().Items.Add( m_resetTimelineButton );


				m_scrubberPosTextBox = new ToolStripTextBox();
				m_scrubberPosTextBox.Name = "m_scrubberPosTextBox";
				m_scrubberPosTextBox.Text = "0";
				m_scrubberPosTextBox.LostFocus += ( object sender, System.EventArgs e ) =>
				{
					int pos;
					if ( int.TryParse( m_scrubberPosTextBox.Text, out pos ) )
					{
						setScrubberPosition( (float)pos, false );
					}
				};
				m_scrubberPosTextBox.KeyPress += ( object sender, KeyPressEventArgs e ) =>
				{
					if ( e.KeyChar == 13 )
					{
						int pos;
						if ( int.TryParse( m_scrubberPosTextBox.Text, out pos ) )
						{
							setScrubberPosition( (float)pos, false );
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

			public void contextRegistry_ActiveContextChanged( object sender, System.EventArgs e )
			{
				int scrubberPosition = 0;
				TimelineContext context = m_contextRegistry.GetActiveContext<TimelineContext>();
				if ( context != null )
				{
					TimelineDocument document = context.As<TimelineDocument>();
					scrubberPosition = (int) document.ScrubberManipulator.Position;
				}

				m_scrubberPosTextBox.Text = scrubberPosition.ToString();
			}

			private IContextRegistry m_contextRegistry;
			private ToolStripButton m_resetTimelineButton;
			private ToolStripTextBox m_scrubberPosTextBox;
		};

		private TimelineAutoPlay m_autoPlay;

		public static MenuInfo TimelineMenu =
            new MenuInfo( "Timeline", "Timeline".Localize( "this is the name of a menu" ), "Timeline Commands".Localize() );

		private CommandInfo m_playPauseCommandInfo;
	}
}
