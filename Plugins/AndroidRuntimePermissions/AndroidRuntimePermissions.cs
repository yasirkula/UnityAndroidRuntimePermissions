#if !UNITY_EDITOR && UNITY_ANDROID
#define IS_ANDROID_PLATFORM
#endif

using System;
using System.Text;
using UnityEngine;
#if IS_ANDROID_PLATFORM
using AndroidRuntimePermissionsNamespace;
#endif

public static class AndroidRuntimePermissions
{
	public enum Permission { Denied = 0, Granted = 1, ShouldAsk = 2 };

	public delegate void PermissionResult( string permission, Permission result );
	public delegate void PermissionResultMultiple( string[] permissions, Permission[] result );

	#region Native Properties
#if IS_ANDROID_PLATFORM
	private static AndroidJavaClass m_ajc = null;
	private static AndroidJavaClass AJC
	{
		get
		{
			if( m_ajc == null )
				m_ajc = new AndroidJavaClass( "com.yasirkula.unity.RuntimePermissions" );

			return m_ajc;
		}
	}

	private static AndroidJavaObject m_context = null;
	private static AndroidJavaObject Context
	{
		get
		{
			if( m_context == null )
			{
				using( AndroidJavaObject unityClass = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) )
				{
					m_context = unityClass.GetStatic<AndroidJavaObject>( "currentActivity" );
				}
			}

			return m_context;
		}
	}
#endif
	#endregion

	#region Permission Functions
	public static void OpenSettings()
	{
#if IS_ANDROID_PLATFORM
		AJC.CallStatic( "OpenSettings", Context );
#else
		Debug.Log( "Opening settings..." );
#endif
	}

	public static Permission CheckPermission( string permission )
	{
#if IS_ANDROID_PLATFORM
		return CheckPermissions( permission )[0];
#else
		return Permission.Granted;
#endif
	}

	public static Permission[] CheckPermissions( params string[] permissions )
	{
		ValidateArgument( permissions );

#if IS_ANDROID_PLATFORM
		string resultRaw = AJC.CallStatic<string>( "CheckPermission", permissions, Context );
		if( resultRaw.Length != permissions.Length )
		{
			Debug.LogError( "CheckPermissions: something went wrong" );
			return null;
		}

		Permission[] result = new Permission[permissions.Length];
		for( int i = 0; i < result.Length; i++ )
		{
			Permission _permission = resultRaw[i].ToPermission();
			if( _permission == Permission.Denied && GetCachedPermission( permissions[i], Permission.ShouldAsk ) != Permission.Denied )
				_permission = Permission.ShouldAsk;

			result[i] = _permission;
		}

		return result;
#else
		return GetDummyResult( permissions );
#endif
	}

	public static Permission RequestPermission( string permission )
	{
#if IS_ANDROID_PLATFORM
		return RequestPermissions( permission )[0];
#else
		return Permission.Granted;
#endif
	}

	public static Permission[] RequestPermissions( params string[] permissions )
	{
		ValidateArgument( permissions );

#if IS_ANDROID_PLATFORM
		PermissionCallback nativeCallback;
		object threadLock = new object();
		lock( threadLock )
		{
			nativeCallback = new PermissionCallback( threadLock );
			AJC.CallStatic( "RequestPermission", permissions, Context, nativeCallback, GetCachedPermissions( permissions ) );

			if( nativeCallback.Result == null )
				System.Threading.Monitor.Wait( threadLock );
		}

		return ProcessPermissionRequest( permissions, nativeCallback.Result );
#else
		return GetDummyResult( permissions );
#endif
	}

	private static void RequestPermissionAsync( string permission, PermissionResult callback )
	{
#if IS_ANDROID_PLATFORM
		RequestPermissionsAsync( new string[1] { permission }, ( permissions, result ) =>
		{
			if( callback != null )
				callback( permissions[0], result[0] );
		} );
#else
		if( callback != null )
			callback( permission, Permission.Granted );
#endif
	}

	private static void RequestPermissionsAsync( string[] permissions, PermissionResultMultiple callback )
	{
		ValidateArgument( permissions );

#if IS_ANDROID_PLATFORM
		PermissionCallbackAsync nativeCallback = new PermissionCallbackAsync( permissions, callback );
		AJC.CallStatic( "RequestPermission", permissions, Context, nativeCallback, GetCachedPermissions( permissions ) );
#else
		if( callback != null )
			callback( permissions, GetDummyResult( permissions ) );
#endif
	}
	#endregion

	#region Helper Functions
	public static Permission[] ProcessPermissionRequest( string[] permissions, string resultRaw )
	{
		if( resultRaw.Length != permissions.Length )
		{
			Debug.LogError( "RequestPermissions: something went wrong" );
			return null;
		}

		bool shouldUpdateCache = false;
		Permission[] result = new Permission[permissions.Length];
		for( int i = 0; i < result.Length; i++ )
		{
			Permission _permission = resultRaw[i].ToPermission();
			result[i] = _permission;

			if( CachePermission( permissions[i], _permission ) )
				shouldUpdateCache = true;
		}

		if( shouldUpdateCache )
			PlayerPrefs.Save();

		return result;
	}

	private static Permission GetCachedPermission( string permission, Permission defaultValue )
	{
		return (Permission) PlayerPrefs.GetInt( "ARTP_" + permission, (int) defaultValue );
	}

	private static string GetCachedPermissions( string[] permissions )
	{
		StringBuilder cachedPermissions = new StringBuilder( permissions.Length );
		for( int i = 0; i < permissions.Length; i++ )
			cachedPermissions.Append( (int) GetCachedPermission( permissions[i], Permission.ShouldAsk ) );

		return cachedPermissions.ToString();
	}

	private static bool CachePermission( string permission, Permission value )
	{
		if( PlayerPrefs.GetInt( "ARTP_" + permission, -1 ) != (int) value )
		{
			PlayerPrefs.SetInt( "ARTP_" + permission, (int) value );
			return true;
		}

		return false;
	}

	private static void ValidateArgument( string[] permissions )
	{
		if( permissions == null || permissions.Length == 0 )
			throw new ArgumentException( "Parameter 'permissions' is null or empty!" );

		for( int i = 0; i < permissions.Length; i++ )
		{
			if( string.IsNullOrEmpty( permissions[i] ) )
				throw new ArgumentException( "A permission is null or empty!" );
		}
	}

	private static Permission[] GetDummyResult( string[] permissions )
	{
		Permission[] result = new Permission[permissions.Length];
		for( int i = 0; i < result.Length; i++ )
			result[i] = Permission.Granted;

		return result;
	}

	private static Permission ToPermission( this char ch )
	{
		return (Permission) ( ch - '0' );
	}
	#endregion
}