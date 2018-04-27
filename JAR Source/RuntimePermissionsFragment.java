package com.yasirkula.unity;

import android.annotation.TargetApi;
import android.app.Fragment;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;

import java.util.ArrayList;

/**
 * Created by yasirkula on 27.04.2018.
 * Based on UnityAndroidPermissions (MIT License): https://github.com/Over17/UnityAndroidPermissions
 */

@TargetApi( Build.VERSION_CODES.M )
public class RuntimePermissionsFragment extends Fragment
{
	public static final String PERMISSIONS = "RTP_Permissions";
	private static final int PERMISSIONS_REQUEST_CODE = 122655;

	private final RuntimePermissionsReceiver permissionReceiver;
	private String[] m_permissions;

	public RuntimePermissionsFragment()
	{
		permissionReceiver = null;
	}

	public RuntimePermissionsFragment( final RuntimePermissionsReceiver permissionReceiver )
	{
		this.permissionReceiver = permissionReceiver;
	}

	@Override
	public void onCreate( Bundle savedInstanceState )
	{
		super.onCreate( savedInstanceState );
		if( permissionReceiver == null )
			getFragmentManager().beginTransaction().remove( this ).commit();
		else
		{
			m_permissions = getArguments().getStringArray( PERMISSIONS );
			requestPermissions( m_permissions, PERMISSIONS_REQUEST_CODE );
		}
	}

	@Override
	public void onRequestPermissionsResult( int requestCode, String[] permissions, int[] grantResults )
	{
		if( requestCode != PERMISSIONS_REQUEST_CODE )
			return;

		if( permissionReceiver == null || m_permissions == null )
		{
			Log.e( "Unity", "Fragment data got reset while asking permissions!" );
			return;
		}

		int resolvedPermissionCount = 0;
		ArrayList<Integer> permissionsResult = new ArrayList<Integer>( m_permissions.length );
		for( int i = 0; i < m_permissions.length; i++ )
			permissionsResult.add( 2 );

		// 0 -> denied, must go to settings
		// 1 -> granted
		// 2 -> denied, can ask again
		for( int i = 0; i < permissions.length && i < grantResults.length; i++ )
		{
			String permission = permissions[i];
			int permissionIndex = -1;
			for( int j = 0; j < m_permissions.length && permissionIndex == -1; j++ )
			{
				if( permission.equals( m_permissions[j] ) )
					permissionIndex = j;
			}

			if( permissionIndex == -1 )
			{
				Log.w( "Unity", "Didn't request permission for: " + permission );
				continue;
			}
			else
				resolvedPermissionCount++;

			if( grantResults[i] == PackageManager.PERMISSION_GRANTED )
				permissionsResult.set( permissionIndex, 1 );
			else if( !shouldShowRequestPermissionRationale( permission ) )
				permissionsResult.set( permissionIndex, 0 );
			else
				permissionsResult.set( permissionIndex, 2 );
		}

		if( resolvedPermissionCount != m_permissions.length )
			Log.e( "Unity", "Missed some permissions!" );

		String result = "";
		for( int i = 0; i < permissionsResult.size(); i++ )
			result += permissionsResult.get( i );

		permissionReceiver.OnPermissionResult( result );
		getFragmentManager().beginTransaction().remove( this ).commit();
	}
}
