#if UNITY_EDITOR || UNITY_ANDROID
using UnityEngine;

namespace AndroidRuntimePermissionsNamespace
{
	public class PermissionCallbackAsync : AndroidJavaProxy
	{
		private readonly string[] permissions;
		private readonly AndroidRuntimePermissions.AsyncPermissionResult callback;
		private readonly PermissionCallbackHelper callbackHelper;

		internal PermissionCallbackAsync( string[] permissions, AndroidRuntimePermissions.AsyncPermissionResult callback ) : base( "com.yasirkula.unity.RuntimePermissionsReceiver" )
		{
			this.permissions = permissions;
			this.callback = callback;
			callbackHelper = new GameObject( "PermissionCallbackHelper" ).AddComponent<PermissionCallbackHelper>();
		}

		public void OnPermissionResult( string result )
		{
			callbackHelper.CallOnMainThread( () => ExecuteCallback( result ) );
		}

		private void ExecuteCallback( string result )
		{
			try
			{
				if( callback != null )
					callback( AndroidRuntimePermissions.ProcessPermissionRequestResult( permissions, result ) );
			}
			finally
			{
				Object.Destroy( callbackHelper.gameObject );
			}
		}
	}
}
#endif