#if !UNITY_EDITOR && UNITY_ANDROID
#define IS_ANDROID_PLATFORM
#endif

using System;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID
using AndroidRuntimePermissionsNamespace;
#endif

public static class AndroidRuntimePermissions
{
	public enum Permission { Denied = 0, Granted = 1, ShouldAsk = 2 };

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

	public static bool CheckPermission( string permission )
	{
		return CheckPermissions( permission )[0];
	}

	public static bool[] CheckPermissions( params string[] permissions )
	{
		ValidateArgument( permissions );

#if IS_ANDROID_PLATFORM
		string resultRaw = AJC.CallStatic<string>( "CheckPermission", permissions, Context );
		if( resultRaw.Length != permissions.Length )
			throw new Exception( "CheckPermissions: something went wrong" );

		bool[] result = new bool[permissions.Length];
		for( int i = 0; i < result.Length; i++ )
			result[i] = resultRaw[i].ToPermission() == Permission.Granted;

		return result;
#else
		return GetDummyResult( permissions, true );
#endif
	}

	public static void RequestPermissionAsync( string permission, Action<Permission> callback )
	{
#if IS_ANDROID_PLATFORM
		RequestPermissionsAsync( new string[] { permission }, ( result ) => callback( result[0] ) );
#else
		callback( Permission.Granted );
#endif
	}

	public static void RequestPermissionsAsync( string[] permissions, Action<Permission[]> callback )
	{
#if IS_ANDROID_PLATFORM
		PermissionCallback nativeCallback = new PermissionCallback( permissions, callback );
		AJC.CallStatic( "RequestPermission", permissions, Context, nativeCallback );
#else
		callback( GetDummyResult( permissions, Permission.Granted ) );
#endif
	}

	public static async Task<Permission> RequestPermissionAsync( string permission )
	{
		return ( await RequestPermissionsAsync( permission ) )[0];
	}

	public static Task<Permission[]> RequestPermissionsAsync( params string[] permissions )
	{
		TaskCompletionSource<Permission[]> tcs = new TaskCompletionSource<Permission[]>();
		RequestPermissionsAsync( permissions, ( result ) => tcs.SetResult( result ) );
		return tcs.Task;
	}
	#endregion

	#region Helper Functions
	internal static Permission[] ProcessPermissionRequestResult( string[] permissions, string resultRaw )
	{
		if( resultRaw.Length != permissions.Length )
			throw new Exception( "RequestPermissions: something went wrong" );

		Permission[] result = new Permission[permissions.Length];
		for( int i = 0; i < result.Length; i++ )
			result[i] = resultRaw[i].ToPermission();

		return result;
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

	private static T[] GetDummyResult<T>( string[] permissions, T value )
	{
		T[] result = new T[permissions.Length];
		for( int i = 0; i < result.Length; i++ )
			result[i] = value;

		return result;
	}

	private static Permission ToPermission( this char ch )
	{
		return (Permission) ( ch - '0' );
	}
	#endregion
}