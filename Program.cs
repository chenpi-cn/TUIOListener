﻿/*

	Simple TUIO v1.1 / OSC v1.1 network listener.

	Defaults to listening for TUIO on port 3333.

	Usage:
		> mono TUIOListener [port] [tuio|osc]

	Libraries:
		https://github.com/valyard/TUIOsharp (v1.1 development branch)
		https://github.com/valyard/OSCsharp

	
	Author:
		Greg Harding greg@flightless.co.nz
		www.flightless.co.nz
	
	Copyright 2015 Flightless Ltd.
	

	The MIT License (MIT)

	Copyright (c) 2015 Flightless Ltd

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.

*/

using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

using TUIOsharp;
using TUIOsharp.DataProcessors;
using TUIOsharp.Entities;

using OSCsharp;
using OSCsharp.Data;
using OSCsharp.Net;
using OSCsharp.Utils;


namespace TUIOListener {

	class Program {

		public enum MessageType {
			TUIO,
			OSC
		};

		public static int port = 3333;
		public static MessageType messageType = MessageType.TUIO;


		public static void Main(string[] args) {
			if (ProcessCommandLineArgs(args)) {
				if (messageType == MessageType.TUIO) {
					ListenForTUIO();
				} else {
					ListenForOSC();
				}
			}
		}

		private static bool ProcessCommandLineArgs(string[] args) {
			if (args == null) return true;

			// help
			if (args.Length == 1 && (args[0] == "-h" || args[0] == "-help")) {
				Console.WriteLine("Usage: mono TUIOListener [port] [tuio|osc]");
				return false;
			}

			bool success = true;

			// port
			if (args.Length >= 1) {
				int _port;
				if (int.TryParse(args[0], out _port)) {
					if (_port > 1024 && _port <= 65535) {
						port = _port;
					} else {
						Console.WriteLine(string.Format("Warning: should be listening on port 1025..65535! Defaulting to port {0}", port));
					}
				} else {
					success = false;
				}
			}

			// message type
			if (args.Length >= 2) {
				string arg = args[1].ToLower();
				if (arg == "tuio") {
					messageType = MessageType.TUIO;
				} else if (arg == "osc") {
					messageType = MessageType.OSC;
				} else {
					Console.WriteLine(string.Format("Warning: should be listening for tuio or osc! Defaulting to {0}", messageType.ToString()));
				}
			}

			return success;
		}


		//
		// tuio listener
		//

		private static TuioServer tuioServer;

		private static void ListenForTUIO() {
			Console.WriteLine(string.Format("TUIO listening on port {0}... (Press escape to quit)", port));

			// tuio
			tuioServer = new TuioServer(port);

			CursorProcessor cursorProcessor = new CursorProcessor();
			cursorProcessor.CursorAdded += OnCursorAdded;
			cursorProcessor.CursorUpdated += OnCursorUpdated;
			cursorProcessor.CursorRemoved += OnCursorRemoved;

			BlobProcessor blobProcessor = new BlobProcessor();
			blobProcessor.BlobAdded += OnBlobAdded;
			blobProcessor.BlobUpdated += OnBlobUpdated;
			blobProcessor.BlobRemoved += OnBlobRemoved;

			ObjectProcessor objectProcessor = new ObjectProcessor();
			objectProcessor.ObjectAdded += OnObjectAdded;
			objectProcessor.ObjectUpdated += OnObjectUpdated;
			objectProcessor.ObjectRemoved += OnObjectRemoved;

			// listen...
			tuioServer.Connect();

			tuioServer.AddDataProcessor(cursorProcessor);
			tuioServer.AddDataProcessor(blobProcessor);
			tuioServer.AddDataProcessor(objectProcessor);

			do {
				while (!Console.KeyAvailable) {
					Thread.Sleep(100);
				}
			} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

			// done
			tuioServer.Disconnect();
			tuioServer = null;

			Console.WriteLine("Bye!");
		}

