using System.Collections;
using UnityEngine;

namespace AndroidRuntimePermissionsNamespace
{
	public class MainThreadDispatcher : MonoBehaviour
	{
		private static MainThreadDispatcher m_instance;
		private static MainThreadDispatcher Instance
		{
			get
			{
				if( m_instance == null )
				{
					m_instance = new GameObject( "MainThreadDispatcher" ).AddComponent<MainThreadDispatcher>();
					DontDestroyOnLoad( m_instance.gameObject );
				}

				return m_instance;
			}
		}

		public static void ExecuteOnMainThread( System.Action functionToExecute )
		{
			if( functionToExecute != null )
				Instance.StartCoroutine( ExecuteOnMainThreadCoroutine( functionToExecute ) );
		}

		private static IEnumerator ExecuteOnMainThreadCoroutine( System.Action functionToExecute )
		{
			yield return null;
			functionToExecute();
		}
	}
}