#if !UNITY_EDITOR && UNITY_ANDROID
using System.Threading;
using UnityEngine;

namespace AndroidRuntimePermissionsNamespace
{
	public class PermissionCallback : AndroidJavaProxy
	{
		private object threadLock;
		public string Result { get; private set; }

		public PermissionCallback( object threadLock ) : base( "com.yasirkula.unity.RuntimePermissionsReceiver" )
		{
			Result = null;
			this.threadLock = threadLock;
		}

		public void OnPermissionResult( string result )
		{
			Result = result;

			lock( threadLock )
			{
				Monitor.Pulse( threadLock );
			}
		}
	}
}
#endif