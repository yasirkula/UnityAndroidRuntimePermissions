#if !UNITY_EDITOR && UNITY_ANDROID
using UnityEngine;

namespace AndroidRuntimePermissionsNamespace
{
	public class PermissionCallbackAsync : AndroidJavaProxy
	{
		private string result;
		private string[] permissions;
		private AndroidRuntimePermissions.PermissionResultMultiple callback;

		public PermissionCallbackAsync( string[] permissions, AndroidRuntimePermissions.PermissionResultMultiple callback ) : base( "com.yasirkula.unity.RuntimePermissionsReceiver" )
		{
			result = null;

			this.permissions = permissions;
			this.callback = callback;
		}

		public void OnPermissionResult( string result )
		{
			this.result = result;
			MainThreadDispatcher.ExecuteOnMainThread( ExecuteCallback );
		}

		private void ExecuteCallback()
		{
			if( callback != null )
			{
				callback( permissions, AndroidRuntimePermissions.ProcessPermissionRequest( permissions, result ) );
				callback = null;
			}
		}
	}
}
#endif