		private static void OnCursorAdded(object sender, TuioCursorEventArgs e) {
			var entity = e.Cursor;
			lock (tuioServer) {
				var x = entity.X;
				var y = (1 - entity.Y);
				Console.WriteLine(string.Format("Cursor Added {0}:{1},{2}", entity.Id, x, y));
			}
		}

		private static void OnCursorUpdated(object sender, TuioCursorEventArgs e) {
			var entity = e.Cursor;
			lock (tuioServer) {
				var x = entity.X;
				var y = (1 - entity.Y);

				Console.WriteLine(string.Format("Cursor Moved {0}:{1},{2}", entity.Id, x, y));
			}
		}

		private static void OnCursorRemoved(object sender, TuioCursorEventArgs e) {
			var entity = e.Cursor;
			lock (tuioServer) {
				Console.WriteLine(string.Format("Cursor Removed {0}", entity.Id));
			}
		}

		private static void OnBlobAdded(object sender, TuioBlobEventArgs e) {
			var entity = e.Blob;
			lock (tuioServer) {
				var x = entity.X;
				var y = (1 - entity.Y);
				Console.WriteLine(string.Format("Blob Added {0}:{1},{2}", entity.Id, x, y));
			}
		}

		private static void OnBlobUpdated(object sender, TuioBlobEventArgs e) {
			var entity = e.Blob;
			lock (tuioServer) {
				var x = entity.X;
				var y = (1 - entity.Y);
				Console.WriteLine(string.Format("Blob Moved {0}:{1},{2}", entity.Id, x, y));
			}
		}

		private static void OnBlobRemoved(object sender, TuioBlobEventArgs e) {
			var entity = e.Blob;
			lock (tuioServer) {
				Console.WriteLine(string.Format("Blob Removed {0}", entity.Id));
			}
		}

		private static void OnObjectAdded(object sender, TuioObjectEventArgs e) {
			var entity = e.Object;
			lock (tuioServer) {
				var x = entity.X;
				var y = (1 - entity.Y);
				Console.WriteLine(string.Format("Object Added {0}/{1}:{2},{3}", entity.ClassId, entity.Id, x, y));
			}
		}

		private static void OnObjectUpdated(object sender, TuioObjectEventArgs e) {
			var entity = e.Object;
			lock (tuioServer) {
				var x = entity.X;
				var y = (1 - entity.Y);
				Console.WriteLine(string.Format("Object Moved {0}/{1}:{2},{3}", entity.ClassId, entity.Id, x, y));
			}
		}

		private static void OnObjectRemoved(object sender, TuioObjectEventArgs e) {
			var entity = e.Object;
			lock (tuioServer) {
				Console.WriteLine(string.Format("Object Removed {0}/{1}", entity.ClassId, entity.Id));
			}
		}


		//
		// osc listener
		//

		private static void ListenForOSC() {
			Console.WriteLine(string.Format("OSC listening on port {0}... (Press escape to quit)", port));

			// osc
			UDPReceiver udpReceiver = new UDPReceiver(port, false);
			udpReceiver.MessageReceived += OscMessageReceivedHandler;
			udpReceiver.ErrorOccured += OscErrorOccuredHandler;

			// listen...
			udpReceiver.Start();

			do {
				while (!Console.KeyAvailable) {
					Thread.Sleep(100);
				}
			} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

			// done
			udpReceiver.Stop();
			udpReceiver = null;

			Console.WriteLine("Bye!");
		}

		private static void OscErrorOccuredHandler(object sender, ExceptionEventArgs exceptionEventArgs) {
			Console.WriteLine(string.Format("Error {0}", exceptionEventArgs.ToString()));
		}

		private static void OscMessageReceivedHandler(object sender, OscMessageReceivedEventArgs oscMessageReceivedEventArgs) {
			OscMessage msg = oscMessageReceivedEventArgs.Message;

			StringBuilder data = new StringBuilder();
			for (int i=0; i<msg.Data.Count; i++) {
				data.AppendFormat(" {0}", msg.Data[i]);
			}

			Console.WriteLine(string.Format("{0}{1}{2}", msg.Address, msg.TypeTag, data.ToString()));
		}
	}
}
