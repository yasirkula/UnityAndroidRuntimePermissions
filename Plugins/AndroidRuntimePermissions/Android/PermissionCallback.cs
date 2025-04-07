#if UNITY_EDITOR || UNITY_ANDROID
using System;
using UnityEngine;

namespace AndroidRuntimePermissionsNamespace
{
	public class PermissionCallback : AndroidJavaProxy
	{
		private readonly string[] permissions;
		private readonly Action<AndroidRuntimePermissions.Permission[]> callback;
		private readonly PermissionCallbackHelper callbackHelper;

		internal PermissionCallback( string[] permissions, Action<AndroidRuntimePermissions.Permission[]> callback ) : base( "com.yasirkula.unity.RuntimePermissionsReceiver" )
		{
			this.permissions = permissions;
			this.callback = callback;
			callbackHelper = PermissionCallbackHelper.Create( true );
		}

		[UnityEngine.Scripting.Preserve]
		public void OnPermissionResult( string result )
		{
			callbackHelper.CallOnMainThread( () => callback( AndroidRuntimePermissions.ProcessPermissionRequestResult( permissions, result ) ) );
		}
	}
}
#endif