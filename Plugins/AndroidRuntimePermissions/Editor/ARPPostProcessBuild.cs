#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AndroidRuntimePermissionsNamespace
{
	public class ARPPostProcessBuild
	{
		[InitializeOnLoadMethod]
		public static void ValidatePlugin()
		{
			string jarPath = "Assets/Plugins/AndroidRuntimePermissions/Android/RuntimePermissions.jar";
			if( File.Exists( jarPath ) )
			{
				Debug.Log( "Deleting obsolete " + jarPath );
				AssetDatabase.DeleteAsset( jarPath );
			}
		}
	}
}
#endif