using System;
using System.Threading;
using System.Text;

namespace pico.LogOutput
{
	public class InputThread
	{
		public InputThread( picoLogOutputEditor editor )
		{
			m_editor = editor;

			m_thread = new Thread( Run );
			m_thread.Name = "LogServer";
			m_thread.IsBackground = true; //so that the thread can be killed if app dies.
			m_thread.SetApartmentState( ApartmentState.STA );
			m_thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			m_thread.Start();

			//m_timer = new System.Windows.Forms.Timer();
			//m_timer.Interval = 1000;
			//m_timer.Tick += ( object sender, System.EventArgs e ) =>
			//{
			//	DataItem di = CreateItem();
			//	m_editor.addDataItem( di, "All" );
			//};
			//m_timer.Start();
		}

		private void Run()
		{
			try
			{
				// server never ends...
				//
				m_server = new AsynchronousSocketListener( m_editor );
				m_server.StartListening();
			}
			catch( Exception ex )
			{
				System.Diagnostics.Debug.WriteLine( "AsynchronousSocketListener exception: " + ex.Message );
			}
			//finally
			//{

			//}
		}

		private DataItem CreateItem()
		{
			DataItem di = new DataItem();

			di.Type = s_random.Next( 0, 3 );
			di.Description = CreateString( s_random.Next( 12, 21 ) );
			di.Tag = CreateString( s_random.Next( 15, 36 ) );
			di.File = CreateString( s_random.Next( 15, 36 ) );
			di.Line = s_random.Next( 0, 10000 );

			return di;
		}

		private static string CreateString( int characters )
		{
			var sb = new StringBuilder();

			var max = Alphabet.Length;
			for (var i = 0; i < characters; i++)
			{
				var ch = Alphabet[s_random.Next( 0, max )];
				sb.Append( ch );
			}

			return sb.ToString();
		}

		private static readonly Random s_random = new Random( 1973 );
		private static readonly string Alphabet = "     ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		//public void AddDataItem( DataItem item )
		//{
		//	lock (this)
		//	{
		//		if (m_dataContainer != null )
		//			m_dataContainer.BeginInvoke( new MethodInvoker( () => this.AddDataItemThreadUnsafe( str ) ) );
		//	}
		//}

		//private void AddDataItemThreadUnsafe( DataItem item )
		//{
		//	m_dataContainer.AddInfo( str );
		//}

		private picoLogOutputEditor m_editor;
		private Thread m_thread;
		//private System.Windows.Forms.Timer m_timer;
		private AsynchronousSocketListener m_server;
	}
}
